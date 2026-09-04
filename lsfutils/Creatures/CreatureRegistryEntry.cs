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
        public Color mapColor = Color.white;

        public string symbolName = null;
        public IconSymbol.IconSymbolData symbolData;

        public bool isHostileForShelter = false;
        public bool isBigForShelter = false;
        public float performanceCost = 10f;

        public MultiplayerUnlocks.SandboxUnlockID unlockID = MultiplayerUnlocks.SandboxUnlockID.Slugcat;
        public MultiplayerUnlocks.SandboxUnlockID unlockParent = MultiplayerUnlocks.SandboxUnlockID.Slugcat;

        public DevInterface.RoomAttractivenessPanel.Category[] roomAttractivenessCategories;

        public Action<AbstractCreature, World, WorldCoordinate> AbstractCtor;
        public Func<AbstractCreature, World, Creature> RealisedCtor;
        public Action<AbstractCreature, World> AICtor;
        public Func<AbstractCreature, CreatureState> StateCtor;

        public Func<Player, PhysicalObject, Player.ObjectGrabability> Grabability;

        public Func<CreatureTemplate> setTemplate;
        public Action setRelationships;

        public CreatureRegistryEntry(CreatureTemplate.Type type, List<string> aliases = null)
        {
            this.type = type;
            this.symbolData = new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, 0);
            this.name = type.ToString();
            aliases ??= [type.ToString()];
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