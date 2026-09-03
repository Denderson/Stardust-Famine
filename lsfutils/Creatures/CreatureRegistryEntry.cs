using DevInterface;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Creatures
{
    public class CreatureRegistryEntry
    {
        public static Dictionary<string, CreatureTemplate.Type> globalAliases;
        
        public readonly CreatureTemplate.Type type;

        public string name = "unknown";
        public string mapName = "un";
        public Color MapColor = Color.white;

        public string spriteName = null;
        public IconSymbol.IconSymbolData symbolData;

        public bool isHostileForShelter = false;
        public float performanceEstimation = 10f;

        public MultiplayerUnlocks.SandboxUnlockID unlockID = MultiplayerUnlocks.SandboxUnlockID.Slugcat;
        public MultiplayerUnlocks.SandboxUnlockID unlockParent = MultiplayerUnlocks.SandboxUnlockID.Slugcat;

        public DevInterface.RoomAttractivenessPanel.Category[] roomAttractivenessCategories;

        public CreatureTemplate.Type arenaFallbackType = CreatureTemplate.Type.PinkLizard;

        public Action<AbstractCreature, World, WorldCoordinate> CtorInit;
        public Func<AbstractCreature, World, Creature> RealizedCreatureFactory;
        public Action<AbstractCreature, World> AIFactory;
       
        public Func<Player, PhysicalObject, Player.ObjectGrabability> Grabability;

        public Action setTemplate;
        public Action setRelationships;

        public CreatureRegistryEntry(CreatureTemplate.Type type, List<string> aliases)
        {
            this.type = type;
            if (aliases == null || aliases.Count <= 0) return;
            foreach (string alias in aliases)
            {
                globalAliases.Add(alias, type);
            }
        }
    }

    public static class CreatureRegistryTemplate
    {
        public static readonly Dictionary<CreatureTemplate.Type, CreatureRegistryEntry> Entries = [];

        public static void Register(CreatureRegistryEntry entry)
        {
            Entries[entry.type] = entry;
        }

        public static void Unregister(CreatureTemplate.Type type)
        {
            Entries.Remove(type);
        }

        public static bool TryGet(CreatureTemplate.Type type, out CreatureRegistryEntry entry)
        {
            if (type == null)
            {
                entry = null;
                return false;
            }
            return Entries.TryGetValue(type, out entry);
        }
    }
}