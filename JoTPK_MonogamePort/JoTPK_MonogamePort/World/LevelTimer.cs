using System;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.World; 

public class LevelTimer {
    private const float Miliseconds = 20_000f;
    
    public static readonly int DefaultLength = TextureManager.GuiElementsCoordsP[(int)GuiElement.TimerGradient].Width;
    //90 sekund => 472px -> dlzka casovaca
    //x sekund => 1px
    //x = 90 / 472
    private static readonly float UpdateInterval = Miliseconds / DefaultLength; //how much miliseconds is one pixel
    private Rectangle _timerBounds;
    private float _timer;

    private readonly Vector2 _coords;
    public int Width => _timerBounds.Width;

    public LevelTimer(int x, int y) {
        _timerBounds = TextureManager.GuiElementsCoordsP[(int)GuiElement.TimerGradient];
        _coords = new Vector2(x, y);
        _timer = 0f;
    }
    
    /// <summary>
    /// Vykreslovanie casovaca
    /// </summary>
    /// <param name="sb">SpriteBatch</param>
    /// <param name="levelNumber">Cislo aktualneho levelu</param>
    public void Draw(SpriteBatch sb, int levelNumber) {
        if (levelNumber is 4 or 8 or 12)
            return;
        
        TextureManager.DrawGuiElement(GuiElement.TimerFrame, _coords.X, _coords.Y, sb);
        if (_timerBounds.Width > 0) {
            TextureManager.DrawGuiElement(GuiElement.TimerGradient, _coords.X + 8, _coords.Y + 8, sb, _timerBounds);
        }
    }
    
    /// <summary>
    /// Aktualizacia casovaca
    /// </summary>
    /// <param name="player">Hrac</param>
    /// <param name="gt">GrameTime v hre</param>
    /// <param name="enemiesManager">EnemieManager</param>
    /// <param name="levelNumber">Cislo Aktualneho levelu</param>
    public void Update(Player player, GameTime gt, EnemiesManager enemiesManager, int levelNumber) {
        if (levelNumber is 4 or 8 or 12 || player.IsDead) 
            return;
        
        _timer += gt.ElapsedGameTime.Milliseconds;
        if (_timer >= UpdateInterval && _timerBounds.Width > 0) {
            _timerBounds.Width--;
            _timer = 0;
        }

        if (_timerBounds.Width <= 0) {
            enemiesManager.CanSpawn = false;
        }
    }

    /// <summary>
    /// Resetnutie Casovaca
    /// </summary>
    public void Reset() {
        _timerBounds.Width = DefaultLength;
        _timer = 0;
    }

    /// <summary>
    /// Pridanie sekund k casovacu
    /// </summary>
    /// <param name="seconds">Pocet sekund</param>
    /// <param name="levelNumber">Cislo aktualneho levelu</param>
    public void AddSeconds(int seconds, int levelNumber) {
        //second => x px
        //Update interval => 1px
        if (levelNumber is 4 or 8 or 12) return;
        _timerBounds.Width += SecondsToPixels(seconds);
    }

    public static int SecondsToPixels(int seconds) {
        return (int)Math.Round(seconds / UpdateInterval);
    }
}