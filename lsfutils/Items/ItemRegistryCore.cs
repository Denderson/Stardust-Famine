using lsfUtils.CWTs;
using lsfUtils.Items.RippleFlower;
using lsfUtils.Items.Spears.TorchSpears;
using lsfUtils.Items.Spears.CopperSpears;
using MoreSlugcats;
using UnityEngine;
using System.Diagnostics.Eventing.Reader;

namespace lsfUtils.Items;

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
        On.ScavengerAI.CollectScore_ItemRepresentation_bool += ScavengerAI_CollectScore_ItemRepresentation_bool;
        On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
        On.ScavengerAI.WeaponScore += ScavengerAI_WeaponScore;
    }

    public static AbstractPhysicalObject GourmandCombos_CraftingResults(On.MoreSlugcats.GourmandCombos.orig_CraftingResults orig, PhysicalObject crafter, Creature.Grasp graspA, Creature.Grasp graspB)
    {
        AbstractPhysicalObject result = orig(crafter, graspA, graspB);
        if (result != null && ItemRegistryTemplate.TryGet(result.type, out ItemRegistryEntry entry) && entry.Ctor != null)
        {
            return entry.Ctor(result.world, result.pos, result.ID);
        }
        if (result.type == AbstractPhysicalObject.AbstractObjectType.Creature)
        {
            CreatureTemplate.Type type = GourmandCombos.CraftingResults_CreatureData(graspA, graspB);
            if (type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            {
                if (graspA.grabbed is Player && graspB.grabbed is SingularityBomb || graspA.grabbed is SingularityBomb && graspB.grabbed is Player)
                {
                    return new AbstractCreature(crafter.room.world, StaticWorld.GetCreatureTemplate(type), null, crafter.abstractPhysicalObject.pos, Plugin.SpecialId);
                }  
                else if (graspA.grabbed is Player && graspB.grabbed is ScavengerBomb || graspA.grabbed is ScavengerBomb && graspB.grabbed is Player)
                {
                    return new AbstractCreature(crafter.room.world, StaticWorld.GetCreatureTemplate(type), null, crafter.abstractPhysicalObject.pos, Plugin.SpecialIdElectricBoogaloo);
                }
            }
            Plugin.Log.LogMessage($"creature was {type}");
        }
        if (result.type == AbstractPhysicalObject.AbstractObjectType.Spear)
        {
            if (graspA.grabbed is Spear spearA && graspB.grabbed is KarmaFlower flowerA)
            {
                if (flowerA.abstractPhysicalObject.type == Enums.AbstractObjectType.RippleFlower)
                {
                    if (spearA.abstractSpear.explosive) result = new TorchSpearAbstract(crafter.room.world, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID());
                    else if (spearA.abstractSpear.electric) result = new CopperSpearAbstract(crafter.room.world, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID());
                    //else if (spearA.abstractSpear.hue != 0f) result = ;
                    //else if (spearA.abstractSpear.poison > 0f) result = ;
                    else result = new RippleFlowerAbstract(crafter.room.world, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), -1, -1, null); //just a placeholder for now
                }
                else if (flowerA.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.KarmaFlower)
                {
                    if (spearA is TorchSpear) result = new AbstractSpear(crafter.room.world, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), false);
                    else if (spearA is CopperSpear) result = new AbstractSpear(crafter.room.world, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), false, true);
                }
            }
            else if (graspB.grabbed is Spear spearB && graspA.grabbed is KarmaFlower flowerB)
            {
                if (flowerB.abstractPhysicalObject.type == Enums.AbstractObjectType.RippleFlower)
                {
                    if (spearB.abstractSpear.explosive) result = new TorchSpearAbstract(crafter.room.world, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID());
                    else if (spearB.abstractSpear.electric) result = new CopperSpearAbstract(crafter.room.world, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID());
                    //else if (spearB.abstractSpear.hue != 0f) result = ;
                    //else if (spearB.abstractSpear.poison > 0f) result = ;
                    else result = new RippleFlowerAbstract(crafter.room.world, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), -1, -1, null); //same thing
                }
                else if (flowerB.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.KarmaFlower)
                {
                    if (spearB is TorchSpear) result = new AbstractSpear(crafter.room.world, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), true);
                    else if (spearB is CopperSpear) result = new AbstractSpear(crafter.room.world, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), false, true);
                }
            }
        }
        return result;
    }

    private static int ScavengerAI_CollectScore_ItemRepresentation_bool(On.ScavengerAI.orig_CollectScore_ItemRepresentation_bool orig, ScavengerAI self, ItemTracker.ItemRepresentation obj, bool weaponFiltered)
    {
        if (obj?.representedItem?.type != null && obj.representedItem.realizedObject != null && ItemRegistryTemplate.TryGet(obj.representedItem.type, out ItemRegistryEntry entry) && entry.ScavCollectScore != null)
        {
            return entry.ScavCollectScore(self.scavenger, obj.representedItem.realizedObject);
        }
        return orig(self, obj, weaponFiltered);
    }

    public static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        if (obj?.abstractPhysicalObject != null && ItemRegistryTemplate.TryGet(obj.abstractPhysicalObject.type, out ItemRegistryEntry entry) && entry.ScavCollectScore != null)
        {
            return entry.ScavCollectScore(self.scavenger, obj);
        }
        return orig(self, obj, weaponFiltered);
    }

    public static int ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection, bool reallyWantsSpear)
    {
        if (obj?.abstractPhysicalObject != null && ItemRegistryTemplate.TryGet(obj.abstractPhysicalObject.type, out ItemRegistryEntry entry) && entry.ScavWeaponPickupScore != null)
        {
            return entry.ScavWeaponPickupScore(self.scavenger, obj);
        }
        return orig(self, obj, pickupDropInsteadOfWeaponSelection, reallyWantsSpear);
    }

    public static IconSymbol.IconSymbolData MultiplayerUnlocks_SymbolDataForSandboxUnlock(On.MultiplayerUnlocks.orig_SymbolDataForSandboxUnlock orig, MultiplayerUnlocks.SandboxUnlockID unlockID)
    {
        ItemRegistryEntry entry = ItemRegistryTemplate.ForUnlock(unlockID);
        if (entry != null) return new IconSymbol.IconSymbolData(entry.iconCreatureType, entry.Type, 0);
        return orig(unlockID);
    }

    public static MultiplayerUnlocks.SandboxUnlockID MultiplayerUnlocks_SandboxUnlockForSymbolData(On.MultiplayerUnlocks.orig_SandboxUnlockForSymbolData orig, IconSymbol.IconSymbolData data)
    {
        if (ItemRegistryTemplate.TryGet(data.itemType, out ItemRegistryEntry entry) && entry.unlockID != null) return entry.unlockID;
        return orig(data);
    }

    public static bool MultiplayerUnlocks_SandboxItemUnlocked(On.MultiplayerUnlocks.orig_SandboxItemUnlocked orig, MultiplayerUnlocks self, MultiplayerUnlocks.SandboxUnlockID unlockID)
    {
        if (ItemRegistryTemplate.ForUnlock(unlockID) != null) return true;
        return orig(self, unlockID);
    }

    public static void SandboxGameSession_SpawnItems(On.SandboxGameSession.orig_SpawnItems orig, SandboxGameSession self, IconSymbol.IconSymbolData data, WorldCoordinate pos, EntityID entityID)
    {
        if (ItemRegistryTemplate.TryGet(data.itemType, out ItemRegistryEntry entry) && entry.Ctor != null)
        {
            AbstractPhysicalObject obj = entry.Ctor(self.game.world, pos, entityID);
            self.game.world.GetAbstractRoom(0).AddEntity(obj);
            return;
        }
        orig(self, data, pos, entityID);
    }

    public static string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        if (ItemRegistryTemplate.TryGet(itemType, out ItemRegistryEntry entry) && entry.iconSprite != null)
        {
            return entry.iconSprite;
        }
        return orig(itemType, intData);
    }

    public static Color ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        if (ItemRegistryTemplate.TryGet(itemType, out ItemRegistryEntry entry) && entry.iconSprite != null)
        {
            return entry.iconColor;
        }
        return orig(itemType, intData);
    }

    public static IconSymbol.IconSymbolData? ItemSymbol_SymbolDataFromItem(On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
    {
        if (ItemRegistryTemplate.TryGet(item.type, out ItemRegistryEntry entry) && entry.iconSprite != null)
        {
            return new IconSymbol.IconSymbolData(entry.iconCreatureType, entry.Type, 0);
        }
        return orig(item);
    }

    public static AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
    {
        AbstractPhysicalObject obj = orig(world, objString);

        if (obj != null && ItemRegistryTemplate.TryGet(obj.type, out ItemRegistryEntry entry))
        {
            AbstractPhysicalObject rebuilt = null;

            if (entry.SaveParser != null) rebuilt = entry.SaveParser(world, objString, obj);
            else if (entry.Ctor != null) rebuilt = entry.Ctor(world, obj.pos, obj.ID);

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
