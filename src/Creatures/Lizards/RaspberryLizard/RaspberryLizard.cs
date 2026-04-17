using UnityEngine;
using Watcher;
using RWCustom;

namespace lsfUtils.Creatures.Lizards.RaspberryLizard;

public class RaspberryLizard : Lizard
{
    public RaspberryLizard(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0 / 360f, 10 / 360f, 0.5f), .85f, Custom.ClampedRandomVariation(.4f, .050f, .5f));
        abstractCreature.personality.dominance = Random.value * 0.1f + 0.9f;
        Random.state = state;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new RaspberryLizardGraphics(this);

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}
