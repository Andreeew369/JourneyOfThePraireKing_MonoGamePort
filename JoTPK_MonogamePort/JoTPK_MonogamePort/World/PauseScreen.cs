using System;
using System.Collections.Generic;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace JoTPK_MonogamePort.World; 

public class PauseScreen {

    private SpriteFont? _font;
    private int _optionNumber; //0 = new game, 1 = exit
    private Action<Game>[] _options;
    private string[] _texts;
    private bool _prevUpState = false;
    private bool _prevDownState = false;
    private float _scale;
    private Vector2 _pos;
    private int _fontHeight;

    public PauseScreen(int x, int y, Level level, Action<Game>[] actions, string[] texts) {
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
        bool currentState = kst.IsKeyDown(Keys.Up);
        
        if (currentState && !_prevUpState) {
            _optionNumber--;
            if (_optionNumber < 0) {
                _optionNumber = _options.Length - 1;
            }
        }
        _prevUpState = currentState;
        currentState = kst.IsKeyDown(Keys.Down);

        if (currentState && !_prevDownState) {
            _optionNumber++;
            if (_optionNumber >= _options.Length) {
                _optionNumber = 0;
            }
        }

        _prevDownState = currentState;

        if (kst.IsKeyDown(Keys.Space) || kst.IsKeyDown(Keys.Enter)) {
            Console.WriteLine(_optionNumber);
            _options[_optionNumber](game);
            _optionNumber = 0;
        }
    }

    public void Draw(SpriteBatch sb) {
        if (_font == null) throw new NullReferenceException($"{GetType().Name} wasn't loaded");
        
        // Vector2 size = _font.MeasureString("New Game");
        // Vector2 pos = new(Middle - (int)(size.X / 2), Middle - (int)(size.Y / 2) - 5 - (int)size.Y);
        // Vector2 arrowPos = new(pos.X - 20, _optionNumber == 0 ? pos.Y : Middle + 5);
        //
        // TextureManager.DrawObject(Drawable.Arrow, arrowPos.X, arrowPos.Y, sb);
        //
        // sb.DrawString(
        //     _font, "New Game", pos, Color.White, 0f,
        //     Vector2.Zero, _scale, SpriteEffects.None, 0f
        // );
        //
        // pos.Y = Middle + 5;
        //
        // sb.DrawString(
        //     _font, "Exit", pos, Color.White, 0f,
        //     Vector2.Zero, _scale, SpriteEffects.None, 0f
        // );
        
        for (int i = 0; i < _texts.Length; i++) {
            Vector2 v = _pos + new Vector2(0, i * (_fontHeight + 5));    
            sb.DrawString(
                _font, _texts[i], v, Color.White, 0f, 
                Vector2.Zero, _scale, SpriteEffects.None, 0f
            );
            if (i == _optionNumber) {
                TextureManager.DrawObject(Drawable.Arrow, v.X - 20, v.Y, sb);
            }
        }
    }
}