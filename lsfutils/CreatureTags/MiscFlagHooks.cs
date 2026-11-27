using HUD;
using lsfUtils.CWTs;
using lsfUtils.DevtoolsObjects.ConditionalFilter;
using lsfUtils.Items.Spears.TorchSpears;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Watcher;

namespace lsfUtils.CreatureTags;

public static class MiscFlagHooks
{
    public static void ApplyHooks()
    {
        On.AbstractCreature.IsVoided += AbstractCreature_IsVoided;
        On.HazerGraphics.ApplyPalette += HazerGraphics_ApplyPalette;

        //all the stupid box worm hooks
        On.Watcher.BoxWorm.PerformAttack += BoxWorm_PerformAttack;
        On.Watcher.BoxWormGraphics.ApplyPalette += BoxWormGraphics_ApplyPalette;
        On.Watcher.BoxWorm.ShortCutColor += BoxWorm_ShortCutColor;
        On.Watcher.BoxWormGraphics.Data.UpdateHeadRotation += Data_UpdateHeadRotation;
        On.Watcher.FireSprite.Explosion += FireSprite_Explosion;
        On.Watcher.FireSprite.ShortCutColor += FireSprite_ShortCutColor;
        On.Watcher.FireSprite.Update += FireSprite_Update;
        On.Watcher.FireSpriteGraphics.ApplyPalette += FireSpriteGraphics_InitiateSprites;
        On.CreatureSymbol.SymbolDataFromCreature += CreatureSymbol_SymbolDataFromCreature;
        On.IconSymbol.Draw += IconSymbol_Draw;
    }

    public static bool AbstractCreature_IsVoided(On.AbstractCreature.orig_IsVoided orig, AbstractCreature self)
    {
        if (self.voidCreature && self.creatureTemplate.type == CreatureTemplate.Type.Hazer) return true;
        else return orig(self);
    }

