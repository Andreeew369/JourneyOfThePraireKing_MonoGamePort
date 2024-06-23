using System;
using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects.Items; 

/// <summary>
/// Interface for items
/// </summary>
public interface IItem {
    /// <summary>
    /// Time after which the item de-spawns
    /// </summary>
    public const float DespawnTime = 10_000f; //milliseconds

    /// <summary>
    /// What should happen when a player picks up the item
    /// </summary>
    /// <param name="player">Instance of player in current game</param>
    /// <param name="level">Instance of current level</param>
    void PickUp(Player player, Level level);
    
    /// <summary>
    /// Update for the item
    /// </summary>
    /// <param name="player"></param>
    /// <param name="level"></param>
    /// <param name="gt"></param>
    void Update(Player player, Level level, GameTime gt);
    public float Timer { get; set; }
}

/// <summary>
/// Class that represents a coin item. When picked up, player's money is increased by the value of the coin
/// </summary>
public class Coin(int x, int y, CoinValue value) : GameObject(x, y), IItem {

    private readonly GameElements _coin = value switch {
        CoinValue.Coin1 => GameElements.Coin1,
        CoinValue.Coin5 => GameElements.Coin5,
        _ => throw new NotImplementedException("Coin value is not implemented")
    };
    private readonly int _value = (int)value;

    public float Timer { get; set; }

    public override void Draw(SpriteBatch sb) => TextureManager.DrawObject(_coin, RoundedX, RoundedY, sb);

    public void PickUp(Player player, Level level) {
        level.RemoveObject(this, Level.GetIndexes(this));
        player.Money += _value;
        //Console.WriteLine(player.Money);
    }

    public void Update(Player player, Level level, GameTime gt) {
        Timer += gt.ElapsedGameTime.Milliseconds;
        if (Timer >= IItem.DespawnTime) {
            level.RemoveObject(this, Level.GetIndexes(this));   
        }
    }
}

/// <summary>
/// Enum for coin values. When adding new coin value, a value should be added to GameElements enum
/// </summary>
public enum CoinValue {
    Coin1 = 1, Coin5 = 5,
}

/// <summary>
/// Class that represents a health point. When picked up, player's health is increased by 1
/// </summary>
public class HealthPoint(int x, int y) : GameObject(x, y), IItem {
    
    public float Timer { get; set; }

    public override void Draw(SpriteBatch sb) => TextureManager.DrawObject(GameElements.HealthPoint, RoundedX, RoundedY, sb);

    public void PickUp(Player player, Level level) {
        level.RemoveObject(this, Level.GetIndexes(this));
        ++player.Health;
    }

    public void Update(Player player, Level level, GameTime gt) {
        Timer += gt.ElapsedGameTime.Milliseconds;
        if (Timer >= IItem.DespawnTime) {
            level.RemoveObject(this, Level.GetIndexes(this));   
        }
    }
}