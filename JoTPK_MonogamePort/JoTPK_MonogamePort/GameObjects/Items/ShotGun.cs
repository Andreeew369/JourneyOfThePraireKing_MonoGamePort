using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects.Items;

/// <summary>
/// Shotgun power up, which changes the player's shooting type to <see cref="ShootingType.ShotGun"/>
/// </summary>
public class ShotGun(float x, float y, int interval = 12_000) : GameObject(x, y), IPowerUp {
    public bool IsInInventory { get; set; }
    public float Timer { get; set; } = 0;

    public void Activate(Player player, bool isInInventory) {
        player.ShootingType = ShootingType.ShotGun;
        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        player.RemoveShootingType(ShootingType.ShotGun);
        IPowerUp.GlobalDeactivate(this, player);
    }
    
    public override void Draw(SpriteBatch sb) => TextureManager.DrawObject(GameElements.Shotgun, RoundedX, RoundedY, sb);

    public void PickUp(Player player, Level level) => IPowerUp.GlobalPickup(this, player, level);

    public void Update(Player player, Level level, GameTime gt) => IPowerUp.GlobalUpdate(this, interval, gt, level, player);
}