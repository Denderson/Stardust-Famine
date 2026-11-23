using lsfUtils.CWTs;
using lsfUtils.Mechanics;
using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static lsfUtils.Mechanics.ConditionalLogic;
using static lsfUtils.Plugin;
using static lsfUtils.RegionParams.RegionTypeParams;

namespace lsfUtils.RegionParams;

public static class GateParams
{
    public delegate bool CustomRequirementCheck(RegionGate gate, int side);
    public static readonly Dictionary<string, CustomRequirementCheck> CustomChecks = [];

    public static readonly ParamTable<string[]> Table = new();

    public static readonly string ConfigPath = "world/customRequirements.txt";
    public static void CheckFile()
    {
        if (!File.Exists(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "world/customRequirements.txt"))
        {
            ParamKey.CreateFile("world/customRequirements.txt");
        }
        else Log.LogMessage("File \"world/customRequirements.txt\" already exists");
    }

    public static readonly HashSet<string> loadedImages = [];

    // TODO: Images
    public static readonly string imageFolder = null;

    public static void ApplyHooks()
    {
        On.RegionGate.customKarmaGateRequirements += RegionGate_customKarmaGateRequirements;
        On.PlayerProgression.ReloadLocksList += PlayerProgression_ReloadLocksList;
        On.RainWorldGame.Update += RainWorldGame_Update;
        new Hook(typeof(RegionGate).GetProperty(nameof(RegionGate.MeetRequirement)).GetGetMethod(), typeof(GateParams).GetMethod(nameof(RegionGate_get_MeetRequirement)));
    }

    public static void LoadParams()
    {
        Table.Clear();
        foreach ((ParamKey key, string value) in ParamFile.Read(ConfigPath))
        {
            string[] fields = ParseFields(value);
            if (fields != null) Table.Add(key, fields);
        }
    }

    public static string[] ParseFields(string value)
    {
        string[] parts = value.Split(':');
        if (parts.Length < 2)
        {
            Log.LogWarning($"Expected Room : Side0 : Side1 [: extra...] ! {value}");
            return null;
        }

        string[] fields = new string[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            string field = parts[i]?.Trim();
            if (string.IsNullOrEmpty(field))
            {
                fields[i] = null;
                continue;
            }
            fields[i] = field;
            if (i < 2) LoadImage(field);
        }

        if (fields[0] == null && fields[1] == null) return null;
        return fields;
    }

    public static void LoadImage(string name)
    {
        if (imageFolder == null) return;

        string imageName = "gateSymbol" + name;
        if (loadedImages.Contains(imageName)) return;
        string path = AssetManager.ResolveFilePath(Path.Combine(imageFolder, imageName));
        if (!File.Exists(path + ".png"))
        {
            Log.LogMessage(path + ".png image doesnt exist!");
            return;
        }

        try
        {
            Futile.atlasManager.LoadImage(path);
            loadedImages.Add(imageName);
        }
        catch (Exception e)
        {
            Log.LogError($"GateParams: failed to load image {path}: {e}");
        }
    }

    public static List<string> MergeFields(string roomName, RainWorldGame game, SlugcatStats.Timeline timeline = null)
    {
        List<string> merged = [];

        foreach (string[] entry in Table.Get(roomName, game, timeline))
        {
            for (int i = 0; i < entry.Length; i++)
            {
                if (i >= merged.Count) merged.Add(null);
                if (entry[i] != null) merged[i] = entry[i];
            }
        }
        return merged;
    }
    
    public static List<string> SplitLockLine(string line)
    {
        return Regex.Split(line, " : ").ToList();
    }

    public static List<string> MergeFields(List<string> custom, List<string> vanilla)
    {
        List<string> result = [];
        int count = Math.Max(custom.Count, vanilla.Count);

        for (int i = 0; i < count; i++)
        {
            string value = (i < custom.Count) ? custom[i] : null;
            if (value == null && i < vanilla.Count) value = vanilla[i];
            value ??= (i < 2) ? "1" : "";
            result.Add(value);
        }

        while (result.Count > 2 && result[result.Count - 1] == "") result.RemoveAt(result.Count - 1);
        return result;
    }

    public static string[] BuildLocks(string[] vanillaLocks, RainWorldGame game, SlugcatStats.Timeline timeline)
    {
        List<string> locks = vanillaLocks.ToList();
        HashSet<string> handledRooms = [];

        foreach (KeyValuePair<ParamKey, string[]> entry in Table.entries)
        {
            string room = entry.Key.RegionName;
            if (room == "everywhere" || !handledRooms.Add(room)) continue;

            List<string> custom = MergeFields(room, game, timeline);
            if (custom.Count == 0) continue;

            int lineIndex = locks.FindIndex(line => SplitLockLine(line)[0].Equals(room, StringComparison.OrdinalIgnoreCase));

            room = room.ToUpperInvariant(); // vanilla uses uppercase

            List<string> vanillaFields = [];
            if (lineIndex >= 0)
            {
                List<string> vanillaParts = SplitLockLine(locks[lineIndex]);
                room = vanillaParts[0];
                vanillaFields = vanillaParts.GetRange(1, vanillaParts.Count - 1);
            }

            List<string> fields = [room];
            fields.AddRange(MergeFields(custom, vanillaFields));
            string newLine = string.Join(" : ", fields);

            if (lineIndex >= 0) locks[lineIndex] = newLine;
            else locks.Add(newLine);
        }

        return locks.ToArray();
    }
    
    public static void PlayerProgression_ReloadLocksList(On.PlayerProgression.orig_ReloadLocksList orig, PlayerProgression self)
    {
        orig(self);

        if (!GetGameFromAnywhere(out RainWorldGame game)) return;
        if (!GetTimelineFromAnywhere(out SlugcatStats.Timeline timeline)) return;

        self.karmaLocks = BuildLocks(self.karmaLocks, game, timeline);
    }
    
    public static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        if (GameCWT.TryGetData(self, out var data) && !data.switchedLocks) 
        {
            data.switchedLocks = true;
            self.rainWorld.progression.ReloadLocksList();
        }
        orig(self);
    }

    public static void RegionGate_customKarmaGateRequirements(On.RegionGate.orig_customKarmaGateRequirements orig, RegionGate self)
    {
        orig(self);

        List<string> fields = MergeFields(self.room.abstractRoom.name, self.room.game);
        if (fields.Count == 0) return;

        for (int side = 0; side < 2; side++)
        {
            if (fields[side] == null) continue;
            self.karmaRequirements[side] = new RegionGate.GateRequirement(fields[side]);
        }
    }

    public static bool RegionGate_get_MeetRequirement(Func<RegionGate, bool> orig, RegionGate self)
    {
        RainWorldGame game = self?.room?.game;
        int side = self.letThroughDir ? 0 : 1;
        string requirement = self.karmaRequirements[side]?.value;
        if (requirement == null) return orig(self);

        if (game != null && LSFConditions(requirement, game) == true) return true;
        if (game != null && LSFRoomConditions(requirement, game, self.room) == true) return true;
        return orig(self);
    }
}