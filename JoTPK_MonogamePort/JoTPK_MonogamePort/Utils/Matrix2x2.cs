using System;
using Microsoft.Xna.Framework;

namespace JoTPK_MonogamePort.Utils; 

/// <summary>
/// 2 by 2 Matrix class
/// </summary>
public class Matrix2X2(float x1, float x2, float y1, float y2) {
    
    private float X1 { get; } = x1;
    private float Y1 { get; } = y1;
    private float X2 { get; } = x2;
    private float Y2 { get; } = y2;

    /// <summary>
    /// Rotates a vector represented with tuple
    /// </summary>
    /// <param name="vec">2 element vector represented by tuple</param>
    /// <param name="rad">Degrees to rotate by</param>
    /// <returns>Tuple that represents rotated vector</returns>
    public static (float x, float y)RotateVector((float x, float y) vec, float rad) {
        Vector2 rotatedVec = RotateVector2(new Vector2(vec.x, vec.y), rad);
        return (rotatedVec.X, rotatedVec.Y);
    }

    /// <summary>
    /// Rotates a vector
    /// </summary>
    /// <param name="vec">Vector you want to rotate</param>
    /// <param name="rad">Degrees to rotate by</param>
    /// <returns>Tuple that represents rotated vector</returns>
    public static Vector2 RotateVector2(Vector2 vec, float rad) {
        return new Matrix2X2(
            MathF.Cos(rad), MathF.Sin(rad),
            -MathF.Sin(rad), MathF.Cos(rad)
        ) * vec;
    }
    
    public static Vector2 operator *(Matrix2X2 matrix, Vector2 vec) {
        return new Vector2(
            vec.X * matrix.X1 + vec.Y * matrix.X2,
            vec.X * matrix.Y1 + vec.Y * matrix.Y2
        );
    }
}