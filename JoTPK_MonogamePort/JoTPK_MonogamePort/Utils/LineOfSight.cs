using System;
using JoTPK_MonogamePort.GameObjects;
using Microsoft.Xna.Framework;

namespace JoTPK_MonogamePort.Utils;

public static class LineOfSight {
    public static bool RayIntersectsRectangle(Vector2 rayOrigin, Vector2 rayDirection, GameObject gameObject) {

        Vector2 topLeft = new(gameObject.X, gameObject.Y);
        Vector2 topRight = new(gameObject.X + Consts.ObjectSize, gameObject.Y);
        Vector2 bottomLeft = new(gameObject.X, gameObject.Y + Consts.ObjectSize);
        Vector2 bottomRight = new(gameObject.X + Consts.ObjectSize, gameObject.Y + Consts.ObjectSize);

        return
            RayIntersectsSegment(rayOrigin, rayDirection, topLeft, topRight) ||
            RayIntersectsSegment(rayOrigin, rayDirection,topRight, bottomRight) ||
            RayIntersectsSegment(rayOrigin, rayDirection,bottomRight, bottomLeft) ||
            RayIntersectsSegment(rayOrigin, rayDirection,bottomLeft, topLeft);
    }


    public static bool RayIntersectsSegment(Vector2 rayOrigin, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd) {

        float crossProduct = Vector2Ext.CrossProduct(segmentEnd, segmentStart, rayDirection, out Vector2 difference);

        if (Math.Abs(crossProduct) < 0.0001f) return false;

        float t = Matrix2X2.Determinant(
            segmentStart.X - rayOrigin.X, segmentStart.Y - rayOrigin.Y,
            difference.X, difference.Y
        );
        float u = Matrix2X2.Determinant(
            rayOrigin.X - segmentStart.X, rayOrigin.Y - segmentStart.Y,
            rayDirection.X, rayDirection.Y
        );

        return t is > 0 and < 1 && u > 0;
    }
}