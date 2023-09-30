using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.Entities.Enemies;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Items;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace JoTPK_MonogamePort.World;

public class Level {

    public const int Width = 16;
    
    private static readonly object Lock = new();
    private (int x, int y) _location;
    private int _levelNumber;
    private bool _paused;
    private bool _prevPState;
    public bool GameOver { get; set; }
    
    private readonly GameObject[,] _field;
    private readonly List<GameObject> _items;
    private readonly EnemiesManager _enemiesManager;
    private readonly List<Wall> _spawners;
    private readonly List<Wall> _solidWalls;
    private readonly Counters _counters;
    private readonly LevelTimer _levelTimer;
    private Player _player;
    private PauseScreen? _gameOverScreen;
    private PauseScreen? _pauseScreen;
    private SpriteFont? _pixelFont;

    public Level(int levelNumber) {
        _levelNumber = levelNumber;
        _field = new GameObject[Width, Width];
        _items = new List<GameObject>();
        _location = (Consts.ObjectSize, Consts.ObjectSize);
        _enemiesManager = new EnemiesManager();
        _player = new Player(Width / 2 * Consts.ObjectSize, Width / 2 * Consts.ObjectSize, this, _enemiesManager);
        _spawners = new List<Wall>();
        _solidWalls = new List<Wall>();
        _levelTimer = new LevelTimer(Consts.LevelXOffset + 20, Consts.LevelYOffset + Consts.LevelWidth + 20);
        _counters = new Counters(_player, Width * Consts.ObjectSize, 0);
        GameOver = false;
        _paused = false;
        _prevPState = false;
    }

    public void PlaceItems() {
        //Coin c = new(4 * Consts.ObjectSize, 6 * Consts.ObjectSize, CoinValue.Coin1);
        //this.AddObject(c, Level.GetIndexes(c));
        //for (int i = 1; i < Width - 1; i++) {
        //    GameObject item = new SmokeBomb(i * Consts.ObjectSize, 4 * Consts.ObjectSize, this._player ,this._enemiesManager, this);
        //    this.AddObject(item, GetIndexes(item));
        //}
        GameObject powerUp = new TombStone(7 * Consts.ObjectSize, 4 * Consts.ObjectSize, _player);
        AddObject(powerUp, GetIndexes(powerUp));
        GameObject powerUp2 = new WagonWheel(9 * Consts.ObjectSize, 4 * Consts.ObjectSize, _player);
        AddObject(powerUp2, GetIndexes(powerUp2));
        //Coffee coffee = new(7 * Consts.ObjectSize, 4 * Consts.ObjectSize, this._player);
        //this.AddObject(coffee, LevelProperty.GetIndexes(coffee));
        //Coffee coffee2 = new(10 * Consts.ObjectSize, 4 * Consts.ObjectSize, this._player);
        //this.AddObject(coffee2, GetIndexes(coffee2));
    }

    public void Draw(SpriteBatch sb) {
        if (_gameOverScreen == null || _pauseScreen == null)
            throw new NullReferenceException($"{GetType().Name} wasn't initialized");

        if (GameOver) {
            _gameOverScreen.Draw(sb);
            return;
        }
        
        if (_paused) {
            _pauseScreen.Draw(sb);
            return;
        }

        if (_paused) return;
        
        TextureManager.DrawMap("Map" + _levelNumber, sb);
        _counters.Draw(sb, _player);
        _levelTimer.Draw(sb);
        
        _enemiesManager.Draw(sb);

        List<GameObject> copy;
        copy = new List<GameObject>(_items);

        foreach (GameObject item in copy) {
            item.Draw(sb);
        }
        _player.Draw(sb);

    }

    public void LoadContent(ContentManager cm, GraphicsDevice gd) {
        _pixelFont = cm.Load<SpriteFont>("PixelFont");
        _levelTimer.LoadContent(gd);
        _counters.LoadContent(_pixelFont);
        string[] gameOverText = { "New Game", "Exit" };
        string[] pauseText = { "Return to game", "Exit"};
        int middle = Width * Consts.ObjectSize / 2;
        Vector2 size = _pixelFont.MeasureString(gameOverText.GetLongestString()); //in pixels
        Vector2 pos = new(middle - (int)(size.X / 2), middle - (int)(size.Y / 2) - 5 - (int)size.Y);
        _gameOverScreen = new PauseScreen((int)pos.X, (int)pos.Y, this, new [] { NewGame, Exit }, gameOverText);
        _pauseScreen = new PauseScreen((int)pos.X, (int)pos.Y, this, new [] { game => _paused = false, Exit }, pauseText);
        _gameOverScreen.LoadContent(_pixelFont, 20f);
        _pauseScreen.LoadContent(_pixelFont, 20f);
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

        bool currentPState = Keyboard.GetState().IsKeyDown(Keys.P);
        // Check if the P key has just been pressed (not held down)
        if (currentPState && !_prevPState) {
            _paused = !_paused;
        }
        _prevPState = currentPState;

        if (_paused) return;
        
        _levelTimer.Update(_player,gt);
        _player.Update(this, _enemiesManager, gt);
        _enemiesManager.Update(_player, this, gt);
        List<GameObject> copy = new(_items);
        foreach (GameObject o in copy) {
            if (o is IItem item) {
                item.Update(_player, this, gt);
            }
        }
        //this.testEnemy.Move(this._player);
    }

