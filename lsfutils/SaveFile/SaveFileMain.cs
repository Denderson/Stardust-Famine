using SlugBase.SaveData;
using static lsfUtils.Enums;
using static lsfUtils.Plugin;

namespace lsfUtils.SaveFile;

public static class SaveFileMain
{
    public const string prefix = "SF";
    public const string saveInit = prefix + "SaveInit";


    public static void CustomSavedataInit(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
        orig(self, str, game);
        if (self?.deathPersistentSaveData != null && (!self.deathPersistentSaveData.GetSlugBaseData().TryGet(saveInit, out bool value) || !value))
        {
            self.InitialSaveSetup();
        }
    }

    public static void InitialSaveSetup(this SaveState save)
    {
        Log.LogMessage("RESETTING SF SAVE DATA");
        save.deathPersistentSaveData.GetSlugBaseData().Set(saveInit, true);
    }

    public static void RainWorldGame_Win(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished, bool fromWarpPoint)
    {
        if (self.manager.upcomingProcess != null || !self.IsStorySession)
        {
            orig(self, malnourished, fromWarpPoint);
            return;
        }
        // tick stuff per cycle here

        orig(self, malnourished, fromWarpPoint);
    }

    public static bool GetBool(this SaveState save, string name)
    {
        if (save.miscWorldSaveData.GetSlugBaseData().TryGet(name, out bool b))
            return b;
        Log.LogMessage($"Failed to get {name}");
        return new bool();
    }

    public static void Set<T>(this SaveState save, string name, T value) => save.miscWorldSaveData.GetSlugBaseData().Set(name, value);
    public static void Set<T>(this DeathPersistentSaveData data, string name, T value) => data.GetSlugBaseData().Set(name, value);

    public static string GetString(this SaveState save, string name)
    {
        if (save.miscWorldSaveData.GetSlugBaseData().TryGet(name, out string value)) return value;
        Log.LogMessage($"Failed to get {name}");
        return null;
    }
    public static int GetInt(this SaveState save, string name)
    {
        if (save.miscWorldSaveData.GetSlugBaseData().TryGet(name, out int value)) return value;
        Log.LogMessage($"Failed to get {name}");
        return -1;
    }
    public static int GetInt(this DeathPersistentSaveData data, string name)
    {
        if (data.GetSlugBaseData().TryGet(name, out int value)) return value;
        Log.LogMessage($"Failed to get {name}");
        return -1;
    }

    public static bool GetBool(this DeathPersistentSaveData save, string name)
    {
        if (save.GetSlugBaseData().TryGet(name, out bool b)) return b;
        Log.LogMessage($"Failed to get {name}");
        return false;
    }

    public static string GetString(this DeathPersistentSaveData save, string name)
    {
        if (save.GetSlugBaseData().TryGet(name, out string value)) return value;
        Log.LogMessage($"Failed to get {name}");
        return null;
    }

    public static CloakColor GetGownColor(this DeathPersistentSaveData save, string name)
    {
        if (save.GetSlugBaseData().TryGet(name, out CloakColor value)) return value;
        Log.LogMessage($"Failed to get {name}");
        return CloakColor.Default;
    }

    public static CloakType GetGownType(this DeathPersistentSaveData save, string name)
    {
        if (save.GetSlugBaseData().TryGet(name, out CloakType value)) return value;
        Log.LogMessage($"Failed to get {name}");
        return CloakType.Default;
    }
}
