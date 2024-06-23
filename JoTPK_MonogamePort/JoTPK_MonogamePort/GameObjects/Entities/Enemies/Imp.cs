using System.Collections.Generic;
using System.Drawing;
using JoTPK_MonogamePort.GameObjects.Items;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects.Entities.Enemies;

/// <summary>
/// Flying enemy that follows the player and kills him on contact. Ignores <see cref="TombStone"/> power up.
/// </summary>
public class Imp(int x, int y, Level level) : Enemy(x, y, EnemyType.Imp, level) {
    public override void Draw(SpriteBatch sb) => TextureManager.DrawObject(ActualSprite, RoundedX, RoundedY, sb);

    public override bool CollisionDetection(float nextX, float nextY, Player player, float velocity, out float diffOut, List<Enemy> enemies) {
        diffOut = velocity;
        return PlayerCollision(nextX, nextY, player);
    }

    protected override bool PlayerCollision(float nextX, float nextY, Player player) {
        RectangleF hitBox = HitBox;
        if (!player.IsColliding(nextX, nextY, hitBox.Width, hitBox.Height)) return false;
        
        if (!player.IsDead) {
            player.KillPlayer();
        }
        return true;
    }
}