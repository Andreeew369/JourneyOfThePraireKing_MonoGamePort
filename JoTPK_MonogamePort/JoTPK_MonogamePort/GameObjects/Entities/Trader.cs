using System;
using System.Collections.Immutable;
using System.Drawing;
using JoTPK_MonogamePort.GameObjects.Items;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace JoTPK_MonogamePort.GameObjects.Entities;

/// <summary>
/// Class representing a trader that sells upgrades to the player in every second level.
/// </summary>
public class Trader(float x, float y) : GameObject(x, y) {
    
    public const int MovingSpeed = 2;
    private const int XUpgradeOffset = 6;
    private const int YUpgradeOffset = 6;
    private const int ItemCount = 3;
    private const int YSpaceBetween = Consts.ObjectSize + 10;

    public static readonly ImmutableArray<GameElements> Sprites = [GameElements.Trader1, GameElements.Trader2];
    private static readonly Vector2 FramePos = new(7 * Consts.ObjectSize, 6 * Consts.ObjectSize);
    private static readonly ImmutableArray<Vector2> UpgradePositions = [
        FramePos + new Vector2(XUpgradeOffset, YUpgradeOffset),
        FramePos + new Vector2(XUpgradeOffset + YSpaceBetween, YUpgradeOffset),
        FramePos + new Vector2(XUpgradeOffset + YSpaceBetween * 2, YUpgradeOffset)
    ];
    private static GameElements?[] _upgradeIcons = [null, null, null];

    private int[] _upgradeLevels = [0, 0, 0];
    private readonly ImmutableArray<ImmutableArray<Upgrade>> _shop = [
        ImmutableArray.Create(
            new Upgrade(UpgradePositions[0], Upgrades.Boots1),
            new Upgrade(UpgradePositions[0], Upgrades.Boots2),
            new Upgrade(UpgradePositions[0], Upgrades.HealthPoint)
        ),
        ImmutableArray.Create(
            new Upgrade(UpgradePositions[1], Upgrades.Gun1),
            new Upgrade(UpgradePositions[1], Upgrades.Gun2),
            new Upgrade(UpgradePositions[1], Upgrades.Gun3),
            new Upgrade(UpgradePositions[1], Upgrades.SuperGun),
            new Upgrade(UpgradePositions[1], Upgrades.SheriffBadge)
        ),
        ImmutableArray.Create(
            new Upgrade(UpgradePositions[2], Upgrades.Ammo1),
            new Upgrade(UpgradePositions[2], Upgrades.Ammo2),
            new Upgrade(UpgradePositions[2], Upgrades.Ammo3),
            new Upgrade(UpgradePositions[2], Upgrades.SheriffBadge)
        )
    ];
    
    private float _scale;
    private SpriteFont? _font;
    public int SpriteNum { get; set; }
    public Vector2 Coords { get; set; } = new(x, y);
    public GameElements Body { private get; set; } = GameElements.Trader1;

    // private TraderState _state = TraderState.OutOfBounds;
    public ITraderState TraderAction { private get; set; } = new OutOfBounds();

    public void LoadContent(SpriteFont sf) {
        _font = sf;
        _scale = 20f / _font.MeasureString("Sample text").Y;
    }

    public void Update(Player player, GameTime gt) => TraderAction.DoAction(this, player , gt);

    /// <summary>
    /// Checking if the player has picket up one of the upgrades
    /// </summary>
    /// <param name="player">Instance of player in the current game</param>
    /// <param name="level">Instance of current level</param>
    public void PickUpUpgrade(Player player, Level level) {
        if (TraderAction is not Still)
            return;

        RectangleF playerHitBox = player.HitBox;
        for (int i = 0; i < ItemCount; ++i) {
            Upgrade item = _shop[i][_upgradeLevels[i]];
            if (
                item is not GameObject itemObj ||
                !itemObj.IsColliding(playerHitBox.X, playerHitBox.Y, playerHitBox.Width, playerHitBox.Height) ||
                player.Money - item.Price < 0
            ) continue;

            player.Money -= item.Price;
            item.PickUp(player, level);
            TraderAction = new MovingUp();
            Body = Sprites[SpriteNum];
                
            if (_upgradeLevels[i] < _shop[i].Length - 1) {
                ++_upgradeLevels[i];
                _upgradeIcons[i] = item.Type.ToGameElement();
            }
            return;
        }
    }

    /// <summary>
    /// Hides the trader from the level.
    /// </summary>
    public void Hide() {
        TraderAction = new OutOfBounds();
        Coords = new Vector2(Coords.X, -32);
    }
    
