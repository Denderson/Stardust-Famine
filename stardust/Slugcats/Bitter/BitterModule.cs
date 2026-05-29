using Mono.Cecil.Metadata;
using RWCustom;
using SlugBase.DataTypes;
using System.Collections;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using System.Xml;
using UnityEngine;

namespace Stardust.Slugcats.Bitter
{
    public static class BitterModule
    {
        private static readonly ConditionalWeakTable<Player, BitterData> BitterCWT = new ConditionalWeakTable<Player, BitterData>();

        public static bool isBitter(this Player pl)
        {
            return pl.SlugCatClass == Enums.SlugcatStatsName.bitter;
        }

        public static bool BitterStory(this RainWorldGame game, bool excludeExpedition = true)
        {
            if (excludeExpedition) return game.IsStorySession && game.StoryCharacter != null && game.StoryCharacter == Enums.SlugcatStatsName.bitter && !(ModManager.Expedition && game.rainWorld.ExpeditionMode);
            else return game.IsStorySession && game.StoryCharacter != null && game.StoryCharacter == Enums.SlugcatStatsName.bitter;
        }

        public static bool isBitter(this PlayerGraphics pl) => pl.player.isBitter();
        public static BitterData GetBitData(this Player player)
        {
            return BitterCWT.GetValue(player, (_) => new BitterData(player));
        }

        public static bool TryGetBit(this Player player, out BitterData data)
        {
            bool flag = player.isBitter();
            bool result;
            if (flag)
            {
                data = player.GetBitData();
                result = true;
            }
            else
            {
                data = null;
                result = false;
            }
            return result;
        }

        public static bool TryGetBit(this PlayerGraphics player, out BitterData data)
        {
            bool flag = player.player.isBitter();
            bool result;
            if (flag)
            {
                data = player.player.GetBitData();
                result = true;
            }
            else
            {
                data = null;
                result = false;
            }
            return result;
        }

        public class BitterData
        {
            public BitterData(Player pl)
            {
                self = pl;
                hasScutes = true;
            }

            public Player self;
            public bool hasScutes;
            public int scuteDropCounter;
            public float scuteGrowthProg;
            public bool didAnInputAnimation;
            public Color SpikeColor;

            public int startSprite;
            public int spikeSpritesEnd;
            public int spikeTipStart;
            public int spikeTipEnd;

            public int endSprite;

            public bool graphicsInit;
            public int Rows => 8;    // rows 0–3 body, rows 4–7 tail
            public int Columns => 3; // 24 sprites total per set


            public void SetScuteProgress(float progress)
            {
                scuteGrowthProg = Mathf.Clamp(progress, 0.0f, 1.0f);
            }

            #region graphics (hell)pers
            public int SpikeSprite(int row, int column, bool overlaySpike = false)
            {// rows 0–3 body, rows 4–7 tail
             // columns 0–2
                if (!overlaySpike) return startSprite + row * Columns + column;
                else return spikeTipStart + row * Columns + column;
            }

            public float ColumnOffsetFac(int column, bool flipped, bool side)
            {
                if (!side)
                {
                    float result = 0f;
                    if (column == 0) result = -1f;
                    else if (column == 1) result = 0f;
                    else result = 1f; // column 2

                    return result;
                }
                else
                {
                    // increased from 1.15 / 0.65 for more separation
                    float offset = 1.4f;
                    if (column == 0 || column == 2)
                    {
                        offset = 0.85f;
                    }
                    if (flipped) offset *= -1f;
                    return offset;
                }
            }

            public float SpinePosition(int row, int column)
            {
                // body rows 0–3: head → hips
                // tail rows 4–7: reuse order 0–3 via % 4
                var order = row % 4;
                var result = 0.01f;

                if (order == 0) result = 0.05f;
                else if (order == 1) result = 0.35f;
                else if (order == 2) result = 0.65f;
                else if (order == 3) result = 0.9f;

                if (column == 1) result += 0.15f;
                return result;
            }

            public bool OrderSpikeRow(int row, int spriteBehind, bool side, RoomCamera.SpriteLeaser sLeaser)
            {
                var leftSprite = sLeaser.sprites[SpikeSprite(row, 0)];
                var midSprite = sLeaser.sprites[SpikeSprite(row, 1)];
                var rightSprite = sLeaser.sprites[SpikeSprite(row, 2)];
                var behind = sLeaser.sprites[spriteBehind];

                if (!side)
                {
                    leftSprite.MoveBehindOtherNode(behind);
                    rightSprite.MoveBehindOtherNode(behind);
                    midSprite.MoveBehindOtherNode(behind);
                    return false;
                    //SPRITE ORDER (base to top): left, right, mid, spriteBehind
                }
                else
                {
                    midSprite.MoveBehindOtherNode(behind);
                    leftSprite.MoveBehindOtherNode(behind);
                    rightSprite.MoveBehindOtherNode(behind);
                    return true;
                    //SPRITE ORDER (base to top): mid, left, right, spriteBehind
                }
            }

