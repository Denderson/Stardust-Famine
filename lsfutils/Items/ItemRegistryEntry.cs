using System;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Items
{
    public class ItemRegistryEntry
    {
        public readonly AbstractPhysicalObject.AbstractObjectType Type;

        public string IconSprite = null;
        public Color IconColor = Color.white;
        public CreatureTemplate.Type IconCreatureType = CreatureTemplate.Type.StandardGroundCreature;

        public MultiplayerUnlocks.SandboxUnlockID UnlockID = MultiplayerUnlocks.SandboxUnlockID.Slugcat;
        public MultiplayerUnlocks.SandboxUnlockID UnlockParent = MultiplayerUnlocks.SandboxUnlockID.Slugcat;
        public int Points;
        
        public Func<World, WorldCoordinate, EntityID, AbstractPhysicalObject> SandboxFactory;
        public Func<World, string, AbstractPhysicalObject, AbstractPhysicalObject> SaveParser;

        public Func<Scavenger, PhysicalObject, int> ScavCollectScore;
        public Func<Scavenger, PhysicalObject, int> ScavWeaponPickupScore;
        public Func<Player, PhysicalObject, Player.ObjectGrabability?> Grabability;

        public ItemRegistryEntry(AbstractPhysicalObject.AbstractObjectType type)
        {
            Type = type;
        }
    }

    public static class ItemRegistryTemplate
    {
        public static readonly Dictionary<AbstractPhysicalObject.AbstractObjectType, ItemRegistryEntry> Entries = new();

        public static void Register(ItemRegistryEntry entry)
        {
            Entries[entry.Type] = entry;

            if (entry.UnlockID != null && !MultiplayerUnlocks.ItemUnlockList.Contains(entry.UnlockID))
            {
                MultiplayerUnlocks.ItemUnlockList.Add(entry.UnlockID);
            }
        }

        public static void Unregister(AbstractPhysicalObject.AbstractObjectType type)
        {
            if (Entries.TryGetValue(type, out ItemRegistryEntry entry))
            {
                if (entry.UnlockID != null)
                {
                    MultiplayerUnlocks.ItemUnlockList.Remove(entry.UnlockID);
                }
                Entries.Remove(type);
            }
        }

        public static bool TryGet(AbstractPhysicalObject.AbstractObjectType type, out ItemRegistryEntry entry)
        {
            if (type == null)
            {
                entry = null;
                return false;
            }
            return Entries.TryGetValue(type, out entry);
        }

        public static ItemRegistryEntry ForUnlock(MultiplayerUnlocks.SandboxUnlockID unlockID)
        {
            foreach (ItemRegistryEntry entry in Entries.Values)
            {
                if (entry.UnlockID == unlockID) return entry;
            }
            return null;
        }
    }
}
