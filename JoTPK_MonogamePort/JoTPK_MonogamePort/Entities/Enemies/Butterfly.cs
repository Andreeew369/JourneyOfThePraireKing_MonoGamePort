using System.Collections.Generic;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Entities.Enemies;

public class Butterfly : Enemy {
    public Butterfly(int x, int y, Level level) : base(EnemyType.Butterfly, x, y, level) {
        
    }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(ActualSprite, RoundedX, RoundedY, sb);
    }

    public override bool CollisionDetection(float nextX, float nextY, Player player, float diffIn, out float diffOut,
        List<Enemy> enemies) {
        diffOut = diffIn;
        return PlayerCollision(nextX, nextY, player) || Level.IsPosOutOfBounds(nextX, nextY);
    }
}