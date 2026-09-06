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
            RegisterScavFlank();
            RegisterScavMessenger();
            RegisterScavSeer();
            RegisterStarSpawn();
            RegisterStarNoodles();
            RegisterStarJelly();
            RegisterPoisonSpider();
            RegisterClimbGrub();
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
                    s.IsInPack(type, 1f);
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

        public static void RegisterScavFlank()
        {
            CreatureTemplate.Type type = ScavFlank;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "Sfla",
                mapColor = RainWorld.SaturatedGold,
                symbolName = "Kill_Scavenger",
                isHostileForShelter = false,
                isBigForShelter = false,
                performanceCost = 300f,
                unlockID = SandboxUnlockID.ScavFlank,
                RealisedCtor = (abstractCreature, world) => new Scavs.ScavFlank.ScavFlank(abstractCreature, world),
                AbstractAICtor = (world, parent) => new ScavengerAbstractAI(world, parent),
                AICtor = (creature, world) => new ScavengerAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                //StateCtor = (creature) => new CreatureState(creature),
                setTemplate = () =>
                {
                    CreatureTemplate ancestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger);
                    CreatureTemplate template = new(type, ancestor, [], [], new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
                    {
                        name = type.ToString(),
                        AI = true,
                        dangerousToPlayer = 0.5f
                    };

                    return template;
                },
                setRelationships = () =>
                {
                    Relationships relationships = new Relationships(type);
                    List<string> entries = ExtEnum<CreatureTemplate.Type>.values.entries;
                    for (int i = 0; i < entries.Count; i++)
                    {
                        relationships.Ignores(new CreatureTemplate.Type(entries[i], false));
                    }
                    relationships.Attacks(CreatureTemplate.Type.LizardTemplate, 0.5f);
                    relationships.Fears(CreatureTemplate.Type.DaddyLongLegs, 0.5f);
                    relationships.Fears(CreatureTemplate.Type.RedCentipede, 0.5f);
                    relationships.Fears(CreatureTemplate.Type.RedLizard, 0.3f);
                    relationships.Fears(CreatureTemplate.Type.Centiwing, 0.2f);
                    relationships.Fears(CreatureTemplate.Type.BrotherLongLegs, 0.3f);
                    relationships.Fears(CreatureTemplate.Type.BigEel, 0.6f);
                    relationships.IsInPack(CreatureTemplate.Type.Scavenger, 0.7f);
                    relationships.IsInPack(type, 0.5f);
                }
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterScavMessenger()
        {
            CreatureTemplate.Type type = ScavMessenger;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "Smes",
                mapColor = RainWorld.SaturatedGold,
                symbolName = "Kill_Scavenger",
                isHostileForShelter = false,
                isBigForShelter = false,
                performanceCost = 300f,
                unlockID = SandboxUnlockID.ScavMessenger,
                RealisedCtor = (abstractCreature, world) => new Scavs.ScavMessenger.ScavMessenger(abstractCreature, world),
                AbstractAICtor = (world, parent) => new ScavengerAbstractAI(world, parent),
                AICtor = (creature, world) => new ScavengerAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                //StateCtor = (creature) => new CreatureState(creature),
                setTemplate = () =>
                {
                    CreatureTemplate ancestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger);
                    CreatureTemplate template = new(type, ancestor, [], [], new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
                    {
                        name = type.ToString(),
                        AI = true,
                        dangerousToPlayer = 0.5f
                    };

                    return template;
                },
                setRelationships = () =>
                {
                    Relationships relationships = new Relationships(type);
                    List<string> entries = ExtEnum<CreatureTemplate.Type>.values.entries;
                    for (int i = 0; i < entries.Count; i++)
                    {
                        relationships.Ignores(new CreatureTemplate.Type(entries[i], false));
                    }
                    relationships.Attacks(CreatureTemplate.Type.LizardTemplate, 0.5f);
                    relationships.Fears(CreatureTemplate.Type.DaddyLongLegs, 0.5f);
                    relationships.Fears(CreatureTemplate.Type.RedCentipede, 0.5f);
                    relationships.Fears(CreatureTemplate.Type.RedLizard, 0.3f);
                    relationships.Fears(CreatureTemplate.Type.Centiwing, 0.2f);
                    relationships.Fears(CreatureTemplate.Type.BrotherLongLegs, 0.3f);
                    relationships.Fears(CreatureTemplate.Type.BigEel, 0.6f);
                    relationships.IsInPack(CreatureTemplate.Type.Scavenger, 0.7f);
                    relationships.IsInPack(type, 0.5f);
                }
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterScavSeer()
        {
            CreatureTemplate.Type type = ScavSeer;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "Ssee",
                mapColor = RainWorld.SaturatedGold,
                symbolName = "Kill_Scavenger",
                isHostileForShelter = false,
                isBigForShelter = false,
                performanceCost = 300f,
                unlockID = SandboxUnlockID.ScavSeer,
                RealisedCtor = (abstractCreature, world) => new Scavs.ScavSeer.ScavSeer(abstractCreature, world),
                AbstractAICtor = (world, parent) => new ScavengerAbstractAI(world, parent),
                AICtor = (creature, world) => new ScavengerAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                //StateCtor = (creature) => new CreatureState(creature),
                setTemplate = () =>
                {
                    CreatureTemplate ancestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger);
                    CreatureTemplate template = new(type, ancestor, [], [], new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
                    {
                        name = type.ToString(),
                        AI = true,
                        dangerousToPlayer = 0.5f
                    };

                    return template;
                },
                setRelationships = () =>
                {
                    Relationships relationships = new(type);
                    List<string> entries = ExtEnum<CreatureTemplate.Type>.values.entries;
                    for (int i = 0; i < entries.Count; i++)
                    {
                        relationships.Ignores(new CreatureTemplate.Type(entries[i], false));
                    }
                    relationships.Attacks(CreatureTemplate.Type.LizardTemplate, 0.5f);
                    relationships.Fears(CreatureTemplate.Type.DaddyLongLegs, 0.5f);
                    relationships.Fears(CreatureTemplate.Type.RedCentipede, 0.5f);
                    relationships.Fears(CreatureTemplate.Type.RedLizard, 0.3f);
                    relationships.Fears(CreatureTemplate.Type.Centiwing, 0.2f);
                    relationships.Fears(CreatureTemplate.Type.BrotherLongLegs, 0.3f);
                    relationships.Fears(CreatureTemplate.Type.BigEel, 0.6f);
                    relationships.IsInPack(CreatureTemplate.Type.Scavenger, 0.7f);
                    relationships.IsInPack(type, 0.5f);
                }
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterStarSpawn()
        {
            CreatureTemplate.Type type = StarSpawn;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "Star",
                mapColor = RainWorld.RippleColor,
                symbolName = "Kill_Scavenger",
                isHostileForShelter = false,
                isBigForShelter = false,
                performanceCost = 100f,
                unlockID = SandboxUnlockID.StarSpawn,
                RealisedCtor = (abstractCreature, world) => new Spawn.StarSpawn(abstractCreature, world),
                AbstractAICtor = (world, parent) => new AbstractCreatureAI(world, parent),
                AICtor = (creature, world) => new Spawn.StarSpawnAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                //StateCtor = (creature) => new CreatureState(creature),
                setTemplate = () =>
                {
                    List<TileTypeResistance> tRs =
                    [
                        new(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed),
                        new(AItile.Accessibility.Climb, 2f, PathCost.Legality.Allowed),
                        new(AItile.Accessibility.Corridor, 1.5f, PathCost.Legality.Allowed),
                        new(AItile.Accessibility.Solid, 100f, PathCost.Legality.Unallowed)
                    ];

                    List<TileConnectionResistance> cRs =
                    [
                        new(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed),
                        new(MovementConnection.MovementType.OpenDiagonal, 1f, PathCost.Legality.Allowed),
                        new(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed),
                        new(MovementConnection.MovementType.BetweenRooms, 2f, PathCost.Legality.Allowed)
                    ];

                    CreatureTemplate ancestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BigNeedleWorm);
                    CreatureTemplate template = new(type, ancestor, tRs, cRs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
                    {
                        name = type.ToString(),
                        AI = true,
                        dangerousToPlayer = 0.2f,
                        lungCapacity = float.MaxValue,
                        scaryness = 0.5f,
                        smallCreature = false,
                        socialMemory = true,
                        wormGrassImmune = true,
                        communityID = Enums.CreatureCommunityID.StarSpawn,
                        communityInfluence = 1f,
                        shortcutColor = RainWorld.RippleColor,
                        shortcutSegments = 3,
                        offScreenSpeed = 0.1f,
                        abstractedLaziness = 200,
                        roamBetweenRoomsChance = 0.07f,
                        bodySize = 1f,
                        stowFoodInDen = true,
                        grasps = 1,
                        visualRadius = 1200f,
                        movementBasedVision = 0.2f,
                        waterRelationship = CreatureTemplate.WaterRelationship.Amphibious,
                        waterPathingResistance = 2f,
                        canFly = true,
                        meatPoints = 1,
                        baseDamageResistance = 2.5f,
                        baseStunResistance = 2f,
                        ghostSedationImmune = true,
                        //damageRestistances = []
                    };
                    return template;
                },
                setRelationships = () =>
                {
                    Relationships self = new(type);

                    foreach (var template in StaticWorld.creatureTemplates)
                    {
                        if (template.quantified)
                        {
                            self.Ignores(template.type);
                            self.IgnoredBy(template.type);
                        }
                    }

                    self.Ignores(type);

                    self.Eats(CreatureTemplate.Type.Slugcat, 1f);
                    self.Eats(CreatureTemplate.Type.Scavenger, 0.6f);
                    self.Eats(CreatureTemplate.Type.LizardTemplate, 0.3f);
                    self.Eats(CreatureTemplate.Type.CicadaA, 0.4f);

                    self.Intimidates(CreatureTemplate.Type.LizardTemplate, 0.35f);
                    self.Intimidates(CreatureTemplate.Type.CicadaA, 0.3f);

                    self.AttackedBy(CreatureTemplate.Type.Slugcat, 0.2f);
                    self.AttackedBy(CreatureTemplate.Type.Scavenger, 0.2f);

                    self.EatenBy(CreatureTemplate.Type.BigSpider, 0.35f);

                    self.Fears(CreatureTemplate.Type.Spider, 0.2f);
                    self.Fears(CreatureTemplate.Type.BigSpider, 0.2f);
                    self.Fears(CreatureTemplate.Type.SpitterSpider, 0.6f);
                }
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterStarJelly()
        {
            CreatureTemplate.Type type = StarJelly;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "Star",
                mapColor = RainWorld.RippleColor,
                symbolName = "Kill_Scavenger",
                isHostileForShelter = false,
                isBigForShelter = false,
                performanceCost = 100f,
                unlockID = SandboxUnlockID.StarJelly,
                RealisedCtor = (abstractCreature, world) => new Spawn.StarJelly(abstractCreature, world),
                AbstractAICtor = (world, parent) => new AbstractCreatureAI(world, parent),
                AICtor = (creature, world) => new Spawn.StarSpawnAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                //StateCtor = (creature) => new CreatureState(creature),
                setTemplate = () =>
                {
                    List<TileTypeResistance> tRs =
                    [
                        new(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed),
                        new(AItile.Accessibility.Climb, 2f, PathCost.Legality.Allowed),
                        new(AItile.Accessibility.Corridor, 1.5f, PathCost.Legality.Allowed),
                        new(AItile.Accessibility.Solid, 100f, PathCost.Legality.Unallowed)
                    ];

                    List<TileConnectionResistance> cRs =
                    [
                        new(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed),
                        new(MovementConnection.MovementType.OpenDiagonal, 1f, PathCost.Legality.Allowed),
                        new(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed),
                        new(MovementConnection.MovementType.BetweenRooms, 2f, PathCost.Legality.Allowed)
                    ];

                    CreatureTemplate ancestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BigNeedleWorm);
                    CreatureTemplate template = new(type, ancestor, tRs, cRs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
                    {
                        name = type.ToString(),
                        AI = true,
                        dangerousToPlayer = 0.2f,
                        lungCapacity = float.MaxValue,
                        scaryness = 0.5f,
                        smallCreature = false,
                        socialMemory = true,
                        wormGrassImmune = true,
                        communityID = Enums.CreatureCommunityID.StarSpawn,
                        communityInfluence = 1f,
                        shortcutColor = RainWorld.RippleColor,
                        shortcutSegments = 3,
                        offScreenSpeed = 0.1f,
                        abstractedLaziness = 200,
                        roamBetweenRoomsChance = 0.07f,
                        bodySize = 1f,
                        stowFoodInDen = true,
                        grasps = 1,
                        visualRadius = 1200f,
                        movementBasedVision = 0.2f,
                        waterRelationship = CreatureTemplate.WaterRelationship.Amphibious,
                        waterPathingResistance = 2f,
                        canFly = true,
                        meatPoints = 1,
                        baseDamageResistance = 2.5f,
                        baseStunResistance = 2f,
                        ghostSedationImmune = true,
                        //damageRestistances = []
                    };
                    return template;
                },
                setRelationships = () =>
                {
                    Relationships self = new(type);

                    foreach (var template in StaticWorld.creatureTemplates)
                    {
                        if (template.quantified)
                        {
                            self.Ignores(template.type);
                            self.IgnoredBy(template.type);
                        }
                    }

                    self.Ignores(type);

                    self.Eats(CreatureTemplate.Type.Slugcat, 1f);
                    self.Eats(CreatureTemplate.Type.Scavenger, 0.6f);
                    self.Eats(CreatureTemplate.Type.LizardTemplate, 0.3f);
                    self.Eats(CreatureTemplate.Type.CicadaA, 0.4f);

                    self.Intimidates(CreatureTemplate.Type.LizardTemplate, 0.35f);
                    self.Intimidates(CreatureTemplate.Type.CicadaA, 0.3f);

                    self.AttackedBy(CreatureTemplate.Type.Slugcat, 0.2f);
                    self.AttackedBy(CreatureTemplate.Type.Scavenger, 0.2f);

                    self.EatenBy(CreatureTemplate.Type.BigSpider, 0.35f);

                    self.Fears(CreatureTemplate.Type.Spider, 0.2f);
                    self.Fears(CreatureTemplate.Type.BigSpider, 0.2f);
                    self.Fears(CreatureTemplate.Type.SpitterSpider, 0.6f);
                }
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterStarNoodles()
        {
            CreatureTemplate.Type type = StarNoodles;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "Star",
                mapColor = RainWorld.RippleColor,
                symbolName = "Kill_Scavenger",
                isHostileForShelter = false,
                isBigForShelter = false,
                performanceCost = 100f,
                unlockID = SandboxUnlockID.StarNoodles,
                RealisedCtor = (abstractCreature, world) => new Spawn.StarNoodle(abstractCreature, world),
                AbstractAICtor = (world, parent) => new AbstractCreatureAI(world, parent),
                AICtor = (creature, world) => new Spawn.StarSpawnAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                //StateCtor = (creature) => new CreatureState(creature),
                setTemplate = () =>
                {
                    List<TileTypeResistance> tRs =
                    [
                        new(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed),
                        new(AItile.Accessibility.Climb, 2f, PathCost.Legality.Allowed),
                        new(AItile.Accessibility.Corridor, 1.5f, PathCost.Legality.Allowed),
                        new(AItile.Accessibility.Solid, 100f, PathCost.Legality.Unallowed)
                    ];

                    List<TileConnectionResistance> cRs =
                    [
                        new(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed),
                        new(MovementConnection.MovementType.OpenDiagonal, 1f, PathCost.Legality.Allowed),
                        new(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed),
                        new(MovementConnection.MovementType.BetweenRooms, 2f, PathCost.Legality.Allowed)
                    ];

                    CreatureTemplate ancestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BigNeedleWorm);
                    CreatureTemplate template = new(type, ancestor, tRs, cRs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
                    {
                        name = type.ToString(),
                        AI = true,
                        dangerousToPlayer = 0.2f,
                        lungCapacity = float.MaxValue,
                        scaryness = 0.5f,
                        smallCreature = false,
                        socialMemory = true,
                        wormGrassImmune = true,
                        communityID = Enums.CreatureCommunityID.StarSpawn,
                        communityInfluence = 1f,
                        shortcutColor = RainWorld.RippleColor,
                        shortcutSegments = 3,
                        offScreenSpeed = 0.1f,
                        abstractedLaziness = 200,
                        roamBetweenRoomsChance = 0.07f,
                        bodySize = 1f,
                        stowFoodInDen = true,
                        grasps = 1,
                        visualRadius = 1200f,
                        movementBasedVision = 0.2f,
                        waterRelationship = CreatureTemplate.WaterRelationship.Amphibious,
                        waterPathingResistance = 2f,
                        canFly = true,
                        meatPoints = 1,
                        baseDamageResistance = 2.5f,
                        baseStunResistance = 2f,
                        ghostSedationImmune = true,
                        //damageRestistances = []
                    };
                    return template;
                },
                setRelationships = () =>
                {
                    Relationships self = new(type);

                    foreach (var template in StaticWorld.creatureTemplates)
                    {
                        if (template.quantified)
                        {
                            self.Ignores(template.type);
                            self.IgnoredBy(template.type);
                        }
                    }

                    self.Ignores(type);

                    self.Eats(CreatureTemplate.Type.Slugcat, 1f);
                    self.Eats(CreatureTemplate.Type.Scavenger, 0.6f);
                    self.Eats(CreatureTemplate.Type.LizardTemplate, 0.3f);
                    self.Eats(CreatureTemplate.Type.CicadaA, 0.4f);

                    self.Intimidates(CreatureTemplate.Type.LizardTemplate, 0.35f);
                    self.Intimidates(CreatureTemplate.Type.CicadaA, 0.3f);

                    self.AttackedBy(CreatureTemplate.Type.Slugcat, 0.2f);
                    self.AttackedBy(CreatureTemplate.Type.Scavenger, 0.2f);

                    self.EatenBy(CreatureTemplate.Type.BigSpider, 0.35f);

                    self.Fears(CreatureTemplate.Type.Spider, 0.2f);
                    self.Fears(CreatureTemplate.Type.BigSpider, 0.2f);
                    self.Fears(CreatureTemplate.Type.SpitterSpider, 0.6f);
                }
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterPoisonSpider()
        {
            CreatureTemplate.Type type = PoisonSpider;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "posp",
                mapColor = Colors.PoisonLizardColor,
                symbolName = "Kill_BigSpider",
                isHostileForShelter = true,
                isBigForShelter = false,
                performanceCost = 50f,
                unlockID = SandboxUnlockID.PoisonSpider,
                RealisedCtor = (abstractCreature, world) => new Spiders.PoisonSpider.PoisonSpider(abstractCreature, world),
                AbstractAICtor = (world, parent) => new AbstractCreatureAI(world, parent),
                AICtor = (creature, world) => new BigSpiderAI(creature, world),
                //Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                //StateCtor = (creature) => new CreatureState(creature),
                setTemplate = () =>
                {
                    CreatureTemplate ancestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BigSpider);
                    CreatureTemplate template = new(type, ancestor, [], [], new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
                    {
                        name = type.ToString(),
                        AI = true,
                        dangerousToPlayer = 0.2f
                    };
                    return template;
                },
                setRelationships = () =>
                {
                    Relationships relationships = new(type);
                    List<string> entries = ExtEnum<CreatureTemplate.Type>.values.entries;
                    for (int i = 0; i < entries.Count; i++)
                    {
                        relationships.Ignores(new CreatureTemplate.Type(entries[i], false));
                    }
                    relationships.Attacks(CreatureTemplate.Type.LizardTemplate, 0.5f);
                    relationships.Fears(CreatureTemplate.Type.DaddyLongLegs, 0.5f);
                    relationships.Fears(CreatureTemplate.Type.RedCentipede, 0.5f);
                    relationships.Fears(CreatureTemplate.Type.RedLizard, 0.3f);
                    relationships.Fears(CreatureTemplate.Type.Centiwing, 0.2f);
                    relationships.Fears(CreatureTemplate.Type.BrotherLongLegs, 0.3f);
                    relationships.Fears(CreatureTemplate.Type.BigEel, 0.6f);
                    relationships.Eats(CreatureTemplate.Type.Centipede, 0.2f);
                    relationships.Eats(CreatureTemplate.Type.LanternMouse, 0.1f);
                    relationships.Eats(CreatureTemplate.Type.Slugcat, 0.2f);
                    relationships.Eats(CreatureTemplate.Type.Scavenger, 0.6f);
                    relationships.AttackedBy(CreatureTemplate.Type.Scavenger, 0.7f);
                    relationships.Ignores(type);
                }
            };
            CreatureRegistryTemplate.Register(entry);
        }

        public static void RegisterClimbGrub()
        {
            CreatureTemplate.Type type = ClimbGrub;
            var entry = new CreatureRegistryEntry(type, [type.ToString()])
            {
                mapName = "clgb",
                mapColor = ClimbGrubColor,
                symbolName = "Kill_Tubeworm",
                isHostileForShelter = false,
                isBigForShelter = false,
                performanceCost = 20f,
                unlockID = SandboxUnlockID.ClimbGrub,
                RealisedCtor = (abstractCreature, world) => new Worm.ClimbGrub(abstractCreature, world),
                AbstractAICtor = (world, parent) => new AbstractCreatureAI(world, parent),
                AICtor = (creature, world) => new TubeWormAI(creature, world),
                Grabability = (player, physicalObject) => Player.ObjectGrabability.OneHand,
                //StateCtor = (creature) => new CreatureState(creature),
                setTemplate = () =>
                {
                    CreatureTemplate ancestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.TubeWorm);
                    CreatureTemplate template = new(type, ancestor, [], [], new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
                    {
                        name = type.ToString(),
                        AI = true,
                        dangerousToPlayer = 0f
                    };
                    return template;
                },
                setRelationships = () =>
                {
                    Relationships relationships = new(type);
                    List<string> entries = ExtEnum<CreatureTemplate.Type>.values.entries;
                    for (int i = 0; i < entries.Count; i++)
                    {
                        relationships.Ignores(new CreatureTemplate.Type(entries[i], false));
                    }
                    relationships.Ignores(type);
                }
            };
            CreatureRegistryTemplate.Register(entry);
        }
    }
}