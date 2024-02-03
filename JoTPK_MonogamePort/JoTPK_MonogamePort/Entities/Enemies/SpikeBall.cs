using System;
using System.Collections.Generic;
using System.Drawing;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timer = System.Timers.Timer;

namespace JoTPK_MonogamePort.Entities.Enemies;

public class SpikeBall : Enemy {

    private static readonly Random Rand = new();
    private readonly Timer _stuckTimer; //for situations when the spikeball gets stuck
    private readonly object _lock = new();
    private (float x, float y) _target;
    private bool _isSpikeBall;

    public override RectangleF HitBox => _isSpikeBall ? new RectangleF(X, Y, Consts.ObjectSize, Consts.ObjectSize) : base.HitBox;

    public SpikeBall(int x, int y, Level level) : base(EnemyType.SpikeBall, x, y, level) {
        _target = GetNewTarget(level);
        _isSpikeBall = false;
        _stuckTimer = new Timer(2000);
        _stuckTimer.Elapsed += (sender, e) => {
            lock (_lock) _target = GetNewTarget(level);
        };
        _stuckTimer.Start();
    }


    public override void Update(Player player, List<Enemy> enemies, GameTime gt) {
        if (!_isSpikeBall) {
            base.Update(player, enemies, gt);
        }
        else {
            if (player.IsColliding(X, Y, HitBox.Width, HitBox.Height)) {
                if (player.IsZombie) {
                    State = EnemyState.KilledByPlayer;
                }
                else if (!player.IsDead) {
                    player.KillPlayer();
                }
            }

            if (ActualSprite == GameElements.SpikeballBall4) return;
            Timer += gt.ElapsedGameTime.Milliseconds;
            if (Timer >= AnimationInterval) {
                Timer = 0;
                SpriteNumber++;
            }

            ActualSprite = Sprites[SpriteNumber];
        }
    }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(ActualSprite, RoundedX, RoundedY, sb);
    }

    public override void Move(Player player, List<Enemy> enemies) {

        float distanceXToPlayer = Math.Abs(X - _target.x);
        float distanceYToPlayer = Math.Abs(Y - _target.y);
        (float dx, float dy) = GetDirTo(_target);


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

        float nextX = HitBox.X + dx;
        float nextY = HitBox.Y + dy;

        if (dx != 0 && dy != 0) {
            dx *= Consts.InverseSqrtOfTwo;
            dy *= Consts.InverseSqrtOfTwo;
        }

        if (!CollisionDetection(nextX, HitBox.Y, player, dx, out dx, enemies)) {
            X += dx;
        }

        if (!CollisionDetection(HitBox.X, nextY, player, dy, out dy, enemies)) {
            Y += dy;
        }

        UpdateIndexes(out bool change);
        if (change) {
            SetSurroundings(LevelProperty.GetSurroundings(XIndex, YIndex));
            _stuckTimer.Reset();
        }

        if (X.IsEqualTo(_target.x) && Y.IsEqualTo(_target.y)) {
            TurnIntoSpikeBall();
        }
    }

    public override bool CollisionDetection(float nextX, float nextY, Player player, float diffIn, out float diffOut,
        List<Enemy> enemies) {
        if (PlayerCollision(nextX, nextY, player)) {
            diffOut = diffIn;
            return true;
        }
        if (WallDetection(nextX, nextY, diffIn, out diffOut)) return true;

        if (EnemiesDetection(nextX, nextY, diffIn, out diffOut, enemies)) return true;

        diffOut = diffIn;
        return false;
    }

    private void TurnIntoSpikeBall() {
        _stuckTimer.Stop();
        _stuckTimer.Dispose();
        _isSpikeBall = true;
        Health = 7;
        Timer = 0;
        SpriteNumber = 3;
    }

    private static (float x, float y) GetNewTarget(Level level) {
        const int min = 2;
        const int max = Level.Width - 2;
        (int x, int y) targetIndexes = ((int x, int y))(Rand.NextInt64(min, max),Rand.NextInt64(min, max));
        while (level.IsOccupiedAt(targetIndexes)) {
            targetIndexes = ((int x, int y))(Rand.NextInt64(min, max), Rand.NextInt64(min, max));
        }

        return (targetIndexes.x * Consts.ObjectSize, targetIndexes.y * Consts.ObjectSize);
    }
}