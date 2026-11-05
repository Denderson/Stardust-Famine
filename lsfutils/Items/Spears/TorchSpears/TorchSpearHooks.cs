using lsfUtils.CWTs;
using lsfUtils.Mechanics.Fire;
using RWCustom;
using System;
using UnityEngine;
using Watcher;
using static lsfUtils.Enums;

namespace lsfUtils.Items.Spears.TorchSpears;

public static class TorchSpearHooks
{
    public static void ApplyHooks()
    {
        On.Spear.HitSomething += Spear_HitSomething;
        On.Watcher.FlameJet.UpdateDamage += Watcher_FlameJet_UpdateDamage;
        On.Smoke.BlackHaze.BigBlackHaze.BlackHazeVisionObscurer.Update += Smoke_BlackHaze_Update_BigBlackHaze_BlackHazeVisionObscurer;
        On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
        On.Boomerang.HitSomething += Boomerang_HitSomething;
        On.Boomerang.ApplyPalette += Boomerang_ApplyPallete;
        On.Rock.HitSomething += Rock_HitSomething;
        On.Rock.ApplyPalette += Rock_ApplyPalette;
        On.FlareBomb.Update += FlareBomb_Update;
        On.BubbleGrass.Update += BubbleGrass_Update;
        On.PuffBall.Explode += PuffBall_Explode;
        On.SporeCloud.Update += SporeCloud_Update;
        On.SeedCob.HitByWeapon += SeedCob_HitByWeapon;
        On.MoreSlugcats.MoonCloak.Color += MoreSlugcats_MoonCloak_Color;
        On.PlayerGraphics.Gown.Color += PlayerGraphics_Gown_Color;
    }


    private static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
    {
        bool hitResult = orig(self, result, eu);

        if (self is TorchSpear torch && torch.isLit)
        {
            if (result.obj is Creature targetCreature)
            {
                if (targetCreature.SpearStick(self, self.spearDamageBonus, result.chunk, null, self.firstChunk.vel) && (targetCreature is not Lizard || (targetCreature is Lizard liz && (result.chunk != liz.bodyChunks[0] || liz.HitInMouth(self.firstChunk.vel)))))
                {
                    if (self.room != null)
                    {
                        Plugin.Log.LogMessage("burning creature");
                        self.room.AddObject(new FlameEffect(targetCreature, result.chunk, result.onAppendagePos, Math.Min(Math.Max(1500 + torch.clothLife * 100, 800), 1600 + (200 * torch.fireLife - 600))));
                        Vector2 hitPos = result.collisionPoint;
                        self.room.AddObject(new Spark(hitPos, Custom.RNV() * 5f, torch.GetTorchFireColor(), null, 10, 20));
                    }
                }
                else
                {
                    if (self.room != null)
                    {
                        Plugin.Log.LogMessage("burning creature (no stick edition)");
                        self.room.AddObject(new FlameEffect(targetCreature, result.chunk, result.onAppendagePos, (Math.Min(Math.Max(1500 + torch.clothLife * 100, 800), 1600) + (200 * torch.fireLife - 600)) / 2));
                        Vector2 hitPos = result.collisionPoint;
                        self.room.AddObject(new Spark(hitPos, Custom.RNV() * 5f, torch.GetTorchFireColor(), null, 10, 20));
                    }
                }
                targetCreature.Violence(
                    source: self.firstChunk,
                    directionAndMomentum: null,
                    hitChunk: result.chunk ?? targetCreature.mainBodyChunk,
                    hitAppendage: result.onAppendagePos,
                    type: Creature.DamageType.Explosion,
                    damage: 0.05f,
                    stunBonus: 0f
                );
            }
            else if (result.obj is PhysicalObject targetObject)
            {
                if (self.room != null)
                {
                    Plugin.Log.LogMessage("burning object");
                    self.room.AddObject(new FlameEffect(targetObject, result.chunk, result.onAppendagePos, Math.Min(Math.Max(1500 + torch.clothLife * 100, 800), 1600) + (200 * torch.fireLife - 600)));
                    Vector2 hitPos = result.collisionPoint;
                    self.room.AddObject(new Spark(hitPos, Custom.RNV() * 5f, torch.GetTorchFireColor(), null, 10, 20));
                }

            }
        }
        return hitResult;
    }


