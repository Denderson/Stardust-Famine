using Fisobs.Creatures;
using Fisobs.Core;
using Fisobs.Sandbox;
using UnityEngine;
using System.Collections.Generic;
using DevInterface;
using IL.Watcher;
using On.Watcher;

namespace lsfUtils.Creatures.Lizards.FlameLizard;


public class FlameLizardCritsob : Critob
{
    public FlameLizardCritsob() : base(Enums.CreatureTemplateType.FlameLizard)
    {
        Icon = new SimpleIcon("Kill_Standard_Lizard", new Color(0.41f, 0.04f, 0.04f));
        LoadedPerformanceCost = 50f;
        SandboxPerformanceCost = new(.25f, .25f);
        RegisterUnlock(KillScore.Configurable(1), Enums.SandboxUnlockID.FlameLizard, MultiplayerUnlocks.SandboxUnlockID.Slugcat);
    }
    public override int ExpeditionScore() => 20;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new Color(0.41f, 0.04f, 0.04f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "FLz";
    public override IEnumerable<string> WorldFileAliases() => new string[] { "Flamelizard", "Flame lizard" };
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new RoomAttractivenessPanel.Category[] { RoomAttractivenessPanel.Category.Lizards };

    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard));

    public override void EstablishRelationships()
    {
        var s = new Relationships(Type);
        s.Ignores(CreatureTemplate.Type.LizardTemplate);
        s.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, .5f);
        s.Ignores(CreatureTemplate.Type.Vulture);
        s.Eats(CreatureTemplate.Type.KingVulture, 1f);
        s.Ignores(CreatureTemplate.Type.TubeWorm);
        s.Eats(CreatureTemplate.Type.Scavenger, .8f);
        s.Eats(CreatureTemplate.Type.CicadaA, .05f);
        s.Eats(CreatureTemplate.Type.LanternMouse, .3f);
        s.Eats(CreatureTemplate.Type.BigSpider, .35f);
        s.Eats(CreatureTemplate.Type.EggBug, .45f);
        s.Ignores(CreatureTemplate.Type.JetFish);
        s.Fears(CreatureTemplate.Type.BigEel, 1f);
        s.Eats(CreatureTemplate.Type.Centipede, .8f);
        s.Eats(CreatureTemplate.Type.BigNeedleWorm, .25f);
        s.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        s.Ignores(CreatureTemplate.Type.SmallNeedleWorm);
        s.Eats(CreatureTemplate.Type.DropBug, .2f);
        s.Fears(CreatureTemplate.Type.RedCentipede, .9f);
        s.Fears(CreatureTemplate.Type.TentaclePlant, .2f);
        s.Ignores(CreatureTemplate.Type.Hazer);
        s.FearedBy(CreatureTemplate.Type.LanternMouse, .7f);
        s.IgnoredBy(CreatureTemplate.Type.Vulture);
        s.FearedBy(CreatureTemplate.Type.CicadaA, .3f);
        s.FearedBy(CreatureTemplate.Type.JetFish, .2f);
        s.FearedBy(CreatureTemplate.Type.Slugcat, 1f);
        s.FearedBy(CreatureTemplate.Type.Scavenger, .5f);
        s.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 1f);
        if (ModManager.DLCShared)
        {
            s.IgnoredBy(DLCSharedEnums.CreatureTemplateType.ZoopLizard);
            s.Ignores(DLCSharedEnums.CreatureTemplateType.ZoopLizard);
        }
    }
    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new LizardAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new FlameLizard(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }
    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.PinkLizard;
}
