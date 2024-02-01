using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Items;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Timer = System.Timers.Timer;

namespace JoTPK_MonogamePort.Utils;

public delegate void UpgradeFunction(Player player, Level level);

public static class Consts {
    
    public const double Tolerance = 0.001;

    public static readonly ImmutableDictionary<Keys, (int x, int y)> MovementDirections =
        ImmutableDictionary.CreateRange(
            new[] {
                KeyValuePair.Create(Keys.W, (0, -1)),
                KeyValuePair.Create(Keys.S, (0, 1)),
                KeyValuePair.Create(Keys.A, (-1, 0)),
                KeyValuePair.Create(Keys.D, (1, 0))
            }
        );

    public static readonly ImmutableDictionary<Keys, (int x, int y)> ShootingDirections =
        ImmutableDictionary.CreateRange(
            new[] {
                KeyValuePair.Create(Keys.Up, (0, -1)),
                KeyValuePair.Create(Keys.Down, (0, 1)),
                KeyValuePair.Create(Keys.Left, (-1, 0)),
                KeyValuePair.Create(Keys.Right, (1, 0))
            }
        );
    public static readonly float InverseSqrtOfTwo = 0.707f;
    public static readonly int ObjectSize = 32;
    public static readonly int ObjectSizeHalf = (int)(ObjectSize * 0.5f);
    public static readonly float PiOver180 = (float)(Math.PI / 180);
    public static readonly int LevelWidth = ObjectSize * Level.Width;

    public const int LevelXOffset = 0;
    public const int LevelYOffset = 0;

    public const int PowerUpCount = 8;

    public static readonly ImmutableArray<(float x, float y)> AllDirections = ImmutableArray.Create(
        (Bullet.Speed, 0), //right
        (InverseSqrtOfTwo * Bullet.Speed, -InverseSqrtOfTwo * Bullet.Speed), //up right
        (0, -Bullet.Speed), //up
        (-InverseSqrtOfTwo * Bullet.Speed, -InverseSqrtOfTwo * Bullet.Speed), //up left
        (-Bullet.Speed, 0), //left
        (-InverseSqrtOfTwo * Bullet.Speed, InverseSqrtOfTwo * Bullet.Speed), //down left
        (0, Bullet.Speed), //down
        (InverseSqrtOfTwo * Bullet.Speed, InverseSqrtOfTwo * Bullet.Speed) //down right
    );

    public static readonly ImmutableDictionary<(float x, float y), float> DirectionToAnglesMap = Functions.GetDirToRadMap().ToImmutableDictionary();
}


public static class Functions {
    public static int Signum(float value) {
        return value switch {
            > 0 => 1,
            < 0 => -1,
            _ => 0
        };
    }

    public static float DegToRadF(float deg) => (float)DegToRad(deg);

    public static double DegToRad(float deg) => deg * Consts.PiOver180;

    public static Dictionary<(float x, float y), float> GetDirToRadMap() {
        int degree = 0;
        Dictionary<(float x, float y), float> map = new();

        foreach ((float x, float y) dir in Consts.AllDirections) {
            map.Add(dir, (float)DegToRad(degree));
            degree += 45;
        }

        return map;
    }

    public static void Reset(this Timer timer) {
        timer.Stop();
        timer.Start();
    }

    public static bool IsEqualTo(this double a, double b) {
        return Math.Abs(a - b) < Consts.Tolerance;
    }

    public static bool IsEqualTo(this float a, float b) {
        return Math.Abs(a - b) < Consts.Tolerance;
    }

    /// <summary>
    /// If the next movement is to long (next move results in collision) this method will return
    /// the distance between those 2 objects. This method is used when you want to move the object
    /// but you want to alligne the object to the the other object.
    /// Use: nextX and nextY input only one at a time for example if you input nextX, put it into
    /// nextX parameter adn into nextY put coords of the middle of the 1st object and as difference
    /// input dx (x velocity). This will output x velocity
    /// </summary>
    /// <param name="nextX">next X coordinates (look method description)</param>
    /// <param name="nextY">next Y coordinates (look method description)</param>
    /// <param name="thisObj">actual object</param>
    /// <param name="otherObj">other object</param>
    /// <param name="velocity">Velocity of actual object</param>
    /// <returns></returns>

    public static float GetDistanceBetweenObjects(
        float nextX,
        float nextY,
        GameObject thisObj,
        GameObject otherObj,
        float velocity
    ) {
        int signum = Signum(velocity);
        if (nextY.IsEqualTo(thisObj.HitBox.Y))
            return signum * (Math.Abs(thisObj.XMiddle - otherObj.XMiddle) - 0.5f * (thisObj.HitBox.Width - otherObj.HitBox.Width));
        if (nextX.IsEqualTo(thisObj.HitBox.X))
            return signum * (Math.Abs(thisObj.YMiddle - otherObj.YMiddle) - 0.5f * (thisObj.HitBox.Height - otherObj.HitBox.Height));
        
        return velocity;
    }

    public static string GetLongestString(this string[] array) {
        string longest = "";
        foreach (string s in  array) {
            if (s.Length < longest.Length) {
                longest = s;
            }
        }

        return longest;
    }

    public static Texture2D CreateGradientTexture(GraphicsDevice gd, Color color1, Color color2, int width, int height) {
        Texture2D gradient = new(gd, width, height);

        Color[] data = new Color[width * height];
        float step = 1f / width;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                Color color = Color.Lerp(color1, color2, x * step);
                data[y * width + x] = color;
            }
        }
        gradient.SetData(data);
        return gradient;
    }

    public static UpgradeFunction GetFunc(this Upgrades powerUp) {
        return powerUp switch {
            Upgrades.Boots1 or Upgrades.Boots2 => (player, _) => {
                player.DefaultSpeed += 1;
            },
            Upgrades.Gun1 or Upgrades.Gun2 or Upgrades.Gun3 => (player, _) => {
                player.DefaultFireRate -= player.DefaultFireRate * 0.2f;
            },
            Upgrades.Ammo1 or Upgrades.Ammo2 or Upgrades.Ammo3 => (player, _) => {
                player.BulletDamage += 1;
            },
            Upgrades.SuperGun => (player, _) => {
                player.ShootingType = ShootingType.ShotGun;
                player.CanRemoveShootGun = false;
            },
            Upgrades.SheriffBadge => (player, level) => {
                new SherrifBadge(0, 0).PickUp(player, level);
            },
            Upgrades.HealthPoint => (player, level) => {
                new HealthPoint(0, 0).PickUp(player, level);
            },
            _ => throw new NotImplementedException()
        };
    }

    public static int GetPrice(this Upgrades powerUp) {
        return powerUp switch {
            Upgrades.Boots1 => 8,
            Upgrades.Boots2 or Upgrades.Gun2 => 20,
            Upgrades.HealthPoint or Upgrades.SheriffBadge or Upgrades.Gun1 => 10,
            Upgrades.Gun3 or Upgrades.Ammo2 => 30,
            Upgrades.SuperGun => 99,
            Upgrades.Ammo1 => 15,
            Upgrades.Ammo3 => 45,
            _ => throw new NotImplementedException()
        };
    }

    public static GameElements ToGameElement(this Upgrades powerUp) {
        if (Enum.TryParse(powerUp.ToString(), out GameElements gameElement))
            return gameElement;
        
        throw new NotImplementedException();
    }
}