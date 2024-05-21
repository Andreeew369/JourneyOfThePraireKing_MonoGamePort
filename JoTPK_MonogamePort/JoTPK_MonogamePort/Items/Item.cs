using System;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RectangleF = System.Drawing.RectangleF;

namespace JoTPK_MonogamePort.Items; 

/// <summary>
/// Interface for items
/// </summary>
public interface IItem {
    /// <summary>
    /// Time after which the item de-spawns
    /// </summary>
    public const float DespawnTime = 10_000f; //milliseconds
    
    /// <summary>
    /// Hitbox of every item
    /// </summary>
    /// <param name="x">X coordinate of the item</param>
    /// <param name="y">Y coordinate of the item</param>
    /// <returns>RectangleF representing the hitbox of the item</returns>
    protected static RectangleF GlobalHitBox(float x, float y) 
        => new(x, y, Consts.ObjectSize, Consts.ObjectSize);
    /// <summary>
    /// What should happen when player picks up the item
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
/// Class that represents a coin item
/// </summary>
public class Coin : GameObject, IItem {

    private readonly GameElements _coin;
    private readonly int _value;

    public float Timer { get; set; }

    public Coin(int x, int y, CoinValue value) : base(x, y) {
        _coin = value switch {
            CoinValue.Coin1 => GameElements.Coin1,
            CoinValue.Coin5 => GameElements.Coin5,
            _ => throw new NotImplementedException()
        };
        _value = (int)value;
    }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(_coin, RoundedX, RoundedY, sb);
    }

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

public class HealthPoint : GameObject, IItem {
    
    public float Timer { get; set; }
    
    public HealthPoint(int x, int y) : base(x, y) { }
    public HealthPoint(Vector2 pos) : base(pos.X, pos.Y) { }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(GameElements.HealthPoint, RoundedX, RoundedY, sb);
    }

    public void PickUp(Player player, Level level) {
        level.RemoveObject(this, Level.GetIndexes(this));
        player.Health++;
    }

    public void Update(Player player, Level level, GameTime gt) {
        Timer += gt.ElapsedGameTime.Milliseconds;
        if (Timer >= IItem.DespawnTime) {
            level.RemoveObject(this, Level.GetIndexes(this));   
        }
    }
}