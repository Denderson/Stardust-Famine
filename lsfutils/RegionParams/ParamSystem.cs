using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static lsfUtils.Plugin;

namespace lsfUtils.RegionParams;

public readonly struct ParamKey
{
    public readonly string RegionName;
    public readonly string WorldTimeline;
    public readonly List<string> Conditions;

    public bool HasTimeline => WorldTimeline != null;
    public int ConditionCount => Conditions.Count;

    public ParamKey(string name, string timeline, List<string> conditions)
    {
        RegionName = Normalise(name);
        WorldTimeline = Normalise(timeline);
        Conditions = new List<string>();
        foreach (string condition in conditions)
        {
            string normalised = Normalise(condition);
            if (normalised != null) Conditions.Add(normalised);
        }
    }

    private static string Normalise(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return s.Trim().ToLowerInvariant();
    }

    public static bool TryParse(string str, out ParamKey? key)
    {
        key = null;
        if (string.IsNullOrWhiteSpace(str)) return false;

        string[] parts = str.Split(',');
        string name = Normalise(parts[0]);
        if (name == null || name.Length <= 0) return false;

        string timeline = null;
        if (parts.Length >= 2) timeline = parts[1];

        List<string> conditions = [];
        for (int i = 2; i < parts.Length; i++) conditions.Add(parts[i]);

        key = new ParamKey(name, timeline, conditions);
        return true;
    }

    public bool IsValid(string timelineName)
    {
        if (WorldTimeline != null && WorldTimeline != timelineName) return false;
        // TODO: check if all conditions are met (requires getting the game from somewhere)
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
            if (splitIndex < 0 || !ParamKey.TryParse(line.Substring(0, splitIndex), out ParamKey? key))
            {
                Log.LogWarning($"Malformed line! {rawLine}");
                continue;
            }

            lines.Add((key.Value, line.Substring(splitIndex + 1).Trim()));
        }
        return lines;
    }
}

public class ParamTable<T>
{
    private readonly List<KeyValuePair<ParamKey, T>> entries = [];

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

    private static bool Overrides(ParamKey a, ParamKey b)
    {
        if (a.HasTimeline && !b.HasTimeline) return true;
        if (!a.HasTimeline && b.HasTimeline) return false;
        return a.ConditionCount > b.ConditionCount;
    }

    public List<T> Get(string name)
    {
        GetTimelineFromAnywhere(out SlugcatStats.Timeline timeline);
        return Get(name, timeline);
    }

    public List<T> Get(string name, SlugcatStats.Timeline timeline)
    {
        List<T> valid = [];
        if (name == null) return valid;
        name = name.Trim().ToLowerInvariant();
        string timelineName = timeline?.value?.ToLowerInvariant();

        foreach (KeyValuePair<ParamKey, T> entry in entries)
        {
            if (entry.Key.RegionName == name && entry.Key.IsValid(timelineName)) valid.Add(entry.Value);
        }
        return valid;
    }
}