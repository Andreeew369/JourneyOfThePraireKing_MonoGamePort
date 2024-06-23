using System;
using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.World; 

/// <summary>
/// Class that represents counters for health and coins
/// </summary>
public class Counters(int x, int y) {
    
    public Vector2 Coords { get; } = new(Consts.LevelXOffset + x, Consts.LevelYOffset + y);
    private SpriteFont? _font;
    private float _scale;

    public void LoadContent(SpriteFont sf) {
        _font = sf;
        _scale = 28f / _font.MeasureString("Sample text").Y;
    }

    public void Draw(SpriteBatch sb, Player player) {
        if (_font == null) throw new NullReferenceException($"{GetType().Name} class wasn't loaded");
        
        //health counter
        TextureManager.DrawObject(GameElements.HealthPoint,Coords.X, Coords.Y, sb);
        float xOffset = Coords.X + Consts.ObjectSize + 6;
        float yOffset = Coords.Y;
        sb.DrawString(
            _font, $"x {player.Health}",
            new Vector2(xOffset, yOffset),
            Color.White, 0f, Vector2.Zero,
            _scale, SpriteEffects.None, 0f
        );
        
        //coin counter
        yOffset = Coords.Y + Consts.ObjectSize + 5;
        TextureManager.DrawObject(GameElements.Coin1, Coords.X + 3, yOffset, sb);
        sb.DrawString(
            _font, $"x {player.Money}",
            new Vector2(xOffset, yOffset),
            Color.White, 0f, Vector2.Zero, 
            _scale, SpriteEffects.None, 0f
        );
    }
}