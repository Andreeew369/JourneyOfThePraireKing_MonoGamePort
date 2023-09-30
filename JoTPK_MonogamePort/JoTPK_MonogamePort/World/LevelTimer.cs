using System;
using System.Threading;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.World; 

public class LevelTimer {
    private const float Seconds = 20f;
    private static readonly int DefaultLength = Consts.LevelWidth - 2 * 20;
    //90 sekund => 472px -> dlzka casovaca
    //x sekund => 1px
    //x = 90 / 472
    private static readonly float UpdateInterval = Seconds / DefaultLength; 
    private Texture2D? _gradient;
    private Rectangle _timerBounds;
    private float _timer;

    public LevelTimer(int x, int y) {
        _timerBounds = new Rectangle(x, y, DefaultLength, 20);
        _timer = 0f;
    }

    public void LoadContent(GraphicsDevice gd) {
        _gradient = CreateGradientTexture(gd,new Color(1f,0,0), new Color(0, 1f, 0), _timerBounds.Width, _timerBounds.Height);
    }

    public void Draw(SpriteBatch sb) {
        if (_gradient == null) throw new NullReferenceException($"{GetType().Name} wasn't loaded");
        sb.Draw(
            _gradient,
            new Vector2(_timerBounds.X, _timerBounds.Y),
            _timerBounds,
            Color.White
        );
    }
    
    public void Update(Player player, GameTime gt) {
        _timer += gt.ElapsedGameTime.Milliseconds / 1000f;
        if (_timer >= UpdateInterval && _timerBounds.Width > 0 && !player.IsDead) {
            _timerBounds.Width--;
            _timer = 0;
        }
    }
    
    private static Texture2D CreateGradientTexture(GraphicsDevice gd, Color color1, Color color2, int width, int height) {
        Texture2D gradient = new(gd, width, height);

        Color[] data = new Color[width * height];
        float step = 1f / width;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                Color color = Color.Lerp(color1, color2, x * step);
                data[y * width + x] = color;
            }
        }
        gradient.SetData(data);
        return gradient;
    }

    public void Reset() {
        _timerBounds.Width = DefaultLength;
    }
}