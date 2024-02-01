using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using System;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Items; 

public class Upgrade : GameObject {
    private readonly Trader _trader;
    private readonly UpgradeFunction _pickUpFunc;
    
    public Upgrades Type { get; }
    public int Price { get; }

    private Upgrade(int x, int y, Upgrades upgrade, Trader trader) : base(x, y) {
        Type = upgrade;
        Price = upgrade.GetPrice();
        _pickUpFunc = upgrade.GetFunc();
        _trader = trader;
    }
    
    public Upgrade(Vector2 pos, Upgrades upgrade, Trader trader)
        : this((int)pos.X, (int)pos.Y, upgrade, trader) { }
    
    public void PickUp(Player player, Level level) {
        _pickUpFunc(player, level);
    }
    
    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(Type.ToGameElement(), RoundedX, RoundedY, sb);
    }
}

public enum Upgrades {
    Boots1, Boots2,
    Gun1, Gun2, Gun3,
    Ammo1, Ammo2, Ammo3,
    SuperGun,
    HealthPoint, SheriffBadge
}