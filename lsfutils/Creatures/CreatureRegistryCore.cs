using DevInterface;
using lsfUtils.Items;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;
using static lsfUtils.Plugin;
using static lsfUtils.Enums;
using static lsfUtils.Creatures.CreatureRegistryEntry;

namespace lsfUtils.Creatures
{
    public static class CreatureRegistryCore
    {
        public static void ApplyHooks()
        {
            On.StaticWorld.InitCustomTemplates += StaticWorld_InitCustomTemplates;
            On.StaticWorld.InitStaticWorldRelationships += StaticWorld_InitStaticWorldRelationships;

            On.AbstractCreature.ctor += AbstractCreature_ctor;
            On.AbstractCreature.Realize += AbstractCreature_Realize;
            On.AbstractCreature.InitiateAI += AbstractCreature_InitiateAI;

            On.WorldLoader.CreatureTypeFromString += WorldLoader_CreatureTypeFromString;

            On.ShelterDoor.IsThisHostileCreatureForShelter += ShelterDoor_IsThisHostileCreatureForShelter;
            On.ShelterDoor.IsThisBigCreatureForShelter += ShelterDoor_IsThisBigCreatureForShelter;
            On.DevInterface.RoomAttractivenessPanel.ctor += RoomAttractivenessPanel_ctor;
            On.DevInterface.MapPage.CreatureVis.CritString += CreatureVis_CritString;
            On.DevInterface.MapPage.CreatureVis.CritCol += CreatureVis_CritCol;

            On.CreatureSymbol.SymbolDataFromCreature += CreatureSymbol_SymbolDataFromCreature;
            On.CreatureSymbol.ColorOfCreature += CreatureSymbol_ColorOfCreature;
            On.CreatureSymbol.SpriteNameOfCreature += CreatureSymbol_SpriteNameOfCreature;

            On.Player.Grabability += Player_Grabability;
            On.RoomRealizer.GetCreaturePerformanceEstimation += RoomRealizer_GetCreaturePerformanceEstimation;
            On.MultiplayerUnlocks.FallBackCrit += ArenaFallback;
        }

        private static bool ShelterDoor_IsThisBigCreatureForShelter(On.ShelterDoor.orig_IsThisBigCreatureForShelter orig, AbstractCreature creature)
        {
            var value = orig(creature);
            if (!CreatureRegistryTemplate.TryGet(creature.creatureTemplate.type, out var entry)) return value;
            return entry.isBigForShelter;
        }

        private static void StaticWorld_InitCustomTemplates(On.StaticWorld.orig_InitCustomTemplates orig)
        {
            orig();
            if (CreatureRegistryTemplate.Entries == null) return;
            foreach (var entry in CreatureRegistryTemplate.Entries)
            {
                if (entry.Value == null) continue;
                int index = entry.Key.Index;
                if (index > -1) StaticWorld.creatureTemplates[index] = entry.Value.setTemplate();
            }
        }

        private static void StaticWorld_InitStaticWorldRelationships(On.StaticWorld.orig_InitStaticWorldRelationships orig)
        {
            orig();
            if (CreatureRegistryTemplate.Entries == null) return;
            foreach (var entry in CreatureRegistryTemplate.Entries)
            {
                if (entry.Value == null) continue;
                entry.Value.setRelationships();
            }
        }

        private static void AbstractCreature_Realize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
        {
            if (self?.Room != null && self.realizedCreature == null && CreatureRegistryTemplate.TryGet(self.creatureTemplate.type, out var entry))
            {
                self.realizedCreature = entry.RealisedCtor(self, self.world);
                self.InitiateAI();
                self.state = entry.StateCtor(self);
                foreach (AbstractPhysicalObject.AbstractObjectStick abstractObjectStick in self.stuckObjects)
                {
                    if (abstractObjectStick.A.realizedObject == null) abstractObjectStick.A.Realize();
                    if (abstractObjectStick.B.realizedObject == null) abstractObjectStick.B.Realize();
                }
                return;
            }
            orig(self);
        }

        public static void AbstractCreature_InitiateAI(On.AbstractCreature.orig_InitiateAI orig, AbstractCreature self)
        {
            if (!CreatureRegistryTemplate.TryGet(self.creatureTemplate.type, out var entry))
            {
                orig(self);
                return;
            }
            entry.AICtor(self, self.world);
        }

        public static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            if (!CreatureRegistryTemplate.TryGet(creatureTemplate.type, out var entry) || entry?.AbstractCtor == null)
            {
                orig(self, world, creatureTemplate, realizedCreature, pos, ID);
                return;
            }
            entry.AbstractCtor(self, world, pos);
        }

        public static CreatureTemplate.Type WorldLoader_CreatureTypeFromString(On.WorldLoader.orig_CreatureTypeFromString orig, string s)
        {
            var value = orig(s);
            string text = s.ToLowerInvariant().Trim();
            if (globalAliases != null && globalAliases.Count > 0 && globalAliases.ContainsKey(text)) return globalAliases[s];
            return value;
        }

        public static bool ShelterDoor_IsThisHostileCreatureForShelter(On.ShelterDoor.orig_IsThisHostileCreatureForShelter orig, AbstractCreature creature)
        {
            var value = orig(creature);
            if (!CreatureRegistryTemplate.TryGet(creature.creatureTemplate.type, out var entry)) return value;
            return entry.isHostileForShelter;
        }

