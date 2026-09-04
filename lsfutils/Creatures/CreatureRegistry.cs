using DevInterface;
using Fisobs.Creatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static lsfUtils.Enums;
using static lsfUtils.Enums.Colors;
using static lsfUtils.Enums.CreatureTemplateType;
using static lsfUtils.Plugin;

namespace lsfUtils.Creatures
{
    public static class CreatureRegistry
    {
        public static void RegisterAll()
        {
            RegisterAirplaneLizard();
            RegisterFlameLizard();
            RegisterMonitorLizard();
            RegisterPoisonLizard();
            RegisterRaspberryLizard();
            RegisterStarnosedLizard();
            RegisterWeaverLizard();
        }

        public static CreatureTemplate LizardTemplate(CreatureTemplate.Type type)
        {
            return LizardBreeds.BreedTemplate(type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard));
        }

        public static void RegisterAirplaneLizard()
        {
            CreatureTemplate.Type type = AirplaneLizard;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "alz",
                mapColor = AirplaneLizardColor,
                symbolName = "Kill_White_Lizard",
                isHostileForShelter = true,
                isBigForShelter = false,
                performanceCost = 50f,
                unlockID = SandboxUnlockID.AirplaneLizard,
                roomAttractivenessCategories = [RoomAttractivenessPanel.Category.Lizards],
                //AbstractCtor = (self, world, pos) => new AbstractCreature(world, template, null, pos, world.game.GetNewID()),
                RealisedCtor = (abstractCreature, world) => new Lizards.AirplaneLizard.AirplaneLizard(abstractCreature, world),
                AICtor = (creature, world) => new LizardAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                StateCtor = (creature) => new LizardState(creature),
                setTemplate = () => LizardTemplate(type),
                setRelationships = () =>
                {
                    var s = new Relationships(AirplaneLizard);
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
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterFlameLizard()
        {
            CreatureTemplate.Type type = FlameLizard;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "flz",
                mapColor = FlameLizardColor,
                symbolName = "Kill_Standard_Lizard",
                isHostileForShelter = true,
                isBigForShelter = false,
                performanceCost = 50f,
                unlockID = SandboxUnlockID.FlameLizard,
                roomAttractivenessCategories = [RoomAttractivenessPanel.Category.Lizards],
                //AbstractCtor = (self, world, pos) => new AbstractCreature(world, template, null, pos, world.game.GetNewID()),
                RealisedCtor = (abstractCreature, world) => new Lizards.FlameLizard.FlameLizard(abstractCreature, world),
                AICtor = (creature, world) => new LizardAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                StateCtor = (creature) => new LizardState(creature),
                setTemplate = () => LizardTemplate(type),
                setRelationships = () => 
                {
                    var s = new Relationships(type);
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
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterMonitorLizard()
        {
            CreatureTemplate.Type type = MonitorLizard;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "mlz",
                mapColor = MonitorLizardColor,
                symbolName = "atlases/Kill_MonitorLizard",
                isHostileForShelter = true,
                isBigForShelter = false,
                performanceCost = 50f,
                unlockID = SandboxUnlockID.MonitorLizard,
                roomAttractivenessCategories = [RoomAttractivenessPanel.Category.Lizards],
                //AbstractCtor = (self, world, pos) => new AbstractCreature(world, template, null, pos, world.game.GetNewID()),
                RealisedCtor = (abstractCreature, world) => new Lizards.MonitorLizard.MonitorLizard(abstractCreature, world),
                AICtor = (creature, world) => new LizardAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                StateCtor = (creature) => new LizardState(creature),
                setTemplate = () => LizardTemplate(type),
                setRelationships = () =>
                {
                    var s = new Relationships(type);
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
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterPoisonLizard()
        {
            CreatureTemplate.Type type = PoisonLizard;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "plz",
                mapColor = PoisonLizardColor,
                symbolName = "Kill_Green_Lizard",
                isHostileForShelter = true,
                isBigForShelter = false,
                performanceCost = 50f,
                unlockID = SandboxUnlockID.PoisonLizard,
                roomAttractivenessCategories = [RoomAttractivenessPanel.Category.Lizards],
                //AbstractCtor = (self, world, pos) => new AbstractCreature(world, template, null, pos, world.game.GetNewID()),
                RealisedCtor = (abstractCreature, world) => new Lizards.PoisonLizard.PoisonLizard(abstractCreature, world),
                AICtor = (creature, world) => new LizardAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                StateCtor = (creature) => new LizardState(creature),
                setTemplate = () => LizardTemplate(type),
                setRelationships = () =>
                {
                    var s = new Relationships(type);
                    s.Ignores(CreatureTemplate.Type.LizardTemplate);
                    s.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, .5f);

                    s.Fears(CreatureTemplate.Type.Vulture, .8f);
                    s.EatenBy(CreatureTemplate.Type.Vulture, .8f);
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
                    s.Eats(CreatureTemplate.Type.CicadaA, .1f);
                    s.Eats(CreatureTemplate.Type.LanternMouse, .3f);
                    s.Eats(CreatureTemplate.Type.BigSpider, .35f);
                    s.Eats(CreatureTemplate.Type.EggBug, .45f);
                    s.Fears(CreatureTemplate.Type.JetFish, .9f);
                    s.Eats(CreatureTemplate.Type.BigNeedleWorm, .25f);
                    s.Eats(CreatureTemplate.Type.SmallNeedleWorm, .5f);
                    s.Eats(CreatureTemplate.Type.DropBug, .4f);

                    s.FearedBy(CreatureTemplate.Type.LanternMouse, .7f);

                    s.FearedBy(CreatureTemplate.Type.CicadaA, .3f);
                    s.FearedBy(CreatureTemplate.Type.JetFish, .2f);
                    s.FearedBy(CreatureTemplate.Type.Slugcat, 1f);
                    s.FearedBy(CreatureTemplate.Type.Scavenger, .5f);
                }
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterRaspberryLizard()
        {
            CreatureTemplate.Type type = RaspberryLizard;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "rlz",
                mapColor = RaspberryLizardColor,
                symbolName = "Kill_Yellow_Lizard",
                isHostileForShelter = true,
                isBigForShelter = false,
                performanceCost = 50f,
                unlockID = SandboxUnlockID.RaspberryLizard,
                roomAttractivenessCategories = [RoomAttractivenessPanel.Category.Lizards],
                //AbstractCtor = (self, world, pos) => new AbstractCreature(world, template, null, pos, world.game.GetNewID()),
                RealisedCtor = (abstractCreature, world) => new Lizards.RaspberryLizard.RaspberryLizard(abstractCreature, world),
                AICtor = (creature, world) => new LizardAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                StateCtor = (creature) => new LizardState(creature),
                setTemplate = () => LizardTemplate(type),
                setRelationships = () =>
                {
                    var s = new Relationships(type);
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
                    s.IsInPack(CreatureTemplate.Type.YellowLizard, 1f);
                    s.IsInPack(Enums.CreatureTemplateType.RaspberryLizard, 1f);
                    if (ModManager.DLCShared)
                    {
                        s.IgnoredBy(DLCSharedEnums.CreatureTemplateType.ZoopLizard);
                        s.Ignores(DLCSharedEnums.CreatureTemplateType.ZoopLizard);
                    }
                }
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterStarnosedLizard()
        {
            CreatureTemplate.Type type = StarNosedLizard;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "slz",
                mapColor = StarNosedLizardColor,
                symbolName = "atlases/Kill_StarNosedLizard",
                isHostileForShelter = true,
                isBigForShelter = false,
                performanceCost = 50f,
                unlockID = SandboxUnlockID.StarNosedLizard,
                roomAttractivenessCategories = [RoomAttractivenessPanel.Category.Lizards],
                //AbstractCtor = (self, world, pos) => new AbstractCreature(world, template, null, pos, world.game.GetNewID()),
                RealisedCtor = (abstractCreature, world) => new Lizards.StarNosedLizard.StarNosedLizard(abstractCreature, world),
                AICtor = (creature, world) => new LizardAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                StateCtor = (creature) => new LizardState(creature),
                setTemplate = () => LizardTemplate(type),
                setRelationships = () =>
                {
                    var s = new Relationships(type);
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
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterWeaverLizard()
        {
            CreatureTemplate.Type type = WeaverLizard;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "wlz",
                mapColor = WeaverLizardColor,
                symbolName = "Kill_Standard_Lizard",
                isHostileForShelter = true,
                isBigForShelter = false,
                performanceCost = 50f,
                unlockID = SandboxUnlockID.WeaverLizard,
                roomAttractivenessCategories = [RoomAttractivenessPanel.Category.Lizards],
                //AbstractCtor = (self, world, pos) => new AbstractCreature(world, template, null, pos, world.game.GetNewID()),
                RealisedCtor = (abstractCreature, world) => new Lizards.WeaverLizard.WeaverLizard(abstractCreature, world),
                AICtor = (creature, world) => new LizardAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                StateCtor = (creature) => new LizardState(creature),
                setTemplate = () => LizardTemplate(type),
                setRelationships = () =>
                {
                    var s = new Relationships(type);
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
            };
            CreatureRegistryTemplate.Register(entry);
        }
    }
}