    private static void Watcher_FlameJet_UpdateDamage(On.Watcher.FlameJet.orig_UpdateDamage orig, FlameJet self)
    {
        orig(self);
        for (int i = 0; i < self.room.physicalObjects.Length; i++)
        {
            foreach (PhysicalObject physicalObject in self.room.physicalObjects[i])
            {
                if (physicalObject is TorchSpear torchSpear && (!torchSpear.isLit || torchSpear.fireLife < 3))
                {
                    Vector2 vector = torchSpear.clothPos[torchSpear.clothSegments - 1] - self.pos;
                    if (self.bounds.Vector2Inside(vector))
                    {
                        for (int j = 0; j < self.jetPos.Length - 1; j++)
                        {
                            float t = (float)(Time.frameCount % 10) * 0.1f;
                            Vector2 b = Vector2.Lerp(self.jetPos[j], self.jetPos[j + 1], t);
                            float num = Mathf.Lerp(self.jetScale[j], self.jetScale[j + 1], t);
                            float num2 = Mathf.Clamp((float)j / 3f, 0.5f, 1f);
                            num2 *= Mathf.Clamp((float)(self.jetPos.Length - 1 - j) / 5f, 0.5f, 1f);
                            float num3 = Vector2.Distance(vector, b);
                            float num4 = num * 0.5f * num2;
                            if (num3 <= num4)
                            {
                                torchSpear.Relight(1);
                            }
                        }
                    }
                }
            }
        }
    }

    private static void Smoke_BlackHaze_Update_BigBlackHaze_BlackHazeVisionObscurer(On.Smoke.BlackHaze.BigBlackHaze.BlackHazeVisionObscurer.orig_Update orig, Smoke.BlackHaze.BigBlackHaze.BlackHazeVisionObscurer self, bool eu)
    {
        orig(self, eu);
        self.obscureFac = Mathf.InverseLerp(0.1f, 0.8f, self.bigBlackHaze.alpha);
        self.rad = self.bigBlackHaze.Rad(1f) * 0.55f;
        self.pos = self.bigBlackHaze.pos;
        if (UnityEngine.Random.value < self.bigBlackHaze.alpha)
        {
            if (self.room.abstractRoom.creatures.Count > 0)
            {
                AbstractCreature abstractCreature = self.room.abstractRoom.creatures[UnityEngine.Random.Range(0, self.room.abstractRoom.creatures.Count)];
                if (CreatureCWT.TryGetData(abstractCreature.realizedCreature, out var data))
                { 
                    if (abstractCreature.realizedCreature != null && (abstractCreature.rippleLayer == self.bigBlackHaze.smoke.rippleLayer || abstractCreature.rippleBothSides) && Custom.DistLess(abstractCreature.realizedCreature.mainBodyChunk.pos, self.pos, self.rad * Mathf.Pow(self.bigBlackHaze.alpha, 0.6f) / 2f))
                    {
                        data.inHazerInk = true;
                    }
                    else data.inHazerInk = false;
                }
            }
            for (int i = 0; i < self.room.physicalObjects.Length; i++)
            {
                foreach (PhysicalObject physicalObject in self.room.physicalObjects[i])
                {
                    if (physicalObject is TorchSpear torchSpear && torchSpear.isLit && (physicalObject.abstractPhysicalObject.rippleLayer == self.bigBlackHaze.smoke.rippleLayer || physicalObject.abstractPhysicalObject.rippleBothSides) && Custom.DistLess(torchSpear.clothPos[torchSpear.clothSegments - 1], self.pos, self.rad * Mathf.Pow(self.bigBlackHaze.alpha, 0.6f) / 2f)) torchSpear.Extinguish(true);
                    else if (PhysicalObjectCWT.TryGetData(physicalObject, out var data))
                    {
                        if ((physicalObject.abstractPhysicalObject.rippleLayer == self.bigBlackHaze.smoke.rippleLayer || physicalObject.abstractPhysicalObject.rippleBothSides) && Custom.DistLess(physicalObject.firstChunk.pos, self.pos, self.rad * Mathf.Pow(self.bigBlackHaze.alpha, 0.6f) / 2f)) data.inHazerInk = true;
                        else data.inHazerInk = false;
                    }
                }
            }
        }
    }


