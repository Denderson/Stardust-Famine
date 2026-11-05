using lsfUtils.CWTs;
using lsfUtils.Items;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using Watcher;
using static lsfUtils.Plugin;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;

namespace lsfUtils.RegionParams;

public static class ScavengerGearParams
{
    public struct ScavItem
    {
        public string type;
        public string faction;

        public string item;
        public int chance;

        public ScavItem(string item, int chance, string faction = null, string type = null)
        {
            this.item = item;
            this.chance = chance;
            this.faction = faction;
            this.type = type;
        }
    }

    public static readonly ParamTable<List<ScavItem>> Table = new();

    public static readonly string ConfigPath = "world/scavGear.txt";

    public static bool skipVanilla;
    public static bool overrideBoomrang;
    public static bool SkipVanilla() => skipVanilla;
    public static bool HasCustomBoomerang() => overrideBoomrang;

    public static void CheckFile()
    {
        if (!File.Exists(RWCustom.Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "world/scavGear.txt"))
        {
            ParamKey.CreateFile("world/scavGear.txt");
        }
        else Log.LogMessage("File \"world/scavGear.txt\" already exists");
    }

    public static void ApplyHooks()
    {
        IL.ScavengerAbstractAI.InitGearUp += ScavengerAbstractAI_InitGearUp;
    }

    public static int AddItems(ScavengerAbstractAI self, int num)
    {
        skipVanilla = false;
        overrideBoomrang = false;

        if (self?.world?.region == null) return num;
        if (self.parent is not AbstractCreature scav) return num;

        foreach (ScavItem scavItem in GetItems(self.world.region.name, self.world.game).Values)
        {
            if (scavItem.item == null) continue;
            if ((scavItem.faction != null && scavItem.faction != "any") && AbstractCreatureCWT.TryGetData(scav, out var data) && data.faction != scavItem.faction) continue;
            if (!IsCorrectType(scavItem.type, scav.creatureTemplate.type)) continue;
            if ((int)(UnityEngine.Random.value * 100) >= scavItem.chance) continue;

            string strItem = scavItem.item.Trim().ToLowerInvariant();

            if (strItem == "novanilla")
            {
                skipVanilla = true;
                Log.LogMessage($"Vanilla gear removed from {scav.creatureTemplate.type} with {scavItem.chance}% chance.");
                continue;
            }

            if (num < 0) continue;

            if (strItem == "spear" || strItem == "explosivespear" || strItem == "electricspear" || strItem == "firespear" || strItem == "poisonspear")
            {
                bool explosive = strItem == "explosivespear";
                bool electric = (ModManager.DLCShared && strItem == "electricspear");
                bool fire = (ModManager.MSC && strItem == "firespear");
                bool poison = (ModManager.Watcher && strItem == "poisonspear");
                if (TryAddSpear(scav, num, explosive, electric, fire, poison)) num--;

                Log.LogMessage($"Trying to add {scavItem.item} to {scav.creatureTemplate.type} with {scavItem.chance}% chance.");
                Log.LogMessage($"explosive: {explosive}, electric: {electric}, fire: {fire}, poison: {poison}");
                continue;
            }

            AbstractPhysicalObject.AbstractObjectType item = new(scavItem.item, false);
            if (item.Index == -1)
            {
                Log.LogWarning($"Unknown item! {scavItem.item}");
                continue;
            }

            if (TryAddItem(scav, item, num))
            {
                num--;
                if (item == WatcherEnums.AbstractObjectType.Boomerang || item == Enums.AbstractObjectType.SingularityBoomerang || item == Enums.AbstractObjectType.ExplosiveBoomerang) overrideBoomrang = true;
            }
            Log.LogMessage($"Trying to add {scavItem.item} item to {scav.creatureTemplate.type} with {scavItem.chance}% chance.");
        }

        return num;
    }

