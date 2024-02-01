using System;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.World; 

public class LevelTimer {

    public int Width => _timerBounds.Width;
    
    private const float Seconds = 20f;
    private static readonly int DefaultLength = TextureManager.GuiElementsCoordsP[(int)GuiElement.TimerGradient].Width;
    
    //90 sekund => 472px -> dlzka casovaca
    //x sekund => 1px
    //x = 90 / 472
    private static readonly float UpdateInterval = Seconds / DefaultLength; //how much seconds is one pixel
    // private Texture2D? _gradient;
    private Rectangle _timerBounds;
    private float _timer;

    private readonly Vector2 _coords;
    // private Effect _gradient;

    public LevelTimer(int x, int y) {
        Console.WriteLine(UpdateInterval);
        // _timerBounds = new Rectangle(x, y, DefaultLength, 2 * 4);
        _timerBounds = TextureManager.GuiElementsCoordsP[(int)GuiElement.TimerGradient];
        _coords = new Vector2(x, y);
        _timer = 0f;
    }

    public void LoadContent(GraphicsDevice gd, ContentManager cm) {
        // _gradient = Functions.CreateGradientTexture(gd,new Color(1f,0,0), new Color(0, 1f, 0), _timerBounds.Width, _timerBounds.Height);
        // _gradient = cm.Load<Effect>("GradientShader");
    }

    public void Draw(SpriteBatch sb, int levelNumber) {
        if (levelNumber is 4 or 8 or 12) return;
        // if (_gradient == null)
        //     throw new NullReferenceException($"{GetType().Name} wasn't loaded");
        
        TextureManager.DrawGuiElement(GuiElement.TimerFrame, _coords.X, _coords.Y, sb);
        if (_timerBounds.Width > 0) {
            TextureManager.DrawGuiElement(GuiElement.TimerGradient, _coords.X + 8, _coords.Y + 8, sb, _timerBounds);
        }
    }
    
    public void Update(Player player, GameTime gt, EnemiesManager enemiesManager, int levelNumber) {
        if (levelNumber is 4 or 8 or 12 || player.IsDead) return;
        
        _timer += gt.ElapsedGameTime.Milliseconds / 1000f;
        if (_timer >= UpdateInterval && _timerBounds.Width > 0) {
            _timerBounds.Width--;
            _timer = 0;
        }

        if (_timerBounds.Width <= 0) {
            enemiesManager.CanSpawn = false;
        }
    }

    public void Reset() {
        _timerBounds.Width = DefaultLength;
        _timer = 0;
    }

    public void AddSeconds(int seconds, int levelNumber) {
        //second => x px
        //Update interval => 1px
        if (levelNumber is 4 or 8 or 12) return;
        _timerBounds.Width = (int)Math.Round(seconds / UpdateInterval);
    }
}