    private static void HazerGraphics_ApplyPalette(On.HazerGraphics.orig_ApplyPalette orig, HazerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self?.owner is Creature creature && creature.abstractCreature.IsVoided())
        {
            HSLColor saturatedGoldHSL = new HSLColor(0.0944f, 1f, 0.68f);
            HSLColor secondaryColor = new HSLColor(saturatedGoldHSL.hue, 0.7f, 0.38f);
            //please dont kill me for the variable names im just copying the vanilla code
            float num = Custom.SCurve(Mathf.InverseLerp(0.1f, 1.1f, self.camo), 0.8f);
            Color color = Color.Lerp(Custom.HSL2RGB(saturatedGoldHSL.hue, saturatedGoldHSL.saturation * (1f - 0.4f * self.deadColor), saturatedGoldHSL.lightness * (1f - 0.3f * self.deadColor)), palette.fogColor, 0.2f + 0.1f * self.deadColor);
            Color color2 = Color.Lerp(Custom.HSL2RGB(secondaryColor.hue, secondaryColor.saturation * (1f - 0.2f * self.deadColor), secondaryColor.lightness * (1f - 0.5f * self.deadColor)), palette.blackColor, 0.05f + 0.4f * Mathf.Max(self.deadColor * 0.5f, Mathf.Pow(num, 2f)));
            color = Color.Lerp(color, palette.blackColor, 0.2f * self.deadColor);
            color2 = Color.Lerp(color2, self.camoPickupColor, num * 0.5f);
            sLeaser.sprites[self.MeshSprite].color = color;
            sLeaser.sprites[self.BodySprite].color = color;
            Color b = Color.Lerp(color2, palette.blackColor, 0.1f + 0.3f * Mathf.Max(self.deadColor, num));

            for (int i = 0; i < self.tentacles.Count; i++)
            {
                for (int j = 0; j < (sLeaser.sprites[self.TentacleSprite(i)] as TriangleMesh).verticeColors.Length; j++)
                {
                    (sLeaser.sprites[self.TentacleSprite(i)] as TriangleMesh).verticeColors[j] = Color.Lerp(color, b, Mathf.InverseLerp(1f, (float)((sLeaser.sprites[self.TentacleSprite(i)] as TriangleMesh).verticeColors.Length - 1), (float)j));
                }
            }
            sLeaser.sprites[self.ClosedEyeSprite].color = Color.Lerp(Color.Lerp(color, color2, 0.8f), palette.blackColor, 0.3f);
            color = Color.Lerp(Color.Lerp(color, palette.blackColor, 0.7f - 0.5f * self.deadColor), color2, 0.6f + 0.4f * num);
            for (int k = 0; k < self.scales.Length; k++)
            {
                sLeaser.sprites[self.ScaleSprite(k)].color = color;
            }
        }
    }
    private static void BoxWorm_PerformAttack(On.Watcher.BoxWorm.orig_PerformAttack orig, Watcher.BoxWorm self)
    {
        if (!CreatureCWT.TryGetData(self, out var data))
        {
            orig(self);
            return;
        }
        if (self.abstractCreature.superSizeMe)
        {
            Plugin.Log.LogMessage("fire box worm");
            if (self.room == null)
            {
                if (data.boxWormFlameJet != null)
                {
                    data.boxWormFlameJet.room.RemoveObject(data.boxWormFlameJet);
                    data.boxWormFlameJet = null;
                }
                return;
            }
            self.AttackStance();
            if (self.attackTimer.isFinished && self.transitionTimer.isFinished)
            {
                self.attackTimer.Reset();
            }
            if (!self.transitionTimer.isFinished)
            {
                self.attackTimer.Reset();
            }
            if (self.steamAvailable.normalized > (self.safariControlled ? 0.4f : 0.8f))
            {
                self.steamCoolDown = false;
            }
            if (self.steamAvailable.normalized < (self.safariControlled ? 0.2f : 0.4f) || self.steamCoolDown)
            {
                self.steamCoolDown = true;
                return;
            }
            if (self.DoSteam && self.Consious && !self.dead)
            {
                Vector2 normalized = (self.mainBodyChunk.pos - self.bodyChunks[1].pos).normalized;
                Vector2 vector = normalized * (self.ai.TargetPosition - self.mainBodyChunk.pos).magnitude;
                vector = Vector2.ClampMagnitude(vector, self.attackRange);
                vector *= 0.175f;
                if (data.boxWormFlameJet == null)
                {
                    Plugin.Log.LogMessage("initialising flame jet");
                    FlameJet.FlameJetData flameJetData = new FlameJet.FlameJetData(null);
                    flameJetData.temperature = 1f;
                    flameJetData.temperatureAnimOffset = 0f;
                    flameJetData.intensityAnimOffset = 0f;
                    flameJetData.linkTempToIntens = true;
                    flameJetData.pos = self.bodyChunks[0].pos;
                    data.boxWormFlameJet = new FlameJet(self.room, flameJetData);
                    self.room.AddObject(data.boxWormFlameJet);
                }
                if (data.boxWormFlameJet != null)
                {
                    Plugin.Log.LogMessage("updating flame jet");
                    data.boxWormFlameJet.pos = self.bodyChunks[0].pos + Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos) * 25;
                    data.boxWormFlameJet.target = Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos) * 170;
                    data.boxWormFlameJet.intensity = Mathf.Min(data.boxWormFlameJet.intensity + 0.04f, 1f);
                }

                self.DepleteSteam((int)(vector.magnitude * 0.15f));
                Vector2 b = vector * 0.01f;
                self.bodyChunks[0].vel -= b;
                self.bodyChunks[1].vel -= b;
                self.bodyChunks[2].vel -= b;
                self.bodyChunks[3].vel -= b;
            }
            else if (data.boxWormFlameJet != null)
            {
                data.boxWormFlameJet.intensity = Mathf.Max(data.boxWormFlameJet.intensity - 0.04f, 0);
                if (data.boxWormFlameJet.intensity <= 0.1 || !self.Consious || self.dead)
                {
                    self.room.RemoveObject(data.boxWormFlameJet);
                    data.boxWormFlameJet = null;
                }
            }
        }
        else orig(self);
    }
    private static void BoxWormGraphics_ApplyPalette(On.Watcher.BoxWormGraphics.orig_ApplyPalette orig, BoxWormGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);

        if (!RegionCWT.TryGetCustomRegionParams(rCam.room?.abstractRoom?.world?.region, out var customRegionParams)) return;

        if (self?.owner is BoxWorm box && box?.abstractCreature != null && box.abstractCreature.superSizeMe)
        {   
            Color color = Color.Lerp(palette.blackColor + palette.skyColor * 0.3f, customRegionParams.FireBoxWormColor, 0.25f);
            Color b = customRegionParams.FireBoxWormColor * 0.1f;

            for (int i = 0; i < self.data.numberOfBodyBlobs; i++)
            {
                float num = (i % self.data.blobsPerChunk == self.data.bandPosition) ? 0.3f : 0f;
                sLeaser.sprites[i].color = Color.Lerp(color, palette.blackColor, (float)i / (float)self.data.numberOfBodyBlobs + num);
            }

            for (int j = 0; j < self.data.numberOfLarvaHolders; j++)
            {
                Color color2 = Color.Lerp(color, palette.blackColor, (float)self.data.larvaHolderSpritePositions[j] / (float)self.data.numberOfBodyBlobs);
                if (j < self.data.maxNumberOfLarva)
                {
                    int num2 = j + self.data.numberOfBodyBlobs + self.data.numberOfLarvaHolders * 2;
                    sLeaser.sprites[num2].color = new Color(0f, color2.maxColorComponent, 0f);
                }
                sLeaser.sprites[j + self.data.numberOfBodyBlobs].color = color2;
                sLeaser.sprites[j + self.data.numberOfBodyBlobs + self.data.numberOfLarvaHolders].color = color2 + b;
            }
            int num3 = self.data.numberOfBodyBlobs + self.data.numberOfLarvaHolders * 2 + self.data.maxNumberOfLarva;
            sLeaser.sprites[num3].color = color;
            sLeaser.sprites[num3 + 1].color = color;
        }
    }
    private static Color BoxWorm_ShortCutColor(On.Watcher.BoxWorm.orig_ShortCutColor orig, BoxWorm self)
    {
        if (!RegionCWT.TryGetCustomRegionParams(self?.abstractCreature?.world?.region, out var customRegionParams)) return orig(self);
        if (self.abstractCreature.superSizeMe) return customRegionParams.FireBoxWormColor;
        return orig(self);
    }
    private static void Data_UpdateHeadRotation(On.Watcher.BoxWormGraphics.Data.orig_UpdateHeadRotation orig, BoxWormGraphics.Data self)
    {
        if (self?.ThisCreature?.abstractCreature != null && self.ThisCreature.abstractCreature.superSizeMe && RegionCWT.TryGetCustomRegionParams(self.ThisCreature.abstractCreature.Room?.world?.region, out var customRegionParams))
        {
            self.lastHeadRotOffset = self.headRotOffset;
            if (!self.ThisCreature.CanMove)
            {
                self.startSpin = false;
                self.headRotOffset = 0f;
            }
            if (self.ThisCreature.CanMove && !self.startSpin)
            {
                self.activationTimer.Reset();
                self.startSpin = true;
            }
            if (!self.activationTimer.isFinished)
            {
                if (self.activationTimer > self.activationTimer.max - 2)
                {
                    Vector2 a = self.ThisCreature.bodyChunks[0].pos - self.ThisCreature.bodyChunks[1].pos;
                    self.room.PlaySound(SoundID.SS_AI_Marble_Hit_Floor, self.ThisCreature.mainBodyChunk, false, UnityEngine.Random.Range(0.7f, 1f), UnityEngine.Random.Range(0.1f, 0.3f));
                    self.room.PlaySound(SoundID.SS_AI_Marble_Hit_Floor, self.ThisCreature.mainBodyChunk, false, UnityEngine.Random.Range(0.7f, 1f), UnityEngine.Random.Range(0.1f, 0.3f));
                    self.room.PlaySound(SoundID.SS_AI_Marble_Hit_Floor, self.ThisCreature.mainBodyChunk, false, UnityEngine.Random.Range(0.7f, 1f), UnityEngine.Random.Range(0.1f, 0.3f));
                    self.room.AddObject(new Spark(self.ThisCreature.bodyChunks[0].pos + a * 0.8f + Custom.RNV() * 4f, a.normalized * 14f + Custom.RNV() * 4f, customRegionParams.FireBoxWormColor, null, 8, 36));
                    self.room.AddObject(new Spark(self.ThisCreature.bodyChunks[0].pos + a * 0.8f + Custom.RNV() * 4f, a.normalized * 10f + Custom.RNV() * 4f, customRegionParams.FireBoxWormColor, null, 12, 26));
                    self.room.AddObject(new Spark(self.ThisCreature.bodyChunks[0].pos + a * 0.8f + Custom.RNV() * 4f, a.normalized * 8f + Custom.RNV() * 4f, customRegionParams.FireBoxWormColor, null, 22, 46));
                }
                self.headRotOffset += Mathf.Pow(self.activationTimer.normalized, 2f) * 20f;
                self.activationTimer.Tick();
            }
            self.HeadRotationCorrection();
            if (!self.activationTimer.isFinished)
            {
                return;
            }
            float b = Custom.AimFromOneVectorToAnother(self.blobPositions[0], self.blobPositions[1]);
            self.lastHeadRotation = self.headRot;
            self.headRot = Mathf.LerpAngle(self.headRot, b, 0.07f);
        }
        else orig(self);
    }
    private static void FireSprite_Explosion(On.Watcher.FireSprite.orig_Explosion orig, FireSprite self)
    {
        if (!RegionCWT.TryGetCustomRegionParams(self?.room?.world?.region, out var customRegionParams))
        {
            orig(self);
            return;
        }
        if (self.abstractCreature.superSizeMe)
        {
            if (!self.room.ViewedByAnyCamera(self.mainBodyChunk.pos, 400f))
            {
                return;
            }
            self.room.AddObject(new ShockWave(self.mainBodyChunk.pos, 120f, 0.1f, 10, false));
            self.room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Activate_Ping, self.mainBodyChunk.pos, 0.5f, UnityEngine.Random.Range(3f, 4f));
            self.room.PlaySound(WatcherEnums.WatcherSoundID.Fire_Sprite_Chirp, self.mainBodyChunk.pos, 0.5f, UnityEngine.Random.Range(1.5f, 2f));
            for (int i = 0; i < 20; i++)
            {
                self.room.AddObject(new Spark(self.mainBodyChunk.pos, Vector2.up + Custom.RNV() * (60f * UnityEngine.Random.value), Color.Lerp(customRegionParams.FireBoxWormColor, Color.white, UnityEngine.Random.value), null, 30, 60));
            }
        }
        else orig(self);
    }
    private static Color FireSprite_ShortCutColor(On.Watcher.FireSprite.orig_ShortCutColor orig, FireSprite self)
    {
        if (!RegionCWT.TryGetCustomRegionParams(self.abstractCreature.world.region, out var customRegionParams)) return orig(self);
        if (self.abstractCreature.superSizeMe) return customRegionParams.FireBoxWormColor;
        return orig(self);
    }
    private static void FireSprite_Update(On.Watcher.FireSprite.orig_Update orig, FireSprite self, bool eu)
    {
        orig(self, eu);
        if (!RegionCWT.TryGetCustomRegionParams(self?.abstractCreature?.world.region, out var customRegionParams)) return;
        if (self.abstractCreature.superSizeMe) self.lightSource.color = customRegionParams.FireBoxWormColor;
    }
    private static void FireSpriteGraphics_InitiateSprites(On.Watcher.FireSpriteGraphics.orig_ApplyPalette orig, FireSpriteGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if ((self.owner as FireSprite).abstractCreature.superSizeMe && RegionCWT.TryGetCustomRegionParams((self.owner as FireSprite).abstractCreature.world.region, out var customRegionParams))
        {
            for (int m = 0; m > sLeaser.sprites.Length; m++)
            {
                sLeaser.sprites[m].color = customRegionParams.FireBoxWormColor;
                if (m == 0)
                {
                    sLeaser.sprites[m].shader = Custom.rainWorld.Shaders["AltFireSpriteBody"];
                }
                else if (sLeaser.sprites[m] is TriangleMesh)
                {
                    sLeaser.sprites[m].shader = Custom.rainWorld.Shaders["AltFireSpriteWing"];
                }
            }
        }
    }
    public static IconSymbol.IconSymbolData CreatureSymbol_SymbolDataFromCreature(On.CreatureSymbol.orig_SymbolDataFromCreature orig, AbstractCreature creature)
    {
        IconSymbol.IconSymbolData value = orig(creature);
        if (creature.creatureTemplate.type != WatcherEnums.CreatureTemplateType.BoxWorm && creature.creatureTemplate.type != WatcherEnums.CreatureTemplateType.FireSprite) return value;
        if (!RegionCWT.TryGetCustomRegionParams(creature.world.region, out var customRegionParams)) return value;
        if (!creature.superSizeMe) return value;
        int colorNumber = (int)(customRegionParams.FireBoxWormColor.r * 255) * 1000000 + (int)(customRegionParams.FireBoxWormColor.g * 255) * 1000 + (int)(customRegionParams.FireBoxWormColor.b * 255) + 1000000000;
        Plugin.Log.LogMessage((int)(customRegionParams.FireBoxWormColor.r * 255));
        Plugin.Log.LogMessage((int)(customRegionParams.FireBoxWormColor.g * 255));
        Plugin.Log.LogMessage((int)(customRegionParams.FireBoxWormColor.b * 255));
        Plugin.Log.LogMessage(colorNumber);
        return new IconSymbol.IconSymbolData(creature.creatureTemplate.type, value.itemType, colorNumber);
    }
    private static void IconSymbol_Draw(On.IconSymbol.orig_Draw orig, IconSymbol self, float timeStacker, Vector2 drawPos)
    {
        orig(self, timeStacker, drawPos);
        if (self.iconData.intData >= 1000000000 && (self.iconData.critType == WatcherEnums.CreatureTemplateType.BoxWorm || self.iconData.critType == WatcherEnums.CreatureTemplateType.FireSprite))
        {
            float f = Mathf.Lerp(self.lastShowFlash, self.showFlash, timeStacker);
            int redColor = (self.iconData.intData / 1000000) % 1000;
            int greenColor = (self.iconData.intData / 1000) % 1000;
            int blueColor = self.iconData.intData % 1000;

            self.symbolSprite?.color = Color.Lerp(new(redColor / 255f, greenColor / 255f, blueColor / 255f), Color.white, Mathf.Pow(f, 3f));
        }
    }
}
