using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JoTPK_MonogamePort.GameObjects.Entities.Enemies;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects; 

/// <summary>
/// Class that represents a game object, which can be drawn on the screen
/// </summary>
public abstract class GameObject(float x, float y) { 
    public float X { get; set; } = x;
    public float Y { get; set; } = y;
    public int RoundedX => (int)Math.Round(X);
    public int RoundedY => (int)Math.Round(Y);

    public float XMiddle => HitBox.X + HitBox.Width * 0.5f;
    public float YMiddle => HitBox.Y + HitBox.Height * 0.5f;
    public virtual RectangleF HitBox => new(X, Y, Consts.ObjectSize, Consts.ObjectSize);

    public abstract void Draw(SpriteBatch sb);
    
    /// <summary>
    /// Collision detection for game objects
    /// </summary>
    /// <param name="otherX">X coordinate of the top left corned of the other object</param>
    /// <param name="otherY">Y coordinate of the top left corned of the other object</param>
    /// <param name="otherWidth">Width of the other game object</param>
    /// <param name="otherHeight">Height of the other game object</param>
    /// <returns></returns>
    public virtual bool IsColliding(float otherX, float otherY, float otherWidth, float otherHeight) {
        return HitBox.X < otherX + otherWidth &&
               HitBox.X + HitBox.Width > otherX &&
               HitBox.Y < otherY + otherHeight &&
               HitBox.Y + HitBox.Height > otherY;
    }
}

/// <summary>
/// Class that represents Wall, which player can't walk through
/// </summary>
public class Wall(int x, int y, WallType type) : GameObject(x, y) {

    public WallType Type { get; private set; } = type;

    public override void Draw(SpriteBatch sb) { }

    public bool IsOccupied(IEnumerable<Enemy> enemies) =>
        enemies
            .Select(enemy => enemy.HitBox)
            .Any(hitBox => IsColliding(hitBox.X, hitBox.Y, hitBox.Width, hitBox.Height));
}

/// <summary>
/// Game Object that is empty, used as a block as a placeholder game object for empty blocks
/// </summary>
public class EmptyObject() : GameObject(0, 0) {
    public override RectangleF HitBox => new(0,0,1,1);
    public override void Draw(SpriteBatch sb) { }
    public override bool IsColliding(float otherX, float otherY, float otherWidth, float otherHeight) => false;
}

/// <summary>
/// Defines property of a Wall in game 
/// </summary>
public enum WallType {
    Shootable, // wall that bullet can't pass though
    NotShootable, // wall that bullet can pass though
    Spawner, // wall that spawns enemies, enemies can walk through it, but player can't
    Edge // edge of the map
}