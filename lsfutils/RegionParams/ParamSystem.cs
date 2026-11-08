using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static lsfUtils.Mechanics.ConditionalLogic;
using static lsfUtils.Plugin;

namespace lsfUtils.RegionParams;

public readonly struct ParamKey
{
    public static void ApplyHooks()
    {
        On.WorldLoader.Preprocessing.TimelineMatch += WorldLoader_Preprocessing_TimelineMatch;
        On.WorldLoader.Preprocessing.PreprocessCustomConditions += WorldLoader_Preprocessing_PreprocessCustomConditions;
    }

    private static bool WorldLoader_Preprocessing_TimelineMatch(On.WorldLoader.Preprocessing.orig_TimelineMatch orig, string text, SlugcatStats.Timeline timeline)
    {
        if (text.ToLower().Contains("before-") || text.ToLower().Contains("after-") || text.ToLower().Contains("between-"))
        {
            Log.LogMessage("Timeline check override!");
            bool comparison = false;
            bool reverse = false;
            foreach (string part in text.Split(','))
            {
                if (string.IsNullOrEmpty(part)) continue;

                string worldTimeline = part.Trim();
                if (worldTimeline.ToLower().StartsWith("x-"))
                {
                    //format example: (X-White)
                    reverse = true;
                    worldTimeline = worldTimeline.Substring(2);
                }
                if (string.IsNullOrEmpty(worldTimeline)) continue;
                if (timeline == null) return reverse;

                if (worldTimeline.ToLower().StartsWith("before-"))
                {
                    //format example: (Before-White)
                    string beforeTimeline = worldTimeline.Substring(worldTimeline.IndexOf("-") + 1);
                    Log.LogMessage($"Before timeline: {beforeTimeline}");
                    SlugcatStats.Timeline compareTimeline = new(beforeTimeline, false);
                    if (compareTimeline.Index == -1)
                    {
                        Log.LogWarning($"Unknown timeline! {beforeTimeline}");
                        continue;
                    }
                    if (SlugcatStats.AtOrBeforeTimeline(timeline, compareTimeline)) comparison = true;
                    else Log.LogMessage($"Timeline {timeline} is not before {compareTimeline}");
                }
                else if (worldTimeline.ToLower().StartsWith("after-"))
                {
                    //format example: (After-White)
                    string afterTimeline = worldTimeline.Substring(worldTimeline.IndexOf("-") + 1);
                    Log.LogMessage($"After timeline: {afterTimeline}");
                    SlugcatStats.Timeline compareTimeline = new(afterTimeline, false);
                    if (compareTimeline.Index == -1)
                    {
                        Log.LogWarning($"Unknown timeline! {afterTimeline}");
                        continue;
                    }
                    if (SlugcatStats.AtOrAfterTimeline(timeline, compareTimeline)) comparison = true;
                    else Log.LogMessage($"Timeline {timeline} is not after {compareTimeline}");
                }
                else if (worldTimeline.ToLower().StartsWith("between-"))
                {
                    //format example: (Between-Red-White)
                    string minTimeline = worldTimeline.Split('-')[1];
                    string maxTimeline = worldTimeline.Split('-')[2];
                    Log.LogMessage($"Between timelines: {minTimeline} and {maxTimeline}");
                    SlugcatStats.Timeline compareMinTimeline = new(minTimeline, false);
                    SlugcatStats.Timeline compareMaxTimeline = new(maxTimeline, false);
                    if (compareMinTimeline.Index == -1 || compareMaxTimeline.Index == -1)
                    {
                        Log.LogWarning($"Unknown timelines! {minTimeline}, {maxTimeline}");
                        continue;
                    }
                    if (SlugcatStats.AtOrAfterTimeline(timeline, compareMinTimeline) && SlugcatStats.AtOrBeforeTimeline(timeline, compareMaxTimeline)) comparison = true;
                    else Log.LogMessage($"Timeline {timeline} is not between {compareMinTimeline} and {compareMaxTimeline}");
                }
                else if (worldTimeline.ToLowerInvariant() == timeline.ToString().ToLowerInvariant()) comparison = true;
            }
            return comparison != reverse;
        }
        else return orig(text, timeline);
    }

    private static bool WorldLoader_Preprocessing_PreprocessCustomConditions(On.WorldLoader.Preprocessing.orig_PreprocessCustomConditions orig, string text, RainWorldGame game)
    {
        if (text.Contains('|'))
        {
            Log.LogMessage("Condition check override!");
            bool group = false;
            foreach (string section in text.Split('|'))
            {
                if (group) break;
                Log.LogMessage($"New group: \"{section}\"");
                bool all = true;
                foreach (string part in section.Split(','))
                {
                    if (!all) break;
                    string condition = part.Trim();
                    bool reverse = false;
                    if (condition.Contains('!'))
                    {
                        reverse = true;
                        condition = condition.Replace("!", "");
                    }
                    foreach (WorldLoader.Preprocessing.PreprocessorCondition preprocessorCondition in WorldLoader.Preprocessing.preprocessorConditions)
                    {
                        bool? check = preprocessorCondition(condition, game);
                        if (check != null)
                        {
                            if (check.GetValueOrDefault() == reverse)
                            {
                                all = false;
                            }
                        }
                    }
                }
                if (all)
                {
                    group = true;
                    Log.LogMessage($"Condition group \"{section}\" passed, returning true");
                }
            }
            if (!group) Log.LogMessage("None of the groups passed, returning false");
            return group;
        }
        else return orig(text, game);
    }

    public enum TimelineCheckType { Exact, Before, After } 

    public readonly string RegionName;
    public readonly string WorldTimelines;
    public readonly string Conditions;

    public static void CreateFile(string path)
    {
        using (StreamWriter streamWriter = File.CreateText(RWCustom.Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + path))
        {
            streamWriter.WriteLine("");
            Log.LogMessage("Created file " + path);
        }
    }

    public bool HasTimeline => WorldTimelines != null;

    public ParamKey(string name, string timelines, string conditions)
    {
        RegionName = Normalise(name);
        WorldTimelines = string.IsNullOrWhiteSpace(timelines) ? null : timelines.Trim();
        Conditions = string.IsNullOrWhiteSpace(conditions) ? null : conditions.Trim();
    }

    public static string Normalise(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return s.Trim().ToLowerInvariant();
    }

    public static bool TryParse(string str, out ParamKey? key)
    {
        key = null;
        if (string.IsNullOrWhiteSpace(str)) return false;

        string timelines = null;
        string conditions = null;
        if (str[0] == '(' && str.Contains(')'))
        {
            timelines = str.Substring(1, str.IndexOf(")") - 1);
            str = str.Substring(str.IndexOf(")") + 1);
        }
        if (str[0] == '{' && str.Contains('}'))
        {
            conditions = str.Substring(1, str.IndexOf("}") - 1);
            str = str.Substring(str.IndexOf("}") + 1);
        }

        string name = Normalise(str);
        key = new ParamKey(name, timelines, conditions);
        return true;
    }

    public bool IsValid(RainWorldGame game = null, SlugcatStats.Timeline timeline = null)
    {
        if (game == null)
        {
            if (Custom.rainWorld.processManager?.currentMainLoop is not RainWorldGame game1)
            {
                Log.LogMessage("Couldnt grab the game!");
                return false;
            }
            else game = game1;
        }

        string timelineString = null; 
        if (timeline != null) timelineString = timeline.ToString().Trim();
        if (game != null && timeline == null) timelineString = game?.GetStorySession?.saveState?.currentTimelinePosition?.value?.Trim();
        SlugcatStats.Timeline timeline1 = new(timelineString, false);

        if (WorldTimelines != null)
        {
            if (!WorldLoader.Preprocessing.TimelineMatch(WorldTimelines, timeline1)) return false;
        }

        if (Conditions != null)
        {
            if (!WorldLoader.Preprocessing.PreprocessCustomConditions(Conditions, game)) return false;
        }
        return true;
    }
}

