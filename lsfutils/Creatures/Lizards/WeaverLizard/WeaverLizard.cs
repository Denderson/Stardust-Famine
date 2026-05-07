using UnityEngine;
using Watcher;
using RWCustom;

namespace lsfUtils.Creatures.Lizards.WeaverLizard;

public class WeaverLizard : Lizard
{
    public WeaverLizard(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        effectColor = RainWorld.SaturatedGold;
        if (rotModule != null && LizardState.rotType != LizardState.RotType.Slight) effectColor = RainWorld.RippleColor;
        abstractCreature.rippleBothSides = true;
        Random.state = state;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new WeaverLizardGraphics(this);

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}
