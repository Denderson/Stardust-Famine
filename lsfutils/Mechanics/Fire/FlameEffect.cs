using lsfUtils.Creatures.Lizards.FlameLizard;
using lsfUtils.Creatures.Lizards.MonitorLizard;
using lsfUtils.Creatures.Lizards.WeaverLizard;
using lsfUtils.CreatureTags;
using lsfUtils.CWTs;
using lsfUtils.DevtoolsEffects.Sunburn;
using lsfUtils.Items.ExplosiveBoomerang;
using MoreSlugcats;
using RWCustom;
using Smoke;
using System;
using HUD;
using UnityEngine;
using Watcher;
using static lsfUtils.Plugin;
using System.Drawing.Imaging;

namespace lsfUtils.Mechanics.Fire;

public class FlameEffect : UpdatableAndDeletable, IProvideWarmth // patoma fire logic, pissing me off early in the morning, removed unnecesarry 2 am hook
{
    public Creature targetCreature;
    public PhysicalObject targetObject;
    public BodyChunk attachedChunk;
    public PhysicalObject.Appendage.Pos attachedAppendage;

    public int burnDuration;
    public float burnAmount = 0f;
    public float permanentBurnAmount = 0f;
    public int tickCounter = 0;
    public FirecrackerPlant.ScareObject scareObj;

    public LightSource lightSource;
    public float[,] flicker;
    public FlameParticle flamePool;

    public int flammability = 2;
    public bool burnable = true;
    public bool extinguish = false;

    public FlameEffect(Creature target, BodyChunk chunk, PhysicalObject.Appendage.Pos appendage, int duration)
    {
        this.targetCreature = target;
        attachedChunk = chunk ?? target.mainBodyChunk;
        attachedAppendage = appendage;
        burnDuration = duration;

        flicker = new float[2, 3];
        for (int i = 0; i < 2; i++)
        {
            flicker[i, 0] = 1f;
            flicker[i, 1] = 1f;
            flicker[i, 2] = 1f;
        }

        // base flammability logic, 0 = immune, 1 = low, 2 = normal, 3 = high, 4 = extreme
        flammability = 2;
        if (target is BigEel || target is GarbageWorm || target is Overseer || target is TempleGuard ||
            ModManager.MSC && (target is EggBug eggbug && eggbug.FireBug || target is Player player && player.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer) ||
            ModManager.DLCShared && target is Inspector ||
            ModManager.Watcher && (target is FireSprite || target.Template.type == WatcherEnums.CreatureTemplateType.BlizzardLizard) ||
            target is FlameLizard || target.Template.type.ToString().ToLower().Contains("fire")) 
            flammability = 0;
        else if (target is Lizard lizard && (lizard.Template.type == CreatureTemplate.Type.Salamander || ModManager.DLCShared && lizard.Template.type == DLCSharedEnums.CreatureTemplateType.EelLizard || ModManager.Watcher && (lizard.Template.type == WatcherEnums.CreatureTemplateType.PeachLizard || lizard.Template.type == WatcherEnums.CreatureTemplateType.IndigoLizard)) || target is Centipede centipede && (centipede.Red || ModManager.DLCShared && centipede.AquaCenti) ||
            target is Leech || target is Snail || target is JetFish || target is Hazer ||
            ModManager.MSC && target is Player playe && playe.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet ||
            ModManager.DLCShared && target is BigJellyFish ||
            ModManager.Watcher && (target is DrillCrab || target is Angler || target is Barnacle || target is Tardigrade) ||
            target is WeaverLizard || target is MonitorLizard) 
            flammability = 1;
        else if (target is BigSpider || target is Centipede centi && centi.Centiwing || target is DaddyLongLegs || target is LanternMouse || target is EggBug || target is NeedleWorm ||
            ModManager.DLCShared && target.Template.type == DLCSharedEnums.CreatureTemplateType.SpitLizard ||
            ModManager.Watcher && (target is Frog || target is Rat || target is Rattler || target is MothGrub || target.Template.type == WatcherEnums.CreatureTemplateType.BasiliskLizard)) 
            flammability = 3;
        else if (target is PoleMimic || target is TentaclePlant || target is Spider || target is Vulture ||
            ModManager.MSC && target is Player play && play.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Saint ||
            ModManager.Watcher && (target is BigMoth || target is SkyWhale)) 
            flammability = 4;

        //rotten modifier, +1 flammability if true
        if ((target is Lizard lizor && lizor.rotModule != null) || target.Template.type.ToString().ToLower().Contains("rot")) flammability += 1;
        //Lavasafe modifier, -1 flammability if true
        if (target.abstractCreature.lavaImmune) flammability -= 1;

        if (AbstractCreatureCWT.TryGetData(target.abstractCreature, out var data) && target.IsCustomFlammable())
        {
            if (data.flammabilitySign == '=')
            {
                flammability = data.flammabilityValue;
            }
            else if (data.flammabilitySign == '+')
            {
                flammability = Math.Min(flammability + data.flammabilityValue, 5);
            }
            else if (data.flammabilitySign == '-')
            {
                flammability = Math.Max(flammability - data.flammabilityValue, 0);
            }
        }
    }

