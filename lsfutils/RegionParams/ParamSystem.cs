using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static lsfUtils.Plugin;
using static lsfUtils.Mechanics.ConditionalLogic;
using RWCustom;
using UnityEngine;

namespace lsfUtils.RegionParams;

public readonly struct ParamKey
{
    public enum TimelineCheckType { Exact, Before, After } 

    public readonly string RegionName;
    public readonly List<string> WorldTimelines;
    public readonly List<string> Conditions;

    public static void CreateFile(string path)
    {
        using (StreamWriter streamWriter = File.CreateText(RWCustom.Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + path))
        {
            streamWriter.WriteLine("");
            Log.LogMessage("Created file " + path);
        }
    }

    public bool HasTimeline => WorldTimelines != null;

    public ParamKey(string name, List<string> timelines, List<string> conditions)
    {
        RegionName = Normalise(name);
        WorldTimelines = new List<string>();
        foreach (string timeline in timelines)
        {
            string normalised = string.IsNullOrWhiteSpace(timeline) ? null : timeline.Trim();
            if (normalised != null) WorldTimelines.Add(normalised);
        }

        Conditions = new List<string>();
        foreach (string condition in conditions)
        {
            string normalised = string.IsNullOrWhiteSpace(condition) ? null : condition.Trim();
            if (normalised != null) Conditions.Add(normalised);
        }
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

        List<string> timelines = [];
        List<string> conditions = [];
        if (str[0] == '(' && str.Contains(')'))
        {
            string timelinePart = str.Substring(1, str.IndexOf(")") - 1);
            string[] timelineParts = timelinePart.Split(',');
            for (int i = 0; i < timelineParts.Length; i++) timelines.Add(timelineParts[i]);
            str = str.Substring(str.IndexOf(")") + 1);
        }
        if (str[0] == '{' && str.Contains('}'))
        {
            string conditionPart = str.Substring(1, str.IndexOf("}") - 1);
            string[] conditionParts = conditionPart.Split(',');
            for (int i = 0; i < conditionParts.Length; i++) conditions.Add(conditionParts[i]);
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
        timelineString = timelineString.ToLowerInvariant();

        if (WorldTimelines != null && WorldTimelines.Count > 0)
        {
            bool comparison = false;
            bool reverse = false;
            foreach (string str in WorldTimelines)
            {
                if (string.IsNullOrEmpty(str)) continue;

                string worldTimeline = str.Trim();
                if (worldTimeline.ToLower().StartsWith("x-"))
                {
                    //format example: (X-White)
                    reverse = true;
                    worldTimeline = worldTimeline.Substring(2);
                }
                if (string.IsNullOrEmpty(worldTimeline)) continue;

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
                    if (SlugcatStats.AtOrBeforeTimeline(timeline1, compareTimeline)) comparison = true;
                    else Log.LogMessage($"Timeline {timeline1} is not before {compareTimeline}");
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
                    if (SlugcatStats.AtOrAfterTimeline(timeline1, compareTimeline)) comparison = true; 
                    else Log.LogMessage($"Timeline {timeline1} is not after {compareTimeline}");
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
                    if (SlugcatStats.AtOrAfterTimeline(timeline1, compareMinTimeline) && SlugcatStats.AtOrBeforeTimeline(timeline1, compareMaxTimeline)) comparison = true;
                    else Log.LogMessage($"Timeline {timeline1} is not between {compareMinTimeline} and {compareMaxTimeline}");
                }
                else if (worldTimeline.ToLowerInvariant() == timelineString) comparison = true;
            }
            if (comparison == reverse) return false;
        }

        if (Conditions != null && Conditions.Count > 0)
        {
            foreach (string condition in Conditions)
            {
                if (string.IsNullOrEmpty(condition)) continue;
                if (game == null) Log.LogMessage($"Game is null when condition {condition} is checked, skipping it!");
                if (WorldLoader.Preprocessing.PreprocessCustomConditions(condition, game) == false) return false;
            }
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
        return a.Conditions.Count > b.Conditions.Count;
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