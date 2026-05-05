using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.ProcessingConditions
{
    public static class ConditionalLogic
    {
        public static void RoomSettings_LoadPlacedObjects_StringArray_Timeline(On.RoomSettings.orig_LoadPlacedObjects_StringArray_Timeline orig, RoomSettings self, string[] s, SlugcatStats.Timeline timelinePoint)
        {
            orig(self, s, timelinePoint);
            if (timelinePoint == null) return;
            List<ConditionFilterData> list = [];
            List<RoomConditionFilterData> list2 = [];
            foreach (PlacedObject placedObject in self.placedObjects)
            {
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

        public static bool? LSFConditions(string text, RainWorldGame game)
        {
            text = text.ToLowerInvariant();
            if (!text.Contains("lsf") || game == null || !game.IsStorySession)
            {
                return null;
            }
            text.Replace("lsf", "");

            string[] array;
            char? sign = null;

            if (text.Contains("="))
            {
                sign = '=';
                array = text.Split('=');
            }
            else if (text.Contains(">"))
            {
                sign = '>';
                array = text.Split('>');
            }
            else if (text.Contains('<'))
            {
                sign = '<';
                array = text.Split('<');
            }
            else if (text.Contains('-'))
            {
                sign = '-';
                array = text.Split('-');
            }
            else
            {
                array = [text];
            }

            bool? result;
            if (sign == null)
            {
                switch (array[0])
                {
                    case "mark":
                        {
                            result = game.GetStorySession.saveState.deathPersistentSaveData.theMark;
                            break;
                        }
                    case "ascended":
                        {
                            result = game.GetStorySession.saveState.deathPersistentSaveData.ascended;
                            break;
                        }
                    case "altending":
                        {
                            result = game.GetStorySession.saveState.deathPersistentSaveData.altEnding;
                            break;
                        }
                    default: return null;
                }
            }
            else if (array.Length > 1 && int.TryParse(array[1], out var condition))
            {
                int value;
                switch (array[0])
                {
                    case "cycle":
                        {
                            value = game.GetStorySession.saveState.cycleNumber;
                            break;
                        }
                    case "random":
                        {
                            value = RXRandom.Int(100);
                            break;
                        }
                    case "staticrandom":
                        {
                            value = StaticRandom;
                            break;
                        }
                    case "dynamicdifficulty":
                        {
                            value = (int)(game.GetStorySession.saveState.deathPersistentSaveData.howWellIsPlayerDoing * 100f) + 100;
                            break;
                        }
                    default: return null;
                }
                result = sign == '=' && value == condition || sign == '>' && value > condition || sign == '<' && value < condition || sign == '-' && value >= condition;
            }
            else return null;

            return result;
        }

        public static bool? LSFRoomConditions(string text, RainWorldGame game, Room room)
        {
            text = text.ToLowerInvariant();
            if (game == null || !game.IsStorySession)
            {
                return null;
            }
            if (text.Contains("lsf")) text.Replace("lsf", "");

            string[] array;
            char? sign = null;

            if (text.Contains("="))
            {
                sign = '=';
                array = text.Split('=');
            }
            else if (text.Contains(">"))
            {
                sign = '>';
                array = text.Split('>');
            }
            else if (text.Contains('<'))
            {
                sign = '<';
                array = text.Split('<');
            }
            else if (text.Contains('-'))
            {
                sign = '-';
                array = text.Split('-');
            }
            else
            {
                array = [text];
            }

            bool? result;
            if (sign == null)
            {
                switch (array[0])
                {
                    case "regioninfected":
                        {
                            if (game.GetStorySession.saveState?.miscWorldSaveData?.regionsInfectedBySentientRot != null)
                            {
                                result = game.GetStorySession.saveState.miscWorldSaveData.regionsInfectedBySentientRot.Contains(room.world.name.ToLowerInvariant());
                            }
                            else result = false;
                            break;
                        }
                    default: return null;
                }
            }
            else if (array.Length > 1 && int.TryParse(array[1], out var condition))
            {
                int value;
                switch (array[0])
                {
                    case "roominfected":
                        {
                            value = (int)(room.world.regionState.sentientRotProgression[room.abstractRoom.name].rotIntensity * 100);
                            break;
                        }
                    case "echopresence":
                        {
                            value = 0;
                            if (room?.world?.worldGhost != null)
                            {
                                for (int i = 0; i < room.cameraPositions.Length; i++)
                                {
                                    value = (int)(Mathf.Max(value, room.world.worldGhost.GhostMode(room, i)) * 100f);
                                }
                            }
                            break;
                        }
                    case "spinningtoppresence":
                        {
                            value = 0;
                            if (room?.world?.worldGhost != null)
                            {
                                for (int i = 0; i < room.cameraPositions.Length; i++)
                                {
                                    for (int j = 0; j < room.world.spinningTopPresences.Count; j++)
                                    {
                                        value = (int)(Mathf.Max(value, room.world.spinningTopPresences[j].GhostMode(room, i)) * 100f);
                                    }
                                }
                            }
                            break;
                        }
                    default: return null;
                }
                result = sign == '=' && value == condition || sign == '>' && value > condition || sign == '<' && value < condition || sign == '-' && value >= condition;
            }
            else return null;

            return result;
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
