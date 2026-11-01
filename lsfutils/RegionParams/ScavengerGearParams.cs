using System.Collections.Generic;
using static lsfUtils.Plugin;
using ScavItem = (string item, int chance);

namespace lsfUtils.RegionParams;

public static class ScavengerGearParams
{
    public static readonly ParamTable<List<ScavItem>> Table = new();

    public static readonly string ConfigPath = "lsf/scavGear.txt";

    private static readonly HashSet<string> unknownItems = [];

    public static void ApplyHooks()
    {
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

    private static List<ScavItem> ParseItems(string value)
    {
        List<ScavItem> items = [];
        foreach (string raw in value.Split(','))
        {
            string[] parts = raw.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int chance))
            {
                Log.LogWarning($"Expected Item:Chance! {raw}");
                continue;
            }

            items.Add((parts[0].Trim(), chance));
        }
        return items;
    }

    private static Dictionary<string, ScavItem> GetItems(string region)
    {
        Dictionary<string, ScavItem> items = [];
        foreach (List<ScavItem> next in Table.Get(region))
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

        foreach (ScavItem scavItem in GetItems(self.world.region.name).Values)
        {
            if ((int)(UnityEngine.Random.value * 100) >= scavItem.chance) continue;

            AbstractPhysicalObject.AbstractObjectType item = new(scavItem.item, false);
            if (item.Index == -1)
            {
                if (unknownItems.Add(scavItem.item)) Log.LogWarning($"Unknown item! {scavItem.item}");
                continue;
            }
            AddItemToScav(scav, item);
        }
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