using System.Drawing;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Items;

public class Coffee : GameObject, IItem, IPowerUp {
    public bool IsInInventory { get; set; } = false;
    
    public float Timer { get; set; } = 0;

    private int _interval;

    public Coffee(int x, int y, int interval = 8000) : base(x, y) {
        _interval = interval;
    }

    public override RectangleF HitBox => IItem.GlobalHitBox(X, Y);

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(GameElements.Coffee, RoundedX, RoundedY, sb);
    }

    public void PickUp(Player player, Level level) {
        IPowerUp.GlobalPickUp(this, player, level);
    }

    public void Update(Player player, Level level, GameTime gt) {
        IPowerUp.GlobalUpdate(this, _interval, gt, level, player);
    }


    public void Activate(Player player, bool isInInventory) {
        player.Speed = player.DefaultSpeed + 1;
        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        player.Speed = player.DefaultSpeed;
        IPowerUp.GlobalDeactivate(this, player);
    }
}