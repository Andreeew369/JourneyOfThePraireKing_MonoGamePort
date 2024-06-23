using System;
using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects.Items; 

/// <summary>
/// Permanent upgrade that can be bought from the trader.
/// </summary>
public class Upgrade : GameObject, IItem {
    private readonly UpgradeMethods.UpgradePickUpFunction _pickUpFunc;
    
    public Upgrades Type { get; }
    public int Price { get; }

    private Upgrade(int x, int y, Upgrades upgrade) : base(x, y) {
        Type = upgrade;
        Price = upgrade.GetPrice();
        _pickUpFunc = upgrade.GetFunc();
    }
    
    public Upgrade(Vector2 pos, Upgrades upgrade) : this((int)pos.X, (int)pos.Y, upgrade) { }
    
    public void PickUp(Player player, Level level) => _pickUpFunc(player, level);

    public void Update(Player player, Level level, GameTime gt) { }

    public float Timer { get; set; } = 0;

    public override void Draw(SpriteBatch sb) => TextureManager.DrawObject(Type.ToGameElement(), RoundedX, RoundedY, sb);
}

/// <summary>
/// static class for extension methods for <see cref="Upgrades"/> enum
/// </summary>
public static class UpgradeMethods {
    /// <summary>
    /// Delegate for functions used in <see cref="Upgrade"/> class
    /// </summary>
    public delegate void UpgradePickUpFunction(Player player, Level level);
    
    /// <summary>
    /// Returns a function for Upgrades enum, what the upgrade should do on pickup
    /// </summary>
    /// <param name="upgrade">Upgrade</param>
    /// <returns>Function that should be called when the upgrade is picked up</returns>
    /// <exception cref="NotImplementedException">If a new Upgrade is implemented and function is not defined</exception>
    public static UpgradePickUpFunction GetFunc(this Upgrades upgrade) {
        return upgrade switch {
            Upgrades.Boots1 or Upgrades.Boots2 => (player, _) => {
                player.DefaultSpeed += 1;
            },
            Upgrades.Gun1 or Upgrades.Gun2 or Upgrades.Gun3 => (player, _) => {
                player.DefaultFireRate -= player.DefaultFireRate * 0.2f;
            },
            Upgrades.Ammo1 or Upgrades.Ammo2 or Upgrades.Ammo3 => (player, _) => {
                player.BulletDamage += 1;
            },
            Upgrades.SuperGun => (player, _) => {
                player.ShootingType = ShootingType.ShotGun;
                player.CanRemoveShootGun = false;
            },
            Upgrades.SheriffBadge => (player, level) => {
                new SheriffBadge(0, 0).PickUp(player, level);
            },
            Upgrades.HealthPoint => (player, level) => {
                new HealthPoint(0, 0).PickUp(player, level);
            },
            _ => throw new NotImplementedException("Upgrade function not implemented for this upgrade.")
        };
    }

    public static int GetPrice(this Upgrades powerUp) {
        return powerUp switch {
            Upgrades.Boots1 => 8,
            Upgrades.HealthPoint or Upgrades.SheriffBadge or Upgrades.Gun1 => 10,
            Upgrades.Ammo1 => 15,
            Upgrades.Boots2 or Upgrades.Gun2 => 20,
            Upgrades.Gun3 or Upgrades.Ammo2 => 30,
            Upgrades.Ammo3 => 45,
            Upgrades.SuperGun => 99,
            _ => throw new NotImplementedException("Price not implemented for this upgrade.")
        };
    }

    public static GameElements ToGameElement(this Upgrades powerUp) {
        return Enum.TryParse(powerUp.ToString(), out GameElements gameElement) 
            ? gameElement 
            : throw new ArgumentException(
            $"Upgrade can't be parsed to {nameof(GameElements)} class." +
                  $" Upgrade probably doesnt have the same name as value in {nameof(GameElements)}" +
                  $" or isn't defined in {nameof(GameElements)}"
                );
    }
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