        public static void RoomAttractivenessPanel_ctor(On.DevInterface.RoomAttractivenessPanel.orig_ctor orig, RoomAttractivenessPanel self, DevUI owner, World world, string IDstring, DevUINode parentNode, Vector2 pos, string title, MapPage mapPage)
        {
            orig(self, owner, world, IDstring, parentNode, pos, title, mapPage);
            if (CreatureRegistryTemplate.Entries == null) return;
            foreach (var entry in CreatureRegistryTemplate.Entries)
            {
                if (entry.Value == null) continue;
                AddRoomAttractivenessFor(self, entry.Key, entry.Value.roomAttractivenessCategories);
            }
            self.Refresh();
        }

        public static string CreatureVis_CritString(On.DevInterface.MapPage.CreatureVis.orig_CritString orig, AbstractCreature crit)
        {
            var value = orig(crit);
            if (!CreatureRegistryTemplate.TryGet(crit.creatureTemplate.type, out var entry)) return value;
            return entry.mapName;
        }

        public static Color CreatureVis_CritCol(On.DevInterface.MapPage.CreatureVis.orig_CritCol orig, AbstractCreature crit)
        {
            var value = orig(crit);
            if (!CreatureRegistryTemplate.TryGet(crit.creatureTemplate.type, out var entry)) return value;
            return entry.mapColor;
        }

        public static IconSymbol.IconSymbolData CreatureSymbol_SymbolDataFromCreature(On.CreatureSymbol.orig_SymbolDataFromCreature orig, AbstractCreature creature)
        {
            var value = orig(creature);
            if (!CreatureRegistryTemplate.TryGet(creature.creatureTemplate.type, out var entry)) return value;
            return entry.symbolData;
        }

        public static Color CreatureSymbol_ColorOfCreature(On.CreatureSymbol.orig_ColorOfCreature orig, IconSymbol.IconSymbolData iconData)
        {
            var value = orig(iconData);
            if (!CreatureRegistryTemplate.TryGet(iconData.critType, out var entry)) return value;
            return entry.mapColor;
        }

        public static string CreatureSymbol_SpriteNameOfCreature(On.CreatureSymbol.orig_SpriteNameOfCreature orig, IconSymbol.IconSymbolData iconData)
        {
            var value = orig(iconData);
            if (!CreatureRegistryTemplate.TryGet(iconData.critType, out var entry)) return value;
            return entry.symbolName;
        }

        public static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            var value = orig(self, obj);
            if (obj is not Creature creature) return value;
            if (!CreatureRegistryTemplate.TryGet(creature.Template.type, out var entry) || entry?.Grabability == null) return value;
            return entry.Grabability(self, creature);
        }

        public static CreatureTemplate.Type ArenaFallback(On.MultiplayerUnlocks.orig_FallBackCrit orig, CreatureTemplate.Type type)
        {
            var value = orig(type);
            if (!CreatureRegistryTemplate.TryGet(type, out var entry)) return value;
            return CreatureTemplate.Type.SeaLeech;
        }

        public static float RoomRealizer_GetCreaturePerformanceEstimation(On.RoomRealizer.orig_GetCreaturePerformanceEstimation orig, AbstractCreature crit)
        {
            var value = orig(crit);
            if (!CreatureRegistryTemplate.TryGet(crit.creatureTemplate.type, out var entry)) return value;
            return entry.performanceCost;
        }

        public static void SetTemplate(CreatureTemplate.Type type, CreatureTemplate template)
        {
            int index = type.Index;
            if (index > -1) StaticWorld.creatureTemplates[index] = template;
        }

        public static void AddRoomAttractivenessFor(RoomAttractivenessPanel panel, CreatureTemplate.Type type, RoomAttractivenessPanel.Category[] categories)
        {
            if (type.Index == -1) return;
            int index = StaticWorld.GetCreatureTemplate(type).index;
            foreach (var category in categories)
            {
                ref int[] templateIndices = ref panel.categories[(int)category];
                if (!templateIndices.Contains(index))
                {
                    Array.Resize(ref templateIndices, templateIndices.Length + 1);
                    templateIndices[templateIndices.Length - 1] = index;
                }
            }
        }

        /*public static CreatureTemplate CreateFoxTemplate()
        {
            List<TileTypeResistance> tRs =
            [
                new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed),
                new TileTypeResistance(AItile.Accessibility.Climb, 2f, PathCost.Legality.Allowed),
                new TileTypeResistance(AItile.Accessibility.Corridor, 1.5f, PathCost.Legality.Allowed),
                new TileTypeResistance(AItile.Accessibility.Solid, 100f, PathCost.Legality.Unallowed)
            ];

            List<TileConnectionResistance> cRs =
            [
                new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed),
                new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 1f, PathCost.Legality.Allowed),
                new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed),
                new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 2f, PathCost.Legality.Allowed)
            ];

            CreatureTemplate ancestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat);
            CreatureTemplate template = new CreatureTemplate(FoxEnums.DesertFox, ancestor, tRs, cRs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
            {
                name = CreatureName,
                AI = true,
                instantDeathDamageLimit = 1.5f,
                baseDamageResistance = 1.2f,
                baseStunResistance = 0.8f,
                offScreenSpeed = 0.4f,
                abstractedLaziness = 50,
                roamBetweenRoomsChance = 0.35f,
                bodySize = 1.1f,
                stowFoodInDen = true,
                shortcutSegments = 2,
                grasps = 1,
                visualRadius = 1400f,
                movementBasedVision = 0.3f,
                communityInfluence = 0.1f,
                waterRelationship = CreatureTemplate.WaterRelationship.AirOnly,
                waterPathingResistance = 5f,
                canFly = false,
                meatPoints = 5,
                dangerousToPlayer = 0.1f,
                doPreBakedPathing = false,
                requireAImap = true,
                smallCreature = false
            };

            return template;
        }*/
    }
}