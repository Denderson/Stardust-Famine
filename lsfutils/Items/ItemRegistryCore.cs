using UnityEngine;

namespace lsfUtils.Items
{
    public static class ItemRegistryCore
    {
        public static void ApplyHooks()
        {
            On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += MultiplayerUnlocks_SymbolDataForSandboxUnlock;
            On.MultiplayerUnlocks.SandboxUnlockForSymbolData += MultiplayerUnlocks_SandboxUnlockForSymbolData;
            On.MultiplayerUnlocks.SandboxItemUnlocked += MultiplayerUnlocks_SandboxItemUnlocked;

            On.SandboxGameSession.SpawnItems += SandboxGameSession_SpawnItems;

            On.ItemSymbol.SpriteNameForItem += ItemSymbol_SpriteNameForItem;
            On.ItemSymbol.ColorForItem += ItemSymbol_ColorForItem;
            On.ItemSymbol.SymbolDataFromItem += ItemSymbol_SymbolDataFromItem;

            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;

            On.Player.Grabability += Player_Grabability;
        }

        public static IconSymbol.IconSymbolData MultiplayerUnlocks_SymbolDataForSandboxUnlock(On.MultiplayerUnlocks.orig_SymbolDataForSandboxUnlock orig, MultiplayerUnlocks.SandboxUnlockID unlockID)
        {
            ItemRegistryEntry entry = ItemRegistryTemplate.ForUnlock(unlockID);
            if (entry != null)
            {
                return new IconSymbol.IconSymbolData(entry.IconCreatureType, entry.Type, 0);
            }
            return orig(unlockID);
        }

        public static MultiplayerUnlocks.SandboxUnlockID MultiplayerUnlocks_SandboxUnlockForSymbolData(On.MultiplayerUnlocks.orig_SandboxUnlockForSymbolData orig, IconSymbol.IconSymbolData data)
        {
            if (ItemRegistryTemplate.TryGet(data.itemType, out ItemRegistryEntry entry) && entry.UnlockID != null)
            {
                return entry.UnlockID;
            }
            return orig(data);
        }

        public static bool MultiplayerUnlocks_SandboxItemUnlocked(On.MultiplayerUnlocks.orig_SandboxItemUnlocked orig, MultiplayerUnlocks self, MultiplayerUnlocks.SandboxUnlockID unlockID)
        {
            if (ItemRegistryTemplate.ForUnlock(unlockID) != null)
            {
                return true;
            }
            return orig(self, unlockID);
        }

        public static void SandboxGameSession_SpawnItems(On.SandboxGameSession.orig_SpawnItems orig, SandboxGameSession self, IconSymbol.IconSymbolData data, WorldCoordinate pos, EntityID entityID)
        {
            if (ItemRegistryTemplate.TryGet(data.itemType, out ItemRegistryEntry entry) && entry.SandboxFactory != null)
            {
                AbstractPhysicalObject obj = entry.SandboxFactory(self.game.world, pos, entityID);
                self.game.world.GetAbstractRoom(0).AddEntity(obj);
                return;
            }
            orig(self, data, pos, entityID);
        }

        public static string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
        {
            if (ItemRegistryTemplate.TryGet(itemType, out ItemRegistryEntry entry) && entry.IconSprite != null)
            {
                return entry.IconSprite;
            }
            return orig(itemType, intData);
        }

        public static Color ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
        {
            if (ItemRegistryTemplate.TryGet(itemType, out ItemRegistryEntry entry) && entry.IconSprite != null)
            {
                return entry.IconColor;
            }
            return orig(itemType, intData);
        }

        public static IconSymbol.IconSymbolData? ItemSymbol_SymbolDataFromItem(On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
        {
            if (ItemRegistryTemplate.TryGet(item.type, out ItemRegistryEntry entry) && entry.IconSprite != null)
            {
                return new IconSymbol.IconSymbolData(entry.IconCreatureType, entry.Type, 0);
            }
            return orig(item);
        }

        public static AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            AbstractPhysicalObject obj = orig(world, objString);

            if (obj != null && ItemRegistryTemplate.TryGet(obj.type, out ItemRegistryEntry entry) && entry.SaveParser != null)
            {
                AbstractPhysicalObject rebuilt = entry.SaveParser(world, objString, obj);
                if (rebuilt != null) return rebuilt;
            }
            return obj;
        }

        public static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (obj?.abstractPhysicalObject != null && ItemRegistryTemplate.TryGet(obj.abstractPhysicalObject.type, out ItemRegistryEntry entry) && entry.Grabability != null)
            {
                Player.ObjectGrabability? custom = entry.Grabability(self, obj);
                if (custom != null) return custom.Value;
            }
            return orig(self, obj);
        }
    }
}
