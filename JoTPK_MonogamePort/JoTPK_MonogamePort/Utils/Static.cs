using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.GameObjects.Items;
using JoTPK_MonogamePort.World;
using Timer = System.Timers.Timer;

namespace JoTPK_MonogamePort.Utils;

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
    
    /// <summary>
    /// Kod vygenerovany Copilotom
    /// </summary>
    public static readonly int PowerUpCount = AppDomain.CurrentDomain
        .GetAssemblies()
        .SelectMany(s => s.GetTypes())
        .Count(p => typeof(IPowerUp).IsAssignableFrom(p) && p.IsClass) - 1;
    
    /// <summary>
    /// Array of tuples which contains velocity for each direction
    /// </summary>
    public static readonly ImmutableArray<(float x, float y)> AllDirections = [
        (Bullet.Speed, 0), //right
        (InverseSqrtOfTwo * Bullet.Speed, -InverseSqrtOfTwo * Bullet.Speed), //up right
        (0, -Bullet.Speed), //up
        (-InverseSqrtOfTwo * Bullet.Speed, -InverseSqrtOfTwo * Bullet.Speed), //up left
        (-Bullet.Speed, 0), //left
        (-InverseSqrtOfTwo * Bullet.Speed, InverseSqrtOfTwo * Bullet.Speed), //down left
        (0, Bullet.Speed), //down
        (InverseSqrtOfTwo * Bullet.Speed, InverseSqrtOfTwo * Bullet.Speed) //down right
    ];

    private const int ShootGunDeg = 15;
    /// <summary>
    /// Array of tuples, that represent velocity for bullets when using <see cref="ShootingType.ShotGun"/>
    /// </summary>
    public static readonly ImmutableArray<(float dirX, float dirY)> ShootGunDir = [
        (Bullet.Speed, 0f),
        (Bullet.Speed, Bullet.Speed * (float)Math.Tan(Functions.DegToRadF(ShootGunDeg))),
        (Bullet.Speed, Bullet.Speed * (float)Math.Tan(Functions.DegToRadF(360-ShootGunDeg)))
    ];

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
    /// Returns 1 if value is positive, -1 if negative and if 0 it returns 0.
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
    /// Creates a map, which maps direction values (as tuples) to angle values in radians
    /// </summary>
    /// <returns>Dictionary with a direction as key and radians as value</returns>
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

    public static bool IsEqualTo(this double a, double b) => Math.Abs(a - b) < Consts.Tolerance;

    public static bool IsEqualTo(this float a, float b) => Math.Abs(a - b) < Consts.Tolerance;

    /// <summary>
    /// If the next movement is too long (next move results in collision), this method will return
    /// the distance between those 2 objects. This method is used when you want to move the object,
    /// but you want to align the object to the other object.
    /// Use: nextX and nextY input only one at a time, for example if you input nextX, put it into
    /// nextX parameter and into nextY put coords of the middle of the 1st object and velocity is
    /// the speed of the object.
    /// This will output x velocity
    /// If the next movement is too long (next move results in collision), this method will return
    /// the distance between the two objects. This method is used when you want to move the object,
    /// but you want to align the object to the other object.
    /// Use: Input nextX and nextY one at a time. For example, if you input nextX, put it into
    /// the nextX parameter and put the coordinates of the middle of the first object into nextY.
    /// The velocity is the speed of the object. Example from this will return x distance to move.
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
}