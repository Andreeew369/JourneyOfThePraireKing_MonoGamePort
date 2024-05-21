using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Items; 

/// <summary>
/// Permanent upgrade that can be bought from the trader.
/// </summary>
public class Upgrade : GameObject, IItem {
    private readonly UpgradePickUpFunction _pickUpFunc;
    
    public Upgrades Type { get; }
    public int Price { get; }

    private Upgrade(int x, int y, Upgrades upgrade) : base(x, y) {
        Type = upgrade;
        Price = upgrade.GetPrice();
        _pickUpFunc = upgrade.GetFunc();
    }
    
    public Upgrade(Vector2 pos, Upgrades upgrade)
        : this((int)pos.X, (int)pos.Y, upgrade) { }
    
    public void PickUp(Player player, Level level) => _pickUpFunc(player, level);

    public void Update(Player player, Level level, GameTime gt) { }

    public float Timer { get; set; } = 0;

    public override void Draw(SpriteBatch sb) => TextureManager.DrawObject(Type.ToGameElement(), RoundedX, RoundedY, sb);
}

/// <summary>
/// Enum for all possible upgrades
/// </summary>
public enum Upgrades {
    Boots1, Boots2,
    Gun1, Gun2, Gun3,
    Ammo1, Ammo2, Ammo3,
    SuperGun,
    HealthPoint, SheriffBadge
}