using UnityEngine;
using Watcher;
using RWCustom;
using System;
using Unity.Mathematics;

namespace lsfUtils.Creatures.Lizards.FlameLizard;

public class FlameLizard : Lizard
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
        if (Consious && jawOpen > 0.3)
        {
            flameTimer = math.min(flameTimer + 1, 20);
        }
        else flameTimer = math.max(flameTimer - 1, 0);

        if (animation == Animation.Lounge)
        {
            JawOpen = 1f;
            flameTimer = 40;
        }

        if (flameTimer == 40 && flameJet == null)
        {
            FlameJet.FlameJetData flameJetData = new FlameJet.FlameJetData(null);
            flameJetData.temperature = 1f;
            flameJetData.temperatureAnimOffset = 0f;
            flameJetData.intensityAnimOffset = 0f;
            flameJetData.linkTempToIntens = true;
            flameJetData.pos = bodyChunks[0].pos;
            flameJet = new FlameJet(room, flameJetData);
            room.AddObject(flameJet);
        }

        if (flameJet != null)
        {
            flameJet.pos = bodyChunks[0].pos + Custom.DirVec(bodyChunks[1].pos, bodyChunks[0].pos) * 25;
            flameJet.target = Custom.DirVec(bodyChunks[1].pos, bodyChunks[0].pos) * 170;
            if (flameTimer == 40)
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
}
