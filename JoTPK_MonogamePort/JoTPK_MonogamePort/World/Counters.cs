using System;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.World; 

public class Counters {
    
    private readonly Vector2 _pos;
    private SpriteFont? _font;
    private float _scale;

    public Counters(Player player, int x, int y) {
        _pos = new Vector2(Consts.LevelXOffset + x, Consts.LevelYOffset + y);
    }

    public void LoadContent(SpriteFont sf) {
        _font = sf;
        _scale = 30f / _font.MeasureString("Sample text").Y;
    }

    public void Draw(SpriteBatch sb, Player player) {
        if (_font == null) throw new NullReferenceException($"{GetType().Name} class wasn't loaded");
        
        _scale = 28f / _font.MeasureString("Sample text").Y;
        
        //health counter
        TextureManager.DrawObject(GameElements.HealthPoint,_pos.X, _pos.Y, sb);
        float xOffset = _pos.X + Consts.ObjectSize + 6;
        float yOffset = _pos.Y;
        sb.DrawString(
            _font, $"x {player.Health}",
            new Vector2(xOffset, yOffset),
            Color.White, 0f, Vector2.Zero,
            _scale, SpriteEffects.None, 0f
        );
        
        //coin counter
        yOffset = _pos.Y + Consts.ObjectSize + 5;
        TextureManager.DrawObject(GameElements.Coin1, _pos.X + 3, yOffset, sb);
        sb.DrawString(
            _font, $"x {player.Money}",
            new Vector2(xOffset, yOffset),
            Color.White, 0f, Vector2.Zero, 
            _scale, SpriteEffects.None, 0f
        );
    }
}