using JoTPK_MonogamePort.GameObjects.Entities;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.GameObjects.Items;

/// <summary>
/// Power up that kills all enemies on the map
/// </summary>
public class Nuke(float x, float y, EnemiesManager enemiesManager) : GameObject(x, y), IPowerUp {

    private const int Interval = 4000;
    public bool IsInInventory { get; set; } = false;
    
    public float Timer { get; set; } = 0;

    public override void Draw(SpriteBatch sb) => TextureManager.DrawObject(GameElements.Nuke, RoundedX, RoundedY, sb);

    public void Activate(Player player, bool isInInventory) {
        enemiesManager.NukeEnemies();
        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        enemiesManager.CanSpawn = true;
        enemiesManager.Nuked = false;
        IPowerUp.GlobalDeactivate(this, player);
    }

    public void PickUp(Player player, Level level) => IPowerUp.GlobalPickup(this, player, level);

    public void Update(Player player, Level level, GameTime gt) => IPowerUp.GlobalUpdate(this, Interval, gt, level, player);
}