using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Items;
using JoTPK_MonogamePort.World;
using Timer = System.Timers.Timer;

namespace JoTPK_MonogamePort.Utils;

/// <summary>
/// Delegate for functions used in <see cref="Upgrade"/> class
/// </summary>
public delegate void UpgradePickUpFunction(Player player, Level level);

/// <summary>
/// Static class that contains constants used in the game
/// </summary>
public static class Consts {
    
    public const double Tolerance = 0.001;

    public const float InverseSqrtOfTwo = 0.707f;
    public const int ObjectSize = 32;
    // public static readonly int ObjectSizeHalf = (int)(ObjectSize * 0.5f);
    public const float PiOver180 = (float)(Math.PI / 180);
    public const int LevelWidth = ObjectSize * Level.Width;

    public const int LevelXOffset = 0;
    public const int LevelYOffset = 0;
    // public const float StartInterval = 3000f; // miliseconds
    
    
    public static readonly int PowerUpCount = AppDomain.CurrentDomain
        .GetAssemblies()
        .SelectMany(s => s.GetTypes())
        .Count(p => typeof(IPowerUp).IsAssignableFrom(p) && p.IsClass) - 1;
    
    /// <summary>
    /// Array of tuples which contains velocity for each direction
    /// </summary>
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

    private const int ShootGunDeg = 15;
    /// <summary>
    /// Array of tuples, that represent velocity for bullets when using <see cref="ShootingType.ShotGun"/>
    /// </summary>
    public static readonly ImmutableArray<(float dirX, float dirY)> ShootGunDir = ImmutableArray.Create(
        (Bullet.Speed, 0f),
        (Bullet.Speed, Bullet.Speed * (float)Math.Tan(Functions.DegToRadF(ShootGunDeg))),
        (Bullet.Speed, Bullet.Speed * (float)Math.Tan(Functions.DegToRadF(360-ShootGunDeg)))
    );

    /// <summary>
    /// Dictionary that maps a tuple of directions to radians
    /// </summary>
    public static readonly ImmutableDictionary<(float x, float y), float> DirectionToAnglesMap =
        Functions.GetDirToRadMap().ToImmutableDictionary();
}

/// <summary>
/// Static class that contains helper functions
/// </summary>
public static class Functions {
    
    /// <summary>
    /// Returns 1 if value is positive, -1 if negative and 0 if 0
    /// </summary>
    /// <param name="value">Value to check</param>
    /// <returns>1 if value is positive, -1 if negative and 0 if 0</returns>
    public static int Signum(float value) {
        return value switch {
            > 0 => 1,
            < 0 => -1,
            _ => 0
        };
    }

    public static float DegToRadF(float deg) => (float)DegToRad(deg);

    public static double DegToRad(double deg) => deg * Consts.PiOver180;
    
    /// <summary>
    /// Creates a map, which map direction values to radians
    /// </summary>
    /// <returns>Dictionary with direction as key and radians as value</returns>
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
    /// If the next movement is too long (next move results in collision) this method will return
    /// the distance between those 2 objects. This method is used when you want to move the object
    /// but you want to aligne the object to the the other object.
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

    public static string GetLongestString(this IEnumerable<string> strings) {
        string longest = "";
        foreach (string s in  strings) {
            if (s.Length < longest.Length) {
                longest = s;
            }
        }

        return longest;
    }

    // public static Texture2D CreateGradientTexture(GraphicsDevice gd, Color color1, Color color2, int width, int height) {
    //     Texture2D gradient = new(gd, width, height);
    //
    //     Color[] data = new Color[width * height];
    //     float step = 1f / width;
    //
    //     for (int y = 0; y < height; y++) {
    //         for (int x = 0; x < width; x++) {
    //             Color color = Color.Lerp(color1, color2, x * step);
    //             data[y * width + x] = color;
    //         }
    //     }
    //     gradient.SetData(data);
    //     return gradient;
    // }

    /// <summary>
    /// Returns a function for Upgrades enum, what the upgrade should do on pickup
    /// </summary>
    /// <param name="upgrade">Upgrade</param>
    /// <returns>Function that should be called when the upgrade is picked up</returns>
    /// <exception cref="NotImplementedException">If new Upgrade is implemented and function is not defined</exception>
    public static UpgradePickUpFunction GetFunc(this Upgrades upgrade) {
        return upgrade switch {
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
        return Enum.TryParse(powerUp.ToString(), out GameElements gameElement) 
            ? gameElement 
            : throw new ArgumentException(
            $"Upgrade can't be parsed to {nameof(GameElements)} class." +
                  $" Upgrade probably doesnt have the same name as value in {nameof(GameElements)}" +
                  $" or isn't defined in {nameof(GameElements)}"
                );
    }
}