    public override void Draw(SpriteBatch sb) {
        if (_font is null)
            throw new NullReferenceException($"{GetType().Name} wasn't initialized");

        const int offset = 5;
        Vector2 iconPos = new(Consts.LevelWidth + offset, Consts.LevelWidth - Consts.ObjectSize);
        for (int i = _upgradeIcons.Length - 1; i >= 0; i--) {
            GameElements? icon = _upgradeIcons[i];
            if (icon != null) {
                TextureManager.DrawObject(icon.Value, iconPos.X, iconPos.Y, sb);
            }
            iconPos.Y -= Consts.ObjectSize + offset;
        }
        
        
        if (TraderAction is Still) {
            TextureManager.DrawObject(GameElements.TraderFrame, FramePos.X, FramePos.Y, sb);
            for (int i = 0; i < ItemCount; ++i) {
                Upgrade item = _shop[i][_upgradeLevels[i]];
                item.Draw(sb);

                int textWidth = (int)(_scale * _font.MeasureString(item.Price.ToString()).X);
                
                // centering text
                Vector2 pos = UpgradePositions[i] + new Vector2(
                    Consts.ObjectSize / 2f - textWidth / 2f,
                    Consts.ObjectSize
                );
                
                // bold effect
                sb.DrawString(_font, item.Price.ToString(), pos, Color.Black, 0f,
                    Vector2.Zero, _scale * 1.1f, SpriteEffects.None, 0f);
                sb.DrawString(_font, item.Price.ToString(), pos + new Vector2(1,0), Color.Black, 0f,
                      Vector2.Zero, _scale * 1.1f, SpriteEffects.None, 0f);
            }
        }
        TextureManager.DrawObject(Body, Coords.X, Coords.Y, sb);
    }

    /// <summary>
    /// Resets the state of the trader when the game is ends.
    /// </summary>
    public void OnGameOver() {
        _upgradeIcons = [null, null, null];
        _upgradeLevels = [0, 0, 0];
    }
}

/// <summary>
/// Interface representing the state of the trader and what the trader can do in that state.
/// </summary>
public interface ITraderState {
    protected const float MoveInterval = 20f;
    protected const float AnimationInterval = 125f;
    
    /// <summary>
    /// Static method that changes the sprite of the trader.
    /// </summary>
    /// <param name="trader"></param>
    static void DoMoveAnimation(Trader trader) {
        trader.SpriteNum = (trader.SpriteNum + 1) % Trader.Sprites.Length;
        trader.Body = Trader.Sprites[trader.SpriteNum];
    }

    /// <summary>
    /// Method that defines the action of the trader in the current state.
    /// </summary>
    /// <param name="trader">Instance of the trader</param>
    /// <param name="player">Instance of the player in current game</param>
    /// <param name="gt">Instance of the current game time</param>
    void DoAction(Trader trader, Player player, GameTime gt);
}

/// <summary>
/// Represents the state of the trader when he out of the area of the level. In this state the trader does nothing.
/// </summary>
public class OutOfBounds : ITraderState { 
    public void DoAction(Trader trader, Player player, GameTime gt) { }
}

/// <summary>
/// Represents the state of the trader when he is still and waiting for the player to buy an upgrade.
/// </summary>
public class Still : ITraderState {
    public void DoAction(Trader trader, Player player, GameTime gt) { 
        (int traderXIndex, int _) = Level.GetIndexes(trader);
        
        if (player.XIndex == traderXIndex) {
            trader.Body = GameElements.TraderIdle;
        } else if (player.XIndex < traderXIndex) {
            trader.Body = GameElements.TraderIdleLeft;
        } else if (player.XIndex > traderXIndex) {
            trader.Body = GameElements.TraderIdleRight;
        }
    }
}

/// <summary>
/// Represents the state of the trader when he is moving down towards the middle of the level.
/// When the trader reaches the middle of the level he changes his state to <see cref="Still"/>.
/// </summary>
public class MovingDown : ITraderState {
    private float _moveTimer;
    private float _animationTimer;
    
    public void DoAction(Trader trader, Player player, GameTime gt) {
        _moveTimer += gt.ElapsedGameTime.Milliseconds;
        if (_moveTimer < ITraderState.MoveInterval)
            return;
        
        _moveTimer = 0;
        if (trader.Coords.Y < 5 * Consts.ObjectSize) {
            trader.Coords += new Vector2(0, Trader.MovingSpeed);

            _animationTimer += gt.ElapsedGameTime.Milliseconds;
            if (_animationTimer >= ITraderState.AnimationInterval) {
                _animationTimer = 0;
                ITraderState.DoMoveAnimation(trader);
            }
                
        } else if (trader.Coords.Y.IsEqualTo(5 * Consts.ObjectSize)) {
            trader.TraderAction = new Still();
            trader.Body = GameElements.TraderIdle;
        }
    }
}

/// <summary>
/// Represents the state of the trader when he is moving up towards the top of the level.
/// When he goes out of the level he changes his state to <see cref="OutOfBounds"/>.
/// </summary>
public class MovingUp : ITraderState {
    private float _moveTimer;
    private float _animationTimer;
    
    public void DoAction(Trader trader, Player player, GameTime gt) {
        _moveTimer += gt.ElapsedGameTime.Milliseconds;
        if (_moveTimer < ITraderState.MoveInterval)
            return;
        
        _moveTimer = 0;
        if (trader.Coords.Y > -Consts.ObjectSize) {
            trader.Coords -= new Vector2(0, Trader.MovingSpeed);
                
            _animationTimer += gt.ElapsedGameTime.Milliseconds;
            if (_animationTimer >= ITraderState.AnimationInterval) {
                _animationTimer = 0;
                ITraderState.DoMoveAnimation(trader);
            }
            
        } else if (trader.Coords.Y.IsEqualTo(-Consts.ObjectSize)) {
            trader.TraderAction = new OutOfBounds();
        }
    }
}