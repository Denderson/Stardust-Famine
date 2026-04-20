using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Creatures.Lizards.MonitorLizard;

public class MonitorLizardCritob : Critob
{
    public MonitorLizardCritob() : base(Enums.CreatureTemplateType.MonitorLizard)
    {
        Icon = new SimpleIcon("atlases/Kill_MonitorLizard", new Color(0.89f, 0.98f, 0.37f));
        LoadedPerformanceCost = 50f;
        SandboxPerformanceCost = new(.25f, .25f);
        RegisterUnlock(KillScore.Configurable(1), Enums.SandboxUnlockID.MonitorLizard, MultiplayerUnlocks.SandboxUnlockID.Slugcat);
    }

    public override int ExpeditionScore() => 10;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new Color(0.89f, 0.98f, 0.37f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "MLz";

    public override IEnumerable<string> WorldFileAliases() => ["monitorlizard", "monitor lizard"];

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.Lizards];

    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard));

    public override void EstablishRelationships()
    {
        var s = new Relationships(Type);
        s.Ignores(CreatureTemplate.Type.LizardTemplate);
        s.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, .5f);

        s.Fears(CreatureTemplate.Type.Vulture, .5f);
        s.Fears(CreatureTemplate.Type.Vulture, .3f);
        s.Fears(CreatureTemplate.Type.KingVulture, 1f);
        s.EatenBy(CreatureTemplate.Type.KingVulture, 0.5f);

        s.Fears(CreatureTemplate.Type.BigEel, 1f);
        s.EatenBy(CreatureTemplate.Type.BigEel, 1f);

        s.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        s.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 1f);

        s.Fears(CreatureTemplate.Type.RedCentipede, .9f);
        s.Fears(CreatureTemplate.Type.TentaclePlant, .2f);

        s.Eats(CreatureTemplate.Type.Centipede, .8f);
        s.Fears(CreatureTemplate.Type.TubeWorm, .5f);
        s.Fears(CreatureTemplate.Type.Hazer, .5f);
        s.Eats(CreatureTemplate.Type.Scavenger, .8f);
        s.Eats(CreatureTemplate.Type.CicadaA, .05f);
        s.Eats(CreatureTemplate.Type.LanternMouse, .3f);
        s.Eats(CreatureTemplate.Type.BigSpider, .35f);
        s.Eats(CreatureTemplate.Type.EggBug, .45f);
        s.Fears(CreatureTemplate.Type.JetFish, .5f);
        s.Eats(CreatureTemplate.Type.BigNeedleWorm, .25f);
        s.Eats(CreatureTemplate.Type.SmallNeedleWorm, .5f);
        s.Eats(CreatureTemplate.Type.DropBug, .2f);

        s.FearedBy(CreatureTemplate.Type.LanternMouse, .7f);

        s.FearedBy(CreatureTemplate.Type.CicadaA, .3f);
        s.FearedBy(CreatureTemplate.Type.JetFish, .2f);
        s.FearedBy(CreatureTemplate.Type.Slugcat, 1f);
        s.FearedBy(CreatureTemplate.Type.Scavenger, .5f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new LizardAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new MonitorLizard(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }
    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.PinkLizard;
}
