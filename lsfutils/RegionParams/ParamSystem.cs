using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static lsfUtils.Plugin;
using static lsfUtils.Mechanics.ConditionalLogic;

namespace lsfUtils.RegionParams;

public readonly struct ParamKey
{
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
            string normalised = Normalise(timeline);
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

    public bool IsValid(RainWorldGame game, SlugcatStats.Timeline timeline = null)
    {
        string timelineString = null; 
        if (timeline != null) timelineString = timeline.ToString().ToLowerInvariant().Trim();
        if (game != null && timeline == null) timelineString = game?.GetStorySession?.saveState?.currentTimelinePosition?.value?.Trim().ToLowerInvariant();

        if (WorldTimelines != null && WorldTimelines.Count > 0)
        {
            bool flag = false;
            bool flag2 = false;
            foreach (string worldTimeline in WorldTimelines)
            {
                string minusTimeline = null;
                if (worldTimeline.StartsWith("x-"))
                {
                    flag2 = true;
                    minusTimeline = worldTimeline.Substring(2);
                }
                if (string.IsNullOrEmpty(worldTimeline)) return flag2;
            //this part requires converting a string to a slugcat id. i've got no clue how so if anyone does go for it it'd be really cool and nice and awesome
                //bool flag3 = false;
                //bool flag4 = false;
                //string compareTimeline;

                /*if (worldTimeline.StartsWith("before-"))
                {
                    flag3 = true;
                    compareTimeline = worldTimeline.Substring(worldTimeline.IndexOf("-") + 1);
                }
                else if (worldTimeline.StartsWith("after-"))
                {
                    flag4 = true;
                    compareTimeline = worldTimeline.Substring(worldTimeline.IndexOf("-") + 1);
                }

                if (flag3)
                {
                    if (SlugcatStats.AtOrBeforeTimeline(timelineString, compareTimeline)) flag = true;
                }
                if (flag4)
                {
                    if (SlugcatStats.AtOrAfterTimeline(timelineString, compareTimeline)) flag = true;
                }*/

                if (worldTimeline == timelineString || minusTimeline == timelineString) flag = true;
            }
            if (flag == flag2) return false;
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