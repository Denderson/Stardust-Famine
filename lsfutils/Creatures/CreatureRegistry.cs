using DevInterface;
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
            //RegisterScavFlank();
            //RegisterScavMessenger();
            //RegisterScavSeer();
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
                Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                StateCtor = (creature) => new LizardState(creature),
                setTemplate = () => LizardTemplate(type),
                setRelationships = () =>
                {
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType("slugcat")) { type.RelationshipBased(type1, .5f);}
                        else if (type1.IsType("vulture")) { type.Ignores(type1, 0.5f);}
                        else if (type1.IsType("kingvulture")) type.Eats(type1, 1f);
                        else if (type1.IsType("tubeworm")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("scavenger")) { type.Eats(type1, .8f);}
                        else if (type1.IsType("cicadaa")) { type.Eats(type1, .05f); }
                        else if (type1.IsType("lanternmouse")) { type.Eats(type1, .3f); }
                        else if (type1.IsType("bigspider")) type.Eats(type1, .35f);
                        else if (type1.IsType("eggbug")) type.Eats(type1, .45f);
                        else if (type1.IsType("jetfish")) { type.Ignores(type1, 0.5f); }
                        else if (type1.IsType("bigeel")) type.EatenBy(type1, 1f);
                        else if (type1.IsType("centipede")) type.Eats(type1, .8f);
                        else if (type1.IsType("bigneedleworm")) type.Eats(type1, .25f);
                        else if (type1.IsType("daddylonglegs")) { type.EatenBy(type1, 1f);}
                        else if (type1.IsType("smallneedleworm")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("dropbug")) type.Eats(type1, .2f);
                        else if (type1.IsType("redcentipede")) type.EatenBy(type1, .9f);
                        else if (type1.IsType("tentacleplant")) type.EatenBy(type1, .2f);
                        else if (type1.IsType("hazer")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("zooplizard")) { type.Ignores(type1, 0.5f); }
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
                Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                StateCtor = (creature) => new LizardState(creature),
                setTemplate = () => LizardTemplate(type),
                setRelationships = () =>
                {
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType("slugcat")) { type.RelationshipBased(type1, .5f); }
                        else if (type1.IsType("vulture")) { type.Ignores(type1, 0.5f); }
                        else if (type1.IsType("kingvulture")) type.Eats(type1, 1f);
                        else if (type1.IsType("tubeworm")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("scavenger")) { type.Eats(type1, .8f); }
                        else if (type1.IsType("cicadaa")) { type.Eats(type1, .05f); }
                        else if (type1.IsType("lanternmouse")) { type.Eats(type1, .3f); }
                        else if (type1.IsType("bigspider")) type.Eats(type1, .35f);
                        else if (type1.IsType("eggbug")) type.Eats(type1, .45f);
                        else if (type1.IsType("jetfish")) { type.Ignores(type1, 0.5f); }
                        else if (type1.IsType("bigeel")) type.EatenBy(type1, 1f);
                        else if (type1.IsType("centipede")) type.Eats(type1, .8f);
                        else if (type1.IsType("bigneedleworm")) type.Eats(type1, .25f);
                        else if (type1.IsType("daddylonglegs")) { type.EatenBy(type1, 1f); }
                        else if (type1.IsType("smallneedleworm")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("dropbug")) type.Eats(type1, .2f);
                        else if (type1.IsType("redcentipede")) type.EatenBy(type1, .9f);
                        else if (type1.IsType("tentacleplant")) type.EatenBy(type1, .2f);
                        else if (type1.IsType("hazer")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("zooplizard")) { type.Ignores(type1, 0.5f); }
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
                Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                StateCtor = (creature) => new LizardState(creature),
                setTemplate = () => LizardTemplate(type),
                setRelationships = () =>
                {
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType("slugcat")) type.RelationshipBased(type1, .5f);
                        else if (type1.IsType("vulture")) { type.EatenBy(type1, .5f); type.EatenBy(type1, .3f); }
                        else if (type1.IsType("kingvulture")) { type.EatenBy(type1, 1f); type.EatenBy(type1, .5f); }
                        else if (type1.IsType("bigeel")) type.EatenBy(type1, 1f);
                        else if (type1.IsType("daddylonglegs")) type.EatenBy(type1, 1f);
                        else if (type1.IsType("redcentipede")) type.EatenBy(type1, .9f);
                        else if (type1.IsType("tentacleplant")) type.EatenBy(type1, .2f);
                        else if (type1.IsType("centipede")) type.Eats(type1, .8f);
                        else if (type1.IsType("tubeworm")) type.EatenBy(type1, .5f);
                        else if (type1.IsType("hazer")) type.EatenBy(type1, .5f);
                        else if (type1.IsType("scavenger")) type.Eats(type1, .8f);
                        else if (type1.IsType("cicadaa")) type.Eats(type1, .05f);
                        else if (type1.IsType("lanternmouse")) type.Eats(type1, .3f);
                        else if (type1.IsType("bigspider")) type.Eats(type1, .35f);
                        else if (type1.IsType("eggbug")) type.Eats(type1, .45f);
                        else if (type1.IsType("jetfish")) type.EatenBy(type1, .5f);
                        else if (type1.IsType("bigneedleworm")) type.Eats(type1, .25f);
                        else if (type1.IsType("smallneedleworm")) type.Eats(type1, .5f);
                        else if (type1.IsType("dropbug")) type.Eats(type1, .2f);
                    }
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
                Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                StateCtor = (creature) => new LizardState(creature),
                setTemplate = () => LizardTemplate(type),
                setRelationships = () =>
                {
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType("slugcat")) type.RelationshipBased(type1, .5f);
                        else if (type1.IsType("vulture")) type.EatenBy(type1, .8f);
                        else if (type1.IsType("kingvulture")) { type.EatenBy(type1, 1f);}
                        else if (type1.IsType("bigeel")) type.EatenBy(type1, 1f);
                        else if (type1.IsType("daddylonglegs")) type.EatenBy(type1, 1f);
                        else if (type1.IsType("redcentipede")) type.EatenBy(type1, .9f);
                        else if (type1.IsType("tentacleplant")) type.EatenBy(type1, .2f);
                        else if (type1.IsType("centipede")) type.Eats(type1, .8f);
                        else if (type1.IsType("tubeworm")) type.EatenBy(type1, .5f);
                        else if (type1.IsType("hazer")) type.EatenBy(type1, .5f);
                        else if (type1.IsType("scavenger")) type.Eats(type1, .8f);
                        else if (type1.IsType("cicadaa")) type.Eats(type1, .1f);
                        else if (type1.IsType("lanternmouse")) type.Eats(type1, .3f);
                        else if (type1.IsType("bigspider")) type.Eats(type1, .35f);
                        else if (type1.IsType("eggbug")) type.Eats(type1, .45f);
                        else if (type1.IsType("jetfish")) type.EatenBy(type1, .9f);
                        else if (type1.IsType("bigneedleworm")) type.Eats(type1, .25f);
                        else if (type1.IsType("smallneedleworm")) type.Eats(type1, .5f);
                        else if (type1.IsType("dropbug")) type.Eats(type1, .4f);
                    }
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
                Grabability = (player, physicalObject) => Player.ObjectGrabability.CantGrab,
                StateCtor = (creature) => new LizardState(creature),
                setTemplate = () => LizardTemplate(type),
                setRelationships = () =>
                {
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null || !t.quantified) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType("slugcat")) type.RelationshipBased(type1, .5f);
                        else if (type1.IsType("vulture")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("kingvulture")) type.Eats(type1, 1f);
                        else if (type1.IsType("tubeworm")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("scavenger")) type.Eats(type1, .8f);
                        else if (type1.IsType("cicadaa")) type.Eats(type1, .05f);
                        else if (type1.IsType("lanternmouse")) type.Eats(type1, .3f);
                        else if (type1.IsType("bigspider")) type.Eats(type1, .35f);
                        else if (type1.IsType("eggbug")) type.Eats(type1, .45f);
                        else if (type1.IsType("jetfish")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("bigeel")) type.EatenBy(type1, 1f);
                        else if (type1.IsType("centipede")) type.Eats(type1, .8f);
                        else if (type1.IsType("bigneedleworm")) type.Eats(type1, .25f);
                        else if (type1.IsType("daddylonglegs")) type.EatenBy(type1, 1f);
                        else if (type1.IsType("smallneedleworm")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("dropbug")) type.Eats(type1, .2f);
                        else if (type1.IsType("redcentipede")) type.EatenBy(type1, .9f);
                        else if (type1.IsType("tentacleplant")) type.EatenBy(type1, .2f);
                        else if (type1.IsType("hazer")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("yellowlizard")) type.PackWith(type1, 1f);
                        else if (type1.IsType("zooplizard")) type.Ignores(type1, 0.5f);
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
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null || !t.quantified) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType("slugcat")) type.RelationshipBased(type1, .5f);
                        else if (type1.IsType("vulture")) { type.EatenBy(type1, .5f);}
                        else if (type1.IsType("kingvulture")) { type.EatenBy(type1, 1f);}
                        else if (type1.IsType("bigeel")) type.EatenBy(type1, 1f);
                        else if (type1.IsType("daddylonglegs")) type.EatenBy(type1, 1f);
                        else if (type1.IsType("redcentipede")) type.EatenBy(type1, .9f);
                        else if (type1.IsType("tentacleplant")) type.EatenBy(type1, .2f);
                        else if (type1.IsType("centipede")) type.Eats(type1, .8f);
                        else if (type1.IsType("tubeworm")) type.EatenBy(type1, .5f);
                        else if (type1.IsType("hazer")) type.EatenBy(type1, .5f);
                        else if (type1.IsType("scavenger")) type.Eats(type1, .8f);
                        else if (type1.IsType("cicadaa")) type.Eats(type1, .05f);
                        else if (type1.IsType("lanternmouse")) type.Eats(type1, .3f);
                        else if (type1.IsType("bigspider")) type.Eats(type1, .35f);
                        else if (type1.IsType("eggbug")) type.Eats(type1, .45f);
                        else if (type1.IsType("jetfish")) type.EatenBy(type1, .5f);
                        else if (type1.IsType("bigneedleworm")) type.Eats(type1, .25f);
                        else if (type1.IsType("smallneedleworm")) type.Eats(type1, .5f);
                        else if (type1.IsType("dropbug")) type.Eats(type1, .2f);
                    }
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
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType("slugcat")) type.RelationshipBased(type1, .5f);
                        else if (type1.IsType("vulture")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("kingvulture")) type.Eats(type1, 1f);
                        else if (type1.IsType("tubeworm")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("scavenger")) type.Eats(type1, .8f);
                        else if (type1.IsType("cicadaa")) type.Eats(type1, .05f);
                        else if (type1.IsType("lanternmouse")) type.Eats(type1, .3f);
                        else if (type1.IsType("bigspider")) type.Eats(type1, .35f);
                        else if (type1.IsType("eggbug")) type.Eats(type1, .45f);
                        else if (type1.IsType("jetfish")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("bigeel")) type.EatenBy(type1, 1f);
                        else if (type1.IsType("centipede")) type.Eats(type1, .8f);
                        else if (type1.IsType("bigneedleworm")) type.Eats(type1, .25f);
                        else if (type1.IsType("daddylonglegs")) type.EatenBy(type1, 1f);
                        else if (type1.IsType("smallneedleworm")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("dropbug")) type.Eats(type1, .2f);
                        else if (type1.IsType("redcentipede")) type.EatenBy(type1, .9f);
                        else if (type1.IsType("tentacleplant")) type.EatenBy(type1, .2f);
                        else if (type1.IsType("hazer")) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("zooplizard")) type.Ignores(type1, 0.5f);
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
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null || !t.quantified) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType("daddylonglegs")) type.EatenBy(type1, 0.5f);
                        else if (type1.IsType("redcentipede")) type.EatenBy(type1, 0.5f);
                        else if (type1.IsType("redlizard")) type.EatenBy(type1, 0.3f);
                        else if (type1.IsType("centiwing")) type.EatenBy(type1, 0.2f);
                        else if (type1.IsType("brotherlonglegs")) type.EatenBy(type1, 0.3f);
                        else if (type1.IsType("bigeel")) type.EatenBy(type1, 0.6f);
                        else if (type1.IsType("scavenger")) type.PackWith(type1, 0.7f);
                        else if (type1.IsType("lizard")) type.Attacks(type1, 0.5f);
                        else type.Ignores(type1, 0.5f);
                    }
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
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null || !t.quantified) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType("daddylonglegs")) type.EatenBy(type1, 0.5f);
                        else if (type1.IsType("redcentipede")) type.EatenBy(type1, 0.5f);
                        else if (type1.IsType("redlizard")) type.EatenBy(type1, 0.3f);
                        else if (type1.IsType("centiwing")) type.EatenBy(type1, 0.2f);
                        else if (type1.IsType("brotherlonglegs")) type.EatenBy(type1, 0.3f);
                        else if (type1.IsType("bigeel")) type.EatenBy(type1, 0.6f);
                        else if (type1.IsType("scavenger")) type.PackWith(type1, 0.7f);
                        else if (type1.IsType("lizard")) type.Attacks(type1, 0.5f);
                        else type.Ignores(type1, 0.5f);
                    }
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
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null || !t.quantified) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType("daddylonglegs")) type.EatenBy(type1, 0.5f);
                        else if (type1.IsType("redcentipede")) type.EatenBy(type1, 0.5f);
                        else if (type1.IsType("redlizard")) type.EatenBy(type1, 0.3f);
                        else if (type1.IsType("centiwing")) type.EatenBy(type1, 0.2f);
                        else if (type1.IsType("brotherlonglegs")) type.EatenBy(type1, 0.3f);
                        else if (type1.IsType("bigeel")) type.EatenBy(type1, 0.6f);
                        else if (type1.IsType("scavenger")) type.PackWith(type1, 0.7f);
                        else if (type1.IsType("lizard")) type.Attacks(type1, 0.5f);
                        else type.Ignores(type1, 0.5f);
                    }
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
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType(type)) { type.PackWith(type1, 1f); }
                        else if (type1.IsType("slugcat")) { type.RelationshipBased(type1, 1f); }
                        else if (type1.IsType("scavenger")) { type.PlaysWith(type1, 0.5f); }
                        else if (type1.IsType("lizard")) { type.Eats(type1, 0.3f); }
                        else if (type1.IsType("cicadaa")) { type.Eats(type1, 0.4f); }
                        else if (type1.IsType("bigspider")) { type.EatenBy(type1, 0.35f); }
                        else if (type1.IsType("spitterspider")) type.EatenBy(type1, 0.6f);
                        else if (type1.IsType("spider")) type.EatenBy(type1, 0.2f);
                        else type.Ignores(type1, 0.5f);
                    }
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
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType(type)) { type.PackWith(type1, 1f); }
                        else if (type1.IsType("slugcat")) { type.RelationshipBased(type1, 1f); }
                        else if (type1.IsType("scavenger")) { type.PlaysWith(type1, 0.5f); }
                        else if (type1.IsType("lizard")) { type.Eats(type1, 0.3f); }
                        else if (type1.IsType("cicadaa")) { type.Eats(type1, 0.4f); }
                        else if (type1.IsType("bigspider")) { type.EatenBy(type1, 0.35f); }
                        else if (type1.IsType("spitterspider")) type.EatenBy(type1, 0.6f);
                        else if (type1.IsType("spider")) type.EatenBy(type1, 0.2f);
                        else type.Ignores(type1, 0.5f);
                    }
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
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1.IsType(type)) { type.PackWith(type1, 1f); }
                        else if (type1.IsType("slugcat")) { type.RelationshipBased(type1, 1f); }
                        else if (type1.IsType("scavenger")) { type.PlaysWith(type1, 0.5f); }
                        else if (type1.IsType("lizard")) { type.Eats(type1, 0.3f); }
                        else if (type1.IsType("cicadaa")) { type.Eats(type1, 0.4f); }
                        else if (type1.IsType("bigspider")) { type.EatenBy(type1, 0.35f); }
                        else if (type1.IsType("spitterspider")) type.EatenBy(type1, 0.6f);
                        else if (type1.IsType("spider")) type.EatenBy(type1, 0.2f);
                        else type.Ignores(type1, 0.5f);
                    }
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
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null) continue;

                        CreatureTemplate.Type type1 = t.type;

                        if (type1 == type) type.Ignores(type1, 0.5f);
                        else if (type1.IsType("daddylonglegs")) type.EatenBy(type1, 0.5f);
                        else if (type1.IsType("redcentipede")) type.EatenBy(type1, 0.5f);
                        else if (type1.IsType("redlizard")) type.EatenBy(type1, 0.3f);
                        else if (type1.IsType("centiwing")) type.EatenBy(type1, 0.2f);
                        else if (type1.IsType("brotherlonglegs")) type.EatenBy(type1, 0.3f);
                        else if (type1.IsType("bigeel")) type.EatenBy(type1, 0.6f);
                        else if (type1.IsType("centipede")) type.Eats(type1, 0.2f);
                        else if (type1.IsType("lanternmouse")) type.Eats(type1, 0.1f);
                        else if (type1.IsType("slugcat")) type.Eats(type1, 0.2f);
                        else if (type1.IsType("scavenger")) { type.EatsDangerously(type1, 0.6f); }
                        else if (type1.IsType("lizard")) type.Eats(type1, 0.5f);
                        else type.Ignores(type1, 0.5f);
                    }
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
                    foreach (var t in StaticWorld.creatureTemplates)
                    {
                        if (t == null) continue;

                        CreatureTemplate.Type type1 = t.type;

                        type.Ignores(type1, 0.5f);
                    }
                }
            };
            CreatureRegistryTemplate.Register(entry);
        }
    }
}