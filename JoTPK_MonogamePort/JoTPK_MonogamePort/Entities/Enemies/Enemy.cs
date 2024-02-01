﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Items;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Entities.Enemies; 

public abstract class Enemy : GameObject {

    protected const int MinDistance = 3;

    protected const float AnimationInterval = 0.125f;

    public EnemyState State { get; set; }
    public EnemyType Type { get; set; }

    public int Health { get; set; }
    public float Speed { get; }

    public int XIndex { get; private set; }
    public int YIndex { get; private set; }
    public Level LevelProperty { get; }
    protected float Timer { get; set; }
    
    protected int SpriteNumber { get; set; }
    private readonly GameElements[] _sprites;
    protected ImmutableArray<GameElements> Sprites => _sprites.ToImmutableArray();
    protected GameElements ActualSprite { get; set; }
    protected ImmutableList<GameObject> Surroundings => _surroundings.ToImmutableList();
    protected void SetSurroundings(List<GameObject> surroundings) => _surroundings = surroundings;
    private List<GameObject> _surroundings;

    protected Enemy(EnemyType type, int x, int y, Level level) : base(x, y) {
        (XIndex, YIndex) = Level.GetIndexes(x, y);
        State = EnemyState.Alive;
        Type = type;
        (Health, Speed, _sprites) = Type.GetEnemyAtributes();
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

    public void Damage(int amount) {
        Health -= amount;
        //Console.WriteLine(HealthPoint);
        if (Health <= 0) {
            State = EnemyState.KilledByPlayer;
        }
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

    public void DropItem(Level level, Player player, EnemiesManager? enemiesManager) {
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
                    3 => new SherrifBadge(x, y),
                    4 => new WagonWheel(x, y),
                    5 => new Nuke(x, y, enemiesManager),
                    6 => new TombStone(x, y),
                    7 => new SmokeBomb(x, y, enemiesManager, level),
                    _ => throw new NotImplementedException()
                };
            }
            
            level.AddObject(item, XIndex, YIndex);
        }

    }

    public virtual void Update(Player player, List<Enemy> enemies,  GameTime gt) {
        Timer += gt.ElapsedGameTime.Milliseconds / 1000f;
        // Console.WriteLine(_timer);
        if (Timer >= AnimationInterval) {
            Timer = 0;
            SpriteNumber = (SpriteNumber + 1) % 2;
        }
        // Console.WriteLine(SpriteNumber);

        ActualSprite = _sprites[SpriteNumber];
        Move(player, enemies);
    }

    public virtual bool CanSeePlayer(Player player) {
        throw new NotImplementedException();
    }

    public virtual void Move(Player player, List<Enemy> enemies) {
        (float dx, float dy) = GetDirTo(player);
        float distanceXToPlayer = Math.Abs(X - player.X);
        float distanceYToPlayer = Math.Abs(Y - player.Y);

        if (distanceXToPlayer < MinDistance) {
            dx *= distanceXToPlayer;
        } else {
            dx *= Speed;
        }

        if (distanceYToPlayer < MinDistance) {
            dy *= distanceYToPlayer;
        } else {
            dy *= Speed;
        }

        //dx *= this.Speed;
        //dy *= this.Speed;


        //if (dx != 0 && dy != 0) {
        //    dx = (int)Math.Round(dx * Consts.InverseSqrtOfTwo);
        //    dy = (int)Math.Round(dy * Consts.InverseSqrtOfTwo);
        //}
        if (dx != 0 && dy != 0) {
            dx *= Consts.InverseSqrtOfTwo;
            dy *= Consts.InverseSqrtOfTwo;
        }
        //Console.WriteLine((dx, dy));

        //Console.WriteLine((dx, dy));

        float nextY = HitBox.Y + dy;
        float nextX = HitBox.X + dx;

        if (!CollisionDetection(nextX, HitBox.Y, player, dx, out dx, enemies)) {
            X += dx;
        }

        if (!CollisionDetection(HitBox.X, nextY, player, dy, out dy, enemies)) {
            Y += dy;
        }

        UpdateIndexes(out bool change);
        if (change) {
            _surroundings = LevelProperty.GetSurroundings(XIndex, YIndex);
        }
    }
    public abstract bool CollisionDetection(float nextX, float nextY, Player player, float diffIn,
        out float diffOut, List<Enemy> enemies);

    protected bool PlayerCollision(float nextX, float nextY, Player player) {
        RectangleF hitBox = HitBox;
        if (player.IsColliding(nextX, nextY, hitBox.Width, hitBox.Height)) {
            if (player.IsZombie) {
                State = EnemyState.KilledByPlayer;
                return true;
            }
            if (!player.IsDead) {
                player.KillPlayer();
            }
            return true;
        }
        return false;
    }

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

    protected bool EnemiesDetection(float nextX, float nextY, float diffIn, out float diffOut, List<Enemy> enemies) {
        foreach (Enemy otherEnemy in enemies) {
            if (!otherEnemy.IsColliding(nextX, nextY, HitBox.Width, HitBox.Height)
            || otherEnemy.Equals(this) || otherEnemy is Butterfly or Imp) continue;
            diffOut = Functions.GetDistanceBetweenObjects(nextX, nextY, this, otherEnemy, diffIn);
            return true;
        }

        diffOut = diffIn;
        return false;
    }

    public (float xDir, float yDir) GetDirTo((float x, float y) target) {
        return (Functions.Signum(target.x - X), Functions.Signum(target.y - Y));
    }

    public (float xDir, float yDir) GetDirTo(Player player) {
        return GetDirTo((player.X, player.Y));
    }
}

public enum EnemyState {
    Alive, Dead, KilledByPlayer
}

public enum EnemyType {
    Orc, SpikeBall, Ogre, Mushroom, Butterfly, Mummy, Imp, CowBoy, Fector 
}

public static class EnemyTypeMethods {
    public static (int health, float speed, GameElements[] sprites) GetEnemyAtributes(this EnemyType type) {
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
            EnemyType.Mushroom => (2, 4.25f, new[] {
                GameElements.Mushroom1, GameElements.Mushroom2, GameElements.MummyHit
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
            EnemyType.CowBoy => throw new NotImplementedException(),
            EnemyType.Fector => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };
    }

    public static Enemy GetEnemy(this EnemyType enemyType, int x, int y, Level level) {
        return enemyType switch {
            EnemyType.Orc or EnemyType.Mushroom or EnemyType.Mummy => new WalkingEnemy(x, y, level, enemyType), //todo walking enemyType
            EnemyType.SpikeBall => new SpikeBall(x, y, level),
            EnemyType.Ogre => new Ogre(x, y, level),
            EnemyType.Butterfly => new Butterfly(x, y, level),
            EnemyType.Imp => new Imp(x, y, level),
            EnemyType.CowBoy => throw new NotImplementedException(),
            EnemyType.Fector => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }
}

public class Imp : Enemy {
    public Imp(int x, int y, Level level) : base(EnemyType.Imp, x, y, level) {
        throw new NotImplementedException();
    }

    public override void Draw(SpriteBatch sb) {
        throw new NotImplementedException();
    }

    public override bool CollisionDetection(float nextX, float nextY, Player player, float diffIn, out float diffOut,
        List<Enemy> enemies) {
        throw new NotImplementedException();
    }
}