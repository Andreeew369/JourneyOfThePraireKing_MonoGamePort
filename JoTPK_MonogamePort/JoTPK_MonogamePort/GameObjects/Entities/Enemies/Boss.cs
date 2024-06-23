using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects.Entities.Enemies;

/// <summary>
/// Interface for a boss enemy
/// </summary>
public interface IBoss {
    void Attack(Player player, Level level);
}

/// <summary>
/// Unfinished class for a boss enemy
/// </summary>
public class CowBoy : Enemy, IBoss {
    private static readonly ImmutableArray<ImmutableArray<CowBoyMove>> ActionSequences = ImmutableArray.Create(
        //Following
        ImmutableArray.Create( CowBoyMove.FollowingPlayer, CowBoyMove.ToCenter),
        //moving attack right stop
        ImmutableArray.Create(
            CowBoyMove.MovingLeft, CowBoyMove.MovingRight, CowBoyMove.MovingLeft,
            CowBoyMove.MovingRight, CowBoyMove.Idle, CowBoyMove.ToCenter
        ),
        //moving left with idle shooting
        ImmutableArray.Create(
            CowBoyMove.MovingRight, CowBoyMove.MovingLeft, CowBoyMove.MovingRight,
            CowBoyMove.MovingLeft, CowBoyMove.Idle, CowBoyMove.ToCenter
        )
    );

    private static readonly Random Random = new();

    private readonly int _spriteIndex;
    private readonly BulletManager _bulletManager;
    private readonly Level _level;
    private readonly Player _player;
    private bool IsMoving { get; set; }
    private double ShootingTime { get; set; }
    private double ActionTimer { get; set; }
    private int _actionNum;
    private int _actionSeqNum;
    
    
    public CowBoy(EnemyType type, int x, int y, Level level, Player player) : base(x, y, type, level) {
        ShootingTime = GetShootingTime;
        // new Vector2(player.X, player.Y);
        _spriteIndex = 0;
        _actionNum = 0;
        _actionSeqNum = 0;
        _bulletManager = new BulletManager(level, GameElements.BossBullets);
        _level = level;
        _player = player;
        ActionTimer = 0;
    }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(GetSprite(), RoundedX, RoundedY, sb);
    }

    public override void Update(Player player, List<Enemy> enemies, GameTime gt) {
        Timer += gt.ElapsedGameTime.Milliseconds;
        // Console.WriteLine(_timer);
        if (Timer >= AnimationInterval) {
            Timer = 0;
            SpriteNumber = (SpriteNumber + 1) % 2;
        }
        // Console.WriteLine(SpriteNumber);
        DoAMove(GetMove(), player, gt);
        _bulletManager.Update(_level,null ,this);
    }

    public override void Move(Player player, List<Enemy> enemies) {
        
    }

    public override bool CollisionDetection(float nextX, float nextY, Player player, float velocity, out float diffOut, List<Enemy> enemies) {
        throw new NotImplementedException();
    }

    public void Attack(Player player, Level level) {
        if (GetMove() is CowBoyMove.FollowingPlayer) {
            _bulletManager.AddBullets(this, level);
        }
    }
    

    private GameElements GetSprite() {
        return IsMoving ? Sprites[_spriteIndex] : Sprites[_spriteIndex + 2];
    }

    
    // todo redo this with interfaces
    private void DoAMove(CowBoyMove move, Player player, GameTime gt) {
        switch (move) {
            case CowBoyMove.FollowingPlayer: {
                if (!IsMoving) {
                    IsMoving = true;
                }
                
                //todo player following
                
                Attack(player, _level);
                ActionTimer += gt.ElapsedGameTime.Milliseconds;
                if (ActionTimer < ShootingTime) {
                    ActionTimer = 0;
                    IsMoving = false;
                    UpdatePlayerPos(player);
                    ShootingTime = GetShootingTime;
                }
            } break;
            case CowBoyMove.MovingLeft: {
                
            } break;
            case CowBoyMove.MovingRight: {
                
            } break;
            case CowBoyMove.Idle: {
                IsMoving = false;
            } break;
            case CowBoyMove.ToCenter: {
                
            } break; 
            default: throw new ArgumentOutOfRangeException(nameof(move), move, "Invalid argument");
        }
    }

    private CowBoyMove GetMove() {
        if (_actionNum >= ActionSequences[_actionSeqNum].Length) {
            _actionSeqNum = Random.Next(ActionSequences.Length);
            _actionNum = 0;
        }

        return ActionSequences[_actionSeqNum][_actionNum];
    }

    private static double GetShootingTime {
        get {
            const double max = 10;
            const double min = 7;
            return Random.NextDouble() * (max - min) + min;
        }
    }
    
    private void UpdatePlayerPos(Player player) {
        // new Vector2(player.X, player.Y);
    }

    private enum CowBoyMove {
        MovingLeft, MovingRight, FollowingPlayer, Idle, ToCenter
    }
}
