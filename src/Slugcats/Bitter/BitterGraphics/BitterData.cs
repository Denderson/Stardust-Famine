using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stardust.Slugcats.Bitter.BitterGraphics
{
    public class BitterData
    {
        public BitterData(Player player)
        {
            self = player;
            hasScutes = true;
        }

        public Player self;
        public bool hasScutes;
        public int scuteDropCounter;
        public float scuteGrowthProg;
        public bool didAnInputAnimation;
        public Color ExtrasColour;


        public int startSprite;
        public int spikeSpritesEnd;
        public int spikeTipStart;
        public int spikeTipEnd;
        //add tail
        public int endSprite;

        public bool graphicsInit;
        public int Rows => 8;
        public int Columns => 3;
        //24 sprites for base spikes, going from startIndex to startIndex + 23
        //first one is rows, second is columns


        public void SetScuteProgress(float progress)
        {
            scuteGrowthProg = Mathf.Clamp(progress, 0.0f, 1.0f);
        }

        #region graphics (hell)pers
        public int SpikeSprite(int row, int column, bool overlaySpike = false)
        {
            //rows go from 0 to 7
            //columns go from 0 to 2
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
                else result = 1f;//column 2

                return result;
            }
            else
            {
                float offset = 1.15f;
                if (column == 0 || column == 2)
                {
                    offset = 0.65f;
                }
                if (flipped) offset *= -1f;
                return offset;
            }
        }

        public float SpinePosition(int row, int column)
        {
            //assuming base to tip for both body and tail
            //base being head and tip being hips for body sprites

            /*
            var order = row % 4;

            var result = 0.01f;
            if (order == 0) result =  0.1f;
            else if (order == 1) result =  0.35f;
            else if (order == 2) result =  0.60f;
            else if (order == 3) result = 0.85f;

            if (column == 1) result += 0.14f;//middle columns pushed down a bit
            return result;
            */

            var order = row % 4;
            var result = 0.01f;

            if (order == 0) return 0f;//smiting the first ones
            else if (order == 1) result = 0.1f;
            else if (order == 2) result = 0.45f;
            else if (order == 3) result = 0.8f;

            if (column == 1) result += 0.15f;
            return result;
            //int r = 0; r < 4; r++
        }
        public bool OrderSpikeRow(int row, int spriteBehind, bool side, RoomCamera.SpriteLeaser sLeaser)
        {//bool is for checking which sprite is at the back
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
            //move first ones in front of specified sprite
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
            if (behind) startBehind = spriteInFront;//reuse as sprite to go behind
            for (int r = behind ? 0 : 1; r < 4; r++)
            {
                var uhh = OrderSpikeRow(r, startBehind, side, sLeaser);
                startBehind = SpikeSprite(r, uhh ? 1 : 0);//start behind the last of the previous sprites
            }
        }
        public void OrderAllTailSpikes(int spriteInFront, bool side, RoomCamera.SpriteLeaser sLeaser, bool behind = false)
        {
            //move first ones in front of specified sprite

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
            if (behind) startBehind = spriteInFront;//reuse as sprite to go behind
            for (int r = behind ? 4 : 5; r < Rows; r++)
            {
                var uhh = OrderSpikeRow(r, startBehind, side, sLeaser);
                startBehind = SpikeSprite(r, uhh ? 1 : 0);//start behind the last of the previous sprites
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
}
