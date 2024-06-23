using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using JoTPK_MonogamePort.GameObjects.Items;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;

namespace JoTPK_MonogamePort.GameObjects.Entities.Enemies; 

/// <summary>
/// Class that represents an enemy in the game. It's a base class for all enemies in the game,
/// and all enemies in the game should inherit from this class
/// </summary>
public abstract class Enemy : GameObject {

    protected const int MinDistance = 3;

    protected const float AnimationInterval = 125f;

    public EnemyState State { get; set; }

    protected int Health { get; set; }
    protected float Speed { get; }

    protected int XIndex { get; private set; }
    protected int YIndex { get; private set; }
    protected Level LevelProperty { get; }
    protected float Timer { get; set; }
    
    /// <summary>
    /// Represents index in the sprite array
    /// </summary>
    protected int SpriteNumber { get; set; }
    
    private readonly GameElements[] _sprites;
    protected ImmutableArray<GameElements> Sprites => [.._sprites];
    protected GameElements ActualSprite { get; set; }
    // protected ImmutableList<GameObject> Surroundings => _surroundings.ToImmutableList();
    protected void SetSurroundings(List<GameObject> surroundings) => _surroundings = surroundings;
    private List<GameObject> _surroundings;

    protected Enemy(int x, int y, EnemyType type, Level level) : base(x, y) {
        (XIndex, YIndex) = Level.GetIndexes(x, y);
        State = EnemyState.Alive;
        (Health, Speed, _sprites) = type.GetEnemyAttributes();
        LevelProperty = level;
        ActualSprite = _sprites[0];
        _surroundings = level.GetSurroundings(XIndex, YIndex);
        Timer = 0;
    }

    public override RectangleF HitBox => new(
        X + 2,
        Y + Consts.ObjectSize / 2f,
        Consts.ObjectSize - 4, //2 pixels
        Consts.ObjectSize / 2f
    );

    public void DamageEnemy(int amount) {
        Health -= amount;
        if (Health <= 0) {
            State = EnemyState.KilledByPlayer;
        }
    }

    /// <summary>
    /// Updates index of an enemy
    /// <returns>True if the index changed, false if the index is the same</returns>
    /// </summary>
    public bool UpdateIndexes() {
        (int indexX, int indexY) = Level.GetIndexes(XMiddle, YMiddle);
        if (indexX == XIndex && indexY == YIndex) return false;

        XIndex = indexX;
        YIndex = indexY;
        return true;
    }

    /// <summary>
    /// Dropping items from enemies
    /// </summary>
    /// <param name="level">Instance of the current level</param>
    /// <param name="enemiesManager">Instance of manager of enemies in current game</param>
    /// <exception cref="NotImplementedException">
    /// If new PowerUp is implemented and this case is not handled
    /// </exception>
    public void DropItem(Level level, EnemiesManager enemiesManager) {
        Random rand = new();

        if (rand.NextDouble() < 0.1) {
            GameObject item;
            if (level.IsOccupiedAt((XIndex, YIndex))) return;

            (int x, int y) = (XIndex * Consts.ObjectSize, YIndex * Consts.ObjectSize);
            double chance = rand.NextDouble();
            if (chance <= 0.3) {
                //coin
                chance = rand.NextDouble();
                item = new Coin(x, y, chance <= 0.95 ? CoinValue.Coin1 : CoinValue.Coin5);
            } else if (chance <= 0.35) {
                // health point
                item = new HealthPoint(x, y);
            } else {
                //power up
                item = rand.Next(Consts.PowerUpCount) switch {
                    0 => new Coffee(x, y),
                    1 => new ShotGun(x, y),
                    2 => new MachineGun(x, y),
                    3 => new SheriffBadge(x, y),
                    4 => new WagonWheel(x, y),
                    5 => new Nuke(x, y, enemiesManager),
                    6 => new TombStone(x, y),
                    7 => new SmokeBomb(x, y, enemiesManager, level),
                    _ => throw new NotImplementedException("Power up is not implemented as a drop")
                };
            }
            
            level.AddObject(item, XIndex, YIndex);
        }

    }

    public virtual void Update(Player player, List<Enemy> enemies,  GameTime gt) {
        Timer += gt.ElapsedGameTime.Milliseconds;
        if (Timer >= AnimationInterval) {
            Timer = 0;
            SpriteNumber = (SpriteNumber + 1) % 2;  
        }

        ActualSprite = _sprites[SpriteNumber];
        Move(player, enemies);
    }

