using RWCustom;
using SlugBase.DataTypes;
using Stardust.CWTs;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Diagnostics;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace Stardust.Slugcats.Bitter.BitterGraphics
{
    public static class BitterGraphics
    {

        public static bool PlayerGraphics_MuddableSprite(On.PlayerGraphics.orig_MuddableSprite orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, int sprite)
        {
            if (BitterGraphicsCWT.TryGetData(self, out var data))
                return orig(self, sLeaser, sprite) || sprite >= data.startSprite && sprite < data.endSprite;
            else
                return orig(self, sLeaser, sprite);
        }

        public static string PlayerGraphics_DefaultFaceSprite_float_int(On.PlayerGraphics.orig_DefaultFaceSprite_float_int orig, PlayerGraphics self, float eyeScale, int imgIndex)
        {
            if (self.player.IsBitter())
            {
                return self._cachedFaceSpriteNames[0, self.blink > 0 ? 1 : 0, imgIndex];
                //First parameter prevents pup eyes
                //second for eyes open or closed
                //don't mess w/ third
            }
            return orig(self, eyeScale, imgIndex);
        }

        public static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (BitterGraphicsCWT.TryGetData(self, out var data))
            {

                for (int r = 0; r < data.Rows; r++)
                {
                    /*
                    var lerp = Mathf.InverseLerp(0f, 7f, r);
                    sLeaser.sprites[data.SpikeSprite(r, 0)].color = Color.Lerp(Color.red, Color.cyan, lerp);
                    sLeaser.sprites[data.SpikeSprite(r, 1)].color = Color.Lerp(Color.green, Color.magenta, lerp);
                    sLeaser.sprites[data.SpikeSprite(r, 2)].color = Color.Lerp(Color.blue, Color.yellow, lerp);
                    */

                    for (int c = 0; c < data.Columns; c++)
                    {
                        sLeaser.sprites[data.SpikeSprite(r, c, true)].color = data.ExtrasColour;
                    }
                }

                if (!self.RenderAsPup)
                {
                    sLeaser.sprites[data.SpikeSprite(7, 0)].color = data.ExtrasColour;
                    sLeaser.sprites[data.SpikeSprite(7, 1)].color = data.ExtrasColour;
                    sLeaser.sprites[data.SpikeSprite(7, 2)].color = data.ExtrasColour;
                }
                /*
                for (int i = data.startSprite; i < data.spikeSpritesEnd; i++)
                {
                    sLeaser.sprites[i].color = new Color(Random.value, Random.value, Random.value);
                }
                */
            }
        }

        public static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (BitterGraphicsCWT.TryGetData(self, out var data) && self.player.room != null)
            {
                //head
                if (sLeaser.sprites[3].element.name.StartsWith("HeadC")) sLeaser.sprites[3].SetElementByName(sLeaser.sprites[3].element.name.Replace("HeadC", "HeadA"));
                if (!sLeaser.sprites[3].element.name.Contains("Bitter") && sLeaser.sprites[3].element.name.StartsWith("HeadA")) sLeaser.sprites[3].SetElementByName($"Bitter{sLeaser.sprites[3].element.name}");

                for (int i = data.startSprite; i < data.spikeSpritesEnd; i++)
                {
                    sLeaser.sprites[i].isVisible = data.hasScutes;

                    //scute regrowth animation
                    if (!data.hasScutes && data.scuteGrowthProg > 0.025f) sLeaser.sprites[i].isVisible = true;
                }
                //no scutes?
                if (!data.hasScutes)
                {//spearmaster proportions
                    sLeaser.sprites[0].scaleX *= Mathf.Lerp(0.76f, 1f, data.scuteGrowthProg);
                    sLeaser.sprites[1].scaleX *= Mathf.Lerp(0.76f, 1f, data.scuteGrowthProg);
                    sLeaser.sprites[3].scaleX *= Mathf.Lerp(0.85f, 1f, data.scuteGrowthProg);
                }

                //body sprites
                /*
                var offset = AttachPosUpdate(Custom.AimFromOneVectorToAnother(self.drawPositions[1, 0], self.drawPositions[0, 0]));
                Vector2 spineDir = Custom.DirVec(self.drawPositions[1, 0], self.drawPositions[0, 0]);
                Vector2 sideways = Custom.PerpendicularVector(spineDir);
                var initPoint = Vector2.Lerp(self.drawPositions[1, 0], self.drawPositions[0, 0], 0.75f);
                offset *= -1f;
                wings[0].fixedPoint = (initPoint - sideways * -offset.x);
                wings[1].fixedPoint = (initPoint + sideways * offset.y);
                */
                bool _side = false;
                bool sideTail = false;
                bool _back = false;
                bool backTail = false;


                var _rot = sLeaser.sprites[0].rotation;
                if (Mathf.Abs(_rot) is >= 18f and < 150f) _side = true;
                if (Mathf.Abs(_rot) >= 150f) _back = true;

                var rotTail = Custom.AimFromOneVectorToAnother(self.tail[0].pos, self.tail[3].pos);

                if (self.player.gravity == 0f || !self.player.Consious)
                {//mke rotTail relative to the player rotation
                    rotTail = Custom.VecToDeg(Custom.DegToVec(rotTail - _rot));
                }

                if (Mathf.Abs(rotTail) is >= 32.5f and < 150f)
                {
                    sideTail = true;
                }
                else if (Mathf.Abs(rotTail) < 32.5f) backTail = true;

                if (data.ForceRotatingSpriteUpright())
                {
                    _side = false;
                    _back = false;
                }




                if (_side || _back)
                {
                    data.OrderAllBodySpikes(3, _side, sLeaser, false);
                }
                else
                {
                    data.OrderAllBodySpikes(0, _side, sLeaser, true);
                }

                bool tailIsbehindBody = self.player.sleepCurlUp <= 0.2f;

                if (tailIsbehindBody)
                {
                    sLeaser.sprites[2].MoveToBack();
                    //UnityEngine.Debug.Log("NOT SLEEP ALIGNMENT");
                }
                else
                {
                    sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[6]);
                    sideTail = false;
                    backTail = true;
                    //UnityEngine.Debug.Log("SLEEP ALIGNMENT");
                }

                data.OrderAllTailSpikes(2, sideTail, sLeaser, !backTail && !sideTail);
                //UnityEngine.Debug.Log($"Behind: {!backTail && !sideTail}");


                if (!_back && !_side || _side && self.player.standing)
                {
                    sLeaser.sprites[data.SpikeSprite(3, 1)].isVisible = false;
                    //UnityEngine.Debug.Log("Removed end body sprite");
                }
                if (!backTail && !sideTail)
                {
                    //sLeaser.sprites[data.SpikeSprite(7, 1)].isVisible = false;
                    //UnityEngine.Debug.Log("Removed end tail sprite");
                    sLeaser.sprites[data.SpikeSprite(5, 1)].isVisible = false;
                    sLeaser.sprites[data.SpikeSprite(6, 1)].isVisible = false;
                }

                if (sideTail) sLeaser.sprites[data.SpikeSprite(7, 1)].isVisible = false;


                for (int r = 0; r < data.Rows; r++)
                {
                    bool flipped = false;
                    bool side = false;
                    if (r < 4) //are we body spikes rn?
                    {
                        var rot = sLeaser.sprites[0].rotation;
                        if (rot < 0) flipped = true;
                        if (Mathf.Abs(rot) is >= 18f and < 150f) side = true;
                        if (!self.player.Consious || self.player.room.gravity <= 0f)
                        {
                            side = false;
                        }
                    }
                    else
                    {
                        var rot = Custom.AimFromOneVectorToAnother(self.tail[0].pos, self.tail[3].pos);


                        if (self.player.gravity == 0f || !self.player.Consious)
                        {
                            //mke rotTail relative to the player rotation
                            rot = Custom.VecToDeg(Custom.DegToVec(rot - sLeaser.sprites[0].rotation));
                        }


                        if (rot < 0) flipped = true;
                        if (Mathf.Abs(rot) is >= 32.5f and < 150f)
                        {
                            side = true;
                        }
                        if (!tailIsbehindBody)
                        {
                            side = false;
                        }
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
                            if (r == 6 && !side && !backTail) spine += 0.075f;//little tweak for facing front

                            var spineData = self.SpinePosition(spine, timeStacker);
                            var finalPos = spineData.pos + spineData.perp * (spineData.rad * (offset * 0.8f));
                            sprite.SetPosition(finalPos - camPos);
                            float lerp = (r - 4) / data.Rows - 4;//avoid NaN error
                            lerp = spine;
                            if (c == 1) lerp -= 0.15f;
                            //sprite.scale = Mathf.Lerp(0.7f, 0.4f, lerp);
                            sprite.scale = Mathf.Lerp(0.7f, 0.35f, lerp);

                            data.RotateSpikeSprite(c, side, flipped, spineData.dir, sprite, true);
                        }

                        if (!data.hasScutes && data.scuteGrowthProg > 0f)
                        {
                            sprite.scale *= data.scuteGrowthProg;
                        }
                        if (self.RenderAsPup) sprite.scale *= 0.65f;
                        else if (data.hasScutes && data.scuteDropCounter > 0)
                        {
                            sprite.rotation += Random.Range(-1f, 1f) * (data.scuteDropCounter / 80) * 10f;
                        }

                        var tip = sLeaser.sprites[data.SpikeSprite(r, c, true)];
                        tip.Follow(sprite);
                        tip.MoveInFrontOfOtherNode(sprite);

                        if (self.RenderAsPup) tip.isVisible = false;

                        if (r == 0 || r == 4)
                        {
                            sprite.isVisible = false;
                            tip.isVisible = false;
                        }
                    }
                }
            }
        }

        public static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);
            if (BitterGraphicsCWT.TryGetData(self, out var data) && data.graphicsInit)
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
            if (self.IsBitter()) self.player.GetBitterData().graphicsInit = false;
            orig(self, sLeaser, rCam);
            if (BitterGraphicsCWT.TryGetData(self, out var data))
            {
                data.graphicsInit = true;
                data.ExtrasColour = PlayerColor.GetCustomColor(self, 2);//Bitter extra colour

                data.startSprite = sLeaser.sprites.Length;

                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + data.Rows * data.Columns);

                data.spikeSpritesEnd = sLeaser.sprites.Length;
                data.spikeTipStart = sLeaser.sprites.Length;

                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + data.Rows * data.Columns);//Bitter spike tips
                data.spikeTipEnd = sLeaser.sprites.Length;

                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 2);

                data.endSprite = sLeaser.sprites.Length;

                for (int i = data.startSprite; i < data.spikeSpritesEnd; i++)
                {
                    sLeaser.sprites[i] = new FSprite("BitterSpikeA");
                    sLeaser.sprites[i].scale = 0.7f;//0.35f;
                    sLeaser.sprites[i].anchorY = 0.25f;//0.35f;
                }
                for (int i = data.spikeTipStart; i < data.spikeTipEnd; i++)
                {
                    sLeaser.sprites[i] = new FSprite("BitterSpikeB");
                }

                /*TriangleMesh.Triangle[] TailTris = new TriangleMesh.Triangle[]
{
                new(0, 1, 2),
                new(1, 2, 3),
                new(4, 5, 6),
                new(5, 6, 7),
                new(8, 9, 10),
                new(9, 10, 11),
                new(12, 13, 14),
                new(2, 3, 4),
                new(3, 4, 5),
                new(6, 7, 8),
                new(7, 8, 9),
                new(10, 11, 12),
                new(11, 12, 13)
    };
                TriangleMesh tailMesh = new("Futile_White", TailTris, false, false)
                {
                    element = Futile.atlasManager.GetElementWithName("BitterTail")
                };
                for (var i = tailMesh.vertices.Length - 1; i >= 0; i--)
                {
                    var perc = i / 2 / (float)(tailMesh.vertices.Length / 2);

                    Vector2 uv;
                    if (i % 2 == 0)
                        uv = new Vector2(perc, 0f);
                    else if (i < tailMesh.vertices.Length - 1)
                        uv = new Vector2(perc, 1f);
                    else
                        uv = new Vector2(1f, 0f);

                    // Map UV values to the element
                    uv.x = Mathf.Lerp(tailMesh.element.uvBottomLeft.x, tailMesh.element.uvTopRight.x, uv.x);
                    uv.y = Mathf.Lerp(tailMesh.element.uvBottomLeft.y, tailMesh.element.uvTopRight.y, uv.y);

                    tailMesh.UVvertices[i] = uv;
                }*/

                self.AddToContainer(sLeaser, rCam, null);
            }
        }
    }
}