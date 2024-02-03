using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.Entities.Enemies;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Utils; 

public class EnemiesManager {

    public event GameEventHandler? LevelCompletionEvent;
    
    protected virtual void OnLevelCompletion() { LevelCompletionEvent?.Invoke(); }
    
    public const float SpawnInterval = 2000; //2 seconds
    private const int MaxamountOfEnemies = 25;
    private static readonly Random Rand = new();
    private Dictionary<EnemyType, float> _enemyTypeList;
    private readonly List<Enemy> _enemies;
    private float _timer;
    
    public float Timer {
        get => _timer;
        set => _timer = value;
    }

    /// <summary>
    /// Urcuje ci sa mozu nepriatelia spawnovat
    /// </summary>
    public bool CanSpawn { get; set; }
    /// <summary>
    /// Urcuje ci sa nepriatelia mozu hybat
    /// </summary>
    public bool CanMove { get; set; }

    /// <summary>
    /// Konstruktor
    /// </summary>
    public EnemiesManager() {
        _enemyTypeList = new Dictionary<EnemyType, float>();
        _enemies = new List<Enemy>();
        CanSpawn = true;
        CanMove = true;
    }

    /// <summary>
    /// Nastanie zoznamu nepriatelov, ktory sa mozu spawnovat
    /// </summary>
    /// <param name="enemies">
    /// Dictionary, ktory obsahuje TypNepriatela a jeho pravdepodobnost na spawnutie.
    /// Sucet pravdepodobnosti musi davat 1 
    /// </param>
    public void SetEnemyTypeList(Dictionary<EnemyType, float> enemies) => _enemyTypeList = enemies;

    /// <summary>
    /// Vykreslovanie nepriatelov
    /// </summary>
    /// <param name="sb">SpriteBatch</param>
    public void Draw(SpriteBatch sb) {
        List<Enemy> enemiesCopy = new(_enemies.Where(e => e.State is not (EnemyState.Dead or EnemyState.KilledByPlayer)));
        foreach (Enemy enemy in enemiesCopy) {
            enemy.Draw(sb);
        }
    }

    /// <summary>
    /// Aktualizacia stavu EnemieManageru
    /// </summary>
    /// <param name="player">hrac</param>
    /// <param name="level">level</param>
    /// <param name="gt">GameTime v hre</param>
    /// <exception cref="NotImplementedException"></exception>
    public void Update(Player player, Level level, GameTime gt) {
        CreateEnemies(level, gt);

        List<Enemy> enemiesCopy = new(_enemies.Count);
        enemiesCopy.AddRange(_enemies);

        foreach (Enemy enemy in enemiesCopy) {
            switch (enemy.State) {
                case EnemyState.KilledByPlayer:
                    enemy.DropItem(level, player, this);
                    _enemies.Remove(enemy);
                    break;
                case EnemyState.Dead:
                    _enemies.Remove(enemy);
                    break;
                case EnemyState.Alive:
                    _enemies.Single(e => e.Equals(enemy)).Update(player, _enemies, gt);
                    break;
                default:
                    throw new NotImplementedException(enemy.State + " is not implemented");
            }
            
            if (_enemies.Count == 0 && !CanSpawn && !player.IsDead) {
                // level.CanSwitchLevel = true;
                OnLevelCompletion();
            } 
        }
    }

    /// <summary>
    /// Vytvorenie nepriatelov
    /// </summary>
    /// <param name="level">Instancia triedy Level</param>
    /// <param name="gt">GameTime v hre</param>
    private void CreateEnemies(Level level, GameTime gt) {
        if (!CanSpawn)
            return;

        _timer += gt.ElapsedGameTime.Milliseconds;
        if (_timer >= SpawnInterval) {
            _timer = 0;
            if (_enemies.Count >= MaxamountOfEnemies) return;

            // get spawn locations that are not occupied by enemy 
            List<Wall> spawnLocations = level.GetSpawners.Where(e => !e.IsOccupied(_enemies)).ToList();
            if (spawnLocations.Count == 0)
                return;

            int enemyCountInWave = GetWaveSize();
            if (spawnLocations.Count < enemyCountInWave) {
                enemyCountInWave = spawnLocations.Count;
            }
            
            for (int i = 0; i < enemyCountInWave; i++) {

                Wall spawner = spawnLocations[(int)Rand.NextInt64(spawnLocations.Count)];
                while (spawner.IsOccupied(_enemies)) {
                    spawner = spawnLocations[(int)Rand.NextInt64(spawnLocations.Count)];
                }
                (int x, int y) = ((int, int))spawner.GetCoords;
                _enemies.Add(GetRandomEnemy(_enemyTypeList, x, y, level));
            }
        }
    }

    
    /// <summary>
    /// Poskodenie nepriatela na mieste kde sa nachadza gulka
    /// </summary>
    /// <param name="bullet">Gulka</param>
    public void DamageEnemiesAt(Bullet bullet) {
        RectangleF hitBox = bullet.HitBox;
        List<Enemy> copy = new(_enemies);
        foreach (Enemy enemy in copy) {
            if (!enemy.IsColliding(hitBox.X, hitBox.Y, hitBox.Width, hitBox.Height)) continue;
            enemy.Damage(bullet.Damage);
            bullet.Collided = true;
        }
    }

    private static readonly float[] WaveSizeProbability = new[] {
        0.18f, 0.25f, 0.3f, 0.1f, 0.07f, 0.05f, 0.03f, 0.015f, 0.005f
    };

    private static int GetWaveSize() {
        float rand = (float)Rand.NextDouble();
        float probability = 0;
        for (int i = 0; i < WaveSizeProbability.Length; i++) {
            probability += WaveSizeProbability[i];
            if (!(rand <= probability)) continue;
            return i + 1;
        }
        return WaveSizeProbability.Length;
    }

    private static Enemy GetRandomEnemy(Dictionary<EnemyType, float> listOfEnemies, int x, int y, Level level) {
        float probability = 0;
        float rand = (float)Rand.NextDouble();
        foreach (KeyValuePair<EnemyType, float> e in listOfEnemies) {
            probability += e.Value;
            if (rand <= probability) {
                return e.Key.GetEnemy(x, y, level);
            }
        }

        throw new ArgumentException("invalid float values inside " + nameof(listOfEnemies) + ".\n" +
                                    "Float values have to add up to 1");
    }

    
    /// <summary>
    /// Zabitie vsetkych nepriatelov aktualne na ploche. 
    /// Vypne sa spawnovanie a casovac sa nastavi na 0
    /// </summary>
    public void NukeEnemies() {
        KillAll();
        CanSpawn = false;
        _timer = 0;
    }

    /// <summary>
    /// Zabitie vsetkych nepriatelov aktualne na ploche
    /// </summary>
    public void KillAll() {
        List<Enemy> copy = new(_enemies.Count);
        copy.AddRange(_enemies);
        foreach (Enemy enemy in copy) {
            enemy.State = EnemyState.Dead;
        }
    }
}