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

        if (this.abstractCreature.superSizeMe)
        {
            this.lizardParams.stunToughness = 60f;
            this.lizardParams.swimSpeed = 4f;
            this.Template.waterVision = 2.5f;
            this.Template.throughSurfaceVision = 2.5f;
            this.Template.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
            this.Template.canSwim = true;
            this.lizardParams.headGraphics = new int[]
            {
                8,
                8,
                8,
                8,
                8
            };

        }
        else
        {
            this.lizardParams.stunToughness = 2.5f;
            this.lizardParams.swimSpeed = 1.2f;
            this.Template.waterVision = .2f;
            this.Template.throughSurfaceVision = .2f;
            this.lizardParams.headGraphics = new int[5];
        }
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new RaspberryLizardGraphics(this);

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}
