using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Items; 

public class SherrifBadge : GameObject, IItem, IPowerUp {

    private const int Interval = 24_000;
    private readonly ShotGun _shotGun;
    private readonly MachineGun _machineGun;
    private readonly Coffee _coffee;
    public bool IsInInventory { get; set; } = false;
    
    public float Timer { get; set; } = 0;

    public SherrifBadge(float x, float y, Player player) : base(x, y) {
        _shotGun = new ShotGun(0, 0, player, Interval);
        _machineGun = new MachineGun(0, 0, player, Interval);
        _coffee = new Coffee(0, 0, player, Interval);
    }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(Drawable.SheriffBadge, RoundedX, RoundedY, sb);
    }

    public void PickUp(Player player, Level level) {
        _shotGun.IsInInventory = true;
        _machineGun.IsInInventory = true;
        _coffee.IsInInventory = true;
        IPowerUp.GlobalPickUp(this, player, level);
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