    public void Generate(Game game) {
        string? map = null;
        try {
            map = File.ReadAllText($"{MainClass.BaseDirectory}/Resources/Levels/level{_levelNumber}Map.txt");
        }
        catch (IOException e) {
            Console.WriteLine($"Invalid level number => {_levelNumber}");
            Console.WriteLine(e);
            game.Exit();
        }

        if (map == null) return;
        
        string[] lines = map.Split(new[] { '\n' }, 2);
        string[] firstLine = lines[0].Split(' ');

        Dictionary<EnemyType, float> enemyTypeList = new();

        for (int i = 0; i < firstLine.Length; i += 2) {

            if (!Enum.TryParse(firstLine[i], out EnemyType enemyType))
                throw new ArgumentException("Cant parse " + firstLine[i]);
            if (!float.TryParse(firstLine[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out float probability))
                throw new ArgumentException("Cant parse " + firstLine[i + 1] + " to float");

            enemyTypeList.Add(enemyType, probability); //todo horde size
        }

        _enemiesManager.SetEnemyTypeList(enemyTypeList);

        lines = lines[1].Split('\n');

        for (int y = 0; y < lines.Length; y++) {
            string line = lines[y];
            string[] columns = line.Split(' ');

            for (int x = 0; x < columns.Length; x++) {

                if (!int.TryParse(columns[x], out int cell)) continue;

                GameObject gameObject = cell switch {
                    1 => new Wall(x * Consts.ObjectSize, y * Consts.ObjectSize, WallType.NotShootAble),
                    3 => new Wall(x * Consts.ObjectSize, y * Consts.ObjectSize, WallType.ShootAble),
                    2 => new Wall(x * Consts.ObjectSize, y * Consts.ObjectSize, WallType.Spawner),
                    4 => new Wall(x * Consts.ObjectSize, y * Consts.ObjectSize, WallType.Edge),
                    _ => new EmptyObject()
                };

                switch (gameObject) {
                    case Wall { Type: WallType.Spawner } spawner:
                        _spawners.Add(spawner);
                        break;
                    case Wall { Type: WallType.ShootAble } solidWall:
                        _solidWalls.Add(solidWall);
                        break;
                }

                _field[y, x] = gameObject;
            }
        }
    }

    public List<GameObject> GetSurroundings(int xIndex, int yIndex) {
        List<GameObject> surroundings = new();

        for (int y = yIndex - 1; y <= yIndex + 1; y++) {
            for (int x = xIndex - 1; x <= xIndex + 1; x++) {
                if (IsIndexOutOfBounds(x, y) || _field[y, x] is EmptyObject or null) continue;

                surroundings.Add(_field[y, x]);
            }
        }
        return surroundings;
    }

    public void AddObject(GameObject gameObject, (int x, int y) indexes) {
        AddObject(gameObject, indexes.x, indexes.y);
    }

    public void AddObject(GameObject gameObject, int indexX, int indexY) {
        if (IsIndexOutOfBounds(indexX, indexY) || IsOccupiedAt((indexX, indexY))) return;

        _field[indexY, indexX] = gameObject;
        _items.Add(gameObject);
    }

    public void ClearLevel() {
        for (int i = 0; i < Width; i++) {
            for (int j = 0; j < Width; j++) {
                if (_field[i, j] is IItem) {
                    _field[i, j] = new EmptyObject();
                }
            }
        }
        _items.Clear();
        _enemiesManager.CanSpawn = false;
        _enemiesManager.KillAll();
    }

    public void RemoveObject(GameObject gameObject, (int x, int y) indexes) {
        RemoveObject(gameObject, indexes.x, indexes.y);
    }

    public void RemoveObject(GameObject gameObject, int indexX, int indexY) {
        if (IsIndexOutOfBounds(indexX, indexY)) return;

        _field[indexY, indexX] = new EmptyObject();
        _items.Remove(gameObject);
    }
    public bool IsOccupiedAt((int x, int y) indexes) {
        return _field[indexes.y, indexes.x] is not EmptyObject;
    }

    public GameObject GetGameObject(int indexX, int indexY) {
        return _field[indexY, indexX];
    }

    public ImmutableList<Wall> GetSpawners => _spawners.ToImmutableList();
    public ImmutableList<Wall> GetSolidWalls => _solidWalls.ToImmutableList();

    public static bool IsIndexOutOfBounds(int xIndex, int yIndex) {
        return xIndex < 0 || yIndex < 0 || xIndex >= Width || yIndex >= Width;
    }

    public static bool IsPosOutOfBounds(float x, float y) {
        return x < 0 || y < 0 || x >= Width * Consts.ObjectSize || y >= Width * Consts.ObjectSize;
    }

    public static (int x, int y) GetIndexes(float xMiddle, float yMiddle) {
        return (
            (int)((xMiddle - Consts.LevelXOffset) / Consts.ObjectSize),
            (int)((yMiddle - Consts.LevelYOffset) / Consts.ObjectSize)
        );
    }


    public static (int x, int y) GetIndexes(GameObject o) {
        return GetIndexes(o.XMiddle, o.YMiddle);
    }

    private void NewGame(Game game) {
        GameOver = false;
        _enemiesManager.CanSpawn = true;
        _levelNumber = 0;
        _levelTimer.Reset();
        Generate(game);
        _player = new Player(Player.Middle, Player.Middle, this, _enemiesManager);
    }
    
    private void Exit(Game game) {
        game.Exit();
    }
}