using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using JoTPK_MonogamePort.GameObjects.Items;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JoTPK_MonogamePort.GameObjects.Entities;

/// <summary>
/// Class that represents the player in the game
/// </summary>
public class Player : GameObject {

    /// <summary>
    /// Event that is invoked when the player respawns
    /// </summary>
    public event GameEventHandler? RespawnEvent;

    private void OnRespawn() => RespawnEvent?.Invoke();

    private const int DefaultHealth = 3;

    private const float AnimationInterval = 125f; //seconds

    // private const int RespawnTime = 3000;
    private const float StartingSpeed = 3.5f;
    private const float StartingFireRate = 0.3f;
    private const int PlayerHitBoxX = 4;
    private const int PlayerHitBoxY = 16;
    private const int HitBoxW = Consts.ObjectSize - 2 * PlayerHitBoxX;
    private const int HitBoxH = Consts.ObjectSize - PlayerHitBoxY - 1;
    private const int Middle = Level.Width / 2 * Consts.ObjectSize;

    /// <summary>
    /// Sprites for the player's legs
    /// </summary>
    private static readonly ImmutableArray<GameElements> LegSprites = [
        GameElements.PlayerLegs1, GameElements.PlayerLegs2, GameElements.PlayerLegs3, GameElements.PlayerLegs2
    ];

    /// <summary>
    /// Sprites for the player's zombie form
    /// </summary>
    private static readonly ImmutableArray<GameElements> ZombieSprites = [GameElements.PlayerZombie1, GameElements.PlayerZombie2];

    public int XIndex { get; private set; }
    public int YIndex { get; private set; }
    public float DefaultSpeed { get; set; } = StartingSpeed;
    public float Speed { get; set; }

    public float DefaultFireRate { get; set; } = StartingFireRate;
    public float FireRate { get; set; }

    public int Money { get; set; }
    public int Health { get; set; }

    public int BulletDamage { get; set; }
    public bool IsZombie { get; set; }
    public bool IsDead { get; private set; }
    public bool CanRemoveShootGun { get; set; } = true;

    /// <summary>
    /// Counter the time between the player's sprite change
    /// </summary>
    private float _timer;

    /// <summary>
    /// Index in a sprite array
    /// </summary>
    private int _sprite;

    public IPowerUp InventoryPowerUp { get; set; }

    private readonly HashSet<IPowerUp> _activePowerUps;
    private readonly SortedSet<ShootingType> _shootingTypes;

    public ShootingType ShootingType {
        get => _shootingTypes.First();
        set => _shootingTypes.Add(value);
    }

    /// Directions for movement
    private bool _mUp;

    private bool _mDown;
    private bool _mLeft;
    private bool _mRight;

    // Directions for shooting
    private bool _sUp;
    private bool _sDown;
    private bool _sLeft;
    private bool _sRight;

    private GameElements _body;
    private GameElements _legs;

    // private Timer _respawnTimer;
    private List<GameObject> _surroundings;

    private readonly Level _level;
    private readonly Trader _trader;
    private readonly EnemiesManager _enemiesManager;
    private readonly BulletManager _bulletManager;
    // private readonly object _lock = new();

    public Player(int x, int y, Level level, Trader trader, EnemiesManager em) : base(x, y) {
        (XIndex, YIndex) = Level.GetIndexes(x, y);
        Speed = DefaultSpeed;
        Money = 0;
        Health = DefaultHealth;
        FireRate = DefaultFireRate;
        _shootingTypes = new SortedSet<ShootingType>(new[] { ShootingType.Normal });
        ShootingType = ShootingType.Normal;
        InventoryPowerUp = EmptyPowerUp.Empty;
        BulletDamage = 1;
        _activePowerUps = new HashSet<IPowerUp>();
        _body = GameElements.Player;
        _legs = LegSprites[0];
        _level = level;
        _trader = trader;
        _enemiesManager = em;
        _surroundings = level.GetSurroundings(XIndex, YIndex);
        _bulletManager = new BulletManager(_level, GameElements.Bullet1);
        RespawnEvent += Respawn;
        // _respawnTimer = new Timer {
        //     AutoReset = false,
        //     Interval = RespawnTime
        // };
        // _respawnTimer.Elapsed += (_, _) => { lock (_lock) OnRespawn(); };
    }

