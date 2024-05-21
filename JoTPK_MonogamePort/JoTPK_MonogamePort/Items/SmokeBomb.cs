using System;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Items; 

/// <summary>
/// Smoke bomb power up, which teleports the player to a random location on the map, and stuns all enemies
/// </summary>
public class SmokeBomb : GameObject, IItem, IPowerUp {

    private const int Interval = 4_000;
    private readonly EnemiesManager _enemiesManager;
    private readonly Random _random;
    private readonly Level _level;
    public bool IsInInventory { get; set; } = false;
    public float Timer { get; set; } = 0;

    public SmokeBomb(float x, float y, EnemiesManager enemiesManager, Level level) : base(x, y) {
        _enemiesManager = enemiesManager;
        _random = new Random();
        _level = level;
    }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(GameElements.SmokeBomb, RoundedX, RoundedY, sb);
    }

    public void PickUp(Player player, Level level) {
        IPowerUp.GlobalPickup(this, player, level);
    }

    public void Update(Player player, Level level, GameTime gt) {
        IPowerUp.GlobalUpdate(this, Interval, gt, level, player);
    }

    public void Activate(Player player, bool isInInventory) {

        (player.X, player.Y) = GetValidTeleportLocation();
        player.UpdateSurroundings(_level);

        _enemiesManager.CanMove = false;
        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        _enemiesManager.CanMove = true;
        IPowerUp.GlobalDeactivate(this, player);
    }

    /// <summary>
    /// Finds a valid position on the map that the player can teleport to
    /// </summary>
    /// <returns>A tuple containing the x and y coordinates of the valid teleport location</returns>
    private (float x, float y) GetValidTeleportLocation() {
        (int x, int y) target = (_random.Next(1, Level.Width - 1), _random.Next(1, Level.Width - 1));

        while (_level.IsOccupiedAt(target)) {
            target = (_random.Next(1, Level.Width - 1), _random.Next(1, Level.Width - 1));
        }

        return (target.x * Consts.ObjectSize, target.y * Consts.ObjectSize);
    }
}