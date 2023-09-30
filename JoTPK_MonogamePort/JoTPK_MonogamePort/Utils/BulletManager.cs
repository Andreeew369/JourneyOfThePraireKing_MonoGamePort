using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.Entities.Enemies;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Utils;

public class BulletManager {

    private static readonly int ShootGunDeg = 15;
    private long _lastTime = DateTime.Now.Ticks;

    private static readonly ImmutableArray<(float dirX, float dirY)> ShootGunDir = ImmutableArray.Create(
        ((float)Bullet.Speed, 0f),
        (Bullet.Speed, Bullet.Speed * (float)Math.Tan(Functions.DegToRadF(ShootGunDeg))),
        (Bullet.Speed, Bullet.Speed * (float)Math.Tan(Functions.DegToRadF(360-ShootGunDeg)))
    );

    public int BulletCount => _bullets.Count;

    private readonly List<Bullet> _bullets;
    private static readonly int BulletPoolSize = 200;
    private readonly Queue<Bullet> _bulletPool;
    private readonly int _compoziteImageSize;

    public BulletManager(Level level) {
        _bullets = new List<Bullet>();
        _bulletPool = new Queue<Bullet>();
        _compoziteImageSize = Level.Width * Consts.ObjectSize;
        for (int i = 0; i < BulletPoolSize; i++) {
            _bulletPool.Enqueue(new Bullet(0, 0, 0, 0, 0, level));
        }
    }

    public void Draw(SpriteBatch sb) {
        List<Bullet> copy = new(_bullets);

        foreach (Bullet b in copy) {
            b.Draw(sb);
        }
    }

    public void Update(Level level, EnemiesManager enemiesManager) {
        List<Bullet> copy = new(_bullets);
        foreach (Bullet b in copy) {
            b.Move(level, out float dx, out float dy);
            if (b.CollisionDetection(b.HitBox.X, b.HitBox.Y, dx, dy, enemiesManager) || b.Collided) {
                _bullets.Remove(b);
                RecycleBullet(b);
            }
        }
    }

    public void AddBullets(object sender, Level level) {

        float dx = 0;
        float dy = 0;

        if (sender is Player player) {

            long nowTime = DateTime.Now.Ticks;
            float fireRate = player.ShootingType is ShootingType.Normal ? player.FireRate : Player.DefaultFireRate;
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
                } break;
                case ShootingType.ShotGun: {
                    if (Consts.DirectionToAnglesMap.TryGetValue((dx, dy), out float rad)) {
                        (float lx, float ly) = Matrix2X2.RotationMatrix(ShootGunDir[1], rad);
                        (float rx, float ry) = Matrix2X2.RotationMatrix(ShootGunDir[2], rad);
                        _bullets.AddRange(new[] {
                            GetBulletFromPool(player.X, player.Y, dx, dy, player.BulletDamage, level),
                            GetBulletFromPool(player.X, player.Y, lx, ly, player.BulletDamage, level), //left bullet
                            GetBulletFromPool(player.X, player.Y, rx, ry, player.BulletDamage, level)  //right bullet
                        });
                    }
                } break;
                case ShootingType.Normal: {
                    _bullets.Add(GetBulletFromPool(player.X, player.Y, dx, dy, player.BulletDamage, level));
                } break;
                default: throw new NotImplementedException();
            }

            //lock (_poolLock) {
            //    Console.WriteLine(this._bulletPool.Count);
            //}
            //lock (_lock) {
            //    Console.WriteLine(this._bullets.Count);
            //}
        }
        else if (sender is IBoss) {

        }
    }

    private Bullet GetBulletFromPool(float x, float y, float dx, float dy, int damage, Level level) {
        
        if (_bulletPool.Count > 0) {
            Bullet bullet = _bulletPool.Dequeue();
            bullet.Reset(x, y, dx, dy, damage, level);
            return bullet;
        }
        
        return new Bullet(x, y, dx, dy, damage, level);
    }

    private void RecycleBullet(Bullet bullet) {
        _bulletPool.Enqueue(bullet);
    }

    private IEnumerable<Bullet> GetWheel(GameObject sender, Level level) {
        foreach ((float x, float y) dir in Consts.AllDirections) {
            yield return GetBulletFromPool(
                sender.X, sender.Y,
                dir.x, dir.y,
                sender is Player player ? player.BulletDamage : 0,
                level
            );
        }
    }

    public void Clear() {
        // List<Bullet> copy = new(_bullets);
        // foreach (Bullet bullet in copy) {
        //     _bullets.Remove(bullet);
        // }
        _bullets.Clear();
    }
}