using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Items;

/// <summary>
/// Wagon wheel power up, which changes the player's shooting type to <see cref="ShootingType.Wheel"/>,
/// which will make player shoot in all 8 directions
/// </summary>
public class WagonWheel : GameObject, IItem, IPowerUp {

    private const int Interval = 12_000;
    public bool IsInInventory { get; set; } = false;
    public float Timer { get; set; } = 0;

    public WagonWheel(int x, int y) : base(x, y) { }

    public override void Draw(SpriteBatch sb) => TextureManager.DrawObject(GameElements.WagonWheel, RoundedX, RoundedY, sb);

    public void PickUp(Player player, Level level) => IPowerUp.GlobalPickup(this, player, level);

    public void Update(Player player, Level level, GameTime gt) => IPowerUp.GlobalUpdate(this, Interval, gt, level, player);

    public void Activate(Player player, bool isInInventory) {
        player.ShootingType = ShootingType.Wheel;
        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        player.RemoveShootingType(ShootingType.Wheel);
        IPowerUp.GlobalDeactivate(this, player);
    }
}