using System;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using JoTPK_MonogamePort.GameObjects;
using JoTPK_MonogamePort.Items;
using JoTPK_MonogamePort.Utils;
using JoTPK_MonogamePort.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace JoTPK_MonogamePort.Entities;

public class Trader : GameObject {
    public const int MovingSpeed = 2;
    
    private const int XUpgradeOffest = 6;
    private const int YUpgradeOffest = 6;
    private const int ItemCount = 3;
    private static readonly int YSpaceBetween = Consts.ObjectSize + 10;
    
    public static readonly ImmutableArray<GameElements> Sprites = ImmutableArray.Create(GameElements.Trader1, GameElements.Trader2);
    private static readonly Vector2 FramePos = new(7 * Consts.ObjectSize, 6 * Consts.ObjectSize);
    private static readonly ImmutableArray<Vector2> UpgradePositions = ImmutableArray.Create(
        FramePos + new Vector2(XUpgradeOffest, YUpgradeOffest),
        FramePos + new Vector2(XUpgradeOffest + YSpaceBetween, YUpgradeOffest),
        FramePos + new Vector2(XUpgradeOffest + YSpaceBetween * 2, YUpgradeOffest)
    );
    private static readonly GameElements?[] UpgradeIcons = { null, null, null };
    
    private readonly int[] _upgradeLevels = { 0, 0, 0 };
    private readonly ImmutableArray<ImmutableArray<Upgrade>> _shop;
    private float _scale;
    private SpriteFont _font;
    public int SpriteNum { get; set; }
    public Vector2 Pos { get; set; }
    public GameElements Body { private get; set; }

    // private TraderState _state = TraderState.OutOfBounds;
    public ITraderState TraderAction { private get; set; } = new Still();

    public Trader(float x, float y) : base(x, y) {
        Pos = new Vector2(x, y);
        Body = GameElements.Trader1;
        _shop = ImmutableArray.Create(
            ImmutableArray.Create(
                new Upgrade(UpgradePositions[0], Upgrades.Boots1, this),
                new Upgrade(UpgradePositions[0], Upgrades.Boots2, this),
                new Upgrade(UpgradePositions[0], Upgrades.HealthPoint, this)
            ),
            ImmutableArray.Create(
                new Upgrade(UpgradePositions[1], Upgrades.Gun1, this),
                new Upgrade(UpgradePositions[1], Upgrades.Gun2, this),
                new Upgrade(UpgradePositions[1], Upgrades.Gun3, this),
                new Upgrade(UpgradePositions[1], Upgrades.SuperGun, this),
                new Upgrade(UpgradePositions[1], Upgrades.SheriffBadge, this)
            ),
            ImmutableArray.Create(
                new Upgrade(UpgradePositions[2], Upgrades.Ammo1, this),
                new Upgrade(UpgradePositions[2], Upgrades.Ammo2, this),
                new Upgrade(UpgradePositions[2], Upgrades.Ammo3, this),
                new Upgrade(UpgradePositions[2], Upgrades.SheriffBadge, this)
            )
        );
    }

    public void LoadContent(SpriteFont sf) {
        _font = sf;
        _scale = 20f / _font.MeasureString("Sample text").Y;
    }

    public void Update(Level level, Player player, GameTime gt) {
        TraderAction.DoAction(this, level, player , gt);
    }

    public void PickUpUpgrade(Player player, Level level) {
        if (TraderAction is not Still)
            return;

        RectangleF hitBox = player.HitBox;
        for (int i = 0; i < ItemCount; i++) {
            Upgrade item = _shop[i][_upgradeLevels[i]];
            if (item is GameObject itemObj) {
                if (!itemObj.IsColliding(hitBox.X, hitBox.Y, hitBox.Width, hitBox.Height))
                    continue;

                if (player.Money - item.Price < 0)
                    continue;

                player.Money -= item.Price;
                item.PickUp(player, level);
                TraderAction = new MovingUp();
                Body = Sprites[SpriteNum];
                
                if (_upgradeLevels[i] < _shop[i].Length - 1) {
                    _upgradeLevels[i]++;
                    UpgradeIcons[i] = item.Type.ToGameElement();
                }

                return;
            }
        }
    }

    // onLevelSwitch
    public void Hide() {
        TraderAction = new OutOfBounds();
        Pos = new Vector2(Pos.X, -32);
    }
    
