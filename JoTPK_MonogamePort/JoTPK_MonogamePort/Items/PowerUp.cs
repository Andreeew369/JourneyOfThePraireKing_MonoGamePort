using System;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;

namespace JoTPK_MonogamePort.Items;

/// <summary>
/// Interface for power ups. Recommended to implemented IItem interface and inherit from GameObject too. 
/// </summary>
public interface IPowerUp {
    // todo try to refactor this into interface that extends IItem
    
    public static GameElements? PowerUpToGameElement(IPowerUp powerUp) {
        return powerUp switch {
            Coffee _ => GameElements.Coffee,
            MachineGun => GameElements.MachineGun,
            ShotGun => GameElements.Shotgun,
            SherrifBadge => GameElements.SheriffBadge,
            TombStone => GameElements.TombStone,
            SmokeBomb => GameElements.SmokeBomb,
            Nuke => GameElements.Nuke,
            WagonWheel => GameElements.WagonWheel,
            EmptyPowerUp => null,
            _ => throw new NotImplementedException()
        };
    }
    
    /// <summary>
    /// Update for every power up. Should be called in every power up's update method
    /// </summary>
    /// <param name="powerUp">Power up you want to update</param>
    /// <param name="interval">Update interval for the power up</param>
    /// <param name="gt">Instance of game time for current game</param>
    /// <param name="level">Instance of current level</param>
    /// <param name="player">Instance of player in current game</param>
    /// <exception cref="ArgumentException">If power up is not instance of GameObject and IItem</exception>
    public static void GlobalUpdate(IPowerUp powerUp, int interval, GameTime gt, Level level, Player player) {
        if (powerUp is not (IItem or GameObject))
            throw new ArgumentException("Invalid power up. Power up has to be instance of GameObject and IItem");

        ((IItem)powerUp).Timer += gt.ElapsedGameTime.Milliseconds;
        IItem item = (IItem)powerUp;
                
        if (!powerUp.IsInInventory) {
            if (item.Timer < IItem.DespawnTime) return;
            level.RemoveObject((GameObject)item, Level.GetIndexes((GameObject)item));
        }
        else {
            // Console.WriteLine("deactivate");
            if (item.Timer < interval) return;
            powerUp.Deactivate(player);
        }
    }
    
    /// <summary>
    /// Activate power up for every power up. Should be called in every power up's activate method
    /// </summary>
    /// <param name="powerUp">Power up you want to activate</param>
    /// <param name="player">Instance of player in current game</param>
    /// <param name="isInInventory">If power up activated from inventory or is activated on pickup</param>
    public static void GlobalActivate(IPowerUp powerUp, Player player, bool isInInventory) {
        if (isInInventory) {
            player.InventoryPowerUp = EmptyPowerUp.Empty;
        }

        if (player.DoesPlayerHavePowerUp(powerUp, out IPowerUp? p) && p != null) {
            player.RemoveActivePowerUp(p);
        }

        player.AddActivePowerUp(powerUp);
    }

    /// <summary>
    /// Deactivate power up for every power up. Should be called in every power up's deactivate method
    /// </summary>
    /// <param name="powerUp">Instance of power up you want to deactivate</param>
    /// <param name="player">Instance of player in current game</param>
    public static void GlobalDeactivate(IPowerUp powerUp, Player player) {
        player.RemoveActivePowerUp(powerUp);
    }

    /// <summary>
    /// Pickup power up for every power up. Should be called in every power up's pick up method
    /// </summary>
    /// <param name="powerUp">Power up you want to pick up</param>
    /// <param name="player">Instance of player in current game</param>
    /// <param name="level">Instance of current level</param>
    /// <exception cref="ArgumentException">If power up is not instance of GameObject and IItem</exception>
    public static void GlobalPickup(IPowerUp powerUp, Player player, Level level) {
        if (powerUp is not (IItem and GameObject))
            throw new ArgumentException("Invalid power up. Power up has to be instance of GameObject and IItem");

        GameObject goPowerUp = (GameObject)powerUp; 
        level.RemoveObject(goPowerUp, Level.GetIndexes(goPowerUp));
        powerUp.IsInInventory = true;

        if (player.InventoryPowerUp is EmptyPowerUp) {
            player.InventoryPowerUp = powerUp;
        } else {
            powerUp.Activate(player, false);
        }
    }

    /// <summary>
    /// What should happen when power up is activated
    /// </summary>
    /// <param name="player"></param>
    /// <param name="isInInventory"></param>
    void Activate(Player player, bool isInInventory);
    
    /// <summary>
    /// What should happen when power up is deactivated
    /// </summary>
    /// <param name="player"></param>
    void Deactivate(Player player);
    
    bool IsInInventory { get; set; }
}

/// <summary>
/// Placeholder power up, used as empty slot for power up
/// </summary>
public class EmptyPowerUp : IPowerUp {

    private static EmptyPowerUp? _empty;

    public static EmptyPowerUp Empty {
        get {
            _empty ??= new EmptyPowerUp();
            return _empty;
        }
    }
    private EmptyPowerUp() { }
    public bool IsInInventory { get; set; } = false;
    public void Activate(Player player, bool isInInventory) { }
    public void Deactivate(Player player) { }
}