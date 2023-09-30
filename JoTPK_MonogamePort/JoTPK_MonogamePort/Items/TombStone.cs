using System;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Items; 

public class TombStone : GameObject, IItem, IPowerUp {

    private const int Interval = 8_000;
    public bool IsInInventory { get; set; } = false;
    public float Timer { get; set; } = 0;

    public TombStone(float x, float y, Player player) : base(x, y) { }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(Drawable.TombStone, RoundedX, RoundedY, sb);
    }

    public void PickUp(Player player, Level level) {
        IPowerUp.GlobalPickUp(this, player, level);
    }

    public void Update(Player player, Level level, GameTime gt) {
        IPowerUp.GlobalUpdate(this, Interval, gt, level, player);
    }

    public void Activate(Player player, bool isInInventory) {
        player.IsZombie = true;
        player.Speed = 4.5f;
        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        player.IsZombie = false;
        player.Speed = Player.DefaultSpeed;
        Console.WriteLine("deactivate");
        IPowerUp.GlobalDeactivate(this, player);
    }

}