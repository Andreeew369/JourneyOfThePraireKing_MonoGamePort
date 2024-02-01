using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.Items;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.World; 

public class PowerUpDisplay {

    private Vector2 _coords;
    private Player _player;
    
    public PowerUpDisplay(int x,  int y, Player player) {
        _coords = new Vector2(x, y);
        _player = player;
    }

    public void Draw(SpriteBatch sb) {
        TextureManager.DrawGuiElement(GuiElement.PowerUpFrame, _coords.X, _coords.Y, sb);
        GameElements? ge = IPowerUp.PowerUpToDrawable(_player.InventoryPowerUp);
        if (ge is not null) {
            TextureManager.DrawObject(
                ge.Value, _coords.X + 8, _coords.Y + 8, sb
            );
        }
    }
}