    /// <summary>
    /// Restarts the player to the default state
    /// </summary>
    /// <param name="level">Instance of the current level</param>
    public void Restart(Level level) {
        (X, Y) = (Middle, Middle);
        (XIndex, YIndex) = Level.GetIndexes(Middle, Middle);
        _surroundings = level.GetSurroundings(XIndex, YIndex);
        DefaultSpeed = StartingSpeed;
        Speed = DefaultSpeed;
        Money = 0;
        Health = DefaultHealth;
        DefaultFireRate = StartingFireRate;
        FireRate = DefaultFireRate;
        _shootingTypes.Clear();
        _shootingTypes.Add(ShootingType.Normal);
        new HashSet<IPowerUp>(_activePowerUps).ToList().ForEach(p => p.Deactivate(this));
        InventoryPowerUp = EmptyPowerUp.Empty;
        BulletDamage = 1;
        IsDead = false;
    }

    /// <summary>
    /// Removes (if possible) a <see cref="ShootingType"/> from the player
    /// </summary>
    /// <param name="shootingType">Shooting type to be removed</param>
    public void RemoveShootingType(ShootingType shootingType) {
        if (shootingType == ShootingType.Normal || (_shootingTypes.Contains(shootingType) && !CanRemoveShootGun)) return;
        _shootingTypes.Remove(shootingType);
    }

    public override RectangleF HitBox =>
        new(
            X + PlayerHitBoxX,
            Y + PlayerHitBoxY,
            HitBoxW,
            HitBoxH + (IsMoving || IsShooting ? 2 : 0)
        );

    /// <summary>
    /// Property that represents the direction the player is shooting
    /// </summary>
    public (bool u, bool d, bool l, bool r) ShootingDirBool {
        get => (_sUp, _sDown, _sLeft, _sRight);
        private set => (_sUp, _sDown, _sLeft, _sRight) = value;
    }

    /// <summary>
    /// Property that represents the direction the player is moving
    /// </summary>
    private (bool u, bool d, bool l, bool r) MovementDirBool {
        get => (_mUp, _mDown, _mLeft, _mRight);
        set => (_mUp, _mDown, _mLeft, _mRight) = value;
    }

    private bool IsShooting => _sUp || _sDown || _sLeft || _sRight;
    private bool IsMoving => _mUp || _mDown || _mLeft || _mRight;

