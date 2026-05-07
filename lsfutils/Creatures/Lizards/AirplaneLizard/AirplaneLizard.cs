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
        if (graphicsModule is LizardGraphics graphics && !room.aimap.getAItile(abstractCreature.pos).narrowSpace && animation == Animation.PrepareToLounge)
        {
            graphics.showDominance = 1f;
            Vector2 p = new Vector2(bodyChunks[2].pos.x + loungeDir.x * 10f, bodyChunks[2].pos.y + 15f);
            bodyChunks[1].vel += Custom.DirVec(bodyChunks[1].pos, p);
            p = new Vector2(bodyChunks[2].pos.x + loungeDir.x * -10f, bodyChunks[2].pos.y + 20f);
            bodyChunks[0].vel += Custom.DirVec(bodyChunks[0].pos, bodyChunks[1].pos + loungeDir * 10f);
            graphics.head.pos = Vector2.Lerp(graphics.head.pos, bodyChunks[0].pos + loungeDir * 20f, 0.25f);
            graphics.head.vel = loungeDir * 20f;
            p = new Vector2(bodyChunks[2].pos.x + loungeDir.x * 10f, bodyChunks[2].pos.y - 15f);
            graphics.limbs[0].mode = Limb.Mode.HuntAbsolutePosition;
            graphics.limbs[0].absoluteHuntPos = p;
            graphics.limbs[1].mode = Limb.Mode.HuntAbsolutePosition;
            graphics.limbs[1].absoluteHuntPos = p;
            p = new Vector2(bodyChunks[2].pos.x + loungeDir.x * 20f, bodyChunks[2].pos.y - 15f);
            //graphics.limbs[2].mode = Limb.Mode.HuntAbsolutePosition;
            //graphics.limbs[2].absoluteHuntPos = p;
            //graphics.limbs[3].mode = Limb.Mode.HuntAbsolutePosition;
            //graphics.limbs[3].absoluteHuntPos = p;
        }
    }
}
