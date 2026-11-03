using lsfUtils.CWTs;
using System.Collections.Generic;
using System.IO;
using Watcher;
using static lsfUtils.Plugin;

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
    public static void CheckFile()
    {
        if (!File.Exists(RWCustom.Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "world/scavGear.txt"))
        {
            ParamKey.CreateFile("world/scavGear.txt");
        }
        else Log.LogMessage("File \"world/scavGear.txt\" already exists");
    }

    public static readonly HashSet<string> unknownItems = [];

    public static void ApplyHooks()
    {
        // currently broken, needing to figure out a way to make it get the correct constructor instead of general one (like AbstractSpear)

        On.ScavengerAbstractAI.InitGearUp += ScavengerAbstractAI_InitGearUp;
    }

    public static void LoadParams()
    {
        Table.Clear();
        unknownItems.Clear();
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

    public static void ScavengerAbstractAI_InitGearUp(On.ScavengerAbstractAI.orig_InitGearUp orig, ScavengerAbstractAI self)
    {
        orig(self);

        if (self?.world?.region == null) return;
        if (self?.parent is not AbstractCreature scav) return;

        foreach (ScavItem scavItem in GetItems(self.world.region.name, self.world.game).Values)
        {
            if (scavItem.item == null) continue;
            if (scavItem.faction != null && AbstractCreatureCWT.TryGetData(scav, out var data) && data.faction != scavItem.faction) continue;
            if (!IsCorrectType(scavItem.type, scav.creatureTemplate.type)) continue;
            if ((int)(UnityEngine.Random.value * 100) >= scavItem.chance) continue;

            AbstractPhysicalObject.AbstractObjectType item = new(scavItem.item, false);
            if (item.Index == -1)
            {
                if (unknownItems.Add(scavItem.item)) Log.LogWarning($"Unknown item! {scavItem.item}");
                continue;
            }
            AddItemToScav(scav, item);
            Log.LogMessage($"Attempting to add {scavItem.item} to {scav.creatureTemplate.type} with {scavItem.chance}% chance. type: {scavItem.type}, faction: {scavItem.faction}");
        }
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

    public static void AddItemToScav(AbstractCreature scav, AbstractPhysicalObject.AbstractObjectType itemType)
    {
        int highestUsedSlot = -1;
        foreach (AbstractPhysicalObject.AbstractObjectStick abstractObjectStick in scav.stuckObjects)
        {
            if (abstractObjectStick is AbstractPhysicalObject.CreatureGripStick grip && grip.A == scav)
            {
                highestUsedSlot = System.Math.Max(highestUsedSlot, grip.grasp);
            }
        }

        int nextSlot = highestUsedSlot + 1;

        if (nextSlot > 3) return;

        AbstractRoom room = scav.world.GetAbstractRoom(scav.pos);
        if (room == null)
        {
            Log.LogError($"ScavengerGearParams: no abstract room for scav at {scav.pos}!'");
            return;
        }

        AbstractPhysicalObject item = new(scav.world, itemType, null, scav.pos, scav.world.game.GetNewID());
        room.AddEntity(item);
        new AbstractPhysicalObject.CreatureGripStick(scav, item, nextSlot, carry: true);
    }
}