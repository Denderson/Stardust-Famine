using UnityEngine;
using Watcher;
using RWCustom;
using System.Runtime.CompilerServices;

namespace lsfUtils.Creatures.Lizards.MonitorLizard;

public class MonitorLizard : Lizard
{
    public MonitorLizard(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        Debug.Log("Monitor Lizard ctor: ");
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        if (IsAlbino())
            effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.18f, 1 / 12f, .5f), 0.6f, Custom.ClampedRandomVariation(.3f, .15f, .5f));
        else
            effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(73f / 360f, 10 / 360f, .5f), 0.8f, Custom.ClampedRandomVariation(.7f, .1f, .5f));
        Random.state = state;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new MonitorLizardGraphics(this);

    public override void LoseAllGrasps() => ReleaseGrasp(0);

    public bool IsAlbino() => abstractCreature.ID.number % 5 == 0;
}
