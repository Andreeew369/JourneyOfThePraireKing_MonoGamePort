using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.GameObjects.Entities.Enemies;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Utils; 

/// <summary>
/// Class represents a manager for enemies which manages movement, updating, collision detection and spawning of
/// enemies in the current game
/// </summary>
public class EnemiesManager {

    /// <summary>
    /// Event that is called when the level is completed
    /// </summary>
    public event GameEventHandler? LevelCompletionEvent;

    private void OnLevelCompletion() => LevelCompletionEvent?.Invoke();

    private const float SpawnInterval = 2000; //2 seconds
    private const int MaxAmountOfEnemies = 25;
    /// <summary>
    /// Probabilities of wave sizes. All probabilities have to add up to 1.
    /// </summary>
    private static readonly float[] WaveSizeProbability = [0.18f, 0.25f, 0.3f, 0.1f, 0.07f, 0.05f, 0.03f, 0.015f, 0.005f];
    private static readonly Random Rand = new();
    private Dictionary<EnemyType, double> _enemyTypeList = new();
    private readonly List<Enemy> _enemies = [];
    private float _timer;
    
    public bool Nuked { get; set; }
    public float Timer {
        set => _timer = value;
    }

    /// <summary>
    /// Defines if enemies can spawn
    /// </summary>
    public bool CanSpawn { get; set; } = true;

    /// <summary>
    /// Defines if enemies can move
    /// </summary>
    public bool CanMove { get; set; } = true;

    /// <summary>
    /// Sets the list of probabilities for enemies that can spawn
    /// </summary>
    /// <param name="enemies">Dictionary of enemies and their spawn probabilities. All values have to add up to 1</param>
    public void SetEnemyTypeList(Dictionary<EnemyType, double> enemies) => _enemyTypeList = enemies;
    
    public void Draw(SpriteBatch sb) =>
        new List<Enemy>(_enemies.Where(e => e.State is not (EnemyState.Dead or EnemyState.KilledByPlayer)))
            .ForEach(e => e.Draw(sb));
    
    public void Update(Player player, Level level, GameTime gt) {
        CreateEnemies(level, gt);
        
        new List<Enemy>(_enemies).ForEach(enemy => {
            switch (enemy.State) {
                case EnemyState.KilledByPlayer: {
                    enemy.DropItem(level, this);
                    _enemies.Remove(enemy);
                    break;
                }
                case EnemyState.Dead: _enemies.Remove(enemy); break;
                case EnemyState.Alive: {
                    if (CanMove) {
                        _enemies.Single(e => e.Equals(enemy)).Update(player, _enemies, gt);
                    }
                    break;
                }
                default: throw new NotImplementedException(enemy.State + " is not implemented");
            }
        });
        
        if (_enemies.Count == 0 && !CanSpawn && !player.IsDead && !Nuked) {
            // level.CanSwitchLevel = true;
            OnLevelCompletion();
        } 
    }

    
    /// <summary>
    /// Damages all enemies that are currently colliding with the bullet
    /// </summary>
    /// <param name="bullet">Bullet that will be checked if it's colliding with one of the enemies</param>
    public void DamageEnemiesAt(Bullet bullet) {
        RectangleF hitBox = bullet.HitBox;
        new List<Enemy>(_enemies.Where(enemy => enemy.IsColliding(hitBox.X, hitBox.Y, hitBox.Width, hitBox.Height)))
            .ForEach(enemy => {
                enemy.DamageEnemy(bullet.Damage);
                bullet.Collided = true;
            });
    }
    
    /// <summary>
    /// Kills all enemies currently on the map, and stops spawning new ones for a while
    /// </summary>
    public void NukeEnemies() {
        KillAll();
        CanSpawn = false;
        Nuked = true;
        _timer = 0;
    }

    /// <summary>
    /// Kills all enemies currently on the mao. Enemies killed this way won't drop any items
    /// </summary>
    public void KillAll() => new List<Enemy>(_enemies).ForEach(e => e.State = EnemyState.Dead);

    /// <summary>
    /// Creates enemies on the map
    /// </summary>
    /// <param name="level">Instancia triedy Level</param>
    /// <param name="gt">GameTime v hre</param>
    private void CreateEnemies(Level level, GameTime gt) {
        if (!CanSpawn)
            return;

        _timer += gt.ElapsedGameTime.Milliseconds;
        if (!(_timer >= SpawnInterval)) 
            return;
        
        _timer = 0;
        if (_enemies.Count >= MaxAmountOfEnemies)
            return;

        // get spawn locations that aren't occupied 
        List<Wall> spawnLocations = level.Spawners.Where(e => !e.IsOccupied(_enemies)).ToList();
        if (spawnLocations.Count == 0)
            return;

        int enemyCountInWave = GetWaveSize();
        if (spawnLocations.Count < enemyCountInWave) {
            enemyCountInWave = spawnLocations.Count;
        }
            
        for (int i = 0; i < enemyCountInWave; ++i) {
            Wall spawner = spawnLocations[(int)Rand.NextInt64(spawnLocations.Count)];
            while (spawner.IsOccupied(_enemies)) {
                spawner = spawnLocations[(int)Rand.NextInt64(spawnLocations.Count)];
            }
            _enemies.Add(GetRandomEnemy(_enemyTypeList, spawner.RoundedX, spawner.RoundedY, level));
        }
    }
    

    /// <summary>
    /// Returns the random size of the wave depending on <see cref="WaveSizeProbability"/>
    /// </summary>
    /// <returns>Random wave size depending on the <see cref="WaveSizeProbability"/> array</returns>
    private static int GetWaveSize() {
        float rand = (float)Rand.NextDouble();
        float probability = 0;
        for (int i = 0; i < WaveSizeProbability.Length; ++i) {
            probability += WaveSizeProbability[i];
            if (!(rand <= probability)) continue;
            return i + 1;
        }
        return WaveSizeProbability.Length;
    }

    /// <summary>
    /// Generates a random enemy depending on the probability of enemies Dictionary
    /// </summary>
    /// <param name="probabilityOfEnemies">Dictionary containing a pair of float number which is spawn probability of
    /// an enemy and the <see cref="EnemyType"/> which should spawn. All af the float values have to add up to 1</param>
    /// <param name="x">X coordinate of the enemy</param>
    /// <param name="y">Y coordinate of the enemy</param>
    /// <param name="level">Instance of the current level</param>
    /// <returns>Instance of a random enemy</returns>
    /// <exception cref="ArgumentException">If the float values inside <paramref name="probabilityOfEnemies"/> don't add up to 1</exception>
    private static Enemy GetRandomEnemy(Dictionary<EnemyType, double> probabilityOfEnemies, int x, int y, Level level) {
        double probability = 0;
        double rand = (float)Rand.NextDouble();
        foreach (KeyValuePair<EnemyType, double> e in probabilityOfEnemies) {
            probability += e.Value;
            if (rand <= probability) {
                return e.Key.GetEnemy(x, y, level);
            }
        }

        throw new ArgumentException("invalid float values inside " + nameof(probabilityOfEnemies) + ".\n" +
                                    "Float values have to add up to 1");
    }
}
