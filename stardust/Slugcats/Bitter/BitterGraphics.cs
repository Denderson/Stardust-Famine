using RWCustom;
using SlugBase.DataTypes;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Diagnostics;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace Stardust.Slugcats.Bitter
{
    public static class BitterGraphics
    {

        public static bool PlayerGraphics_MuddableSprite(On.PlayerGraphics.orig_MuddableSprite orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, int sprite)
        {
            if (self.TryGetBit(out var data))
            {
                return orig(self, sLeaser, sprite) || sprite >= data.startSprite && sprite < data.endSprite;
            }
            else
            {
                return orig(self, sLeaser, sprite);
            }
        }

        public static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self.TryGetBit(out var data))
            {
                for (int r = 0; r < data.Rows; r++)
                {
                    for (int c = 0; c < data.Columns; c++)
                    {
                        sLeaser.sprites[data.SpikeSprite(r, c, true)].color = data.SpikeColor;
                    }
                }
            }
        }

        public static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.TryGetBit(out var data) && self.player.room != null)
            {
                for (int i = data.startSprite; i < data.spikeSpritesEnd; i++)
                {
                    sLeaser.sprites[i].isVisible = data.hasScutes;
                    if (!data.hasScutes && data.scuteGrowthProg > 0.025f) sLeaser.sprites[i].isVisible = true;
                }

                if (!data.hasScutes)
                {
                    sLeaser.sprites[0].scaleX *= Mathf.Lerp(0.76f, 1f, data.scuteGrowthProg);
                    sLeaser.sprites[1].scaleX *= Mathf.Lerp(0.76f, 1f, data.scuteGrowthProg);
                    sLeaser.sprites[3].scaleX *= Mathf.Lerp(0.85f, 1f, data.scuteGrowthProg);
                }

                bool _side = false;
                bool _back = false;

                var _rot = sLeaser.sprites[0].rotation;
                if (Mathf.Abs(_rot) is >= 18f and < 150f) _side = true;
                if (Mathf.Abs(_rot) >= 150f) _back = true;

                if (data.ForceRotatingSpriteUpright())
                {
                    _side = false;
                    _back = false;
                }

                if (_side || _back)
                    data.OrderAllBodySpikes(3, _side, sLeaser, false);
                else
                    data.OrderAllBodySpikes(0, _side, sLeaser, true);

                if (!_back && !_side || _side && self.player.standing)
                    sLeaser.sprites[data.SpikeSprite(3, 1)].isVisible = false;

                bool sideTail = false;
                bool backTail = false;

                var rotTail = Custom.AimFromOneVectorToAnother(self.tail[0].pos, self.tail[3].pos);

                if (self.player.gravity == 0f || !self.player.Consious)
                {
                    rotTail = Custom.VecToDeg(Custom.DegToVec(rotTail - _rot));
                }

                if (Mathf.Abs(rotTail) is >= 32.5f and < 150f)
                    sideTail = true;
                else if (Mathf.Abs(rotTail) < 32.5f)
                    backTail = true;

                bool tailIsbehindBody = self.player.sleepCurlUp <= 0.2f;

                if (tailIsbehindBody)
                {
                    sLeaser.sprites[2].MoveToBack();
                }
                else
                {
                    sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[9]);
                    sideTail = false;
                    backTail = true;
                }

                data.OrderAllTailSpikes(2, sideTail, sLeaser, !backTail && !sideTail);

                if (sideTail) sLeaser.sprites[data.SpikeSprite(7, 1)].isVisible = false;
                if (!backTail && !sideTail)
                {
                    sLeaser.sprites[data.SpikeSprite(5, 1)].isVisible = false;
                    sLeaser.sprites[data.SpikeSprite(6, 1)].isVisible = false;
                }

                for (int r = 0; r < data.Rows; r++)
                {
                    bool flipped = false;
                    bool side = false;

                    if (r < 4)
                    {
                        var rot = sLeaser.sprites[0].rotation;
                        if (rot < 0) flipped = true;
                        if (Mathf.Abs(rot) is >= 18f and < 150f) side = true;
                        if (!self.player.Consious || self.player.room.gravity <= 0f)
                            side = false;
                    }
                    else
                    {
                        var rot = Custom.AimFromOneVectorToAnother(self.tail[0].pos, self.tail[3].pos);

                        if (self.player.gravity == 0f || !self.player.Consious)
                            rot = Custom.VecToDeg(Custom.DegToVec(rot - sLeaser.sprites[0].rotation));

                        if (rot < 0) flipped = true;
                        if (Mathf.Abs(rot) is >= 32.5f and < 150f)
                            side = true;
                        if (!tailIsbehindBody)
                            side = false;
                    }

                    for (int c = 0; c < data.Columns; c++)
                    {
                        var sprite = sLeaser.sprites[data.SpikeSprite(r, c)];

                        var offset = data.ColumnOffsetFac(c, flipped, side);
                        var spine = data.SpinePosition(r, c);

                        if (r < 4)
                        {
                            var hips = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
                            var body = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);

                            Vector2 spineDir = Custom.DirVec(hips, body);
                            Vector2 sideways = Custom.PerpendicularVector(spineDir);
                            var initPoint = Vector2.Lerp(body, hips, spine);

                            if (Mathf.Abs(Custom.VecToDeg(spineDir)) < 18f ||
                                !self.player.Consious || self.player.room.gravity <= 0f) offset *= 0.6f;

                            var finalPos = initPoint + sideways * (offset * 5f);
                            sprite.SetPosition(finalPos - camPos);

                            data.RotateSpikeSprite(c, side, flipped, Custom.DirVec(body, hips), sprite);
                        }
                        else
                        {
                            if (r == 6 && !side && !backTail) spine += 0.075f;

                            var spineData = self.SpinePosition(spine, timeStacker);
                            var finalPos = spineData.pos + spineData.perp * (spineData.rad * (offset * 0.8f));
                            sprite.SetPosition(finalPos - camPos);
                            sprite.scale = Mathf.Lerp(0.7f, 0.35f, spine);

                            data.RotateSpikeSprite(c, side, flipped, spineData.dir, sprite, true);
                        }

                        if (!data.hasScutes && data.scuteGrowthProg > 0f)
                            sprite.scale *= data.scuteGrowthProg;
                        if (self.RenderAsPup) sprite.scale *= 0.65f;
                        else if (data.hasScutes && data.scuteDropCounter > 0)
                            sprite.rotation += Random.Range(-1f, 1f) * (data.scuteDropCounter / 80) * 10f;

                        var tip = sLeaser.sprites[data.SpikeSprite(r, c, true)];
                        tip.Follow(sprite);
                        tip.MoveInFrontOfOtherNode(sprite);

                        if (self.RenderAsPup) tip.isVisible = false;
                    }
                }
            }
        }

        public static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);
            if (self.TryGetBit(out var data) && data.graphicsInit)
            {
                newContatiner ??= rCam.ReturnFContainer("Midground");
                sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[0]);
                for (int i = data.startSprite; i < data.endSprite; i++)
                {
                    newContatiner.AddChild(sLeaser.sprites[i]);
                }
            }
        }

        public static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (self.isBitter()) self.player.GetBitData().graphicsInit = false;
            orig(self, sLeaser, rCam);
            if (self.TryGetBit(out var data))
            {
                data.graphicsInit = true;
                data.SpikeColor = PlayerColor.GetCustomColor(self, "Spikes");

                data.startSprite = sLeaser.sprites.Length;

                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + data.Rows * data.Columns);
                data.spikeSpritesEnd = sLeaser.sprites.Length;
                data.spikeTipStart = sLeaser.sprites.Length;

                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + data.Rows * data.Columns);
                data.spikeTipEnd = sLeaser.sprites.Length;

                data.endSprite = sLeaser.sprites.Length;

                for (int i = data.startSprite; i < data.spikeSpritesEnd; i++)
                {
                    sLeaser.sprites[i] = new FSprite("atlases/BitterSpike");
                    sLeaser.sprites[i].scale = 0.7f;
                    sLeaser.sprites[i].anchorY = 0.25f;
                }
                for (int i = data.spikeTipStart; i < data.spikeTipEnd; i++)
                {
                    sLeaser.sprites[i] = new FSprite("atlases/BitterSpikeEnd");
                }

                self.AddToContainer(sLeaser, rCam, null);
            }
        }
    }
}