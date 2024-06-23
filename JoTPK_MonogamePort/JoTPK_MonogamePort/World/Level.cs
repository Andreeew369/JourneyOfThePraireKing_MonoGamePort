using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;

using System.Collections.Generic;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Timers;

using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.GameObjects.Entities.Enemies;
using JoTPK_MonogamePort.GameObjects.Items;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace JoTPK_MonogamePort.World;

public delegate void GameEventHandler();

/// <summary>
/// Class representing a level in the game
/// </summary>
public class Level {
    
    public const int Width = 16;

    /// <summary>
    /// Indicates if the game is over
    /// </summary>
    public bool GameOver { get; set; }
    /// <summary>
    /// Indicates if it's possible to switch levels
    /// </summary>
    private bool CanSwitchLevel { get; set; }
    public int LevelNum { get; private set; }
    /// <summary>
    /// Gets all places that can spawn enemies
    /// </summary>
    public ImmutableList<Wall> Spawners => _spawners.ToImmutableList();

    private bool _paused;
    private bool _prevPState;
    private readonly GameObject[,] _field;
    private readonly List<GameObject> _items;
    private readonly EnemiesManager _enemiesManager;
    private readonly List<Wall> _spawners;
    private readonly Counters _counters;
    private readonly LevelTimer _levelTimer;
    private readonly PowerUpDisplay _pwuDisplay;
    private readonly Player _player;
    private readonly Trader _trader;
    private MenuScreen? _gameOverScreen;
    private MenuScreen? _pauseScreen;
    private SpriteFont? _pixelFont;
    private readonly Timer _controlsHintTimer;
    private readonly object _lock = new();
    private bool _showGameControls = true;

    public Level(int levelNumber = 0) {
        LevelNum = levelNumber;
        _paused = false;
        _prevPState = false;
        GameOver = false;
        CanSwitchLevel = false;
        
        _field = new GameObject[Width, Width];
        _items = new List<GameObject>();
        _spawners = new List<Wall>();
        _enemiesManager = new EnemiesManager();
        
        _trader = new Trader(8 * Consts.ObjectSize, -Consts.ObjectSize);
        _player = new Player(Width / 2 * Consts.ObjectSize, Width / 2 * Consts.ObjectSize, this, _trader, _enemiesManager);
        
        _levelTimer = new LevelTimer(Consts.LevelXOffset, Consts.LevelYOffset + Consts.LevelWidth + 2 * 6);
        _pwuDisplay = new PowerUpDisplay(Consts.LevelXOffset + Consts.LevelWidth + 10, Consts.LevelYOffset);
        _counters = new Counters(Consts.LevelXOffset + Consts.LevelWidth,  Consts.LevelYOffset + 4 + 2 * 24);

        _controlsHintTimer = new Timer(5000);
        _controlsHintTimer.Elapsed += (_, _) => {
            lock (_lock) {
                _showGameControls = false;
                _controlsHintTimer.Stop();
            }
        };

        _enemiesManager.LevelCompletionEvent += () => {
            CanSwitchLevel = true;
            if ((LevelNum + 1) % 2 == 0) {
                _trader.TraderAction = new MovingDown();
            }
        };
        _controlsHintTimer.Start();
    }

    /// <summary>
    /// Method only for testing purposes
    /// </summary>
    public void PlaceItems() {
        for (int i = 1; i < Width - 1; ++i) {
            // GameObject item = new SmokeBomb(i * Consts.ObjectSize, 4 * Consts.ObjectSize, _enemiesManager, this);
            GameObject item = new Nuke(i * Consts.ObjectSize, 4 * Consts.ObjectSize, _enemiesManager);
            AddObject(item, GetIndexes(item));
            // Console.WriteLine(GetIndexes(item));
        }
        GameObject powerUp = new TombStone(7 * Consts.ObjectSize, 4 * Consts.ObjectSize);
        AddObject(powerUp, GetIndexes(powerUp));
        // Console.WriteLine(GetIndexes(powerUp));
        GameObject powerUp2 = new WagonWheel(9 * Consts.ObjectSize, 4 * Consts.ObjectSize);
        AddObject(powerUp2, GetIndexes(powerUp2));
        //Coffee coffee = new(7 * Consts.ObjectSize, 4 * Consts.ObjectSize, this._player);
        //this.AddObject(coffee, LevelProperty.GetIndexes(coffee));
        //Coffee coffee2 = new(10 * Consts.ObjectSize, 4 * Consts.ObjectSize, this._player);
        //this.AddObject(coffee2, GetIndexes(coffee2));
    }