    // public virtual bool CanSeePlayer(Player player) {
    //     throw new NotImplementedException();
    // }

    public virtual void Move(Player player, List<Enemy> enemies) {
        (float dx, float dy) = GetDirTo(player);
        float distanceXToPlayer = Math.Abs(X - player.X);
        float distanceYToPlayer = Math.Abs(Y - player.Y);

        if (distanceXToPlayer < MinDistance) 
            dx *= distanceXToPlayer;
        else 
            dx *= Speed;

        if (distanceYToPlayer < MinDistance) 
            dy *= distanceYToPlayer;
        else 
            dy *= Speed;
        
        if (dx != 0 && dy != 0) {
            dx *= Consts.InverseSqrtOfTwo;
            dy *= Consts.InverseSqrtOfTwo;
        }
        //Console.WriteLine((dx, dy));
        //Console.WriteLine((dx, dy));

        float nextY = HitBox.Y + dy;
        float nextX = HitBox.X + dx;

        if (!CollisionDetection(nextX, HitBox.Y, player, dx, out dx, enemies)) 
            X += dx;

        if (!CollisionDetection(HitBox.X, nextY, player, dy, out dy, enemies)) 
            Y += dy;
        
        if (UpdateIndexes()) _surroundings = LevelProperty.GetSurroundings(XIndex, YIndex);
    }
    
    /// <summary>
    /// Detects collision with player, walls and other enemies
    /// </summary>
    /// <param name="nextX">X coordinate in the next frame</param>
    /// <param name="nextY">Y coordinate in the next frame</param>
    /// <param name="player">Instance of the player in the current game</param>
    /// <param name="velocity">Velocity of the enemy</param>
    /// <param name="diffOut">Distance between this and other Game Object</param>
    /// <param name="enemies">List of all enemies in the current game</param>
    /// <return>True, if the enemy is going to collide with another enemy</return>
    public abstract bool CollisionDetection(float nextX, float nextY, Player player, float velocity,
        out float diffOut, List<Enemy> enemies);


    protected (float xDir, float yDir) GetDirTo((float x, float y) target) 
        => (Functions.Signum(target.x - X), Functions.Signum(target.y - Y));

    protected (float xDir, float yDir) GetDirTo(Player player) => GetDirTo((player.X, player.Y));
    
    /// <summary>
    /// Collision detection with player
    /// </summary>
    /// <param name="nextX">X coordinate in the next frame</param>
    /// <param name="nextY">Y coordinate in the next frame</param>
    /// <param name="player">Instance of the player in the current game</param>
    /// <returns></returns>
    protected virtual bool PlayerCollision(float nextX, float nextY, Player player) {
        RectangleF hitBox = HitBox;
        if (!player.IsColliding(nextX, nextY, hitBox.Width, hitBox.Height)) return false;
        
        if (player.IsZombie) {
            State = EnemyState.KilledByPlayer;
            return true;
        }
        if (!player.IsDead) {
            player.KillPlayer();
        }
        return true;
    }

    /// <summary>
    /// Collision detection with walls
    /// </summary>
    /// <param name="nextX">X coordinate in the next frame</param>
    /// <param name="nextY">Y coordinate in the next frame</param>
    /// <param name="diffIn"></param>
    /// <param name="diffOut"></param>
    /// <returns></returns>
    protected bool WallDetection(float nextX, float nextY, float diffIn, out float diffOut) {
        foreach (GameObject o in _surroundings) {
            bool isColliding = o.IsColliding(nextX, nextY, HitBox.Width, HitBox.Height);
            bool isItemOrSpawner = o is IItem or Wall { Type: WallType.Spawner };
            if (!isColliding || isItemOrSpawner) continue;

            diffOut = Functions.GetDistanceBetweenObjects(nextX, nextY, this, o, diffIn);
            return true;
        }

        diffOut = diffIn;
        return false;
    }

