﻿using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects.Items; 

/// <summary>
/// Power up that turns the player into a zombie, which kills all enemies on contact with player
/// </summary>
public class TombStone(float x, float y) : GameObject(x, y), IPowerUp {

    private const int Interval = 8_000;
    public bool IsInInventory { get; set; } = false;
    public float Timer { get; set; } = 0;

    public override void Draw(SpriteBatch sb) => TextureManager.DrawObject(GameElements.TombStone, RoundedX, RoundedY, sb);

    public void PickUp(Player player, Level level) => IPowerUp.GlobalPickup(this, player, level);

    public void Update(Player player, Level level, GameTime gt) => IPowerUp.GlobalUpdate(this, Interval, gt, level, player);

    public void Activate(Player player, bool isInInventory) {
        player.IsZombie = true;
        player.Speed = 4.5f;
        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        player.IsZombie = false;
        player.Speed = player.DefaultSpeed;
        IPowerUp.GlobalDeactivate(this, player);
    }

}