    public void LoadContent(ContentManager cm) {
        _pixelFont = cm.Load<SpriteFont>("PixelFont");
        _counters.LoadContent(_pixelFont);
        
        string[] gameOverText = ["New Game", "Exit"];
        string[] pauseText = ["Return to game", "Exit"];
        const int middle = Width * Consts.ObjectSize / 2;
        Vector2 size = _pixelFont.MeasureString(gameOverText.GetLongestString()); //in pixels
        Vector2 pos = new(middle - (int)(size.X / 2), middle - (int)(size.Y / 2) - 5 - (int)size.Y);
        
        _gameOverScreen = new MenuScreen((int)pos.X, (int)pos.Y, new[] { NewGame, Exit }, gameOverText);
        _pauseScreen = new MenuScreen(
            (int)pos.X,
            (int)pos.Y,
            new [] {
                _ => { _paused = false; if (_showGameControls) _controlsHintTimer.Start(); },
                Exit
            },
            pauseText
        );
        _trader.LoadContent(_pixelFont);
        _gameOverScreen.LoadContent(_pixelFont, 20f);
        _pauseScreen.LoadContent(_pixelFont, 20f);
    }
    
    public void Draw(SpriteBatch sb) {
        
        sb.Begin(
            SpriteSortMode.Deferred, 
            BlendState.AlphaBlend, 
            SamplerState.PointClamp, //nearest neighbor
            DepthStencilState.None, 
            RasterizerState.CullCounterClockwise, 
            null, 
            Matrix.Identity
        );
        
        if (_gameOverScreen == null || _pauseScreen == null)
            throw new NullReferenceException($"{GetType().Name} wasn't initialized");

        if (GameOver) {
            _gameOverScreen.Draw(sb);
            sb.End();
            return;
        }
        
        if (_paused) {
            _pauseScreen.Draw(sb);
            sb.End();
            return;
        }

        bool showGameControls;
        lock(_lock)  showGameControls = _showGameControls;
        
        if (showGameControls) {
            Rectangle textureBounds = TextureManager.GuiElementsCoordsP[(int)GuiElement.ControlsHint];
            Rectangle timerBounds = TextureManager.GuiElementsCoordsP[(int)GuiElement.TimerFrame];
            int x = Width * Consts.ObjectSize / 2 - textureBounds.Width / 2 - Consts.ObjectSize;
            int y = (int)_levelTimer.Coords.Y + timerBounds.Height + 20;
            TextureManager.DrawGuiElement(GuiElement.ControlsHint, x, y, sb);
        }

        if (CanSwitchLevel) {
            const int x = Width * Consts.ObjectSize + 10;
            int y = (int)_counters.Coords.Y + 75;
            TextureManager.DrawGuiElement(GuiElement.EKeyHint, x, y, sb);
        }
        
        TextureManager.DrawMap("Map" + LevelNum, sb);
        new List<GameObject>(_items).ForEach(item => item.Draw(sb));
        _counters.Draw(sb, _player);
        _enemiesManager.Draw(sb);
        
        _levelTimer.Draw(sb, LevelNum);
        _pwuDisplay.Draw(sb, _player);
        _trader.Draw(sb);
        _player.Draw(sb);
        
        sb.End();
    }
    
    public void Update(GameTime gt, Game game) {
        if (_gameOverScreen == null || _pauseScreen == null)
            throw new NullReferenceException($"{GetType().Name} wasn't initialized");
        
        if (GameOver) {
            _gameOverScreen.Update(game);
            return;
        }

        if (_paused) {
            _pauseScreen.Update(game);
            return;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.E) && CanSwitchLevel)
            SwitchLevel(game);
        bool currentPState = Keyboard.GetState().IsKeyDown(Keys.P);
        if (currentPState && !_prevPState) {
            bool showGameControls;
            lock (_lock) showGameControls = _showGameControls;
            _paused = !_paused;
            if (showGameControls) {
                if (_paused) _controlsHintTimer.Stop();
                else _controlsHintTimer.Start();
            }
        }
        _prevPState = currentPState;
        
