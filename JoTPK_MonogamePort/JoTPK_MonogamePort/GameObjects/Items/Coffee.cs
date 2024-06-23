using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects.Items;

/// <summary>
/// Power up that speeds up the player
/// </summary>
public class Coffee(int x, int y, int interval = 8000) : GameObject(x, y), IPowerUp {
    public bool IsInInventory { get; set; }
    
    public float Timer { get; set; } = 0;

    public override void Draw(SpriteBatch sb) => TextureManager.DrawObject(GameElements.Coffee, RoundedX, RoundedY, sb);

    public void PickUp(Player player, Level level) => IPowerUp.GlobalPickup(this, player, level);

    public void Update(Player player, Level level, GameTime gt) => IPowerUp.GlobalUpdate(this, interval, gt, level, player);
    
    public void Activate(Player player, bool isInInventory) {
        player.Speed = player.DefaultSpeed + 1;
        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        player.Speed = player.DefaultSpeed;
        IPowerUp.GlobalDeactivate(this, player);
    }
}