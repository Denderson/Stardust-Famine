using DevInterface;
using lsfUtils.Items;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Creatures
{
    public class CreatureRegistryEntry
    {
        public static Dictionary<string, CreatureTemplate.Type> globalAliases = new();

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
        public Func<AbstractCreature, World, Creature> RealisedCtor;
        public Func<AbstractCreature, World, ArtificialIntelligence> AICtor;
        public Func<World, AbstractCreature, AbstractCreatureAI> AbstractAICtor;
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

        public static bool unlockHookApplied = false;

        public static void Register(CreatureRegistryEntry entry)
        {
            EnsureUnlockHook();
            TryAddUnlock(entry);
            Entries[entry.type] = entry;
        }

        public static void Unregister(CreatureTemplate.Type type)
        {
            if (Entries.TryGetValue(type, out CreatureRegistryEntry entry))
            {
                TryRemoveUnlock(entry);
                Entries.Remove(type);
            }
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

        public static void EnsureUnlockHook()
        {
            if (unlockHookApplied) return;
            unlockHookApplied = true;

            On.RainWorld.OnModsInit += (orig, self) =>
            {
                orig(self);
                foreach (CreatureRegistryEntry entry in Entries.Values)
                {
                    TryAddUnlock(entry);
                }
            };
        }

        public static void TryAddUnlock(CreatureRegistryEntry entry)
        {
            if (entry.unlockID == null) return;
            if (MultiplayerUnlocks.CreatureUnlockList == null) return;
            if (!MultiplayerUnlocks.CreatureUnlockList.Contains(entry.unlockID))
            {
                MultiplayerUnlocks.CreatureUnlockList.Add(entry.unlockID);
            }
        }

        public static void TryRemoveUnlock(CreatureRegistryEntry entry)
        {
            if (entry.unlockID == null) return;
            if (MultiplayerUnlocks.CreatureUnlockList == null) return;
            MultiplayerUnlocks.CreatureUnlockList.Remove(entry.unlockID);
        }

        public static CreatureRegistryEntry ForUnlock(MultiplayerUnlocks.SandboxUnlockID unlockID)
        {
            foreach (CreatureRegistryEntry entry in Entries.Values)
            {
                if (entry?.unlockID != null && unlockID != null && entry.unlockID == unlockID) return entry;
            }
            return null;
        }
    }
}