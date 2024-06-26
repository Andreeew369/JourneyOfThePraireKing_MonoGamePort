﻿using System.Collections.Generic;
using System.Drawing;
using JoTPK_MonogamePort.GameObjects.Items;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects.Entities; 

public class Bullet : GameObject {

    public const float Speed = 7f;
    private const int HitBoxSize = 8;

    public int Damage { get; private set; }
    public bool Collided { get; set; }
    
    private int XIndex { get; set; }
    private int YIndex { get; set; }
    private readonly GameElements _type;

    private (float x, float y) _velocity;
    private List<GameObject> _surroundings;

    public override RectangleF HitBox => new(
        X + (Consts.ObjectSize - HitBoxSize) / 2f,
        Y + (Consts.ObjectSize - HitBoxSize) / 2f,
        HitBoxSize, HitBoxSize
    );

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(_type, (int)HitBox.X, (int)HitBox.Y, sb);
    }
    
    public Bullet(float x, float y, float xVelocity, float yVelocity, int damage, Level level, GameElements type) 
        : base(x, y) {
        _type = type;
        _velocity = (xVelocity, yVelocity);
        Collided = false;
        (XIndex, YIndex) = Level.GetIndexes(XMiddle, YMiddle);
        _surroundings = level.GetSurroundings(XIndex, YIndex);
        Damage = damage;
    }

    /// <summary>
    /// Resets the attributes of the bullet
    /// </summary>
    /// <param name="x">X coordinate of the bullet</param>
    /// <param name="y">Y coordinate of the bullet</param>
    /// <param name="xVelocity">X velocity of the bullet</param>
    /// <param name="yVelocity">Y velocity of the bullet</param>
    /// <param name="damage">Damage of the bullet</param>
    /// <param name="level">Instance of the current level</param>
    public void Reset(float x, float y, float xVelocity, float yVelocity, int damage, Level level) {
        X = x; Y = y;
        _velocity = (xVelocity, yVelocity);
        (XIndex, YIndex) = Level.GetIndexes(XMiddle, YMiddle);
        _surroundings = level.GetSurroundings(XIndex, YIndex);
        Damage = damage;
        Collided = false;
    }

    public void Move(Level level, out float dx, out float dy) {
        X += _velocity.x;
        Y += _velocity.y;
        dx = _velocity.x;
        dy = _velocity.y;

        UpdateIndexes(out bool change);
        if (change) {
            _surroundings = level.GetSurroundings(XIndex, YIndex);
        }

        //Console.WriteLine(_surroundings.Count);
    }

    public bool CollisionDetection(float x, float y, float dx, float dy, EnemiesManager? enemiesManager, GameObject sender) {
        foreach (GameObject o in _surroundings) {
            if (!o.IsColliding(x, y, HitBox.Width, HitBox.Height)) continue;
            if (o is IItem or Wall { Type: WallType.NotShootable or WallType.Spawner }) continue;

            return true;
        }
        
        if (sender is Player && enemiesManager is not null) {
            enemiesManager.DamageEnemiesAt(this);
        }
        
        return Level.IsPosOutOfBounds(x + dx, y + dy);
    }

    private void UpdateIndexes(out bool didChange) {
        (int indexX, int indexY) = Level.GetIndexes(XMiddle, YMiddle);
        
        if (indexX == XIndex && indexY == YIndex) {
            didChange = false;
            return;
        }

        XIndex = indexX;
        YIndex = indexY;
        didChange = true;
    }
}