    private static void ScavengerAbstractAI_InitGearUp(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel skipBoomerang = null;

        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchLdcI4(0),
            x => x.MatchStloc(5)))
        {
            Log.LogMessage("flag2 = false; not found!");
            return;
        }

        var endCursor = new ILCursor(il);
        if (!endCursor.TryGotoNext(MoveType.Before,
            x => x.MatchLdloc(3),
            x => x.MatchBrfalse(out _)))
        {
            Log.LogMessage("if (flag) not found!");
            return;
        }
        ILLabel skipEverything = endCursor.MarkLabel();

        var num = il.Body.Variables[0];

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloc, num);
        c.Emit(OpCodes.Call, typeof(ScavengerGearParams).GetMethod(nameof(AddItems)));
        c.Emit(OpCodes.Stloc, num);

        c.Emit(OpCodes.Call, typeof(ScavengerGearParams).GetMethod(nameof(SkipVanilla)));
        c.Emit(OpCodes.Brtrue, skipEverything);

        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(0),
            x => x.MatchLdcI4(0),
            x => x.MatchBle(out skipBoomerang)))
        {
            Log.LogMessage("boomerangs num > 0 check not found!");
            return;
        }

        c.Emit(OpCodes.Call, typeof(ScavengerGearParams).GetMethod(nameof(HasCustomBoomerang)));
        c.Emit(OpCodes.Brtrue, skipBoomerang);
    }

    public static void LoadParams()
    {
        Table.Clear();
        foreach ((ParamKey key, string value) in ParamFile.Read(ConfigPath))
        {
            List<ScavItem> items = ParseItems(value);
            if (items.Count > 0) Table.Add(key, items);
        }
    }

    public static List<ScavItem> ParseItems(string value)
    {
        List<ScavItem> items = [];
        foreach (string raw in value.Split(','))
        {
            string[] parts = raw.Split(':');
            if (parts.Length < 2)
            {
                Log.LogWarning($"Expected Item:Chance ! {raw}");
                continue;
            }

            string item = parts[0].Trim();
            if (string.IsNullOrWhiteSpace(item)) continue;

            if (!int.TryParse(parts[1], out int chance)) continue;

            string faction = null;
            if (parts.Length >= 3) faction = parts[2].Trim();

            string type = null;
            if (parts.Length >= 4) type = parts[3].Trim();

            ScavItem scavItem = new ScavItem(item, chance, faction, type);
            items.Add(scavItem);
        }
        return items;
    }

    public static Dictionary<string, ScavItem> GetItems(string region, RainWorldGame game)
    {
        Dictionary<string, ScavItem> items = [];
        foreach (List<ScavItem> next in Table.Get(region, game))
        {
            foreach (ScavItem scavItem in next) items[scavItem.item] = scavItem;
        }
        return items;
    }

    public static bool IsCorrectType(string typeString, CreatureTemplate.Type type)
    {
        if (typeString == null) return true;
        typeString = typeString.Trim();
        if (typeString.ToLower().Contains("any")) return true;
        if ((typeString.ToLower().Contains("base") || typeString.ToLower().Contains("basic") || typeString.ToLower().Contains("normal")) && type != DLCSharedEnums.CreatureTemplateType.ScavengerElite && type != WatcherEnums.CreatureTemplateType.ScavengerTemplar && type != WatcherEnums.CreatureTemplateType.ScavengerDisciple) return true;
        if (ModManager.DLCShared && typeString.ToLower().Contains("elite") && type == DLCSharedEnums.CreatureTemplateType.ScavengerElite) return true;
        if (ModManager.Watcher && typeString.ToLower().Contains("templar") && type == WatcherEnums.CreatureTemplateType.ScavengerTemplar) return true;
        if (ModManager.Watcher && typeString.ToLower().Contains("disciple") && type == WatcherEnums.CreatureTemplateType.ScavengerDisciple) return true;
        if (!(typeString.ToLower().Contains("base") || typeString.ToLower().Contains("basic") || typeString.ToLower().Contains("normal") || typeString.ToLower().Contains("elite") || typeString.ToLower().Contains("templar") || typeString.ToLower().Contains("disciple"))) return true;
        return false;
    }

    public static bool TryAddItem(AbstractCreature scav, AbstractPhysicalObject.AbstractObjectType itemType, int num)
    {
        AbstractPhysicalObject abstractPhysicalObject = null;

        if (ItemRegistryTemplate.TryGet(itemType, out var item))
        {
            abstractPhysicalObject = item.Ctor(scav.world, scav.pos, scav.world.game.GetNewID());
        }
        else if (itemType == AbstractPhysicalObject.AbstractObjectType.GraffitiBomb)
        {
            abstractPhysicalObject = new GraffitiBomb.AbstractGraffitiBomb(scav.world, null, scav.pos, scav.world.game.GetNewID(), -1, -1, null);
        }
        else if (itemType == AbstractPhysicalObject.AbstractObjectType.DataPearl)
        {
            abstractPhysicalObject = new DataPearl.AbstractDataPearl(scav.world, itemType, null, scav.pos, scav.world.game.GetNewID(), -1, -1, null, default);
        }
        else if (AbstractConsumable.IsTypeConsumable(itemType))
        {
            abstractPhysicalObject = new AbstractConsumable(scav.world, itemType, null, scav.pos, scav.world.game.GetNewID(), -1, -1, null);
        }
        else
        {
            abstractPhysicalObject = new AbstractPhysicalObject(scav.world, itemType, null, scav.pos, scav.world.game.GetNewID());
        }

        scav.world.GetAbstractRoom(scav.pos).AddEntity(abstractPhysicalObject);
        new AbstractPhysicalObject.CreatureGripStick(scav, abstractPhysicalObject, num, carry: true);
        return true;
    }

    public static bool TryAddSpear(AbstractCreature scav, int num, bool explosive, bool electric, bool fire, bool poison)
    {
        float hue = 0f;
        if (fire) hue = UnityEngine.Mathf.Lerp(UnityEngine.Random.value, 1f, 0.001f);
        AbstractSpear spear = new AbstractSpear(scav.world, null, scav.pos, scav.world.game.GetNewID(), explosive, electric);
        AbstractSpear spearf = new AbstractSpear(scav.world, null, scav.pos, scav.world.game.GetNewID(), explosive, hue);
        if (poison)
        {
            spear.poison = 1f;
            spear.poisonHue = UnityEngine.Random.value;
        }
        if (hue != 0f)
        {
            scav.world.GetAbstractRoom(scav.pos).AddEntity(spearf);
            new AbstractPhysicalObject.CreatureGripStick(scav, spearf, num, carry: true);
        }
        else
        {
            scav.world.GetAbstractRoom(scav.pos).AddEntity(spear);
            new AbstractPhysicalObject.CreatureGripStick(scav, spear, num, carry: true);
        }
        return true;
    }
}