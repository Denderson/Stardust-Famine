using lsfUtils.CWTs;
using lsfUtils.Items.Spears.TorchSpears;
using lsfUtils.Mechanics.Fire;
using MoreSlugcats;
using RWCustom;
using System;
using Unity.Mathematics;
using UnityEngine;
using Watcher;

namespace lsfUtils.Creatures.Lizards.FlameLizard;

public class FlameLizard : Lizard, IProvideWarmth
{
    public FlameJet flameJet;
    public int flameTimer;
    public FlameLizard(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        flameTimer = 40;
        var state = UnityEngine.Random.state;
        UnityEngine.Random.InitState(abstractCreature.ID.RandomSeed);
        effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(8 / 360f, 8 / 360f, 0.5f), .85f, Custom.ClampedRandomVariation(.225f, .050f, .5f));
        UnityEngine.Random.state = state;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new FlameLizardGraphics(this);

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
    {
        if (type == DamageType.Explosion)
        {
            return;
        }
        base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room == null)
        {
            if (flameJet != null)
            {
                flameJet.room.RemoveObject(flameJet);
                flameJet = null;
            }
            return;
        }
        if (bodyChunks == null || bodyChunks.Length < 1)
        {
            return; 
        }
        if (bodyChunks[0].submersion > 0)
        {
            JawOpen = 0f;
        }
        if (Consious && jawOpen > 0.25)
        {
            flameTimer = math.min(flameTimer + 1, 40);
        }
        else flameTimer = math.max(flameTimer - 1, 0);

        if (animation == Animation.Lounge)
        {
            JawOpen = 1f;
            flameTimer = 40;
        }

        if (flameTimer > 20 && flameJet == null)
        {
            FlameJet.FlameJetData flameJetData = new FlameJet.FlameJetData(null);
            flameJetData.temperature = 1f;
            flameJetData.temperatureAnimOffset = 0f;
            flameJetData.intensityAnimOffset = 0f;
            flameJetData.linkTempToIntens = true;
            flameJetData.pos = bodyChunks[0].pos;
            if (this.abstractCreature.superSizeMe) flameJetData.lethality = 0;
            flameJet = new FlameJet(room, flameJetData);
            room.AddObject(flameJet);
        }
        
        if (flameJet != null)
        {
            flameJet.pos = bodyChunks[0].pos + Custom.DirVec(bodyChunks[1].pos, bodyChunks[0].pos) * 25;
            flameJet.target = Custom.DirVec(bodyChunks[1].pos, bodyChunks[0].pos) * 170;
            if (flameTimer > 20)
            {
                flameJet.intensity = Mathf.Min(flameJet.intensity + 0.04f, jawOpen);
            }
            
            else
            {
                flameJet.intensity = Mathf.Max(flameJet.intensity - 0.04f, 0);
                if (flameJet.intensity <= 0.1)
                {
                    flameJet.room.RemoveObject(flameJet);
                    flameJet = null;
                }
            }
            if (flameJet != null && flameJet.lethality == 0)
            {
                for (int i = 0; i < room.physicalObjects.Length; i++)
                {
                    foreach (PhysicalObject physicalObject in room.physicalObjects[i])
                    {
                        Vector2 vector = physicalObject.firstChunk.pos - flameJet.pos;
                        if (flameJet.bounds.Vector2Inside(vector))
                        {
                            for (int j = 0; j < flameJet.jetPos.Length - 1; j++)
                            {
                                float t = (float)(Time.frameCount % 10) * 0.1f;
                                Vector2 b = Vector2.Lerp(flameJet.jetPos[j], flameJet.jetPos[j + 1], t);
                                float num = Mathf.Lerp(flameJet.jetScale[j], flameJet.jetScale[j + 1], t);
                                float num2 = Mathf.Clamp((float)j / 3f, 0.5f, 1f);
                                num2 *= Mathf.Clamp((float)(flameJet.jetPos.Length - 1 - j) / 5f, 0.5f, 1f);
                                float num3 = Vector2.Distance(vector, b);
                                float num4 = num * 0.5f * num2;
                                if (num3 <= num4)
                                {
                                    if (physicalObject is Creature creature && creature is not FlameLizard && CreatureCWT.TryGetData(creature, out var creatureData) && !creatureData.onFire)
                                    {
                                        room.AddObject(new FlameEffect(creature, creature.firstChunk, null, (int)(flameJet.intensity * 1600)));
                                    }
                                    else if (physicalObject is not Creature && PhysicalObjectCWT.TryGetData(physicalObject, out var objectData) && !objectData.onFire)
                                    {
                                        room.AddObject(new FlameEffect(physicalObject, physicalObject.firstChunk, null, (int)(flameJet.intensity * 1600)));
                                    }
                                }
                            }
                        }
                    }
                }
            }        
        }
    }

    public override void Abstractize()
    {
        if (flameJet != null)
        {
            flameJet.room.RemoveObject(flameJet);
            flameJet = null;
        }
        base.Abstractize();
    }

    public override void Destroy()
    {
        if (flameJet != null)
        {
            flameJet.room.RemoveObject(flameJet);
            flameJet = null;
        }
        base.Destroy();
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);

    float IProvideWarmth.warmth
    {
        get
        {
            if (flameJet == null) return RainWorldGame.DefaultHeatSourceWarmth * 3f;
            else return RainWorldGame.DefaultHeatSourceWarmth * 5f;
        }
    }
    Room IProvideWarmth.loadedRoom
    {
        get
        {
            return this.room;
        }
    }
    Vector2 IProvideWarmth.Position()
    {
        return this.firstChunk.pos;
    }
    float IProvideWarmth.range
    {
        get
        {
            return 350f;
        }
    }

}
