using System;
using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.World; 

public class LevelTimer(int x, int y) {
    private const float Milliseconds = 20_000f;
    
    public static readonly int DefaultLength = TextureManager.GuiElementsCoordsP[(int)GuiElement.TimerGradient].Width;
    //90 sekund => 472px -> dlzka casovaca
    //x sekund => 1px
    //x = 90 / 472
    private static readonly float UpdateInterval = Milliseconds / DefaultLength; //represents how much milliseconds are one pixel
    private Rectangle _timerBounds = TextureManager.GuiElementsCoordsP[(int)GuiElement.TimerGradient];
    private float _timer;

    public readonly Vector2 Coords = new(x, y);
    private bool _canMove = true;
    public int Width => _timerBounds.Width;

    public void Draw(SpriteBatch sb, int levelNumber) {
        if (levelNumber is 4 or 8 or 12)
            return;
        
        TextureManager.DrawGuiElement(GuiElement.TimerFrame, Coords.X, Coords.Y, sb);
        if (_timerBounds.Width > 0) {
            TextureManager.DrawGuiElement(Coords.X + 8, Coords.Y + 8, sb, _timerBounds);
        }
    }
    
    public void Update(Player player, GameTime gt, EnemiesManager enemiesManager, int levelNumber) {
        if (levelNumber is 4 or 8 or 12 || player.IsDead) 
            return;
        
        _timer += gt.ElapsedGameTime.Milliseconds;
        if (_timer >= UpdateInterval && _timerBounds.Width > 0 && _canMove) {
            --_timerBounds.Width;
            _timer = 0;
        }

        if (_timerBounds.Width <= 0) {
            enemiesManager.CanSpawn = false;
            enemiesManager.Nuked = false;
        }
    }

    /// <summary>
    /// Timer reset
    /// </summary>
    public void Reset() {
        _timerBounds.Width = DefaultLength;
        _timer = 0;
    }
    
    
    public void Stop() => _canMove = false;

    public void Start() => _canMove = true;

    /// <summary>
    /// Add seconds to the timer
    /// </summary>
    /// <param name="seconds">Amount of seconds to add</param>
    /// <param name="levelNumber">Number of the current level</param>
    public void AddSeconds(int seconds, int levelNumber) {
        //second => x px
        //Update interval => 1px
        if (levelNumber is 4 or 8 or 12) return;
        _timerBounds.Width += SecondsToPixels(seconds);
    }

    /// <summary>
    /// Converts seconds to how many pixels the seconds are on the Level Timer
    /// </summary>
    /// <param name="seconds">Amount of seconds</param>
    /// <returns>Number of pixels</returns>
    public static int SecondsToPixels(int seconds) {
        return (int)Math.Round(seconds / UpdateInterval);
    }
}