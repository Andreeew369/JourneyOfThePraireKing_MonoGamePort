using Microsoft.Xna.Framework;

namespace JoTPK_MonogamePort.Utils; 

public static class Vector2Ext {

    public static float CrossProduct(Vector2 end, Vector2 start, Vector2 rayDir, out Vector2 difference) {
        difference = end - start;
        return CrossProduct(difference, rayDir);
    }

    public static float CrossProduct(Vector2 a, Vector2 b) {
        return a.X * b.Y - a.Y * b.X;
    }
}