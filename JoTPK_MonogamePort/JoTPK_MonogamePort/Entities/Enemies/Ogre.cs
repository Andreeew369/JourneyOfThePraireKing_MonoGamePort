using System.Collections.Generic;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Entities.Enemies;

public class Ogre : Enemy {
    public Ogre(int x, int y, Level level) : base(EnemyType.Ogre, x, y, level) { }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(ActualSprite, RoundedX, RoundedY, sb);
    }

    public override bool CollisionDetection(float nextX, float nextY, Player player, float diffIn, out float diffOut, List<Enemy> enemies) {

        if (PlayerCollision(nextX, nextY, player)) {
            diffOut = diffIn;
            return true;
        }
        if (WallDetection(nextX, nextY, diffIn, out diffOut)) return true;

        foreach (Enemy otherEnemy in enemies) {
            bool isColliding = otherEnemy.IsColliding(nextX, nextY, HitBox.Width, HitBox.Height);
            if (otherEnemy.Equals(this) || !isColliding) continue;
            if (otherEnemy is SpikeBall) {
                otherEnemy.State = EnemyState.Dead;
                continue;
            }
            if (otherEnemy is Butterfly or Imp) continue;

            diffOut = Functions.GetDistanceBetweenObjects(nextX, nextY, this, otherEnemy, diffIn);
            return true;
        }
        
        diffOut = diffIn;
        return false;
    }
}