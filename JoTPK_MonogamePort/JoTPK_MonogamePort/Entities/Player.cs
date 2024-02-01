using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Timers;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Items;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Timer = System.Timers.Timer;

namespace JoTPK_MonogamePort.Entities;

public delegate void EventHandler();

public class Player : GameObject, ISender {
    private const float AnimationInterval = 0.125f; //seconds
    private const int RespawnTime = 3000;

    public static readonly int Middle = Level.Width / 2 * Consts.ObjectSize;
    
    private static readonly ImmutableArray<GameElements> LegSprites = ImmutableArray.Create(new[] {
        GameElements.PlayerLegs1, GameElements.PlayerLegs2, GameElements.PlayerLegs3, GameElements.PlayerLegs2
    });

    private static readonly ImmutableArray<GameElements> ZombieSprites = ImmutableArray.Create(new[] {
        GameElements.PlayerZombie1, GameElements.PlayerZombie2
    });

    public int XIndex { get; private set; }
    public int YIndex { get; private set; }
    public float DefaultSpeed { get; set; } = 3.5f;
    public float Speed { get; set; }
    
    public float DefaultFireRate { get; set; } = 0.3f;
    public float FireRate { get; set; }

    public int Money { get; set; }
    public int Health { get; set; }

    public int BulletDamage { get; set; }
    public bool IsZombie { get; set; }
    public bool IsInvincible { get; set; }
    public bool IsDead { get; set; }
    
    private float _timer;
    private int _sprite;

    public IPowerUp InventoryPowerUp { get; set; }
    //public IPowerUp ActivePowerUp { get; set; } 
    public ImmutableHashSet<IPowerUp> ActivePowerUps => _activePowerUps.ToImmutableHashSet();
    public bool CanRemoveShootGun { get; set; } = true;

    private readonly HashSet<IPowerUp> _activePowerUps;
    private readonly SortedSet<ShootingType> _shootingTypes;
    
    private bool _mUp;
    private bool _mDown;
    private bool _mLeft;
    private bool _mRight;

    private bool _sUp;
    private bool _sDown;
    private bool _sLeft;
    private bool _sRight;
    
    private GameElements _body;
    private GameElements _legs;

    private List<GameObject> _surroundings;

    private readonly Level _level;
    private readonly Trader _trader;
    private readonly EnemiesManager _enemiesManager;
    private readonly Timer _respawnTimer;
    private readonly BulletManager _bulletManager;

    public Player(int x, int y, Level level, Trader trader, EnemiesManager em) : base(x, y) {
        (XIndex, YIndex) = Level.GetIndexes(x, y);
        Speed = DefaultSpeed;
        Money = 100; // todo zmenit
        Health = 5; // todo zmenit
        FireRate = DefaultFireRate;
        _shootingTypes = new SortedSet<ShootingType>(new[] { ShootingType.Normal });
        ShootingType = ShootingType.ShotGun;
        InventoryPowerUp = EmptyPowerUp.Empty;
        BulletDamage = 1;
        _activePowerUps = new HashSet<IPowerUp>();
        _body = GameElements.Player;
        _legs = LegSprites[0];
        _level = level;
        _trader = trader;
        _enemiesManager = em;
        _respawnTimer = new Timer(RespawnTime) { AutoReset = false };
        _respawnTimer.Elapsed += OnRespawn;
        _activePowerUps = new HashSet<IPowerUp>();
        _surroundings = level.GetSurroundings(XIndex, YIndex);
        _bulletManager = new BulletManager(_level, GameElements.Bullet1);
    }

    public ShootingType ShootingType {
        get => _shootingTypes.First();
        set => _shootingTypes.Add(value);
    }
    
    public void RemoveShootingType(ShootingType shootingType) {
        if (shootingType == ShootingType.Normal || (_shootingTypes.Contains(shootingType) && !CanRemoveShootGun)) return;
        _shootingTypes.Remove(shootingType);
    }

    public const int PlayerHitBoxX = 4;
    public const int PlayerHitBoxY = 16;
    public static readonly int HitBoxW = Consts.ObjectSize - 2 * PlayerHitBoxX;
    public static readonly int HitBoxH = Consts.ObjectSize - PlayerHitBoxY - 1;

    public override RectangleF HitBox {
        get {
            int legPixels = 0;
            if (IsMoving || IsShooting) {
                legPixels = 2;
            }
            return new RectangleF(X + PlayerHitBoxX, Y + PlayerHitBoxY, HitBoxW, HitBoxH + legPixels);
        }
    }

