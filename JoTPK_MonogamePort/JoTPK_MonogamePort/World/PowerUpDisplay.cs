using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.GameObjects.Items;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.World; 

/// <summary>
/// Class that represents a power up display, which shows the current power up in the player's inventory
/// </summary>
public class PowerUpDisplay(int x, int y) {
    private readonly Vector2 _coords = new(x, y);

    public void Draw(SpriteBatch sb, Player player) {
        TextureManager.DrawGuiElement(GuiElement.PowerUpFrame, _coords.X, _coords.Y, sb);
        GameElements? ge = IPowerUp.PowerUpToGameElement(player.InventoryPowerUp);
        if (ge is not null)
            TextureManager.DrawObject(ge.Value, _coords.X + 8, _coords.Y + 8, sb);
    }
}