    /// <summary>
    /// Collision detection with other enemies
    /// </summary>
    /// <param name="nextX">X coordinate in the next frame</param>
    /// <param name="nextY">Y coordinate in the next frame</param>
    /// <param name="velocity">Velocity of the enemy</param>
    /// <param name="diffOut">Distance between this and other enemy</param>
    /// <param name="enemies">List of all enemies in the current game</param>
    /// <returns>True, if the enemy is going to collide with another enemy</returns>
    protected bool EnemiesDetection(float nextX, float nextY, float velocity, out float diffOut, List<Enemy> enemies) {
        foreach (Enemy otherEnemy in enemies) {
            if (
                otherEnemy.Equals(this) || otherEnemy is Butterfly or Imp || 
                !otherEnemy.IsColliding(nextX, nextY, HitBox.Width, HitBox.Height)
            )
                continue;
            
            diffOut = Functions.GetDistanceBetweenObjects(nextX, nextY, this, otherEnemy, velocity);
            return true;
        }

        diffOut = velocity;
        return false;
    }
}

/// <summary>
/// Enum that represents the state of the enemy
/// </summary>
public enum EnemyState {
    Alive, Dead, KilledByPlayer
}

/// <summary>
/// Enum that represents the type of the enemy
/// </summary>
public enum EnemyType {
    Orc, SpikeBall, Ogre, Mushroom, Butterfly, Mummy, Imp, CowBoy, Fector 
}

/// <summary>
/// Static class that contains methods for <see cref="EnemyType"/> enum
/// </summary>
public static class EnemyTypeMethods {
    
    /// <summary>
    /// Function for getting tuple of attributes for the <see cref="EnemyType"/>
    /// </summary>
    /// <param name="type">Type of the enemy</param>
    /// <returns>Tuple of atrributes for the enemy, where first element is health,
    /// second is speed and third is array of sprites for that enemy</returns>
    /// <exception cref="NotImplementedException">If a new enemy is implemented and a return type is not defined for this case</exception>
    public static (int health, float speed, GameElements[] sprites) GetEnemyAttributes(this EnemyType type) {
        return type switch {
            EnemyType.Orc => (1, 1.75f, new[] {
                GameElements.Orc1 , GameElements.Orc2, GameElements.OrcHit
            }),
            EnemyType.SpikeBall => (2, 4.25f, new[] {
                GameElements.Spikeball1, GameElements.Spikeball2,
                GameElements.SpikeballHit, GameElements.SpikeballBall1,
                GameElements.SpikeballBall2, GameElements.SpikeballBall3,
                GameElements.SpikeballBall4 , GameElements.SpikeballBallHit
            }),
            EnemyType.Butterfly => (1, 1.75f, new[] {
                GameElements.Butterfly1, GameElements.Butterfly2, GameElements.ButterflyHit
            }),
            EnemyType.Mushroom => (2, 3.7f, new[] {
                GameElements.Mushroom1, GameElements.Mushroom2, GameElements.MushroomHit
            }),
            EnemyType.Ogre => (3, 0.75f, new[] {
                GameElements.Ogre1 , GameElements.Ogre2, GameElements.OgreHit
            }),
            EnemyType.Imp => (3, 3f, new[] {
                GameElements.Imp1 , GameElements.Imp2, GameElements.ImpHit
            }),
            EnemyType.Mummy => (6, 1.5f, new [] {
                GameElements.Mummy1 , GameElements.Mummy2, GameElements.MummyHit
            }),
            _ => throw new NotImplementedException("Enemy type is not implemented"),
        };
    }
    
    /// <summary>
    /// Factory method for creating enemy
    /// </summary>
    /// <param name="enemyType">Type of the enemy</param>
    /// <param name="x">X coordinate of the enemy where to create it</param>
    /// <param name="y">Y coordinate of the enemy where to create it</param>
    /// <param name="level">Instance of the current level</param>
    /// <returns>Instance of the enemy depending on the <paramref name="enemyType"/></returns>
    /// <exception cref="NotImplementedException">If a new enemy is implemented and a return type is not defined for this case</exception>
    public static Enemy GetEnemy(this EnemyType enemyType, int x, int y, Level level) {
        return enemyType switch {
            EnemyType.Orc or EnemyType.Mushroom or EnemyType.Mummy => new WalkingEnemy(x, y, level, enemyType),
            EnemyType.SpikeBall => new SpikeBall(x, y, level),
            EnemyType.Ogre => new Ogre(x, y, level),
            EnemyType.Butterfly => new Butterfly(x, y, level),
            EnemyType.Imp => new Imp(x, y, level),
            _ => throw new NotImplementedException("Enemy typeis not implemented")
        };
    }
}