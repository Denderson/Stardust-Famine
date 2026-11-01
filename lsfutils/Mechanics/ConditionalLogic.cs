using lsfUtils.CWTs;
using lsfUtils.SaveFile;
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

namespace lsfUtils.Mechanics;

public static class ConditionalLogic
{
    public enum BoolCondition
    {
        Starving,
        Reinforced,
        Glowing,
        Marked,
        Ascended,
        AltEnding,
        MetEcho,
        MetLttm,
        MetLttmMark,
        MetFp,
        HunterFp,
        HunterLttm,
        HunterIllness,

        // MSC
        OeUnlocked,
        ArtiDrone,
        SpearPearl,
        RivuletFp,
        RivuletLttm,
        SaintLttm,
        SaintFp,
        SaintDone,

        // Expedition
        Expedition,

        // Watcher
        WatcherDial,
        WatcherEnding1,
        WatcherEnding2,
        WatcherEnding3,
        WatcherEnding4
    }
    public enum IntCondition
    {
        Cycles,
        Random,
        StaticRandom,
        DynamicDifficulty,
        Karma,
        MaxKarma,
        PlayerCount,
        TimerMinutes,

        // Watcher
        Ripple,
        MinRipple,
        MaxRipple,
        WatcherPrince,
        WatcherWeaver
    }
    public enum RoomBoolCondition
    {
        RegionInfected
    }
    public enum RoomIntCondition
    {
        RoomInfected,
        EchoPresence,
        SpinningTopPresence,
        RegionState
    }


    public delegate bool ConditionalBoolCheck(RainWorldGame game);
    public static readonly Dictionary<BoolCondition, ConditionalBoolCheck> CustomBoolChecks = [];

    public delegate int ConditionalIntValue(RainWorldGame game);
    public static readonly Dictionary<IntCondition, ConditionalIntValue> CustomIntValues = [];

    public delegate bool ConditionalRoomBoolCheck(RainWorldGame game, Room room);
    public static readonly Dictionary<RoomBoolCondition, ConditionalRoomBoolCheck> CustomRoomBoolChecks = [];

    public delegate int ConditionalRoomIntValue(RainWorldGame game, Room room);
    public static readonly Dictionary<RoomIntCondition, ConditionalRoomIntValue> CustomRoomIntValues = [];