    public override void Draw(SpriteBatch sb) {
        if (_font is null)
            throw new NullReferenceException($"{GetType().Name} wasn't initialized");

        const int offset = 5;
        Vector2 iconPos = new(Consts.LevelWidth + offset, Consts.LevelWidth - Consts.ObjectSize);
        for (int i = UpgradeIcons.Length - 1; i >= 0; i--) {
            GameElements? icon = UpgradeIcons[i];
            if (icon != null) {
                TextureManager.DrawObject(icon.Value, iconPos.X, iconPos.Y, sb);
            }
            iconPos.Y -= Consts.ObjectSize + offset;
        }
        
        
        if (TraderAction is Still) {
            TextureManager.DrawObject(GameElements.TraderFrame, FramePos.X, FramePos.Y, sb);
            for (int i = 0; i < ItemCount; i++) {
                Upgrade item = _shop[i][_upgradeLevels[i]];
                item.Draw(sb);

                int textWidth = (int)(_scale * _font.MeasureString(item.Price.ToString()).X);
                
                // centering text
                Vector2 pos = UpgradePositions[i] + new Vector2(
                    Consts.ObjectSize / 2 - textWidth / 2,
                    Consts.ObjectSize
                );
                
                // bold effect
                sb.DrawString(_font, item.Price.ToString(), pos, Color.Black, 0f,
                    Vector2.Zero, _scale * 1.1f, SpriteEffects.None, 0f);
                sb.DrawString(_font, item.Price.ToString(), pos + new Vector2(1,0), Color.Black, 0f,
                      Vector2.Zero, _scale * 1.1f, SpriteEffects.None, 0f);
            };
        }
        TextureManager.DrawObject(Body, Pos.X, Pos.Y, sb);
    }
}

public interface ITraderState {
    protected const float MoveInterval = 20f;
    protected const float AnimationInterval = 125f;
    
    static void DoMoveAnimation(Trader trader) {
        trader.SpriteNum = (trader.SpriteNum + 1) % Trader.Sprites.Length;
        trader.Body = Trader.Sprites[trader.SpriteNum];
    }
    void DoAction(Trader trader, Level level, Player player, GameTime gt);
}

public class OutOfBounds : ITraderState {
    // private const float TempInterval = 5000f;
    // private float _timer;
    
    public void DoAction(Trader trader, Level level, Player player, GameTime gt) {
        // _timer += gt.ElapsedGameTime.Milliseconds;
        // if (_timer >= TempInterval) {
        //     _timer = 0;
        //     trader.TraderAction = new MovingDown();
        // }
    }
}

public class Still : ITraderState {
    public void DoAction(Trader trader, Level level, Player player, GameTime gt) {
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

public class MovingDown : ITraderState {
    private float _moveTimer;
    private float _animationTimer;
    
    public void DoAction(Trader trader, Level level, Player player, GameTime gt) {
        _moveTimer += gt.ElapsedGameTime.Milliseconds;
        if (_moveTimer < ITraderState.MoveInterval)
            return;
        
        _moveTimer = 0;
        if (trader.Pos.Y < 5 * Consts.ObjectSize) {
            trader.Pos += new Vector2(0, Trader.MovingSpeed);

            _animationTimer += gt.ElapsedGameTime.Milliseconds;
            if (_animationTimer >= ITraderState.AnimationInterval) {
                _animationTimer = 0;
                ITraderState.DoMoveAnimation(trader);
            }
                
        } else if (trader.Pos.Y.IsEqualTo(5 * Consts.ObjectSize)) {
            trader.TraderAction = new Still();
            trader.Body = GameElements.TraderIdle;
        }
    }
}


public class MovingUp : ITraderState {
    private float _moveTimer;
    private float _animationTimer;
    
    public void DoAction(Trader trader, Level level, Player player, GameTime gt) {
        _moveTimer += gt.ElapsedGameTime.Milliseconds;
        if (_moveTimer < ITraderState.MoveInterval)
            return;
        
        _moveTimer = 0;
        if (trader.Pos.Y > -Consts.ObjectSize) {
            trader.Pos -= new Vector2(0, Trader.MovingSpeed);
                
            _animationTimer += gt.ElapsedGameTime.Milliseconds;
            if (_animationTimer >= ITraderState.AnimationInterval) {
                _animationTimer = 0;
                ITraderState.DoMoveAnimation(trader);
            }
            
        } else if (trader.Pos.Y.IsEqualTo(-Consts.ObjectSize)) {
            trader.TraderAction = new OutOfBounds();
        }
    }
}