    public (bool u, bool d, bool l, bool r) ShootingDirBool => (_sUp, _sDown, _sLeft, _sRight);
    private (bool u, bool d, bool l, bool r) MovementDirBool => (_mUp, _mDown, _mLeft, _mRight);

    private bool IsShooting => _sUp || _sDown || _sLeft || _sRight;
    private bool IsMoving => _mUp || _mDown || _mLeft || _mRight;

    public override void Draw(SpriteBatch sb) {
        if (IsDead)
            return;
        
        _bulletManager.Draw(sb);
        
        if (!IsZombie) {
            if (IsMoving && (!_mDown || !_mUp) && (!_mLeft || !_mRight)) {
                TextureManager.DrawObject(_legs, RoundedX, RoundedY + Consts.ObjectSize - 4, sb);
            }
            else if (IsShooting && (!_sDown || !_sUp) && (!_sLeft || !_sRight)) {
                TextureManager.DrawObject(GameElements.PlayerLegs2, RoundedX, RoundedY + Consts.ObjectSize - 4, sb);
            }
        }
        TextureManager.DrawObject(_body, RoundedX, RoundedY, sb);
    }

    public void Update(Level level, EnemiesManager? enemiesManager, GameTime gt) {

        if (IsDead) return;
        
        if (_bulletManager.BulletCount != 0) {
            _bulletManager.Update(level, enemiesManager, this);
        }

        HashSet<IPowerUp> copy = new(_activePowerUps);
        foreach (IPowerUp powerUp in copy) {
            if (powerUp is IItem item) {
                item.Update(this, level, gt);
            }
        }

        KeyboardState kst = Keyboard.GetState();

        _mUp = kst.IsKeyDown(Keys.W);
        _mDown = kst.IsKeyDown(Keys.S);
        _mRight = kst.IsKeyDown(Keys.D);
        _mLeft = kst.IsKeyDown(Keys.A);

        _sUp = kst.IsKeyDown(Keys.Up);
        _sDown = kst.IsKeyDown(Keys.Down);
        _sRight = kst.IsKeyDown(Keys.Right);
        _sLeft = kst.IsKeyDown(Keys.Left);

        if (InventoryPowerUp is not EmptyPowerUp && kst.IsKeyDown(Keys.Space)) {
            InventoryPowerUp.Activate(this, true);
        }

        if (!IsZombie) {
            _body = GetPlayerSprite();
        }

        if (IsMoving) {
            _timer += gt.ElapsedGameTime.Milliseconds / 1000f;
            bool updateSprites = false;
            if (_timer >= AnimationInterval) {
                _timer = 0;
                _sprite++;
                updateSprites = true;
            }

            Move(level);
            // todo redo this
            if (!updateSprites)
                return;

            if (IsZombie) {
                if (_sprite >= ZombieSprites.Length) {
                    _sprite = 0;
                }
                _body = ZombieSprites[_sprite];
            }
            else {
                if (_sprite >= LegSprites.Length) {
                    _sprite = 0;
                }
                _legs = LegSprites[_sprite];
            }
        } else if (IsZombie && !IsMoving) {
            if (_sprite >= ZombieSprites.Length) {
                _sprite = 0;
            }
            _body = ZombieSprites[_sprite];
        }

        if (IsShooting)
            Shoot();
    }
    
    public void Move(Level level) {

        float dx = 0;
        float dy = 0;

        if (_mUp) dy -= Speed;
        if (_mDown) dy += Speed;
        if (_mLeft) dx -= Speed;
        if (_mRight) dx += Speed;

        if (dx != 0 && dy != 0) {
            dx *= Consts.InverseSqrtOfTwo;
            dy *= Consts.InverseSqrtOfTwo;
        }

        RectangleF hitbox = HitBox;
        float nextY = hitbox.Y + dy;
        float nextX = hitbox.X + dx;

        IItem? item;
        if (!CollisionDetection(nextX, hitbox.Y, out item, dx, out dx)) {
            X += dx;
            //Console.WriteLine(dx);
        }
        item?.PickUp(this, _level);

        if (!CollisionDetection(hitbox.X, nextY, out item, dy, out dy)) {
            Y += dy;
            //Console.WriteLine(dy);
        }
        item?.PickUp(this, _level);
        _trader.PickUpUpgrade(this, _level);


        UpdateIndexes(out bool didChange);

        if (didChange) {
            _surroundings = level.GetSurroundings(XIndex, YIndex);
        }
    }
    
