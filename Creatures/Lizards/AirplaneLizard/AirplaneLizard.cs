using UnityEngine;
using Watcher;
using RWCustom;
using System;
using Unity.Mathematics;

namespace lsfUtils.Creatures.Lizards.AirplaneLizard;

public class AirplaneLizard : Lizard
{
    public AirplaneLizard(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = UnityEngine.Random.state;
        UnityEngine.Random.InitState(abstractCreature.ID.RandomSeed);
        effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(280 / 360f, 10 / 360f, 0.5f), .85f, Custom.ClampedRandomVariation(.4f, .050f, .5f));
        UnityEngine.Random.state = state;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new AirplaneLizardGraphics(this);
    public override void LoseAllGrasps() => ReleaseGrasp(0);

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (this != null && graphicsModule != null && !room.aimap.getAItile(abstractCreature.pos).narrowSpace && animation == Animation.PrepareToLounge)
        {
            (graphicsModule as LizardGraphics).showDominance = 1f;
            Vector2 p = new Vector2(bodyChunks[2].pos.x + loungeDir.x * 10f, bodyChunks[2].pos.y + 15f);
            bodyChunks[1].vel += Custom.DirVec(bodyChunks[1].pos, p);
            p = new Vector2(bodyChunks[2].pos.x + loungeDir.x * -10f, bodyChunks[2].pos.y + 20f);
            bodyChunks[0].vel += Custom.DirVec(bodyChunks[0].pos, bodyChunks[1].pos + loungeDir * 10f);
            (graphicsModule as LizardGraphics).head.pos = Vector2.Lerp((graphicsModule as LizardGraphics).head.pos, bodyChunks[0].pos + loungeDir * 20f, 0.25f);
            (graphicsModule as LizardGraphics).head.vel = loungeDir * 20f;
            p = new Vector2(bodyChunks[2].pos.x + loungeDir.x * 10f, bodyChunks[2].pos.y - 15f);
            (graphicsModule as LizardGraphics).limbs[0].mode = Limb.Mode.HuntAbsolutePosition;
            (graphicsModule as LizardGraphics).limbs[0].absoluteHuntPos = p;
            (graphicsModule as LizardGraphics).limbs[1].mode = Limb.Mode.HuntAbsolutePosition;
            (graphicsModule as LizardGraphics).limbs[1].absoluteHuntPos = p;
            p = new Vector2(bodyChunks[2].pos.x + loungeDir.x * 20f, bodyChunks[2].pos.y - 15f);
            //(base.graphicsModule as LizardGraphics).limbs[2].mode = Limb.Mode.HuntAbsolutePosition;
            //(base.graphicsModule as LizardGraphics).limbs[2].absoluteHuntPos = p;
            //(base.graphicsModule as LizardGraphics).limbs[3].mode = Limb.Mode.HuntAbsolutePosition;
            //(base.graphicsModule as LizardGraphics).limbs[3].absoluteHuntPos = p;
        }
    }
}