    public FlameEffect(PhysicalObject target, BodyChunk chunk, PhysicalObject.Appendage.Pos appendage, int duration)
    {
        this.targetObject = target;
        attachedChunk = chunk ?? target.firstChunk;
        attachedAppendage = appendage;
        burnDuration = duration;

        flicker = new float[2, 3];
        for (int i = 0; i < 2; i++)
        {
            flicker[i, 0] = 1f;
            flicker[i, 1] = 1f;
            flicker[i, 2] = 1f;
        }

        // base flammability for objects, only affects the speed at which burnable objects will burn down and which ones won't burn at all, and the likelihood to spread fire
        flammability = 2;
        if ((target is Spear && target is not ExplosiveSpear) || target is OverseerCarcass) flammability = 0;
        /*else if () flammability = 1;
        else if () flammability = 3;
        else if () flammability = 4;*/

        //list of objects that won't burn out completely if they're hit by fire, made to have the most mod compatability possible by default
        if (target is KarmaFlower || target is SwollenWaterNut || target is SSOracleSwarmer || target is SLOracleSwarmer || target is NSHSwarmer || target is SeedCob cob || target is Weapon || target is Lantern || target is VultureMask || target is NeedleEgg || target is OverseerCarcass || target is DataPearl ||
            (ModManager.MSC && (target is FireEgg || target is MoonCloak || target is EnergyCell)) || 
            (ModManager.DLCShared && target is GooieDuck) ||
            (ModManager.Watcher && (target is Pomegranate || target is BoxWorm.Larva || target is UrbanToys.WeirdToy || target is UrbanToys.SpinToy || target is UrbanToys.SoftToy || target is UrbanToys.BallToy)) ||
            target is Creature /*this one is just here because sometimes creatures use the object burning code, no clue why*/) burnable = false;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if ((targetCreature == null && targetObject == null) || (targetCreature != null && targetCreature.slatedForDeletetion) || (targetObject != null && targetObject.slatedForDeletetion) || burnDuration <= 0 || attachedChunk == null || attachedChunk.submersion > 0.2f || attachedChunk.sandSubmersion > 0.7f || targetCreature?.muddy >= 800f || (CreatureCWT.TryGetData(targetCreature, out var creatureData) && creatureData.inHazerInk) || PhysicalObjectCWT.TryGetData(targetObject, out var objectData) && objectData.inHazerInk || (targetObject != null && flammability == 0) || extinguish)
        {
            if (lightSource != null)
            {
                lightSource.Destroy();
                lightSource = null;
            }
            if (CreatureCWT.TryGetData(targetCreature, out var creatureData2)) { creatureData2.onFire = false; creatureData2.fireBuildupAmount = Math.Max(creatureData2.fireBuildupAmount, 0.5f); }
            if (PhysicalObjectCWT.TryGetData(targetObject, out var objectData2)) { objectData2.onFire = false; objectData2.fireBuildupAmount = Math.Max(objectData2.fireBuildupAmount, 0.5f); }
            Destroy();
            while (burnAmount > 0f)
            {
                burnAmount -= 1 / 400f;
                if (targetCreature != null) creatureData2?.fireAmount = burnAmount;
            }
            scareObj = null;
            return;
        }

        int decayRate = attachedChunk.vel.magnitude > 2.5f ? 2 : 1;
        if ((room.roomSettings.DangerType == DLCSharedEnums.RoomRainDangerType.Blizzard && room.world.rainCycle.CycleProgression > 0.7f) || Math.Max(room.roomSettings.GetEffectAmount(WatcherEnums.RoomEffectType.SurfaceSandstorm) - Mathf.InverseLerp((float)Sandstorm.lethalMaxTime, 0f, (float)(this.room.world.rainCycle.timer - this.room.world.rainCycle.cycleLength)), 0f) >= 0.3f) decayRate += 1;
        if (targetObject?.grabbedBy.Count > 0 && targetObject.grabbedBy[0].grabber is Player && (targetObject.grabbedBy[0].grabber as Player).SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet) decayRate += 1;
        if ((targetObject != null && Sunburn.InSunlight(targetObject) || targetCreature != null && Sunburn.InSunlight(targetCreature)) && burnDuration < 400) decayRate -= 2;
        if (burnDuration <= 400) burnDuration = Math.Min(burnDuration - decayRate, 400);
        else burnDuration -= decayRate;
        if (targetObject != null && objectData.fireLoss > 0)
        {
            Log.LogMessage($"{targetObject} lost {objectData.fireLoss} fire. new time: {burnDuration / 40f} seconds");
            burnDuration -= objectData.fireLoss;
            objectData.fireLoss = 0;
        }
        tickCounter++;

        // le flick
        for (int i = 0; i < flicker.GetLength(0); i++)
        {
            flicker[i, 1] = flicker[i, 0];
            flicker[i, 0] += Mathf.Pow(UnityEngine.Random.value, 3f) * 0.1f * (UnityEngine.Random.value < 0.5f ? -1f : 1f);
            flicker[i, 0] = Custom.LerpAndTick(flicker[i, 0], flicker[i, 2], 0.05f, 0.033333335f);

            if (UnityEngine.Random.value < 0.2f)
            {
                flicker[i, 2] = 1f + Mathf.Pow(UnityEngine.Random.value, 3f) * 0.2f * (UnityEngine.Random.value < 0.5f ? -1f : 1f);
            }
            flicker[i, 2] = Mathf.Lerp(flicker[i, 2], 1f, 0.01f);
        }

        float effectiveness = Mathf.Max(0f, burnDuration / 1600f);
        if (targetCreature != null) targetCreature.repelLocusts = Math.Max(targetCreature.repelLocusts, (int)(effectiveness * 100));
        if (room != null && ((targetCreature != null && !targetCreature.inShortcut) || targetObject != null))
        {
            if (UnityEngine.Random.value < effectiveness)
            {
                if (flamePool == null || flamePool.slatedForDeletetion || flamePool.room != room)
                {
                    flamePool = new FlameParticle();
                    room.AddObject(flamePool);
                }

                float spreadRad = Mathf.Min(attachedChunk.rad * 0.8f, 15f);
                Vector2 particlePos = attachedChunk.pos + UnityEngine.Random.insideUnitCircle * spreadRad;
                Vector2 flameVel = attachedChunk.vel * 0.5f + new Vector2(0f, 2.5f) + Custom.RNV() * UnityEngine.Random.value * 2f;

                flamePool.Emit(particlePos, flameVel, 1.5f + effectiveness);
            }

            if (lightSource != null && (lightSource.slatedForDeletetion || lightSource.room != room))
            {
                lightSource = null;
            }

            if (lightSource == null)
            {
                lightSource = new LightSource(attachedChunk.pos, false, GetTorchFireColor(), null);
                lightSource.requireUpKeep = true;
                lightSource.affectedByPaletteDarkness = 0f;
                lightSource.HardSetAlpha(1f);
                lightSource.HardSetRad(150f);
                room.AddObject(lightSource);
            }

            lightSource.stayAlive = true;
            lightSource.setPos = attachedChunk.pos;
            lightSource.setRad = (150f + 60f * effectiveness) * flicker[0, 0];
            lightSource.color = GetTorchFireColor();
            lightSource.setAlpha = effectiveness;
        }
        else if (lightSource != null)
        {
            lightSource.Destroy();
            lightSource = null;
        }

        if (this.scareObj == null)
        {
            scareObj = new FirecrackerPlant.ScareObject(attachedChunk.pos, attachedChunk.owner.abstractPhysicalObject.rippleLayer);
            scareObj.fearRange = 400f;
            scareObj.fearScavs = true;
            room.AddObject(this.scareObj);
            Log.LogMessage("spooky scary oooooooooooh");
        }

        float currentFlammability = flammability;
        if (targetCreature?.muddy > 200f && flammability > 0)
        {
            currentFlammability = Math.Max(flammability - 1, 0.5f);
        }
        if (targetCreature?.Hypothermia >= 0.8f)
        {
            currentFlammability = flammability + 1;
        }
        if (targetCreature != null)
        {
            creatureData.flammability = currentFlammability;
            creatureData.onFire = true;
            creatureData.fireDuration = burnDuration;
            creatureData.permanentFireAmount = permanentBurnAmount;
            creatureData.fireAmount = burnAmount;
            if (targetCreature?.grabbedBy.Count > 0 && targetCreature.grabbedBy[0].grabber is Creature otherCreature && tickCounter % 80 == 0 && CreatureCWT.TryGetData(otherCreature, out var otherData) && !otherData.onFire && UnityEngine.Random.value < (creatureData.flammability + otherData.flammability + 1) / 10) targetCreature.room.AddObject(new FlameEffect(otherCreature, otherCreature.mainBodyChunk, null, 2 * creatureData.fireDuration / 3));
        }
        if (targetObject != null)
        {
            objectData.flammability = currentFlammability;
            objectData.onFire = true;
            objectData.fireDuration = burnDuration;
            objectData.permanentFireAmount = permanentBurnAmount;
            if (targetObject?.grabbedBy.Count > 0 && targetObject.grabbedBy[0].grabber is Creature otherCreature && tickCounter % 80 == 0 && CreatureCWT.TryGetData(otherCreature, out var otherData) && !otherData.onFire && UnityEngine.Random.value < (objectData.flammability + otherData.flammability + 1) / 10) targetObject.room.AddObject(new FlameEffect(otherCreature, otherCreature.mainBodyChunk, null, 2 * objectData.fireDuration / 3));
        }
        if (targetCreature != null && targetCreature is Player play && play.slugcatStats.name != MoreSlugcatsEnums.SlugcatStatsName.Artificer) burnAmount += 1 / 800f * currentFlammability;
        if (tickCounter % 20 == 0 && targetCreature != null && !targetCreature.dead)
        {
            creatureData.canSpreadFire = true;
            if (targetCreature is not Player pla)
            {
                if (burnAmount >= 1.5f) room.AddObject(new Smolder(room, attachedChunk.pos, attachedChunk, attachedAppendage));
                targetCreature.Violence(source: null, directionAndMomentum: null, hitChunk: attachedChunk, hitAppendage: attachedAppendage, type: Creature.DamageType.Explosion, damage: 0.025f * currentFlammability, stunBonus: 0);
                if (targetCreature.dead && (targetCreature is Vulture || targetCreature is SkyWhale))
                {
                    var room = targetCreature.room;
                    var pos = targetCreature.mainBodyChunk.pos;
                    if (targetCreature?.room == null || pos == null) return;
                    room.AddObject(new Explosion(room, targetCreature, pos, 7, 80f * targetCreature.TotalMass, 6.2f, 2f, 280f, 0.4f, targetCreature, 0.7f, 160f, 1f));
                    room.AddObject(new Explosion.ExplosionLight(pos, 75f * targetCreature.TotalMass, 1f, 3, GetTorchFireColor()));
                    room.AddObject(new ExplosionSpikes(room, pos, 14, 9.5f * targetCreature.TotalMass, 9f, 7f, 170f, GetTorchFireColor()));
                    room.PlaySound(SoundID.Bomb_Explode, pos, 1.2f, 0.3f);
                    targetCreature.Die();
                    targetCreature.Destroy();
                    room.ScreenMovement(pos, default, 0.3f * targetCreature.TotalMass);
                    Log.LogMessage(":leditor::overload:");
                }
                Log.LogMessage("Burning " + targetCreature + " with currentFlammability " + currentFlammability + " for " + 0.025f * currentFlammability + " damage");
                if (targetCreature is Lizard lizard && attachedChunk != lizard.bodyChunks[0])
                {
                    if (lizard.Template.type == CreatureTemplate.Type.CyanLizard)
                    {
                        burnAmount += 1 / 8f;
                        if (UnityEngine.Random.value * burnAmount >= 1.5)
                        {
                            var room = targetCreature.room;
                            var pos = targetCreature.mainBodyChunk.pos;
                            if (targetCreature?.room == null || pos == null) return;
                            room.AddObject(new Explosion(room, targetCreature, pos, 7, 350f, 6.2f, 2f, 280f, 0.4f, targetCreature, 0.7f, 160f, 1f));
                            room.AddObject(new Explosion.ExplosionLight(pos, 330f, 1f, 3, lizard.effectColor));
                            room.AddObject(new ExplosionSpikes(room, pos, 14, 40f, 9f, 7f, 170f, lizard.effectColor));
                            room.PlaySound(SoundID.Bomb_Explode, pos, 0.8f, 0.4f);
                            targetCreature.Die();
                            targetCreature.Destroy();
                            Log.LogMessage(":leditoroverload:");
                        }
                    }
                    if (ModManager.Watcher && lizard.Template.type == WatcherEnums.CreatureTemplateType.BasiliskLizard)
                    {
                        var room = targetCreature.room;
                        var pos = targetCreature.mainBodyChunk.pos;
                        if (targetCreature?.room == null || pos == null) return;
                        room.AddObject(new Explosion(room, targetCreature, pos, 7, 400f, 6.2f, 2f, 280f, 0.4f, targetCreature, 0.7f, 160f, 1f));
                        room.AddObject(new Explosion.ExplosionLight(pos, 380f, 1f, 3, lizard.effectColor));
                        room.AddObject(new ExplosionSpikes(room, pos, 14, 45f, 9f, 7f, 170f, lizard.effectColor));
                        room.PlaySound(SoundID.Bomb_Explode, pos, 0.85f, 0.35f);
                        targetCreature.Die();
                    }
                }
            }
            else
            {
                if (pla.animation == Player.AnimationIndex.Roll || pla.animation == Player.AnimationIndex.BellySlide) burnAmount -= 1 * currentFlammability; // TODO

                if (pla.slugcatStats.name != MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                {
                    permanentBurnAmount += 1 / 160f * currentFlammability;
                    if (burnAmount > 1f || permanentBurnAmount > 1f)
                    {
                        room.PlaySound(SoundID.Firecracker_Burn);
                        targetCreature.Die();
                    }
                    Log.LogMessage(pla.slugcatStats.name + " with currentFlammability " + currentFlammability + " is burning! Progress: " + burnAmount);

                }
                else
                {
                    burnAmount += 1 / 10f;
                    if (burnAmount >= 1f)
                    {
                        pla.pyroJumpCounter++;
                        if (pla.pyroJumpCounter >= MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value) pla.PyroDeath();
                    }
                }
            }
        }
        else if (targetCreature != null) creatureData.canSpreadFire = false;
        if (tickCounter % 20 == 0 && targetObject != null)
        {
            objectData.canSpreadFire = true;
            if (burnable)
            {
                burnAmount += 1 / 80f * currentFlammability;
                Log.LogMessage("Burning " + targetObject + " with Flammability " + currentFlammability + "!!!! Progress: " + burnAmount);
                if (burnAmount >= 1)
                {
                    targetObject.Destroy();
                    room.PlaySound(SoundID.Fire_Spear_Ignite);
                }
            }
            else if (targetObject is MoonCloak cloak)
            {
                permanentBurnAmount += 1 / 40f;
                Log.LogMessage("darkening the cloak! " + permanentBurnAmount * 100 + "%");
            }
        }
        else if (targetObject != null)
        {
            objectData.canSpreadFire = false;
            if (targetObject is SporePlant beehive && !beehive.Used)
            {
                beehive.Used = true;
            }
            else if (targetObject is GraffitiBomb graffiti)
            {
                graffiti.Explode();
            }
            else if (targetObject is ScavengerBomb bomb)
            {
                bomb.Explode(null);
            }
            else if (targetObject is ExplosiveSpear eSpear)
            {
                eSpear.Explode();
            }
            else if (targetObject is ExplosiveBoomerang eBoomerang && !eBoomerang.isSingularity)
            {
                eBoomerang.Explode(null);
                extinguish = true;
            }
        }
    }

    public Color GetTorchFireColor()
    {
        if (!RegionCWT.TryGetCustomRegionParams(room?.world?.region, out var customRegionParams)) return new Color(1f, 0.4f, 0.1f);
        
        return customRegionParams.FireCoreColor;
    }

    float IProvideWarmth.warmth {
        get
        {
            return RainWorldGame.DefaultHeatSourceWarmth * 2 * burnDuration / 1600f;
        }
    }
    Room IProvideWarmth.loadedRoom
    {
        get
        {
            return room;
        }
    }
    Vector2 IProvideWarmth.Position()
    {
        return attachedChunk.pos;
    }
    float IProvideWarmth.range
    {
        get
        {
            return 150f + 60f * burnDuration / 1600f;
        }
    }
}

public class FireMeter : HudPart
{
    public Vector2 pos;
    public Vector2 lastPos;
    public int remainVisibleCounter;
    public float fade;
    public float lastFade;
    public HUDCircle[] circles;

