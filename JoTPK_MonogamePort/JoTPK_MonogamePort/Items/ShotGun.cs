using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Items;

/// <summary>
/// Shotgun power up, which changes the player's shooting type to <see cref="ShootingType.ShotGun"/>
/// </summary>
public class ShotGun : GameObject, IPowerUp, IItem {

    private readonly int _interval;
    public bool IsInInventory { get; set; }
    public float Timer { get; set; } = 0;

    public ShotGun(float x, float y, int interval = 12_000) : base(x, y) {
        _interval = interval;
    }

    public void Activate(Player player, bool isInInventory) {
        player.ShootingType = ShootingType.ShotGun;
        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        player.RemoveShootingType(ShootingType.ShotGun);
        IPowerUp.GlobalDeactivate(this, player);
    }
    
    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(GameElements.Shotgun, RoundedX, RoundedY, sb);
    }

    public void PickUp(Player player, Level level) {
        IPowerUp.GlobalPickup(this, player, level);
    }

    public void Update(Player player, Level level, GameTime gt) {
        IPowerUp.GlobalUpdate(this, _interval, gt, level, player);
    }
}