        _player.Update(this, _enemiesManager, gt);
        _enemiesManager.Update(_player, this, gt);
        
        if (_enemiesManager.CanSpawn)
            _levelTimer.Update(_player, gt, _enemiesManager, LevelNum);
        
        
        new List<GameObject>(_items).ForEach(o => {
            if (o is IItem item) item.Update(_player, this, gt);
        });
        _trader.Update(_player , gt);
    }

    /// <summary>
    /// Generates a level based on the current level number and sets spawn probabilities of the enemies. If it can't find the map file,
    /// the game will be closed.
    /// </summary>
    /// <param name="game">Instance of current game</param>
    /// <exception cref="ArgumentException">When the first line which contains probabilities can't be parsed</exception>
    public void Generate(Game game) {
        string? map = null;
        try {
            map = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/../../../Resources/Levels/level{LevelNum}Map.txt");
        }
        catch (IOException e) {
            Console.WriteLine($"Invalid level number => {LevelNum}");
            Console.WriteLine(e);
            game.Exit();
        }

        if (map == null) return;
        
        string[] lines = map.Split(['\n'], 2);
        string[] firstLine = lines[0].Split(' ');

        Dictionary<EnemyType, double> enemyTypeList = new();

        for (int i = 0; i < firstLine.Length; i += 2) {

            if (!Enum.TryParse(firstLine[i], out EnemyType enemyType))
                throw new ArgumentException("Cant parse " + firstLine[i]);
            if (!double.TryParse(firstLine[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out double probability))
                throw new ArgumentException("Cant parse " + firstLine[i + 1] + " to float");

            enemyTypeList.Add(enemyType, probability);
        }

        _enemiesManager.SetEnemyTypeList(enemyTypeList);

        lines = lines[1].Split('\n');

        for (int y = 0; y < lines.Length; ++y) {
            string line = lines[y];
            string[] columns = line.Split(' ');

            for (int x = 0; x < columns.Length; ++x) {

                if (!int.TryParse(columns[x], out int cell)) continue;

                GameObject gameObject = cell switch {
                    1 => new Wall(x * Consts.ObjectSize, y * Consts.ObjectSize, WallType.NotShootable),
                    3 => new Wall(x * Consts.ObjectSize, y * Consts.ObjectSize, WallType.Shootable),
                    2 => new Wall(x * Consts.ObjectSize, y * Consts.ObjectSize, WallType.Spawner),
                    4 => new Wall(x * Consts.ObjectSize, y * Consts.ObjectSize, WallType.Edge),
                    _ => new EmptyObject()
                };

                if (gameObject is Wall { Type: WallType.Spawner } spawner)
                    _spawners.Add(spawner);

                _field[y, x] = gameObject;
            }
        }
    }

    /// <summary>
    /// Returns a list of surrounding objects of the object at the given indexes. Index indicates the index of a block in the level.
    /// </summary>
    /// <param name="xIndex">X Index of a block</param>
    /// <param name="yIndex">Y Index of a block</param>
    /// <returns>List of blocks that are in surroundings of block with give indexes</returns>
    public List<GameObject> GetSurroundings(int xIndex, int yIndex) {
        List<GameObject> surroundings = new();

        for (int y = yIndex - 1; y <= yIndex + 1; ++y) {
            for (int x = xIndex - 1; x <= xIndex + 1; ++x) {
                if (IsIndexOutOfBounds(x, y) || _field[y, x] is EmptyObject or null) continue;

                surroundings.Add(_field[y, x]);
            }
        }
        return surroundings;
    }

    /// <summary>
    /// Adds an object to the level at the given indexes
    /// </summary>
    /// <param name="gameObject">Object we want to add to the level</param>
    /// <param name="indexes">Tuple of X and Y coordinates of the object</param>
    public void AddObject(GameObject gameObject, (int x, int y) indexes) {
        AddObject(gameObject, indexes.x, indexes.y);
    }

    public void AddObject(GameObject gameObject, int indexX, int indexY) {
        if (IsIndexOutOfBounds(indexX, indexY) || IsOccupiedAt((indexX, indexY))) return;
        
        _field[indexY, indexX] = gameObject;
        _items.Add(gameObject);
    }

    /// <summary>
    /// Clears the level of all items and enemies
    /// </summary>
    public void ClearLevel() {
        for (int i = 0; i < Width; ++i) {
            for (int j = 0; j < Width; ++j) {
                if (_field[i, j] is IItem) {
                    _field[i, j] = new EmptyObject();
                }
            }
        }
        _items.Clear();
        _enemiesManager.CanSpawn = false;
        _enemiesManager.KillAll();
    }

    public void RemoveObject(GameObject gameObject, (int x, int y) indexes) => RemoveObject(gameObject, indexes.x, indexes.y);

    public void RemoveObject(GameObject gameObject, int indexX, int indexY) {
        if (IsIndexOutOfBounds(indexX, indexY)) return;
        
        _field[indexY, indexX] = new EmptyObject();
        _items.Remove(gameObject);
    }
    
    /// <summary>
    /// Returns true if the given indexes are occupied by an object
    /// </summary>
    /// <param name="indexes">Tuple of X and Y indexes of the object</param>
    /// <returns>True if the indexes are occupied, false otherwise</returns>
    public bool IsOccupiedAt((int x, int y) indexes) => _field[indexes.y, indexes.x] is not EmptyObject;

    /// <summary>
    /// Adds seconds to a timer. If the timer is full, it will add the remaining seconds to the next level timer
    /// </summary>
    public void AddPenaltySecondsToTimer() {
        const int seconds = 5;
        if (_levelTimer.Width + LevelTimer.SecondsToPixels(seconds) > LevelTimer.DefaultLength) {
            _levelTimer.AddSeconds(LevelTimer.DefaultLength - _levelTimer.Width, LevelNum);
        }
        else {
            _levelTimer.AddSeconds(seconds, LevelNum);
        }
    }

    /// <summary>
    /// Checks if the given indexes are out of bounds
    /// </summary>
    /// <param name="xIndex">X index</param>
    /// <param name="yIndex">Y index</param>
    /// <returns>True if the indexes are out of bounds, false otherwise</returns>
    public static bool IsIndexOutOfBounds(int xIndex, int yIndex) => xIndex < 0 || yIndex < 0 || xIndex >= Width || yIndex >= Width;

    /// <summary>
    /// Checks if the given position is out of bounds
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>True if the position is out of bounds, false otherwise</returns>
    public static bool IsPosOutOfBounds(float x, float y) => x < 0 || y < 0 || x >= Width * Consts.ObjectSize || y >= Width * Consts.ObjectSize;

    /// <summary>
    /// Returns the indexes of the given position
    /// </summary>
    /// <param name="xMiddle">X coordinates of the middle of the object</param>
    /// <param name="yMiddle">Y coordinates of the middle of the object</param>
    /// <returns>Tuple that contains X and Y indexes of the object</returns>
    public static (int x, int y) GetIndexes(float xMiddle, float yMiddle) =>
        (
            (int)((xMiddle - Consts.LevelXOffset) / Consts.ObjectSize),
            (int)((yMiddle - Consts.LevelYOffset) / Consts.ObjectSize)
        );

    /// <summary>
    /// Returns the indexes of the given object
    /// </summary>
    /// <param name="o">Object we want to get the indexes of</param>
    /// <returns>Tuple that contains X and Y indexes of the object</returns>
    public static (int x, int y) GetIndexes(GameObject o) => GetIndexes(o.XMiddle, o.YMiddle);


    private void SwitchLevel(Game game) {
        CanSwitchLevel = false;
        _enemiesManager.CanSpawn = true;
        _enemiesManager.Timer = 0f;
        _levelTimer.Reset();
        _trader.Hide();
        LevelNum = (LevelNum + (LevelNum + 1 is 4 or 8 or 12 ? 2 : 1)) % TextureManager.MapCount;
        _player.ResetPosition(LevelNum);

        Generate(game);
    }

    private void NewGame(Game game) {
        GameOver = false;
        LevelNum = 0;
        _levelTimer.Stop();
        _levelTimer.Reset();
        Generate(game);
        _player.Restart(this);
        _trader.OnGameOver();
        _enemiesManager.CanSpawn = true;
        _levelTimer.Start();
    }

    private static void Exit(Game game) => game.Exit();
}