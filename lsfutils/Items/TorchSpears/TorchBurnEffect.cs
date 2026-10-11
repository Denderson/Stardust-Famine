using MoreSlugcats;
using lsfUtils.Creatures.Lizards.FlameLizard;
using lsfUtils.CWTs;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Watcher;
using static lsfUtils.Plugin;
using lsfUtils.Creatures.Lizards.WeaverLizard;
using System.Threading;
using lsfUtils.Creatures.Lizards.MonitorLizard;
using lsfUtils.CreatureTags;

namespace lsfUtils.Items.TorchSpears
{
    public class TorchBurnEffect : UpdatableAndDeletable // patoma fire logic, pissing me off early in the morning, removed unnecesarry 2 am hook
    {
        public Creature target;
        public BodyChunk attachedChunk;
        public PhysicalObject.Appendage.Pos attachedAppendage;

        public int burnDuration = 1600;
        public float burnAmount = 0f;
        public int tickCounter = 0;

        public LightSource lightSource;
        public float[,] flicker;
        public TorchFlameParticle flamePool;
        

        public TorchBurnEffect(Creature target, BodyChunk chunk, PhysicalObject.Appendage.Pos appendage)
        {
            this.target = target;
            attachedChunk = chunk ?? target.mainBodyChunk;
            attachedAppendage = appendage;

            flicker = new float[2, 3];
            for (int i = 0; i < 2; i++)
            {
                flicker[i, 0] = 1f;
                flicker[i, 1] = 1f;
                flicker[i, 2] = 1f;
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (target == null || target.slatedForDeletetion || burnDuration <= 0 || attachedChunk == null || attachedChunk.submersion > 0.2f || attachedChunk.sandSubmersion > 0.7f || target.muddy >= 800f)
            {
                if (lightSource != null)
                {
                    lightSource.Destroy();
                    lightSource = null;
                }
                Destroy();
                while (burnAmount > 0f) burnAmount -= 1 / 200f;
                return;
            }

            int decayRate = attachedChunk.vel.magnitude > 2.5f ? 2 : 1;
            burnDuration -= decayRate;
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
            target.repelLocusts = Math.Max(target.repelLocusts, (int)(effectiveness * 100));
            if (room != null && !target.inShortcut)
            {
                if (UnityEngine.Random.value < effectiveness)
                {
                    if (flamePool == null || flamePool.slatedForDeletetion || flamePool.room != room)
                    {
                        flamePool = new TorchFlameParticle();
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

            // base flammability logic, 0 = immune, 1 = low, 2 = normal, 3 = high, 4 = extreme
            int flammability;
            if (target is BigEel || target is GarbageWorm || target is Overseer || target is TempleGuard || 
                (ModManager.MSC && ((target is EggBug eggbug && eggbug.FireBug) || (target is Player player && player.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer))) || 
                (ModManager.DLCShared && target is Inspector) || 
                (ModManager.Watcher && (target is FireSprite || target.Template.type == WatcherEnums.CreatureTemplateType.BlizzardLizard)) || 
                target is FlameLizard) flammability = 0;
            else if (target is Lizard lizard && (lizard.Template.type == CreatureTemplate.Type.Salamander || (ModManager.DLCShared && lizard.Template.type == DLCSharedEnums.CreatureTemplateType.EelLizard) || (ModManager.Watcher && (lizard.Template.type == WatcherEnums.CreatureTemplateType.PeachLizard || lizard.Template.type == WatcherEnums.CreatureTemplateType.IndigoLizard))) || (target is Centipede centipede && (centipede.Red || (ModManager.DLCShared && centipede.AquaCenti))) || 
                target is Leech || target is Snail || target is JetFish || target is Hazer || 
                (ModManager.MSC && (target is Player playe && playe.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)) ||
                (ModManager.DLCShared && target is BigJellyFish) || 
                (ModManager.Watcher && (target is DrillCrab || target is Angler || target is Barnacle || target is Tardigrade)) || 
                target is WeaverLizard || target is MonitorLizard) flammability = 1;
            else if (target is BigSpider || (target is Centipede centi && centi.Centiwing) || target is DaddyLongLegs || target is LanternMouse || target is EggBug || target is NeedleWorm || 
                (ModManager.DLCShared && target.Template.type == DLCSharedEnums.CreatureTemplateType.SpitLizard) ||
                (ModManager.Watcher && (target is Frog || target is Rat || target is Rattler || target is MothGrub || target.Template.type == WatcherEnums.CreatureTemplateType.BasiliskLizard))) flammability = 3;
            else if (target is PoleMimic || target is TentaclePlant || target is Spider || target is Vulture /*5.1, 7.14*/|| 
                (ModManager.MSC && (target is Player play && play.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Saint)) ||
                (ModManager.Watcher && (target is BigMoth || target is SkyWhale/*99.99999, 99.99999*/))) flammability = 4;
            else flammability = 2;
            //rotten modifier, +1 flammability if true
            if (target is Lizard lizor && lizor.rotModule != null) flammability = flammability + 1;

            if (target.abstractCreature is AbstractCreature abstractCreature) //ignore this, just getting abstractCreature :3
            //flammability creature tag check, = overrides, + adds, - subtracts
            if (AbstractCreatureCWT.TryGetData(abstractCreature, out var data) && target.IsCustomFlammable())
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
            //muddy modifier, -1 flammability if true
            if (target.muddy > 200f && flammability > 0)
            {
                flammability = Math.Max(flammability - 1, 1);
            }
            
            if (tickCounter % 20 == 0 && !target.dead)
            {
                if (target is not Player pla)
                {
                    target.Violence(
                        source: null,
                        directionAndMomentum: null,
                        hitChunk: attachedChunk,
                        hitAppendage: attachedAppendage,
                        type: Creature.DamageType.Explosion,
                        damage: 0.025f * flammability,
                        stunBonus: 0.5f
                    );
                    if (target.dead && (target is Vulture || target is SkyWhale))
                    {
                        var room = target.room;
                        var pos = target.mainBodyChunk.pos;
                        if (target?.room == null || pos == null) return;
                        room.AddObject(new Explosion(room, target, pos, 7, 80f * target.TotalMass, 6.2f, 2f, 280f, 0.4f, target, 0.7f, 160f, 1f));
                        room.AddObject(new Explosion.ExplosionLight(pos, 75f * target.TotalMass, 1f, 3, GetTorchFireColor()));
                        room.AddObject(new ExplosionSpikes(room, pos, 14, 9.5f * target.TotalMass, 9f, 7f, 170f, GetTorchFireColor()));
                        room.PlaySound(SoundID.Bomb_Explode, pos, 1.2f, 0.3f);
                        target.Die();
                        target.Destroy();
                        Log.LogMessage(":leditor::overload:");
                    }
                    Log.LogMessage("Burning " + target + " with flammability " + flammability + " for " + (0.025f * flammability) + " damage");
                    if (target is Lizard lizard)
                    {

                        if (lizard.Template.type == CreatureTemplate.Type.CyanLizard)
                        {
                            burnAmount += 1 / 8f;
                            if (burnAmount >= 1f)
                            {
                                
                                if (burnAmount >= 1.5f && UnityEngine.Random.value * burnAmount > 1.5)
                                {
                                    var room = target.room;
                                    var pos = target.mainBodyChunk.pos;
                                    if (target?.room == null || pos == null) return;
                                    room.AddObject(new Explosion(room, target, pos, 7, 350f, 6.2f, 2f, 280f, 0.4f, target, 0.7f, 160f, 1f));
                                    room.AddObject(new Explosion.ExplosionLight(pos, 330f, 1f, 3, lizard.effectColor));
                                    room.AddObject(new ExplosionSpikes(room, pos, 14, 40f, 9f, 7f, 170f, lizard.effectColor));
                                    room.PlaySound(SoundID.Bomb_Explode, pos, 0.8f, 0.4f);
                                    target.Die();
                                    target.Destroy();
                                    Log.LogMessage(":leditoroverload:");
                                }
                            }
                        }
                        if (ModManager.Watcher && lizard.Template.type == WatcherEnums.CreatureTemplateType.BasiliskLizard)
                        {
                            var room = target.room;
                            var pos = target.mainBodyChunk.pos;
                            if (target?.room == null || pos == null) return;
                            room.AddObject(new Explosion(room, target, pos, 7, 400f, 6.2f, 2f, 280f, 0.4f, target, 0.7f, 160f, 1f));
                            room.AddObject(new Explosion.ExplosionLight(pos, 380f, 1f, 3, lizard.effectColor));
                            room.AddObject(new ExplosionSpikes(room, pos, 14, 45f, 9f, 7f, 170f, lizard.effectColor));
                            room.PlaySound(SoundID.Bomb_Explode, pos, 0.85f, 0.35f);
                            target.Die();
                        }
                    }
                }
                else
                {
                    if (pla.slugcatStats.name != MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                    {
                        burnAmount += 1 / 40f * flammability;
                        if (burnAmount > 1f)
                        {
                            room.PlaySound(SoundID.Firecracker_Burn);
                            target.Die();
                        }                 
                        Log.LogMessage(pla.slugcatStats.name + " with flammability " + flammability + " is burning! Progress: " + burnAmount);
                        
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
        }
        public Color GetTorchFireColor()
        {
            if (RegionCWT.TryGetCustomRegionParams(this.room?.world?.region, out var customRegionParams)) return customRegionParams.TorchFireColor;
            return new Color(1f, 0.4f, 0.1f);
        }
    }
}
