using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Items;

public class MachineGun : GameObject, IItem, IPowerUp {

    private int _interval;
    public bool IsInInventory { get; set; } = false;
    
    public float Timer { get; set; } = 0;

    public MachineGun(float x, float y, Player player, int interval) : base(x, y) {
        _interval = interval;
    }

    public MachineGun(float x, float y, Player player) : base(x, y) {
        _interval = 12_000;
    }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(Drawable.MachineGun, RoundedX, RoundedY, sb);
    }

    public void PickUp(Player player, Level level) {
        IPowerUp.GlobalPickUp(this, player, level);
    }

    public void Update(Player player, Level level, GameTime gt) {
        IPowerUp.GlobalUpdate(this, _interval, gt, level, player);
    }

    public void Activate(Player player, bool isInInventory) {
        player.FireRate = 0.075f;
        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        player.FireRate = Player.DefaultFireRate;
        IPowerUp.GlobalDeactivate(this, player);
    }
}