using lsfUtils.CWTs;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.DevtoolsObjects.ConditionalFilter
{
    public static class ConditionalLogic
    {
        public static bool? LSFConditions(string text, RainWorldGame game)
        {
            Log.LogMessage("original text: " + text);
            string modifiedText = text;
            
            modifiedText = modifiedText.ToLowerInvariant();
            if (game == null || !game.IsStorySession)
            {
                return null;
            }
            if (modifiedText.Contains("lsf")) modifiedText.Replace("lsf", "");

            string[] array;
            char? sign = null;

            if (modifiedText.Contains("="))
            {
                sign = '=';
                array = modifiedText.Split('=');
            }
            else if (modifiedText.Contains(">"))
            {
                sign = '>';
                array = modifiedText.Split('>');
            }
            else if (modifiedText.Contains('<'))
            {
                sign = '<';
                array = modifiedText.Split('<');
            }
            else if (modifiedText.Contains('-'))
            {
                sign = '-';
                array = modifiedText.Split('-');
            }
            else
            {
                array = [modifiedText];
            }

            bool? result = null;
            Log.LogMessage("modified text: " + text);
            Log.LogMessage("sign: " + sign);
            if (sign == null)
            {
                switch (array[0])
                {
                    case "starving":
                        {
                            result = game.GetStorySession.saveState.malnourished;
                            break;
                        }
                    case "reinforced":
                        {
                            result = game.GetStorySession.saveState.deathPersistentSaveData.reinforcedKarma;
                            break;
                        }
                    case "glowing":
                        {
                            result = game.GetStorySession.saveState.theGlow;
                            break;
                        }
                    case "marked":
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
                    case "metecho":
                        {
                            result = game.GetStorySession.saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(GhostWorldPresence.GetGhostID(game.world.region.name));
                            break;
                        }
                    case "metlttm":
                        {
                            result = game.GetStorySession.saveState.miscWorldSaveData.SLOracleState.playerEncounters > 0;
                            break;
                        }
                    case "metlttmmark":
                        {
                            result = game.GetStorySession.saveState.miscWorldSaveData.SLOracleState.playerEncountersWithMark > 0;
                            break;
                        }
                    case "metfp":
                        {
                            result = game.GetStorySession.lastEverMetPebbles;
                            break;
                        }
                    case "hunterfp":
                        {
                            result = game.GetStorySession.saveState.miscWorldSaveData.pebblesSeenGreenNeuron;
                            break;
                        }
                    case "hunterlttm":
                        {
                            result = game.GetStorySession.saveState.miscWorldSaveData.moonRevived;
                            break;
                        }
                    case "hunterillness":
                        {
                            result = game.GetStorySession.RedIsOutOfCycles;
                            break;
                        }
                }

                if (ModManager.MSC)
                {
                    switch(array[0])
                    {
                        case "oeunlocked":
                            {
                                result = (game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && game.GetStorySession.saveState.deathPersistentSaveData.theMark) || ((game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Gourmand || game.StoryCharacter == SlugcatStats.Name.White || game.StoryCharacter == SlugcatStats.Name.Yellow) && (game.rainWorld.progression.miscProgressionData.beaten_Gourmand || game.rainWorld.progression.miscProgressionData.beaten_Gourmand_Full || global::MoreSlugcats.MoreSlugcats.chtUnlockOuterExpanse.Value));
                                break;
                            }
                        case "artidrone":
                            {
                                result = game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer && game.GetStorySession.saveState.hasRobo && game.GetStorySession.saveState.deathPersistentSaveData.theMark;
                                break;
                            }
                        case "spearpearl":
                            {
                                result = game.GetStorySession.saveState.miscWorldSaveData.smPearlTagged;
                                break;
                            }
                        case "rivuletfp":
                            {
                                result = game.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken;
                                break;
                            }
                        case "rivuletlttm":
                            {
                                result = game.GetStorySession.saveState.miscWorldSaveData.moonHeartRestored;
                                break;
                            }
                        case "saintlttm":
                            {
                                result = game.GetStorySession.saveState.deathPersistentSaveData.ripMoon;
                                break;
                            }
                        case "saintfp":
                            {
                                result = game.GetStorySession.saveState.deathPersistentSaveData.ripPebbles;
                                break;
                            }
                        case "saintdone":
                            {
                                result = game.rainWorld.progression.miscProgressionData.beaten_Saint;
                                break;
                            }
                    }
                }

                if (ModManager.Expedition)
                {
                    switch (array[0])
                    {
                        case "expedition":
                            {
                                result = game.rainWorld.ExpeditionMode;
                                break;
                            }
                    }
                }

                if (ModManager.Watcher)
                {
                    switch (array[0])
                    {
                        case "watcherdial":
                            {
                                result = game.GetStorySession.saveState.miscWorldSaveData.hasRippleEggWarpAbility;
                                break;
                            }
                        case "watcherending1":
                            {
                                result = game.rainWorld.progression.miscProgressionData.beaten_Watcher_SpinningTop;
                                break;
                            }
                        case "watcherending2":
                            {
                                result = game.rainWorld.progression.miscProgressionData.beaten_Watcher_SentientRot;
                                break;
                            }
                        case "watcherending3":
                            {
                                result = game.rainWorld.progression.miscProgressionData.beaten_Watcher_VoidWeaver;
                                break;
                            }
                        case "watcherending4":
                            {
                                result = game.rainWorld.progression.miscProgressionData.beaten_Watcher_Ascension;
                                break;
                            }
                    }
                }
            }
            else if (array.Length > 1 && int.TryParse(array[1], out var condition))
            {
                int value = 0;
                switch (array[0])
                {
                    case "cycles":
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
                    case "karma":
                        {
                            value = game.GetStorySession.saveState.deathPersistentSaveData.karma;
                            break;
                        }
                    case "maxkarma":
                        {
                            value = game.GetStorySession.saveState.deathPersistentSaveData.karmaCap;
                            break;
                        }
                    case "playercount":
                        {
                            value = game.StoryPlayerCount;
                            break;
                        }
                    case "timerminutes":
                        {
                            TimeSpan totalFreeTimeSpan = SpeedRunTimer.GetCampaignTimeTracker(game.GetStorySession.saveStateNumber).TotalFreeTimeSpan;
                            value = totalFreeTimeSpan.Days * 1440 + totalFreeTimeSpan.Hours * 60 + totalFreeTimeSpan.Minutes;
                            break;
                        }
                }

                if (ModManager.Watcher)
                {
                    switch (array[0])
                    {
                        case "ripple":
                            {
                                value = (int)(game.GetStorySession.saveState.deathPersistentSaveData.rippleLevel * 2f);
                                break;
                            }
                        case "minripple":
                            {
                                value = (int)(game.GetStorySession.saveState.deathPersistentSaveData.minimumRippleLevel * 2f);
                                break;
                            }
                        case "maxripple":
                            {
                                value = (int)(game.GetStorySession.saveState.deathPersistentSaveData.maximumRippleLevel * 2f);
                                break;
                            }
                        case "watcherprince":
                            {
                                value = game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen;
                                break;
                            }
                        case "watcherweaver":
                            {
                                value = game.GetStorySession.saveState.miscWorldSaveData.numberOfVoidWeaverEncounters;
                                break;
                            }
                    }
                }

                result = sign == '=' && value == condition || sign == '>' && value > condition || sign == '<' && value < condition || sign == '-' && value >= condition;
            }
            else return null;

            Log.LogMessage("result: " + result);

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
                int value = 0;
                switch (array[0])
                {
                    case "roominfected":
                        {
                            if (room.world.regionState.sentientRotProgression.TryGetValue(room.abstractRoom.name, out var rotData))
                            {
                                value = (int)(rotData.rotIntensity * 100);
                            }
                            break;
                        }
                    case "echopresence":
                        {
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
                    case "regionState":
                        {
                            if (WorldCWT.TryGetData(room.world, out var data))
                            {
                                value = data.regionState;
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
    }
}