    public static void LoadConditions()
    {
        CustomBoolChecks.Clear();
        CustomIntValues.Clear();
        CustomRoomBoolChecks.Clear();
        CustomRoomIntValues.Clear();

        CustomBoolChecks[BoolCondition.Starving] = game => game.GetStorySession.saveState.malnourished;

        CustomBoolChecks[BoolCondition.Reinforced] = game => game.GetStorySession.saveState.deathPersistentSaveData.reinforcedKarma;

        CustomBoolChecks[BoolCondition.Glowing] = game => game.GetStorySession.saveState.theGlow;

        CustomBoolChecks[BoolCondition.Marked] = game => game.GetStorySession.saveState.deathPersistentSaveData.theMark;

        CustomBoolChecks[BoolCondition.Ascended] = game => game.GetStorySession.saveState.deathPersistentSaveData.ascended;

        CustomBoolChecks[BoolCondition.AltEnding] = game => game.GetStorySession.saveState.deathPersistentSaveData.altEnding;

        CustomBoolChecks[BoolCondition.MetEcho] = game => game.GetStorySession.saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(GhostWorldPresence.GetGhostID(game.world.region.name));

        CustomBoolChecks[BoolCondition.MetLttm] = game => game.GetStorySession.saveState.miscWorldSaveData.SLOracleState.playerEncounters > 0;

        CustomBoolChecks[BoolCondition.MetLttmMark] = game => game.GetStorySession.saveState.miscWorldSaveData.SLOracleState.playerEncountersWithMark > 0;

        CustomBoolChecks[BoolCondition.MetFp] = game => game.GetStorySession.lastEverMetPebbles;

        CustomBoolChecks[BoolCondition.HunterFp] = game => game.GetStorySession.saveState.miscWorldSaveData.pebblesSeenGreenNeuron;

        CustomBoolChecks[BoolCondition.HunterLttm] = game => game.GetStorySession.saveState.miscWorldSaveData.moonRevived;

        CustomBoolChecks[BoolCondition.HunterIllness] = game => game.GetStorySession.RedIsOutOfCycles;

        CustomIntValues[IntCondition.Cycles] = game => game.GetStorySession.saveState.cycleNumber;

        CustomIntValues[IntCondition.Random] = game => RXRandom.Int(100);

        CustomIntValues[IntCondition.StaticRandom] = game => StaticRandom;

        CustomIntValues[IntCondition.DynamicDifficulty] = game => (int)(game.GetStorySession.saveState.deathPersistentSaveData.howWellIsPlayerDoing * 100f) + 100;

        CustomIntValues[IntCondition.Karma] = game => game.GetStorySession.saveState.deathPersistentSaveData.karma;

        CustomIntValues[IntCondition.MaxKarma] = game => game.GetStorySession.saveState.deathPersistentSaveData.karmaCap;

        CustomIntValues[IntCondition.PlayerCount] = game => game.StoryPlayerCount;

        CustomIntValues[IntCondition.TimerMinutes] = game =>
        {
            TimeSpan totalFreeTimeSpan = SpeedRunTimer.GetCampaignTimeTracker(game.GetStorySession.saveStateNumber).TotalFreeTimeSpan;
            return totalFreeTimeSpan.Days * 1440 + totalFreeTimeSpan.Hours * 60 + totalFreeTimeSpan.Minutes;
        };

        if (ModManager.MSC)
        {
            CustomBoolChecks[BoolCondition.OeUnlocked] = game =>
            {
                bool isGourmand = game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Gourmand;
                bool isGourmandWhiteOrYellow = isGourmand || game.StoryCharacter == SlugcatStats.Name.White || game.StoryCharacter == SlugcatStats.Name.Yellow;

                bool gourmandMarked = isGourmand && game.GetStorySession.saveState.deathPersistentSaveData.theMark;
                bool beatenOrCheat = game.rainWorld.progression.miscProgressionData.beaten_Gourmand
                    || game.rainWorld.progression.miscProgressionData.beaten_Gourmand_Full
                    || MoreSlugcats.MoreSlugcats.chtUnlockOuterExpanse.Value;

                return gourmandMarked || (isGourmandWhiteOrYellow && beatenOrCheat);
            };

            CustomBoolChecks[BoolCondition.ArtiDrone] = game =>
            {
                return game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer
                    && game.GetStorySession.saveState.hasRobo
                    && game.GetStorySession.saveState.deathPersistentSaveData.theMark;
            };

            CustomBoolChecks[BoolCondition.SpearPearl] = game => game.GetStorySession.saveState.miscWorldSaveData.smPearlTagged;

            CustomBoolChecks[BoolCondition.RivuletFp] = game => game.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken;

            CustomBoolChecks[BoolCondition.RivuletLttm] = game => game.GetStorySession.saveState.miscWorldSaveData.moonHeartRestored;

            CustomBoolChecks[BoolCondition.SaintLttm] = game => game.GetStorySession.saveState.deathPersistentSaveData.ripMoon;

            CustomBoolChecks[BoolCondition.SaintFp] = game => game.GetStorySession.saveState.deathPersistentSaveData.ripPebbles;

            CustomBoolChecks[BoolCondition.SaintDone] = game => game.rainWorld.progression.miscProgressionData.beaten_Saint;
        }

        if (ModManager.Expedition)
        {
            CustomBoolChecks[BoolCondition.Expedition] = game => game.rainWorld.ExpeditionMode;
        }

        if (ModManager.Watcher)
        {
            CustomBoolChecks[BoolCondition.WatcherDial] = game => game.GetStorySession.saveState.miscWorldSaveData.hasRippleEggWarpAbility;

            CustomBoolChecks[BoolCondition.WatcherEnding1] = game => game.rainWorld.progression.miscProgressionData.beaten_Watcher_SpinningTop;

            CustomBoolChecks[BoolCondition.WatcherEnding2] = game => game.rainWorld.progression.miscProgressionData.beaten_Watcher_SentientRot;

            CustomBoolChecks[BoolCondition.WatcherEnding3] = game => game.rainWorld.progression.miscProgressionData.beaten_Watcher_VoidWeaver;

            CustomBoolChecks[BoolCondition.WatcherEnding4] = game => game.rainWorld.progression.miscProgressionData.beaten_Watcher_Ascension;

            CustomIntValues[IntCondition.Ripple] = game => (int)(game.GetStorySession.saveState.deathPersistentSaveData.rippleLevel * 2f);

            CustomIntValues[IntCondition.MinRipple] = game => (int)(game.GetStorySession.saveState.deathPersistentSaveData.minimumRippleLevel * 2f);

            CustomIntValues[IntCondition.MaxRipple] = game => (int)(game.GetStorySession.saveState.deathPersistentSaveData.maximumRippleLevel * 2f);

            CustomIntValues[IntCondition.WatcherPrince] = game => game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen;

            CustomIntValues[IntCondition.WatcherWeaver] = game => game.GetStorySession.saveState.miscWorldSaveData.numberOfVoidWeaverEncounters;
        }

        CustomRoomBoolChecks[RoomBoolCondition.RegionInfected] = (game, room) =>
        {
            if (game.GetStorySession.saveState?.miscWorldSaveData?.regionsInfectedBySentientRot != null)
            {
                return game.GetStorySession.saveState.miscWorldSaveData.regionsInfectedBySentientRot.Contains(room.world.name.ToLowerInvariant());
            }
            else return false;
        };

        CustomRoomIntValues[RoomIntCondition.RoomInfected] = (game, room) =>
        {
            if (room.world.regionState.sentientRotProgression.TryGetValue(room.abstractRoom.name, out var rotData))
            {
                return (int)(rotData.rotIntensity * 100);
            }
            else return 0;
        };

        CustomRoomIntValues[RoomIntCondition.EchoPresence] = (game, room) =>
        {
            int value = 0;
            if (room?.world?.worldGhost != null)
            {
                for (int i = 0; i < room.cameraPositions.Length; i++)
                {
                    value = (int)(Mathf.Max(value, room.world.worldGhost.GhostMode(room, i)) * 100f);
                }
            }
            return value;
        };

        CustomRoomIntValues[RoomIntCondition.SpinningTopPresence] = (game, room) =>
        {
            int value = 0;
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
            return value;
        };

        CustomRoomIntValues[RoomIntCondition.RegionState] = (game, room) =>
        {
            if (WorldCWT.TryGetData(room.world, out var data))
            {
                return data.regionState;
            }
            else return 0;
        };
    }

