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
    public readonly string WorldTimeline;
    public readonly List<string> Conditions;

    public static void CreateFile(string path)
    {
        using (StreamWriter streamWriter = File.CreateText(RWCustom.Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + path))
        {
            streamWriter.WriteLine("");
            Log.LogMessage("Created file " + path);
        }
    }

    public bool HasTimeline => WorldTimeline != null;

    public ParamKey(string name, string timeline, List<string> conditions)
    {
        RegionName = Normalise(name);
        WorldTimeline = Normalise(timeline);
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

    public bool IsValid(RainWorldGame game, SlugcatStats.Timeline timeline = null)
    {
        string timelineString = null; 
        if (timeline != null) timelineString = timeline.ToString().ToLowerInvariant().Trim();
        if (game != null && timeline == null) timelineString = game?.GetStorySession?.saveState?.currentTimelinePosition?.value?.Trim().ToLowerInvariant();

        if (WorldTimeline != null && WorldTimeline != timelineString) return false;

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