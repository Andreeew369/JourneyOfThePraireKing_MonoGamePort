using JoTPK_MonogamePort.Entities;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JoTPK_MonogamePort.Items;

public class Nuke : GameObject, IPowerUp, IItem {

    private const int Interval = 4000;
    private readonly EnemiesManager _enemiesManager;
    public bool IsInInventory { get; set; } = false;
    
    public float Timer { get; set; } = 0;

    public Nuke(float x, float y, Player player ,EnemiesManager enemiesManager) : base(x, y) {
        _enemiesManager = enemiesManager;
    }

    public override void Draw(SpriteBatch sb) {
        TextureManager.DrawObject(Drawable.Nuke, RoundedX, RoundedY, sb);
    }

    public void Activate(Player player, bool isInInventory) {
        _enemiesManager.NukeEnemies();
        IPowerUp.GlobalActivate(this, player, isInInventory);
    }

    public void Deactivate(Player player) {
        _enemiesManager.CanSpawn = true;
        IPowerUp.GlobalDeactivate(this, player);
    }

    public void PickUp(Player player, Level level) {
        IPowerUp.GlobalPickUp(this, player, level);
    }

    public void Update(Player player, Level level, GameTime gt) {
        IPowerUp.GlobalUpdate(this, Interval, gt, level, player);
    }
}