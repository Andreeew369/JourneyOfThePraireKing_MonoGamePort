using System;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace JoTPK_MonogamePort.World;

/// <summary>
/// Class that represents a game menu screen with options
/// </summary>
public class MenuScreen {
    private SpriteFont? _font;
    private int _optionNumber; // index in _options array
    private readonly Action<Game>[] _options;
    private readonly string[] _texts;
    private bool _prevUpState;
    private bool _prevDownState;
    private float _scale;
    private readonly Vector2 _pos;
    private int _fontHeight;

    /// <summary>
    /// Constructor for MenuScreen
    /// </summary>
    /// <param name="x">X position of the menu</param>
    /// <param name="y">Y position of the menu</param>
    /// <param name="actions">Array of actions that will be called when the option is selected</param>
    /// <param name="texts">Array of texts that will be displayed as options</param>
    /// <exception cref="ArgumentException">
    /// If the length of <paramref name="actions"/> has different length than <paramref name="texts"/>
    /// </exception>
    public MenuScreen(int x, int y, Action<Game>[] actions, string[] texts) {
        if (actions.Length != texts.Length)
            throw new ArgumentException($"{nameof(actions)}'s length has to be the same size as {nameof(texts)}");

        _pos = new Vector2(x, y);
        _options = actions;
        _texts = texts;
        _optionNumber = 0;
    }
    
    public void LoadContent(SpriteFont sf, float fontSize) {
        _font = sf;
        _scale = fontSize / _font.MeasureString("Sample Text").Y;
        _fontHeight = (int)(_scale * fontSize);
    }
    
    public void Update(Game game) {
        KeyboardState kst = Keyboard.GetState();
        if (kst.IsKeyDown(Keys.Down) && kst.IsKeyDown(Keys.Up)) return;
        
        bool currentState = kst.IsKeyDown(Keys.Up);
        if (currentState && !_prevUpState) {
            --_optionNumber;
            if (_optionNumber < 0) {
                _optionNumber = _options.Length - 1;
            }
        }
        _prevUpState = currentState;
        currentState = kst.IsKeyDown(Keys.Down);

        if (currentState && !_prevDownState) {
            ++_optionNumber;
            if (_optionNumber >= _options.Length) {
                _optionNumber = 0;
            }
        }

        _prevDownState = currentState;

        if (kst.IsKeyDown(Keys.Space) || kst.IsKeyDown(Keys.Enter)) {
            // Console.WriteLine(_optionNumber);
            _options[_optionNumber](game);
            _optionNumber = 0;
        }
    }

    public void Draw(SpriteBatch sb) {
        if (_font == null) throw new NullReferenceException($"{GetType().Name} wasn't loaded");
        
        for (int i = 0; i < _texts.Length; ++i) {
            Vector2 v = _pos + new Vector2(0, i * (_fontHeight + 5));    
            sb.DrawString(
                _font, _texts[i], v, Color.White, 0f, 
                Vector2.Zero, _scale, SpriteEffects.None, 0f
            );
            if (i == _optionNumber) {
                TextureManager.DrawGuiElement(GuiElement.Arrow, v.X - 20, v.Y, sb);
            }
        }
    }
}