public static class ParamFile
{
    public static List<(ParamKey key, string value)> Read(string configPath)
    {
        List<(ParamKey key, string value)> lines = [];
        string path = AssetManager.ResolveFilePath(configPath);
        if (!File.Exists(path))
        {
            Log.LogWarning($"Config not found! {path}");
            return lines;
        }

        foreach (string rawLine in File.ReadAllLines(path))
        {
            string line = rawLine.Split('/')[0].Trim();
            if (line.Length == 0) continue;

            int splitIndex = line.IndexOf('=');
            if (splitIndex < 0)
            {
                Log.LogMessage($"No = found in a line {rawLine}, skipping...");
                continue;
            }
            if (!ParamKey.TryParse(line.Substring(0, splitIndex), out ParamKey? key))
            {
                Log.LogMessage($"Cannot parse line {rawLine}, skipping...");
            }

            lines.Add((key.Value, line.Substring(splitIndex + 1).Trim()));
        }
        return lines;
    }
}

public class ParamTable<T>
{
    public readonly List<KeyValuePair<ParamKey, T>> entries = [];

    public void Clear()
    {
        entries.Clear();
    }

    public void Add(ParamKey key, T value)
    {
        int index = entries.Count;
        while (index > 0 && Overrides(entries[index - 1].Key, key)) index--;
        entries.Insert(index, new KeyValuePair<ParamKey, T>(key, value));
    }

    public static bool Overrides(ParamKey a, ParamKey b)
    {
        if (a.HasTimeline && !b.HasTimeline) return true;
        if (!a.HasTimeline && b.HasTimeline) return false;

        //i have no idea what i'm doing send help please. it works at least so i ain't touching it
        List<string> aCount = [], bCount = [];

        if (a.Conditions != null)
        { 
            string[] countA = a.Conditions.Split(',');
            for (int i = 0; i < countA.Length; i++) aCount.Add(countA[i]);
        }
        if (b.Conditions != null)
        {
            string[] countB = b.Conditions.Split(',');
            for (int i = 0; i < countB.Length; i++) bCount.Add(countB[i]);
        }

        return aCount.Count > bCount.Count;
    }

    public List<T> Get(string name, RainWorldGame game, SlugcatStats.Timeline timeline = null)
    {
        List<T> valid = [];

        if (name == null) return valid;
        name = name.Trim().ToLowerInvariant();

        foreach (KeyValuePair<ParamKey, T> entry in entries)
        {
            if ((entry.Key.RegionName == name || entry.Key.RegionName.ToLower() == "everywhere") && entry.Key.IsValid(game, timeline)) valid.Add(entry.Value);
        }
        return valid;
    }
}