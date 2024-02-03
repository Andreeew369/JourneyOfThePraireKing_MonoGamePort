using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Entities.Enemies;

public interface IBoss {
    void Attack(Player player, Level level);
}

public class CowBoy : Enemy, IBoss, ISender {

    private const int Speed = 4;

    private static readonly ImmutableArray<ImmutableArray<CBMove>> ActionSequences = ImmutableArray.Create(
        //Following
        ImmutableArray.Create(
            CBMove.FollowingPlayer, CBMove.ToCenter
        ),
        //moving attack right stop
        ImmutableArray.Create(
            CBMove.MovingLeft, CBMove.MovingRight, CBMove.MovingLeft,
            CBMove.MovingRight, CBMove.Idle, CBMove.ToCenter
        ),
        //moving left with idle shooting
        ImmutableArray.Create(
            CBMove.MovingRight, CBMove.MovingLeft, CBMove.MovingRight,
            CBMove.MovingLeft, CBMove.Idle, CBMove.ToCenter
        )
    );
    
    private static readonly ImmutableArray<GameElements> _sprites = ImmutableArray.Create(
        GameElements.CowboyIdle1, GameElements.CowboyIdle2, GameElements.CowboyShooting1,
        GameElements.CowboyShooting2, GameElements.CowboyHit
    );

    private static Random _random = new();

    private int _spriteIndex;
    private Vector2 _playerPos;
    private BulletManager _bulletManager;
    private Level _level;
    private bool IsMoving { get; set; } = false;
    private double ShootingTime { get; set; }
    private double ActionTimer { get; set; }
    private int _actionNum;
    private int _actionSeqNum;
    
    
    public CowBoy(EnemyType type, int x, int y, Level level, Player player) : base(type, x, y, level) {
        ShootingTime = GetShootingTime;
        _playerPos = new Vector2(player.X, player.Y);
        _spriteIndex = 0;
        _bulletManager = new BulletManager(level, GameElements.CowboyBullet);
        _level = level;
        _actionNum = 0;
        _actionSeqNum = 0;
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

    public override bool CollisionDetection(float nextX, float nextY, Player player, float diffIn, out float diffOut, List<Enemy> enemies) {
        throw new System.NotImplementedException();
    }

    public void Attack(Player player, Level level) {
        if (GetMove() is CBMove.FollowingPlayer) {
            _bulletManager.AddBullets(this, level);
        }
        else {
            
        }
    }
    

    private GameElements GetSprite() {
        return IsMoving ? _sprites[_spriteIndex] : _sprites[_spriteIndex + 2];
    }

    
    // todo redo this with interfaces
    private void DoAMove(CBMove move, Player player, GameTime gt) {
        switch (move) {
            case CBMove.FollowingPlayer: {
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
            case CBMove.MovingLeft: {
                
            } break;
            case CBMove.MovingRight: {
                
            } break;
            case CBMove.Idle: {
                IsMoving = false;
            } break;
            case CBMove.ToCenter: {
                
            } break; 
            default: throw new ArgumentOutOfRangeException(nameof(move), move, "Invalid argument");
        };
    }

    private CBMove GetMove() {
        if (_actionNum >= ActionSequences[_actionSeqNum].Length) {
            _actionSeqNum = _random.Next(ActionSequences.Length);
            _actionNum = 0;
        }

        return ActionSequences[_actionSeqNum][_actionNum];
    }

    private static double GetShootingTime {
        get {
            double max = 10;
            double min = 7;
            return _random.NextDouble() * (max - min) + min;
        }
    }

    
    private void UpdatePlayerPos(Player player) {
        _playerPos = new Vector2(player.X, player.Y);
    }

    private void test() {
        
    }

    private enum CBMove {
        MovingLeft, MovingRight, FollowingPlayer, Idle, ToCenter
    }
}
