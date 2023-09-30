using System;
using System.Collections.Generic;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Entities.Enemies;

public class WalkingEnemy : Enemy {

    public WalkingEnemy(int x, int y, Level level, EnemyType enemyType) : base(enemyType, x, y, level) {
        if (enemyType is not (EnemyType.Mushroom or EnemyType.Orc or EnemyType.Mummy))
            throw new ArgumentException(
                $"walking enemyType cant be type: {enemyType} \n" +
                $"Possible values {EnemyType.Orc}, {EnemyType.Mushroom} or {EnemyType.Mummy}"
            );
    }

    public override void Draw(SpriteBatch sb) {
        //g.FillRectangle(Brushes.BlueViolet, X, Y, Consts.ObjectSize, Consts.ObjectSize);
        TextureManager.DrawObject(ActualSprite, RoundedX, RoundedY, sb);

        //g.DrawRectangle(new Pen(Brushes.Crimson, 3), Rectangle.Round(HitBox));
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
}