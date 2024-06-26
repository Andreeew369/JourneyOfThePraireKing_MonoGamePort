﻿using System;
using System.Collections.Generic;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects.Entities.Enemies;

public class WalkingEnemy : Enemy {

    public WalkingEnemy(int x, int y, Level level, EnemyType enemyType) : base(x, y, enemyType, level) {
        if (enemyType is not (EnemyType.Mushroom or EnemyType.Orc or EnemyType.Mummy))
            throw new ArgumentException(
                $"walking enemyType cant be type: {enemyType} \n" +
                $"Possible values {EnemyType.Orc}, {EnemyType.Mushroom} or {EnemyType.Mummy}"
            );
    }

    public override void Draw(SpriteBatch sb) => TextureManager.DrawObject(ActualSprite, RoundedX, RoundedY, sb);

    public override bool CollisionDetection(float nextX, float nextY, Player player, float velocity, out float diffOut,
        List<Enemy> enemies) {
        if (PlayerCollision(nextX, nextY, player)) {
            diffOut = velocity;
            return true;
        }
        if (WallDetection(nextX, nextY, velocity, out diffOut)) return true;

        if (EnemiesDetection(nextX, nextY, velocity, out diffOut, enemies)) return true;

        diffOut = velocity;
        return false;
    }
}