    public void KillPlayer() {
        Health--;
        _level.ClearLevel();
        _bulletManager.Clear();
        if (Health <= 0) {
            _level.GameOver = true;
            return;
        }

        _level.ResetLevelTimer();
        IsDead = true;
        if (!_level.GameOver) {
            _respawnTimer.Start();
        } 
    }

    public void AddActivePowerUp(IPowerUp powerUp) {
        _activePowerUps.Add(powerUp);
    }

    public void RemoveActivePowerUp(IPowerUp powerUp) {
        if (!DoesPlayerHavePowerUp(powerUp, out IPowerUp? p) || p == null) return;
        _activePowerUps.Remove(p);
    }

    public void UpdateSurroundings(Level level) {
        UpdateIndexes(out bool _);
        _surroundings = level.GetSurroundings(XIndex, YIndex);
    }

    public void Shoot() {
        _bulletManager.AddBullets(this, _level);
    }


    public void UpdateIndexes(out bool didChange) {
        (int indexX, int indexY) = Level.GetIndexes(XMiddle, YMiddle);

        if (indexX == XIndex && indexY == YIndex) {
            didChange = false;
            return;
        }

        XIndex = indexX;
        YIndex = indexY;
        didChange = true;
    }

    public bool CollisionDetection(float nextX, float nextY, out IItem? item,
            float differenceIn, out float differenceOut
        ) {

        item = null;

        List<GameObject> pickedUpItems = new();
        RectangleF hitbox = HitBox;

        foreach (GameObject? o in _surroundings) {
            if (!o.IsColliding(nextX, nextY, hitbox.Width, hitbox.Height)) continue;

            if (o is IItem i) {
                item?.PickUp(this, _level);
                item = i;
                pickedUpItems.Add(o);
                //Console.WriteLine((o.X, o.Y));
                continue;
            }

            differenceOut = Functions.GetDistanceBetweenObjects(nextX, nextY, this, o, differenceIn);
            //Console.WriteLine(@"X, Y: " + (X, Y));
            //Console.WriteLine(@"XMiddle, YMiddle: " + (XMiddle, YMiddle));
            //Console.WriteLine(Consts.ObjectSize);
            return true;
        }

        foreach (GameObject i in pickedUpItems) {
            _surroundings.Remove(i);
        }
        
        differenceOut = differenceIn;
        return false;
    }
    
    public bool DoesPlayerHavePowerUp(IPowerUp powerUp, out IPowerUp? powerUpOut) {
        foreach (IPowerUp pu in ActivePowerUps) {
            if (pu.GetType() != powerUp.GetType())
                continue;
            
            powerUpOut = pu;
            return true;
        }

        powerUpOut = null;
        return false;
    }

    public void ResetPosition(int levelNumber) {
        Y = levelNumber is 4 or 8 or 12 ? 2 * Consts.ObjectSize : Middle;
        X = Middle;
    }

    private void OnRespawn(object? sender, ElapsedEventArgs e) {
        IsDead = false;
        (X, Y) = (Middle, Middle);
        _enemiesManager.CanSpawn = true;
        
        //todo stuff when player dies
    }

    private void ActivatePowerUp() {
        InventoryPowerUp.Activate(this, true);
    }

    private GameElements GetPlayerSprite() {
        if (IsShooting) {
            (bool u, bool d, bool l, bool r) sDir = ShootingDirBool;
            return sDir switch {
                (_, _, true, false) => GameElements.PlayerLeft,
                (_, _, false, true) => GameElements.PlayerRight,
                (true, false, false, false) => GameElements.PlayerUp,
                (false, true, false , false) => GameElements.PlayerDown,
                _ => GameElements.Player
            };
        }

        (bool u, bool d, bool l, bool r) mDir = MovementDirBool;
        return mDir switch {
            (true, false, _, _) => GameElements.PlayerUp,
            (false, true, _, _) => GameElements.PlayerDown,
            (false, false, true, false) => GameElements.PlayerLeft,
            (false, false, false, true) => GameElements.PlayerRight,
            _ => GameElements.Player
        };
    }

    public event EventHandler PlayerDeathEvent;
    
    protected virtual void OnPlayerDeath() => PlayerDeathEvent?.Invoke();
    
}

public enum ShootingType {
    Normal = 3, Wheel = 1, ShotGun = 2
}