    public FireMeter(HUD.HUD hud, FContainer fContainer) : base(hud)
    {
        this.pos = new Vector2(80f, 20f);
        this.lastPos = this.pos;
        this.circles = new HUDCircle[10];
        this.fade = 0f;
        this.lastFade = 0f;
        for (int i = 0; i < this.circles.Length; i++)
        {
            this.circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 1); //pure red as a placeholder while the custom color doesn't work
            this.circles[i].fade = 0f;
            this.circles[i].lastFade = 0f;
            //this.circles[i].forceColor = new(1f, 0.56f, 0.39f);
        }
    }
    private bool Show
    {
        get
        {
            return this.hud.showKarmaFoodRain || this.hud.owner.RevealMap;
        }
    }
    public override void Update()
    {
        if (!CreatureCWT.TryGetData(this.hud.owner as Player, out var playerData)) return;
        float fire = playerData.fireAmount;
        if (this.hud.foodMeter != null)
        {
            this.pos.x = this.hud.foodMeter.pos.x;
            this.pos.y = this.hud.foodMeter.pos.y - 27f;
            if (fire <= 0.05f && this.hud.rainWorld.processManager.currentMainLoop is RainWorldGame && (this.hud.rainWorld.processManager.currentMainLoop as RainWorldGame).cameras[0].room != null && !playerData.onFire)
            {
                this.fade = Mathf.Lerp(this.fade, 0f, 0.1f);
            }
            else
            {
                this.fade = Mathf.Lerp(this.fade, this.Show ? this.hud.foodMeter.fade : 0f, ((this.Show && this.fade < this.hud.foodMeter.fade) || (!this.Show && this.fade > this.hud.foodMeter.fade)) ? 0.15f : 0.005f);
            }
        }
        this.lastPos = this.pos;
        this.lastFade = this.fade;
        if (this.hud.HideGeneralHud)
        {
            this.fade = 0f;
        }
        float num = 1f / (float)this.circles.Length;
        for (int i = 0; i < this.circles.Length; i++)
        {
            float value = fire - num * (float)i;
            this.circles[i].Update();
            this.circles[i].thickness = Mathf.Lerp(6f, 1f, Mathf.InverseLerp(num, 0f, value));
            this.circles[i].fade = Mathf.Lerp(this.circles[i].fade, Mathf.InverseLerp(num * (float)i, num * ((float)i + 1f), this.fade), 0.1f);
            this.circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
            this.circles[i].snapRad = 0.45f;
            this.circles[i].snapThickness = 0.45f;
            this.circles[i].rad = Mathf.Lerp(0.1f, 5f, this.circles[i].fade);
        }
    }
    public Vector2 DrawPos(float timeStacker)
    {
        return Vector2.Lerp(this.lastPos, this.pos, timeStacker);
    }
    public override void Draw(float timeStacker)
    {
        for (int i = 0; i < this.circles.Length; i++)
        {
            this.circles[i].pos = this.pos + new Vector2((float)i * 21.6f, 0f);
            this.circles[i].Draw(timeStacker);
        }
    }
}
