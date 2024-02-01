using System;
using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;

namespace JoTPK_MonogamePort.Items;

public interface IPowerUp {
    public static GameElements? PowerUpToDrawable(IPowerUp powerUp) {
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
    
    public static void GlobalUpdate(IPowerUp powerUp, int interval, GameTime gt, Level level, Player player) {
        
        if (powerUp is not (IItem or GameObject)) return;
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
    
    public static void GlobalActivate(IPowerUp powerUp, Player player, bool isInInventory) {
        if (isInInventory) {
            player.InventoryPowerUp = EmptyPowerUp.Empty;
        }

        if (player.DoesPlayerHavePowerUp(powerUp, out IPowerUp? p) && p != null) {
            player.RemoveActivePowerUp(p);
        }

        player.AddActivePowerUp(powerUp);
    }

    public static void GlobalDeactivate(IPowerUp powerUp, Player player) {
        player.RemoveActivePowerUp(powerUp);
    }

    public static void GlobalPickUp(IPowerUp powerUp, Player player, Level level) {
        if (powerUp is not (IItem and GameObject))
            throw new ArgumentException(@"Invalid power up. Power up has to be instance of GameObject and IItem");

        level.RemoveObject((GameObject)powerUp, Level.GetIndexes((GameObject)powerUp));
        powerUp.IsInInventory = true;

        if (player.InventoryPowerUp is EmptyPowerUp) {
            player.InventoryPowerUp = powerUp;
        } else {
            powerUp.Activate(player, false);
        }
    }

    void Activate(Player player, bool isInInventory);
    void Deactivate(Player player);
    public bool IsInInventory { get; set; }
}

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