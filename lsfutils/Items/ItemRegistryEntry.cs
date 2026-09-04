using System;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Items
{
    public class ItemRegistryEntry
    {
        public readonly AbstractPhysicalObject.AbstractObjectType Type;

        public string iconSprite = null;
        public Color iconColor = Color.white;
        public CreatureTemplate.Type iconCreatureType = CreatureTemplate.Type.StandardGroundCreature;

        public MultiplayerUnlocks.SandboxUnlockID unlockID = MultiplayerUnlocks.SandboxUnlockID.Slugcat;
        public MultiplayerUnlocks.SandboxUnlockID unlockParent = MultiplayerUnlocks.SandboxUnlockID.Slugcat;

        public Func<World, WorldCoordinate, EntityID, AbstractPhysicalObject> SandboxFactory;
        public Func<World, string, AbstractPhysicalObject, AbstractPhysicalObject> SaveParser;

        public Func<Scavenger, PhysicalObject, int> ScavCollectScore;
        public Func<Scavenger, PhysicalObject, int> ScavWeaponPickupScore;
        public Func<Player, PhysicalObject, Player.ObjectGrabability> Grabability;

        public ItemRegistryEntry(AbstractPhysicalObject.AbstractObjectType type)
        {
            Type = type;
        }
    }

    public static class ItemRegistryTemplate
    {
        public static readonly Dictionary<AbstractPhysicalObject.AbstractObjectType, ItemRegistryEntry> Entries = new();

        public static bool unlockHookApplied = false;

        public static void Register(ItemRegistryEntry entry)
        {
            Entries[entry.Type] = entry;
            EnsureUnlockHook();
            TryAddUnlock(entry);
        }

        public static void Unregister(AbstractPhysicalObject.AbstractObjectType type)
        {
            if (Entries.TryGetValue(type, out ItemRegistryEntry entry))
            {
                TryRemoveUnlock(entry);
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
                if (entry?.unlockID != null && unlockID != null && entry.unlockID == unlockID) return entry;
            }
            return null;
        }

        public static void EnsureUnlockHook()
        {
            if (unlockHookApplied) return;
            unlockHookApplied = true;

            On.RainWorld.OnModsInit += (orig, self) =>
            {
                orig(self);
                foreach (ItemRegistryEntry entry in Entries.Values)
                {
                    TryAddUnlock(entry);
                }
            };
        }

        public static void TryAddUnlock(ItemRegistryEntry entry)
        {
            if (entry.unlockID == null) return;
            if (MultiplayerUnlocks.ItemUnlockList == null) return;
            if (!MultiplayerUnlocks.ItemUnlockList.Contains(entry.unlockID))
            {
                MultiplayerUnlocks.ItemUnlockList.Add(entry.unlockID);
            }
        }

        public static void TryRemoveUnlock(ItemRegistryEntry entry)
        {
            if (entry.unlockID == null) return;
            if (MultiplayerUnlocks.ItemUnlockList == null) return;
            MultiplayerUnlocks.ItemUnlockList.Remove(entry.unlockID);
        }
    }
}