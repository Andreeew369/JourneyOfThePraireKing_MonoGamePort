﻿using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Items;

/// <summary>
/// Machine gun power up, which speeds up the player's fire rate
/// </summary>
public class MachineGun : GameObject, IItem, IPowerUp {

    private readonly int _interval;
    public bool IsInInventory { get; set; }
    
    public float Timer { get; set; } = 0;

    public MachineGun(float x, float y, int interval = 12_000) : base(x, y) {
        _interval = interval;
    }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(GameElements.MachineGun, RoundedX, RoundedY, sb);
    }

    public void PickUp(Player player, Level level) {
        IPowerUp.GlobalPickup(this, player, level);
    }

    public void Update(Player player, Level level, GameTime gt) {
        IPowerUp.GlobalUpdate(this, _interval, gt, level, player);
    }

    public void Activate(Player player, bool isInInventory) {
        player.FireRate = 0.075f;
        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        player.FireRate = player.DefaultFireRate;
        IPowerUp.GlobalDeactivate(this, player);
    }
}