            public void RotateSpikeSprite(int column, bool side, bool flipped, Vector2 rot, FSprite spike, bool isTail = false)
            {
                float dir = Custom.VecToDeg(rot);
                float modifier = 47.5f;
                if (side)
                {
                    if (isTail) spike.rotation = dir + (flipped ? modifier : -modifier);
                    else spike.rotation = dir + (flipped ? -modifier : modifier);
                }
                else
                {
                    if (column == 1)
                    {
                        spike.rotation = dir;
                    }
                    else spike.rotation = dir + (column == 0 ? -modifier : modifier);

                    if (isTail)
                    {
                        if (column == 1) spike.rotation = dir;
                        else spike.rotation = dir - (column == 0 ? -modifier : modifier);
                    }
                }
            }

            public void OrderAllBodySpikes(int spriteInFront, bool side, RoomCamera.SpriteLeaser sLeaser, bool behind = false)
            {
                if (!behind)
                {
                    var front = sLeaser.sprites[spriteInFront];
                    var leftSprite = sLeaser.sprites[SpikeSprite(0, 0)];
                    var midSprite = sLeaser.sprites[SpikeSprite(0, 1)];
                    var rightSprite = sLeaser.sprites[SpikeSprite(0, 2)];

                    if (!side)
                    {
                        midSprite.MoveInFrontOfOtherNode(front);
                        rightSprite.MoveInFrontOfOtherNode(front);
                        leftSprite.MoveInFrontOfOtherNode(front);
                        //SPRITE ORDER (base to top): spriteInFront, left, right, mid
                    }
                    else
                    {
                        rightSprite.MoveInFrontOfOtherNode(front);
                        leftSprite.MoveInFrontOfOtherNode(front);
                        midSprite.MoveInFrontOfOtherNode(front);
                        //SPRITE ORDER (base to top): spriteInFront, mid, left, right
                    }
                }

                int startBehind = SpikeSprite(0, side ? 1 : 0);
                if (behind) startBehind = spriteInFront;
                for (int r = behind ? 0 : 1; r < 4; r++)
                {
                    var uhh = OrderSpikeRow(r, startBehind, side, sLeaser);
                    startBehind = SpikeSprite(r, uhh ? 1 : 0);
                }
            }

            public void OrderAllTailSpikes(int spriteInFront, bool side, RoomCamera.SpriteLeaser sLeaser, bool behind = false)
            {
                if (!behind)
                {
                    var front = sLeaser.sprites[spriteInFront];
                    var leftSprite = sLeaser.sprites[SpikeSprite(4, 0)];
                    var midSprite = sLeaser.sprites[SpikeSprite(4, 1)];
                    var rightSprite = sLeaser.sprites[SpikeSprite(4, 2)];

                    if (!side)
                    {
                        midSprite.MoveInFrontOfOtherNode(front);
                        rightSprite.MoveInFrontOfOtherNode(front);
                        leftSprite.MoveInFrontOfOtherNode(front);
                    }
                    else
                    {
                        rightSprite.MoveInFrontOfOtherNode(front);
                        leftSprite.MoveInFrontOfOtherNode(front);
                        midSprite.MoveInFrontOfOtherNode(front);
                    }
                }

                int startBehind = SpikeSprite(4, side ? 1 : 0);
                if (behind) startBehind = spriteInFront;
                for (int r = behind ? 4 : 5; r < Rows; r++)
                {
                    var uhh = OrderSpikeRow(r, startBehind, side, sLeaser);
                    startBehind = SpikeSprite(r, uhh ? 1 : 0);
                }
            }

            public bool ForceRotatingSpriteUpright()
            {
                return self.animation == Player.AnimationIndex.ClimbOnBeam ||
                    self.bodyMode == Player.BodyModeIndex.WallClimb ||
                    self.animation == Player.AnimationIndex.BeamTip ||
                    self.animation == Player.AnimationIndex.StandOnBeam ||
                    self.room.gravity == 0f || !self.Consious ||
                    self.animation == Player.AnimationIndex.HangFromBeam ||
                    self.animation == Player.AnimationIndex.HangUnderVerticalBeam;
            }
            #endregion
        }

        public static void Follow(this FSprite sprite, FSprite follow)
        {
            sprite.SetPosition(follow.GetPosition());
            sprite.rotation = follow.rotation;
            sprite.scaleX = follow.scaleX;
            sprite.scaleY = follow.scaleY;
            sprite.isVisible = follow.isVisible;
            sprite.alpha = follow.alpha;
            sprite.anchorX = follow.anchorX;
            sprite.anchorY = follow.anchorY;
        }
    }
}