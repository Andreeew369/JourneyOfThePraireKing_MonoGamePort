﻿using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

//.\editor.bat

namespace JoTPK_MonogamePort; 

public class Game1 : Game {
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
        

    private const int NewWidth = 700;
    private const int NewHeight = 700;

    private readonly Level _level;
        
    public Game1() {
        _graphics = new GraphicsDeviceManager(this);
            
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferHeight = NewHeight;
        _graphics.PreferredBackBufferWidth = NewWidth;
        _level = new Level(2);
    }

    protected override void Initialize() {
        _level.Generate( this);
        _level.PlaceItems();
        base.Initialize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        TextureManager.Inicialize(Content);
        _level.LoadContent(Content, GraphicsDevice);
    }

    protected override void Update(GameTime gameTime) {
        KeyboardState kst = Keyboard.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || kst.IsKeyDown(Keys.Escape))
            Exit();
        
        _level.Update(gameTime, this);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Black);
        

        _level.Draw(_spriteBatch);

        // float scale = 100f / customFont.MeasureString("Sample test").Y;
        // Vector2 pos = new(100, 100);
        // _spriteBatch.DrawString(
        //     customFont, "Test", pos, Color.Black,
        //     0f, Vector2.Zero, scale, SpriteEffects.None, 0f
        // );

        base.Draw(gameTime);
    }
        
    

}