using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.Entities.Enemies;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Items;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Utils;

/// <summary>
/// Class that manages the movement, spawning and collision detection of bullets
/// </summary>
public class BulletManager {

    private long _lastTime = DateTime.Now.Ticks;

    public int BulletCount => _bullets.Count;

    private const int BulletPoolSize = 200;
    private readonly List<Bullet> _bullets;
    private readonly Queue<Bullet> _bulletPool;
    private GameElements _bulletType;

    public BulletManager(Level level, GameElements type) {
        _bulletType = type;
        _bullets = new List<Bullet>();
        _bulletPool = new Queue<Bullet>();
        for (int i = 0; i < BulletPoolSize; i++) {
            _bulletPool.Enqueue(new Bullet(0, 0, 0, 0, 0, level, type));
        }
    }

    public void Draw(SpriteBatch sb) => new List<Bullet>(_bullets).ForEach(b => b.Draw(sb));

    
    public void Update(Level level, EnemiesManager? enemiesManager, GameObject sender) {
        new List<Bullet>(_bullets).ForEach(b => {
            b.Move(level, out float dx, out float dy);
            RectangleF hitBox = b.HitBox;
            if (b.CollisionDetection(hitBox.X, hitBox.Y, dx, dy, enemiesManager, sender) || b.Collided) {
                _bullets.Remove(b);
                RecycleBullet(b);
            }
        });
    }

    /// <summary>
    /// Adds bullets to the list of bullets
    /// </summary>
    /// <param name="sender">Object that called this method</param>
    /// <param name="level">Intance of the current level</param>
    /// <exception cref="NotImplementedException">If new enum values for <see cref="ShootingType"/> are added and this case isn't implemented</exception>
    public void AddBullets(GameObject sender, Level level) {
        float dx = 0;
        float dy = 0;

        if (sender is Player player) {
            long nowTime = DateTime.Now.Ticks;
            float fireRate = player.ShootingType is ShootingType.Normal 
                ? player.FireRate 
                : player.DefaultFireRate;
            long ticks = (long)(fireRate * 10_000_000);
            
            if (_lastTime + ticks >= nowTime) return;
            _lastTime = nowTime;
            
            (bool up, bool down, bool left, bool right) = player.ShootingDirBool;
            if ((up && down) || (right && left)) return;

            if (up) dy -= Bullet.Speed;
            if (down) dy += Bullet.Speed;
            if (left) dx -= Bullet.Speed;
            if (right) dx += Bullet.Speed;

            if (dx != 0 && dy != 0) {
                dx *= Consts.InverseSqrtOfTwo;
                dy *= Consts.InverseSqrtOfTwo;
            }

            switch (player.ShootingType) {
                case ShootingType.Wheel: {
                    _bullets.AddRange(GetWheel(player, level));
                    break;
                } 
                case ShootingType.ShotGun: {
                    if (Consts.DirectionToAnglesMap.TryGetValue((dx, dy), out float rad)) {
                        (float lx, float ly) = Matrix2X2.RotateVector(Consts.ShootGunDir[1], rad);
                        (float rx, float ry) = Matrix2X2.RotateVector(Consts.ShootGunDir[2], rad);
                        
                        _bullets.AddRange(new[] {
                            GetBulletFromPool(player.X, player.Y, dx, dy, player.BulletDamage, level), // center bullet
                            GetBulletFromPool(player.X, player.Y, lx, ly, player.BulletDamage, level), // left bullet
                            GetBulletFromPool(player.X, player.Y, rx, ry, player.BulletDamage, level)  // right bullet
                        });
                    }
                    break;
                } 
                case ShootingType.Normal: {
                    _bullets.Add(GetBulletFromPool(player.X, player.Y, dx, dy, player.BulletDamage, level));
                    break;
                } 
                default: throw new NotImplementedException();
            }
        }
        else if (sender is CowBoy) {
            // todo: implement cowBoy shooting
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Returns bullets to the pool of bullets
    /// </summary>
    /// <param name="bullet">Bullet to be recycled</param>
    private void RecycleBullet(Bullet bullet) => _bulletPool.Enqueue(bullet);
    
    public void Clear() {
        _bullets.ForEach(b => _bulletPool.Enqueue(b));
        _bullets.Clear();
    }

    public void ChangeBullerType(GameElements type, Level level) {
        _bulletPool.Clear();
        for (int i = 0; i < BulletPoolSize; i++) {
            _bulletPool.Enqueue(new Bullet(0, 0, 0, 0, 0, level, type));
        }
        _bulletType = type;
    }
    
    /// <summary>
    /// Creates 8 bullets that go in all directions. Used in <see cref="WagonWheel"/>
    /// </summary>
    /// <param name="sender"><see cref="GameObject"/> that is calling this method</param>
    /// <param name="level">Instance of the current level</param>
    /// <returns>Enumerable of 8 bullets</returns>
    private IEnumerable<Bullet> GetWheel(GameObject sender, Level level) =>
        Consts.AllDirections.Select(dir => GetBulletFromPool(
            sender.X, sender.Y,
            dir.x, dir.y,
            sender is Player player ? player.BulletDamage : 0, 
            level
        ));
    
    /// <summary>
    /// Gets a bullet from the pool of bullets or creates a new one if the pool is empty.
    /// </summary>
    /// <param name="x">X coordinate of the bullet</param>
    /// <param name="y">Y coordinate of the bullet</param>
    /// <param name="dx">X velocity of the bullet</param>
    /// <param name="dy">Y velocity of the bullet</param>
    /// <param name="damage">Damage of the bullet</param>
    /// <param name="level">Instance of the current level</param>
    /// <returns>Instance of the bullet from pool</returns>
    private Bullet GetBulletFromPool(float x, float y, float dx, float dy, int damage, Level level) {
        if (_bulletPool.Count == 0) 
            return new Bullet(x, y, dx, dy, damage, level, _bulletType);
        
        Bullet bullet = _bulletPool.Dequeue();
        bullet.Reset(x, y, dx, dy, damage, level);
        return bullet;
    }
}
