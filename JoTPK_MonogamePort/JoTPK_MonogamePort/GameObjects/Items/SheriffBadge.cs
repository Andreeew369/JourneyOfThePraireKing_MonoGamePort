using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects.Items; 

/// <summary>
/// It's a combination of <see cref="Coffee"/>,  <see cref="MachineGun"/> and  <see cref="ShotGun"/> power ups
/// </summary>
public class SheriffBadge(float x, float y) : GameObject(x, y), IPowerUp {

    private const int Interval = 24_000;
    private readonly ShotGun _shotGun = new(0, 0, Interval);
    private readonly MachineGun _machineGun = new(0, 0, Interval);
    private readonly Coffee _coffee = new(0, 0, Interval);
    
    public bool IsInInventory { get; set; } = false;
    public float Timer { get; set; } = 0;

    public override void Draw(SpriteBatch sb) => TextureManager.DrawObject(GameElements.SheriffBadge, RoundedX, RoundedY, sb);

    public void PickUp(Player player, Level level) {
        _shotGun.IsInInventory = true;
        _machineGun.IsInInventory = true;
        _coffee.IsInInventory = true;
        IPowerUp.GlobalPickup(this, player, level);
    }

    public void Update(Player player, Level level, GameTime gt) {
        _shotGun.Update(player, level, gt);
        _machineGun.Update(player, level, gt);
        _coffee.Update(player, level, gt);
        
        IPowerUp.GlobalUpdate(this, Interval, gt, level, player);
    }


    public void Activate(Player player, bool isInInventory) {
        _shotGun.Activate(player, isInInventory);
        _coffee.Activate(player, isInInventory);
        _machineGun.Activate(player, isInInventory);

        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        _shotGun.Deactivate(player);
        _coffee.Deactivate(player);
        _machineGun.Deactivate(player);

        IPowerUp.GlobalDeactivate(this, player);
    }
}