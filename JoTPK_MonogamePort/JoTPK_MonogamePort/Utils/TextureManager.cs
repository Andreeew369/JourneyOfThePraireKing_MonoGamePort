using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Utils; 

public class TextureManager {
        
    private static readonly int Size = Consts.ObjectSize;
    private static Dictionary<string, Texture2D>? _textures;

    private static readonly Rectangle[] ObjectCoords = {
        new(0, 0, Size, Size), //player
        new(Size, 0, Size, Size), //player down
        new(2 * Size, 0, Size, Size), //player right
        new(3 * Size, 0, Size, Size), //player up
        new(4 * Size, 0, Size, Size), //player left
        new(5 * Size, 0, Size, Size), //player zombie1
        new(6 * Size, 0, Size, Size), //player zombie2

        new(7 * Size, 0, Size, 6), //legs1
        new(7 * Size, 8, Size, 6), //legs2
        new(7 * Size, 16, Size, 6), //legs3

        new(0, Size, Size, Size), //orc1
        new(Size, Size, Size, Size), //orc2
        
        new(2 * Size,  Size, Size, Size), //orcHit
        new(3 * Size, Size, Size, Size), //spikeball1
        new(4 * Size, Size, Size, Size), //spikeball2
        new(5 * Size, Size, Size, Size), //spikeBallHit

        new(6 * Size, Size, Size, Size), //spikeballBall1
        new(7 * Size, Size, Size, Size), //spikeballBall2
        new(8 * Size, Size, Size, Size), //spikeballBall3
        new(9 * Size, Size, Size, Size), //spikeballBall4
        new(10 * Size, Size, Size, Size), //spikeballBallHit

        new(11 * Size, Size, Size, Size), //ogre1
        new(12 * Size, Size, Size, Size), //ogre2
        new(13 * Size, Size, Size, Size), //ogreHit

        new(0,  2 * Size, Size, Size), //mushroom1
        new(Size, 2 *Size, Size, Size), //mushroom2
        new(2 * Size, 2 *Size, Size, Size), //mushroomHit

        new(3 * Size, 2 * Size, Size, Size), //butterfly1
        new(4 * Size, 2 * Size, Size, Size), //butterfly2
        new(5 * Size,  2 * Size, Size, Size), //butterflyHit

        new(6 * Size, 2 * Size, Size, Size), //mummy1
        new(7 * Size, 2 * Size, Size, Size), //mummy2
        new(8 * Size,  2 * Size, Size, Size), //mummyHit
        
        new(9 * Size, 2 * Size, Size, Size), //imp1
        new(10 * Size, 2 * Size, Size, Size), //imp2
        new(11 * Size, 2 * Size, Size, Size), //impHit

        new(12 * Size,  2 * Size, 8, 8),//bullet1
        new(12 * Size,  2 * Size + 8, 8, 8),//bullet2
        new(12 * Size,  2 * Size + 2 * 8, 8, 8),//bullet3
        new(12 * Size,  2 * Size + 3 * 8, 8, 8),//bullet4

        new(13 * Size, 2 * Size, Size, Size), //coin 1
        new(14 * Size, 2 * Size, Size, Size), //coin 5
        new(15 * Size, 2 * Size, Size, Size), //life
        new(14 * Size, 3 * Size, Size, Size), //sheriff badge
        new(15 * Size, 3 * Size, Size, Size), //wagon wheel
        new(14 * Size, 4 * Size, Size, Size), //nuke
        new(15 * Size, 4 * Size, Size, Size), //smoke bomb
        new(14 * Size, 5 * Size, Size, Size), //machine gun
        new(15 * Size, 5 * Size, Size, Size), //tombstone
        new(14 * Size, 6 * Size, Size, Size), //shotgun
        new(15 * Size, 6 * Size, Size, Size), //coffee
        
        new(0, 512 - 8 * 2, 6 * 2, 8 * 2)
    };

    public static void Inicialize(ContentManager cm) {
        _textures = new Dictionary<string, Texture2D> {
            {"GameObjects", cm.Load<Texture2D>("GameObjects")},
            {"Map0", cm.Load<Texture2D>("Levels\\level0")},
            {"Map1", cm.Load<Texture2D>("Levels\\level1")},
            {"Map2", cm.Load<Texture2D>("Levels\\level2")},
            {"Map3", cm.Load<Texture2D>("Levels\\level3")},
            {"Map4", cm.Load<Texture2D>("Levels\\level4")},
            {"Map5", cm.Load<Texture2D>("Levels\\level5")},
            {"Map6", cm.Load<Texture2D>("Levels\\level6")},
            {"Map7", cm.Load<Texture2D>("Levels\\level7")},
            {"Map8", cm.Load<Texture2D>("Levels\\level8")},
            {"Map9", cm.Load<Texture2D>("Levels\\level9")},
            {"Map10", cm.Load<Texture2D>("Levels\\level10")},
            {"Map11", cm.Load<Texture2D>("Levels\\level11")},
            //{"Map12", cm.Load<Texture2D>("Levels\\level12")},
        };
    }

    public static Texture2D GetTexture(string texture) {
        if (_textures == null) 
            throw new NullReferenceException("Texture Manager wasnt inicialized.");
        
        if (_textures.TryGetValue(texture, out Texture2D? geTexture))
            return geTexture;

        throw new ArgumentException($"Texture manager doesnt contain {texture}");
    }


    public static void DrawMap(string map, SpriteBatch sb) {
        if (_textures == null)
            throw new NullReferenceException("Texture Manager wasnt inicialized.");

        if (_textures.TryGetValue(map, out Texture2D? texture)) {
            sb.Draw(texture, new Vector2(Consts.LevelXOffset, Consts.LevelYOffset), Color.White);
        }
    }

    public static void DrawObject(Drawable d, float x, float y, SpriteBatch sb) {
        if (_textures == null) 
            throw new NullReferenceException("Texture Manager wasnt inicialized.");

        if ((int)d >= ObjectCoords.Length)
            throw new NotImplementedException();

        if (_textures.TryGetValue("GameObjects", out Texture2D? texture)) {
            Rectangle coords = ObjectCoords[(int)d];
            sb.Draw(texture, new Rectangle((int)x, (int)y, coords.Width, coords.Height), coords, Color.White);
        }
    }

    // public static void DrawObject(Drawable d, Rectangle scale, SpriteBatch sb) {
    //     if (_textures == null) 
    //         throw new NullReferenceException("Texture Manager wasnt initialized.");
    //
    //     if ((int)d >= ObjectCoords.Length)
    //         throw new NotImplementedException();
    //
    //     if (_textures.TryGetValue("GameObjects", out Texture2D? texture)) {
    //         Rectangle coords = ObjectCoords[(int)d];
    //         sb.Draw(texture, scale, coords, Color.White);
    //     }
    // }

    public static void Dispose() {
        if (_textures == null)
            throw new NullReferenceException("Texture Manager wasnt inicialized.");

        foreach (Texture2D texture in _textures.Values) {
            if (texture.IsDisposed) continue;
            texture.Dispose();
        }
    }
}

