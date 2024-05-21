using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Utils; 

public static class TextureManager {
    
    private const int Size = Consts.ObjectSize;
    //fix arrow
    
    /// <summary>
    /// Array represents coordinates of all GUI textures in the sprite sheet. Index of the array the int
    /// value of the <see cref="GuiElement"/> enum
    /// </summary>
    private static readonly Rectangle[] GuiElementsCoords = {
        new(0, 0, 256, 12), //Timer frame
        new(0, 12 + 1, 248, 4), //Gradient
        new(0, 16 + 1, 91, 40), //Controls hint
        new(91, 16 + 1, 28, 30), //E Key Hint
        new(91 + 28, 16 + 1, 24, 24), //PowerUp Frame
        new(91 + 28 + 24, 16 + 1, 6, 8), //Arrow
        new(91 + 28 + 24 + 6, 16 + 1, 9, 11),//Timer Icon
    };

    /// <summary>
    /// Array represents coordinates of all textures of game objects in the sprite sheet. Index of the array the int
    /// value of the <see cref="GameElements"/> enum
    /// </summary>
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
        
        new(13 * Size, 3 * Size, Size, Size), //Speed1
        new(13 * Size, 4 * Size, Size, Size), //Speed2
        new(11 * Size, 3 * Size, Size, Size), //BulletDmg1
        new(11 * Size, 4 * Size, Size, Size), //BulletDmg2
        new(11 * Size, 5 * Size, Size, Size), //BulletDmg3 
        new(12 * Size, 3 * Size, Size, Size), //ShootingBoost1 
        new(12 * Size, 4 * Size, Size, Size), //ShootingBoost2
        new(12 * Size, 5 * Size, Size, Size), //ShootingBoost3
        new(13 * Size, 5 * Size, Size, Size), //SuperGun
        
        new(0, 4 * Size, Size, Size), //CowboyIdle1
        new(Size, 4 * Size, Size, Size), //CowboyIdle2
        new(2 * Size, 4 * Size, Size, Size), //CowBoyShooting1
        new(3 * Size, 4 * Size, Size, Size), //CowboyShooting2
        new(4 * Size, 4 * Size, Size, Size), //CowboyHit
        
        new(5 * Size, 4 * Size, Size, Size), //Fector1
        new(6 * Size, 4 * Size, Size, Size), //Fector2
        new(7 * Size, 4 * Size, Size, Size), //Fector3
        new(8 * Size, 4 * Size, Size, Size), //FectorHit
        
        new(0, 3 * Size, Size, Size), //Gopher1
        new(Size, 3 * Size, Size, Size), //Gopher2
        new(2 * Size, 3 * Size, Size, Size), //Gopher3
        
        new(3 * Size, 3 * Size, Size, Size), //Trader1
        new(4 * Size, 3 * Size, Size, Size), //Trader2
        new(5 * Size, 3 * Size, Size, Size), //TraderIdle
        new(6 * Size, 3 * Size, Size, Size), //TraderIdleLeft
        new(7 * Size, 3 * Size, Size, Size), //TraderIdleRight
        new(0, 6 * Size, 4 * Size, 2 * Size), //TraderFrame
        
        new(10 * Size, 3 * Size, Size, Size), //Hearth
        new(10 * Size, 4 * Size, Size, Size), //Skull
        new(10 * Size, 5 * Size, Size, Size), //Log
        new(15 * Size, Size, Size, Size), //bridge
        
        new(12 * Size + 5, 2 * Size, 5, 5),
        new(12 * Size + 5, 2 * Size + 6, 5, 5),
    };

    public static ImmutableArray<Rectangle> ObjectCoordsP => ObjectCoords.ToImmutableArray();
    public static ImmutableArray<Rectangle> GuiElementsCoordsP => GuiElementsCoords.ToImmutableArray();

    private static Dictionary<string, Texture2D>? _textures;
    private static Dictionary<string, Texture2D>? _mapTextures;

    /// <summary>
    /// Initialization of the texture manager. Should be called at in the initialization method of the game
    /// </summary>
    /// <param name="cm"></param>
    public static void Initialize(ContentManager cm) {
        _mapTextures = new Dictionary<string, Texture2D> {
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
        _textures = new Dictionary<string, Texture2D> {
            {"GameObjects", cm.Load<Texture2D>("GameObjects")},
            {"GuiElements", cm.Load<Texture2D>("Gui")},
        };
    }

    public static int MapCount => _mapTextures?.Count ?? 0;

    public static void DrawMap(string map, SpriteBatch sb) {
        if (_mapTextures == null)
            throw new NullReferenceException("Texture Manager wasnt inicialized.");

        if (_mapTextures.TryGetValue(map, out Texture2D? texture)) {
            sb.Draw(texture, new Vector2(Consts.LevelXOffset, Consts.LevelYOffset), Color.White);
        }
    }

    public static void DrawObject(GameElements o, float x, float y, SpriteBatch sb) {
        if (_textures == null) 
            throw new NullReferenceException("Texture Manager wasnt inicialized.");

        if ((int)o >= ObjectCoords.Length)
            throw new NotImplementedException();

        if (_textures.TryGetValue("GameObjects", out Texture2D? texture)) {
            Rectangle coords = ObjectCoords[(int)o];
            // sb.Draw(texture, new Rectangle((int)x, (int)y, coords.Width, coords.Height), coords, Color.White);
            sb.Draw(texture, new Vector2(x, y), coords, Color.White);
        }
    }
    
    public static void DrawGuiElement(GuiElement g, float x, float y, SpriteBatch sb) {
        if ((int)g >= ObjectCoords.Length)
            throw new NotImplementedException();
        
        DrawGuiElement(x, y, sb, GuiElementsCoords[(int)g]);
    }

    public static void DrawGuiElement(float x, float y, SpriteBatch sb, Rectangle partOfTexture) {
        if (_textures == null) 
            throw new NullReferenceException("Texture Manager wasnt inicialized.");

        if (_textures.TryGetValue("GuiElements", out Texture2D? texture)) {
            sb.Draw(texture, new Vector2(x, y), partOfTexture, Color.White, 0f, Vector2.Zero, 2f ,SpriteEffects.None, 0f);
        }
    }
}

