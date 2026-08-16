using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.DevtoolsObjects.ConditionalFilter
{
    public static class ConditionalFilterHooks
    {
        public static void ApplyHooks()
        {
            On.RoomSettings.LoadPlacedObjects_StringArray_Timeline += ConditionalFilterHooks.RoomSettings_LoadPlacedObjects_StringArray_Timeline;
        }

        public static void RoomSettings_LoadPlacedObjects_StringArray_Timeline(On.RoomSettings.orig_LoadPlacedObjects_StringArray_Timeline orig, RoomSettings self, string[] s, SlugcatStats.Timeline timelinePoint)
        {
            orig(self, s, timelinePoint);
            if (timelinePoint == null) return;
            List<ConditionFilterData> list = [];
            List<RoomConditionFilterData> list2 = [];
            foreach (PlacedObject placedObject in self.placedObjects)
            {
                if (self.room == null) break;

                if (placedObject.data is ConditionFilterData filter && !filter.Active(ref self.room.game))
                {
                    list.Add(filter);
                }
                if (placedObject.data is RoomConditionFilterData roomfilter && !roomfilter.Active(ref self.room.game, self.room))
                {
                    list2.Add(roomfilter);
                }
            }
            for (int j = 0; j < self.placedObjects.Count; j++)
            {
                if (!self.placedObjects[j].deactivattable)
                {
                    continue;
                }
                for (int k = 0; k < list.Count; k++)
                {
                    if (Custom.DistLess(self.placedObjects[j].pos, list[k].owner.pos, list[k].radius.magnitude))
                    {
                        list[k].DeactivatePlacedObject(self.placedObjects[j]);
                        break;
                    }
                }
                for (int k = 0; k < list2.Count; k++)
                {
                    if (Custom.DistLess(self.placedObjects[j].pos, list2[k].owner.pos, list2[k].radius.magnitude))
                    {
                        list2[k].DeactivatePlacedObject(self.placedObjects[j]);
                        break;
                    }
                }
            }
        }

        /*
        public static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
        {
            if (self?.game != null && RefreshSpawns.TryGet(self.game, out var refreshSpawns) && refreshSpawns)
            {
                self.respawnCreatures = new List<int> { };
                self.waitRespawnCreatures = new List<int> { };
            }
            return orig(self);
        }
        */
    }
}