    public override void Draw(SpriteBatch sb) {
        if (IsDead) return;
        
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
        
        if (_bulletManager.BulletCount != 0) 
            _bulletManager.Update(level, enemiesManager, this);

        new HashSet<IPowerUp>(_activePowerUps)
            .ToList()
            .ForEach(i => i.Update(this, level, gt));

        KeyboardState kst = Keyboard.GetState();

        MovementDirBool = (kst.IsKeyDown(Keys.W), kst.IsKeyDown(Keys.S), kst.IsKeyDown(Keys.A), kst.IsKeyDown(Keys.D));
        ShootingDirBool = (kst.IsKeyDown(Keys.Up), kst.IsKeyDown(Keys.Down), kst.IsKeyDown(Keys.Left), kst.IsKeyDown(Keys.Right));

        if (InventoryPowerUp is not EmptyPowerUp && kst.IsKeyDown(Keys.Space)) 
            InventoryPowerUp.Activate(this, true);

        if (!IsZombie) 
            _body = GetPlayerSprite();

        if (IsMoving) {
            _timer += gt.ElapsedGameTime.Milliseconds;
            bool updateSprites = false;
            if (_timer >= AnimationInterval) {
                _timer = 0;
                ++_sprite;
                updateSprites = true;
            }

            Move(level);
            if (!updateSprites) return;

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

        if (IsShooting) Shoot();
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

        RectangleF hitBox = HitBox;
        float nextY = hitBox.Y + dy;
        float nextX = hitBox.X + dx;
        
        if (!CollisionDetection(nextX, hitBox.Y, dx, out dx)) {
            X += dx;
            //Console.WriteLine(dx);
        }

        if (!CollisionDetection(hitBox.X, nextY, dy, out dy)) {
            Y += dy;
            //Console.WriteLine(dy);
        }
        
        _trader.PickUpUpgrade(this, _level);
        UpdateIndexes(out bool didChange);

        if (didChange) _surroundings = level.GetSurroundings(XIndex, YIndex);
    }
    
    
    /// <summary>
    /// Kills the player and immediately respawns him
    /// </summary>
    public void KillPlayer() {
        --Health;
        _level.ClearLevel();
        _bulletManager.Clear();
        new HashSet<IPowerUp>(_activePowerUps)
            .ToList()
            .ForEach(p => p.Deactivate(this));
        InventoryPowerUp = EmptyPowerUp.Empty;
        
        if (Health <= 0) {
            _level.GameOver = true;
            return;
        }

        _level.AddPenaltySecondsToTimer();
        IsDead = true;
        
        if (!_level.GameOver) { 
            OnRespawn();
            // _respawnTimer.Start();
        } 
    }

    /// <summary>
    /// Adds a power up into player's inventory
    /// </summary>
    /// <param name="powerUp">Power up to add</param>
    public void AddActivePowerUp(IPowerUp powerUp) => _activePowerUps.Add(powerUp);

    /// <summary>
    /// Removes active power up from player's inventory
    /// </summary>
    /// <param name="powerUp">Power up to remove</param>
    public void RemoveActivePowerUp(IPowerUp powerUp) {
        if (!DoesPlayerHavePowerUp(powerUp, out IPowerUp? p) || p == null)
            return;
        _activePowerUps.Remove(p);
    }

    /// <summary>
    /// Updates the list of Game Objects in player's surroundings
    /// </summary>
    /// <param name="level"></param>
    public void UpdateSurroundings(Level level) {
        UpdateIndexes(out bool _);
        _surroundings = level.GetSurroundings(XIndex, YIndex);
    }

    /// <summary>
    /// Makes player shoot bullets
    /// </summary>
    public void Shoot() => _bulletManager.AddBullets(this, _level);


    /// <summary>
    /// Updates the Block indexes of the player in level
    /// </summary>
    /// <param name="didChange">True if the indexes changed</param>
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

    /// <summary>
    /// Collision detection for the player. When using <paramref name="nextX"/> or <paramref name="nextY"/> should be 0. Only one should have a
    /// non-zero value. This is used for detecting collision in one direction at a time and to know which direction to check. If one is 0 then it knows
    /// to check the other one. 
    /// </summary>
    /// <param name="nextX">X coordinate of the next position</param>
    /// <param name="nextY">Y coordinate of the next position. </param>
    /// <param name="velocity">Velocity of the player</param>
    /// <param name="distanceToMove">Distance the player can move in the next move</param>
    /// <returns>True, if the player is colliding with a Game Object that player can't move through</returns>
    public bool CollisionDetection(float nextX, float nextY, float velocity, out float distanceToMove) {
        RectangleF hitBox = HitBox;
        List<GameObject> copy = [.._surroundings];
        foreach (GameObject o in copy) {
            if (!o.IsColliding(nextX, nextY, hitBox.Width, hitBox.Height)) continue;

            if (o is IItem i) {
                i.PickUp(this, _level);
                _surroundings.Remove(o);
                continue;
            }

            distanceToMove = Functions.GetDistanceBetweenObjects(nextX, nextY, this, o, velocity);
            return true;
        }
        
        distanceToMove = velocity;
        return false;
    }
    
    public bool DoesPlayerHavePowerUp(IPowerUp powerUp, out IPowerUp? powerUpOut) {
        powerUpOut = _activePowerUps.FirstOrDefault(pu => pu.GetType() == powerUp.GetType());
        return powerUpOut != null;
    }

    /// <summary>
    /// Reset the player's position to the default position
    /// </summary>
    /// <param name="levelNumber">Current level number</param>
    public void ResetPosition(int levelNumber) {
        Y = levelNumber is 4 or 8 or 12 
            ? 2 * Consts.ObjectSize 
            : Middle;
        X = Middle;
    }

    /// <summary>
    /// Returns the sprite of the player based on the direction he is moving or shooting
    /// </summary>
    /// <returns>Game Element that represents current player's sprite</returns>
    private GameElements GetPlayerSprite() {
        return IsShooting
            ? ShootingDirBool switch {
                (_, _, true, false) => GameElements.PlayerLeft,
                (_, _, false, true) => GameElements.PlayerRight,
                (true, false, false, false) => GameElements.PlayerUp,
                (false, true, false, false) => GameElements.PlayerDown,
                _ => GameElements.Player
            }
            : MovementDirBool switch {
                (true, false, _, _) => GameElements.PlayerUp,
                (false, true, _, _) => GameElements.PlayerDown,
                (false, false, true, false) => GameElements.PlayerLeft,
                (false, false, false, true) => GameElements.PlayerRight,
                _ => GameElements.Player
            };
    }
    /// <summary>
    /// Respawns the player
    /// </summary>
    private void Respawn() {
        IsDead = false;
        ResetPosition(_level.LevelNum);
        _enemiesManager.CanSpawn = true;
    }
}

/// <summary>
/// Enum that represents how the player will be shooting
/// <see cref="Normal"/> - Player shoots in the direction he is facing
/// <see cref="Wheel"/> - player shoots in all 8 directions
/// <see cref="ShotGun"/> - player shoots in 3 directions front and at 15 degrees to the left and right
/// </summary>
public enum ShootingType {
    Normal = 3, Wheel = 1, ShotGun = 2
}