    private static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (CreatureCWT.TryGetData(self.player, out var data) && data.permanentFireAmount > 0f)
        {
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                if (i != 9)
                {
                    sLeaser.sprites[i].color = UnityEngine.Color.Lerp((ModManager.MSC && self.player.SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Slugpup) ? self.player.ShortCutColor() : PlayerGraphics.SlugcatColor(self.CharacterForColor), new Color(0.25f, 0.11f, 0.06f), data.permanentFireAmount);
                }
            }
        }
    }

    private static bool Boomerang_HitSomething(On.Boomerang.orig_HitSomething orig, Boomerang self, SharedPhysics.CollisionResult result, bool eu)
    {
        bool hitResult = orig(self, result, eu);
        if (PhysicalObjectCWT.TryGetData(self, out var data) && data.onFire)
        {
            if (result.obj is Creature targetCreature)
            {
                if (self.room != null)
                {
                    Plugin.Log.LogMessage("burning creature");
                    self.room.AddObject(new FlameEffect(targetCreature, result.chunk, result.onAppendagePos, data.fireDuration / 2));
                    data.fireLoss = 100;
                    Vector2 hitPos = result.collisionPoint;
                    self.room.AddObject(new Spark(hitPos, Custom.RNV() * 5f, new Color(1f, 0.4f, 0.1f), null, 10, 20));
                }

                targetCreature.Violence(
                    source: self.firstChunk,
                    directionAndMomentum: null,
                    hitChunk: result.chunk ?? targetCreature.mainBodyChunk,
                    hitAppendage: result.onAppendagePos,
                    type: Creature.DamageType.Explosion,
                    damage: 0.05f,
                    stunBonus: 0f
                );
            }
            else if (result.obj is PhysicalObject targetObject)
            {
                if (self.room != null)
                {
                    Plugin.Log.LogMessage("burning object");
                    self.room.AddObject(new FlameEffect(targetObject, result.chunk, result.onAppendagePos, data.fireDuration / 2));
                    data.fireLoss = 100;
                    Vector2 hitPos = result.collisionPoint;
                    self.room.AddObject(new Spark(hitPos, Custom.RNV() * 5f, new Color(1f, 0.4f, 0.1f), null, 10, 20));
                }

            }
        }
        return hitResult;
    }

    private static void Boomerang_ApplyPallete(On.Boomerang.orig_ApplyPalette orig, Boomerang self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (PhysicalObjectCWT.TryGetData(self, out var data) && data.onFire)
        {
            Color newGradColor = Color.Lerp(self.gradColor, new Color(1f, 0.4f, 0.1f), data.fireDuration / 1600f);
            sLeaser.sprites[0].color = newGradColor;
            sLeaser.sprites[2].color = newGradColor;
        }
    }

    private static bool Rock_HitSomething(On.Rock.orig_HitSomething orig, Rock self, SharedPhysics.CollisionResult result, bool eu)
    {
        bool hitResult = orig(self, result, eu);
        if (PhysicalObjectCWT.TryGetData(self, out var data) && data.onFire)
        {
            if (result.obj is Creature targetCreature)
            {
                if (self.room != null)
                {
                    Plugin.Log.LogMessage("burning creature");
                    self.room.AddObject(new FlameEffect(targetCreature, result.chunk, result.onAppendagePos, data.fireDuration / 2));
                    data.fireLoss = 100;
                    Vector2 hitPos = result.collisionPoint;
                    self.room.AddObject(new Spark(hitPos, Custom.RNV() * 5f, new Color(1f, 0.4f, 0.1f), null, 10, 20));
                }

                targetCreature.Violence(
                    source: self.firstChunk,
                    directionAndMomentum: null,
                    hitChunk: result.chunk ?? targetCreature.mainBodyChunk,
                    hitAppendage: result.onAppendagePos,
                    type: Creature.DamageType.Explosion,
                    damage: 0.05f,
                    stunBonus: 0f
                );
            }
            else if (result.obj is PhysicalObject targetObject)
            {
                if (self.room != null)
                {
                    Plugin.Log.LogMessage("burning object");
                    self.room.AddObject(new FlameEffect(targetObject, result.chunk, result.onAppendagePos, data.fireDuration / 2));
                    data.fireLoss = 100;
                    Vector2 hitPos = result.collisionPoint;
                    self.room.AddObject(new Spark(hitPos, Custom.RNV() * 5f, new Color(1f, 0.4f, 0.1f), null, 10, 20));
                }

            }
        }
        return hitResult;
    }
    private static void Rock_ApplyPalette(On.Rock.orig_ApplyPalette orig, Rock self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (PhysicalObjectCWT.TryGetData(self, out var data) && data.onFire)
        {
            self.color = Color.Lerp(palette.blackColor, new Color(1f, 0.4f, 0.1f), data.fireDuration / 1600f);
            sLeaser.sprites[0].color = self.color;
            sLeaser.sprites[1].color = self.color;
        }
        else orig(self, sLeaser, rCam, palette);
    }

    private static void FlareBomb_Update(On.FlareBomb.orig_Update orig, FlareBomb self, bool eu)
    {
        orig(self, eu);
        if (PhysicalObjectCWT.TryGetData(self, out var data) && data.onFire)
        {
                for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
                {
                    if (self.room.abstractRoom.creatures[i].realizedCreature != null && (self.room.abstractRoom.creatures[i].rippleLayer == self.abstractPhysicalObject.rippleLayer || self.room.abstractRoom.creatures[i].rippleBothSides || self.abstractPhysicalObject.rippleBothSides) && (Custom.DistLess(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, self.LightIntensity * 600f) || (Custom.DistLess(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, self.LightIntensity * 1600f) && self.room.VisualContact(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos))))
                    {
                        if (self.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Spider && !self.room.abstractRoom.creatures[i].realizedCreature.dead)
                        {
                            self.room.abstractRoom.creatures[i].realizedCreature.firstChunk.vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * (UnityEngine.Random.value * 7f);
                            self.room.abstractRoom.creatures[i].realizedCreature.Die();
                        }
                        else if (self.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.BigSpider)
                        {
                            (self.room.abstractRoom.creatures[i].realizedCreature as BigSpider).poison = 1f;
                            (self.room.abstractRoom.creatures[i].realizedCreature as BigSpider).State.health -= UnityEngine.Random.value * 0.2f;
                            self.room.abstractRoom.creatures[i].realizedCreature.Stun(UnityEngine.Random.Range(10, 20));
                            if (self.thrownBy != null)
                            {
                                self.room.abstractRoom.creatures[i].realizedCreature.SetKillTag(self.thrownBy.abstractCreature);
                            }
                        }
                        self.room.abstractRoom.creatures[i].realizedCreature.Blind((int)Custom.LerpMap(Vector2.Distance(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.VisionPoint), 60f, 600f, 400f, 20f));
                        self.room.abstractRoom.creatures[i].realizedCreature.Stun((int)Custom.LerpMap(Vector2.Distance(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.VisionPoint), 60f, 600f, 400f, 20f));
                        self.room.AddObject(new CreatureSpasmer(self.room.abstractRoom.creatures[i].realizedCreature, true, (int)Custom.LerpMap(Vector2.Distance(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.VisionPoint), 60f, 600f, 400f, 20f)));
                        if (CreatureCWT.TryGetData(self.room.abstractRoom.creatures[i].realizedCreature, out var cData) && !cData.onFire && Vector2.Distance(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos) <= 150f) self.room.AddObject(new FlameEffect(self.room.abstractRoom.creatures[i].realizedCreature, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk, null, (int)Custom.LerpMap(Vector2.Distance(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos), 20f, 150f, 1600f, 400f)));
                    }
                
            }
            if (self.light != null)
            {
                if (self.room.Darkness(self.firstChunk.pos) == 0f)
                {
                    self.light.Destroy();
                }
                else
                {
                    self.light.setPos = new Vector2?(self.firstChunk.pos + self.firstChunk.vel);
                    self.light.setAlpha = new float?(((self.mode == Weapon.Mode.Thrown) ? Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value) : 0.5f) * (1f - 0.6f * self.LightIntensity));
                    self.light.setRad = new float?(Mathf.Max(self.flashRad, ((self.mode == Weapon.Mode.Thrown) ? Mathf.Lerp(60f, 290f, UnityEngine.Random.value) : 60f) * 1f + self.LightIntensity * 10f));
                    self.light.color = new(1f, 0.4f, 0.1f);
                }
                if (self.light.slatedForDeletetion || self.light.room != self.room)
                {
                    self.light = null;
                    return;
                }
            }
            else if (self.room.Darkness(self.firstChunk.pos) > 0f)
            {
                self.light = new LightSource(self.firstChunk.pos, false, new(1f, 0.4f, 0.1f), self);
                self.room.AddObject(self.light);
            }
        }
    }

    private static void BubbleGrass_Update(On.BubbleGrass.orig_Update orig, BubbleGrass self, bool eu)
    {
        orig(self, eu);
        if (self.room != null && self.AbstrBubbleGrass.oxygenLeft > 0f && PhysicalObjectCWT.TryGetData(self, out var data) && data.onFire)
        {
            self.AbstrBubbleGrass.oxygenLeft -= 0.04f;
            if (UnityEngine.Random.value < 0.1f)
            {
                self.room.AddObject(new Smoke.Smolder(self.room, self.firstChunk.pos, self.firstChunk, null));
            }
        }
    }

    private static void PuffBall_Explode(On.PuffBall.orig_Explode orig, PuffBall self)
    {
        if (PhysicalObjectCWT.TryGetData(self, out var data) && data.onFire)
        {
            if (self.slatedForDeletetion)
            {
                return;
            }
            InsectCoordinator smallInsects = null;
            for (int i = 0; i < self.room.updateList.Count; i++)
            {
                if (self.room.updateList[i] is InsectCoordinator)
                {
                    smallInsects = (self.room.updateList[i] as InsectCoordinator);
                    break;
                }
            }
            for (int j = 0; j < 70; j++)
            {
                self.room.AddObject(new SporeCloud(self.firstChunk.pos, Custom.RNV() * (UnityEngine.Random.value * 10f), new(1f, 0.4f, 0.1f), 1f, (self.thrownBy != null) ? self.thrownBy.abstractCreature : null, j % 20, smallInsects, self.abstractPhysicalObject.rippleLayer));
            }
            self.room.AddObject(new SporePuffVisionObscurer(self.firstChunk.pos, self.abstractPhysicalObject.rippleLayer));
            for (int k = 0; k < 7; k++)
            {
                self.room.AddObject(new PuffBallSkin(self.firstChunk.pos, Custom.RNV() * (UnityEngine.Random.value * 16f), new(1f, 0.4f, 0.1f), Color.Lerp(new(1f, 0.4f, 0.1f), self.sporeColor, 0.5f)));
            }
            self.room.PlaySound(SoundID.Puffball_Eplode, self.firstChunk);
            self.Destroy();

            return;
        }
        orig(self);
    }

    private static void SporeCloud_Update(On.SporeCloud.orig_Update orig, SporeCloud self, bool eu)
    {
        if (self.color == new Color(1f, 0.4f, 0.1f))
        {
            if (!self.nonToxic && self.checkInsectsDelay > -1)
            {
                self.checkInsectsDelay--;
                if (self.checkInsectsDelay < 1)
                {
                    self.checkInsectsDelay = 20;
                    for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
                    {
                        Creature realizedCreature = self.room.abstractRoom.creatures[i].realizedCreature;
                        if (realizedCreature != null && (self.room.abstractRoom.creatures[i].rippleLayer == self.rippleLayer || self.room.abstractRoom.creatures[i].rippleBothSides) && CreatureCWT.TryGetData(realizedCreature, out var data) && !data.onFire)
                        {
                            if (Custom.DistLess(self.pos, realizedCreature.mainBodyChunk.pos, self.rad + realizedCreature.mainBodyChunk.rad + 20f))
                            {
                                self.room.AddObject(new FlameEffect(realizedCreature, realizedCreature.mainBodyChunk, null, 1600));
                            }
                        }
                    }
                }
            }
        }
        orig(self, eu);
    }

    private static void SeedCob_HitByWeapon(On.SeedCob.orig_HitByWeapon orig, SeedCob self, Weapon weapon)
    {
        if (weapon is TorchSpear tSpear && tSpear.isLit)
        {
            if (weapon == null || self.room == null || self.room.roomSettings == null)
            {
                return;
            }
            self.Open();
            self.spawnUtilityFoods();
            return;
        }
        else orig(self, weapon);
    }

    private static Color MoreSlugcats_MoonCloak_Color(On.MoreSlugcats.MoonCloak.orig_Color orig, MoreSlugcats.MoonCloak self, float f)
    {
        if (PhysicalObjectCWT.TryGetData(self, out var data) && data.onFire)
        {
            return Color.Lerp(orig(self, f), new Color(0.2f, 0.2f, 0.05f), data.permanentFireAmount);
        }
        else return orig(self, f);
    }
    private static Color PlayerGraphics_Gown_Color(On.PlayerGraphics.Gown.orig_Color orig, PlayerGraphics.Gown self, float f)
    {
        if (CreatureCWT.TryGetData(self.owner.player, out var data) && data.onFire)
        {
            return Color.Lerp(orig(self, f), new Color(0.2f, 0.2f, 0.05f), data.permanentFireAmount);
        }
        else return orig(self, f);
    }

}