using System.Collections.Generic;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Entities.Enemies;

/// <summary>
/// Very slow walking enemy that kills the player on contact and also kills <see cref="SpikeBall"/> enmies on contact.
/// </summary>
public class Ogre : Enemy {
    public Ogre(int x, int y, Level level) : base(EnemyType.Ogre, x, y, level) { }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(ActualSprite, RoundedX, RoundedY, sb);
    }

    public override bool CollisionDetection(float nextX, float nextY, Player player, float velocity, out float diffOut, List<Enemy> enemies) {

        if (PlayerCollision(nextX, nextY, player)) {
            diffOut = velocity;
            return true;
        }
        if (WallDetection(nextX, nextY, velocity, out diffOut)) return true;

        foreach (Enemy otherEnemy in enemies) {
            bool isColliding = otherEnemy.IsColliding(nextX, nextY, HitBox.Width, HitBox.Height);
            if (otherEnemy.Equals(this) || !isColliding) continue;
            if (otherEnemy is SpikeBall) {
                otherEnemy.State = EnemyState.Dead;
                continue;
            }
            if (otherEnemy is Butterfly or Imp) continue;

            diffOut = Functions.GetDistanceBetweenObjects(nextX, nextY, this, otherEnemy, velocity);
            return true;
        }
        
        diffOut = velocity;
        return false;
    }
}