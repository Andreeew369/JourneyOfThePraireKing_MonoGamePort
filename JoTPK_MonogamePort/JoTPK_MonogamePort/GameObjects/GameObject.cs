using System;
using System.Collections.Generic;
using System.Drawing;
using JoTPK_MonogamePort.Entities.Enemies;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects; 

public abstract class GameObject { 

    public float X { get; set; }
    public float Y { get; set; }
    public int RoundedX => (int)Math.Round(X);
    public int RoundedY => (int)Math.Round(Y);

    public float XMiddle => HitBox.X + HitBox.Width * 0.5f;
    public float YMiddle => HitBox.Y + HitBox.Height * 0.5f;

    protected GameObject(float x, float y) {
        X = x; Y = y;
    }

    public virtual RectangleF HitBox => new(X, Y, Consts.ObjectSize, Consts.ObjectSize);
    public abstract void Draw(SpriteBatch sb);
    public virtual bool IsColliding(int otherX, int otherY, int otherSize) => IsColliding(otherX, otherY, otherSize, otherSize);

    public virtual bool IsColliding(float otherX, float otherY, float otherWidth, float otherHeight) {
        return HitBox.X < otherX + otherWidth &&
               HitBox.X + HitBox.Width > otherX &&
               HitBox.Y < otherY + otherHeight &&
               HitBox.Y + HitBox.Height > otherY;
    }
}

public class Wall : GameObject {

    public WallType Type { get; private set; }

    public Wall(int x, int y, WallType type) :
        base(x, y) {
        Type = type;
    }

    public override RectangleF HitBox => new(
        X, Y, Consts.ObjectSize, Consts.ObjectSize
    );

    public (float x, float y) GetCoords => (X, Y);

    public override void Draw(SpriteBatch sb) { }

    public bool IsOcupied(List<Enemy> enemies) {
        foreach (Enemy enemy in enemies) {
            RectangleF hitBox = enemy.HitBox;
            if (IsColliding(hitBox.X, hitBox.Y, hitBox.Width, hitBox.Height)) {
                return true;
            }
        }
        return false;
    }
}

public class EmptyObject : GameObject {
    public EmptyObject() : base(0, 0) {}
    public override RectangleF HitBox => new(0,0,0,0);
    public override void Draw(SpriteBatch sb) { }
    public override bool IsColliding(int otherX, int otherY, int otherSize) => false;
    public override bool IsColliding(float otherX, float otherY, float otherWidth, float otherHeight) => false;
    public override bool Equals(object? obj) {
        if (obj == null) return false;
        return GetType() == obj.GetType();
    }
    public override int GetHashCode() => GetType().GetHashCode();
}

public enum WallType {
    ShootAble, NotShootAble, Spawner, Edge
}