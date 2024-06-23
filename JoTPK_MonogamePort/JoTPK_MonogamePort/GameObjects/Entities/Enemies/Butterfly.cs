using System.Collections.Generic;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects.Entities.Enemies;

/// <summary>
/// Flying enemy, which can move though obstacles and follows the player
/// </summary>
public class Butterfly(int x, int y, Level level) : Enemy(x, y, EnemyType.Butterfly, level) {
    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(ActualSprite, RoundedX, RoundedY, sb);
    }

    
    public override bool CollisionDetection(float nextX, float nextY, Player player, float velocity, out float diffOut, List<Enemy> enemies) {
        diffOut = velocity;
        return PlayerCollision(nextX, nextY, player) || Level.IsPosOutOfBounds(nextX, nextY);
    }
}