    public static bool TryParseCondition<T>(string name, out T result) where T : struct, Enum
    {
        if (Enum.TryParse(name, true, out result) && Enum.IsDefined(typeof(T), result)) return true;
        else return false;
    }

    public static bool Compare(char? sign, int value, int condition)
    {
        if (sign == '=') return value == condition;
        else if (sign == '>') return value > condition;
        else if (sign == '<') return value < condition;
        else if (sign == '-') return value >= condition;
        else return false;
    }

    public static bool? LSFConditions(string text, RainWorldGame game)
    {
        Log.LogMessage("original text: " + text);
        string modifiedText = text.ToLowerInvariant();

        if (game == null || !game.IsStorySession) return null;
        if (!modifiedText.Contains("lsf")) return null;

        modifiedText = modifiedText.Replace("lsf", "");

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
        Log.LogMessage("modified text: " + modifiedText);
        Log.LogMessage("sign: " + sign);

        if (array[0] != null && array[0].StartsWith("getevent"))
        {
            string eventName = array[0].Replace("getevent", "");
            result = game.GetStorySession.saveState.GetBool(eventName);
        }
        else if (sign == null)
        {
            if (TryParseCondition(array[0], out BoolCondition boolCondition) && CustomBoolChecks.TryGetValue(boolCondition, out var boolCheck))
            {
                result = boolCheck(game);
            }
            else return null;
        }
        else if (array.Length > 1 && int.TryParse(array[1], out var condition))
        {
            if (TryParseCondition(array[0], out IntCondition intCondition) && CustomIntValues.TryGetValue(intCondition, out var intValue))
            {
                result = Compare(sign, intValue(game), condition);
            }
            else return null;
        }
        else return null;

        Log.LogMessage("result: " + result);

        return result;
    }

    public static bool? LSFRoomConditions(string text, RainWorldGame game, Room room)
    {
        text = text.ToLowerInvariant();
        if (game == null || !game.IsStorySession) return null;
        if (!text.Contains("lsf")) return null;
        text = text.Replace("lsf", "");

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
            if (TryParseCondition(array[0], out RoomBoolCondition roomBoolCondition) && CustomRoomBoolChecks.TryGetValue(roomBoolCondition, out var roomBoolCheck))
            {
                result = roomBoolCheck(game, room);
            }
            else return null;
        }
        else if (array.Length > 1 && int.TryParse(array[1], out var condition))
        {
            if (TryParseCondition(array[0], out RoomIntCondition roomIntCondition) && CustomRoomIntValues.TryGetValue(roomIntCondition, out var roomIntValue))
            {
                result = Compare(sign, roomIntValue(game, room), condition);
            }
            else return null;
        }
        else return null;

        return result;
    }
}