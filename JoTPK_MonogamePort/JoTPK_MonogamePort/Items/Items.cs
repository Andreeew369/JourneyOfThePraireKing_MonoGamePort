using System;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RectangleF = System.Drawing.RectangleF;

namespace JoTPK_MonogamePort.Items; 

public interface IItem {

    public const float DespawnTime = 10_000f; //miliseconds
    protected static RectangleF GlobalHitBox(float x, float y) => new(x, y, Consts.ObjectSize, Consts.ObjectSize);
    void PickUp(Player player, Level level);
    void Update(Player player, Level level, GameTime gt);
    public float Timer { get; set; }
}

public class Coin : GameObject, IItem {

    private readonly GameElements _coin;
    private readonly int _value;

    public float Timer { get; set; } = 0;

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
        Timer += gt.ElapsedGameTime.Milliseconds / 1000f;
        if (Timer >= IItem.DespawnTime) {
            level.RemoveObject(this, Level.GetIndexes(this));   
        }
    }
}

public enum CoinValue {
    Coin1 = 1, Coin5 = 5,
}

public class HealthPoint : GameObject, IItem {
    
    public float Timer { get; set; } = 0;
    
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
        Timer += gt.ElapsedGameTime.Milliseconds / 1000f;
        if (Timer >= IItem.DespawnTime) {
            level.RemoveObject(this, Level.GetIndexes(this));   
        }
    }
}