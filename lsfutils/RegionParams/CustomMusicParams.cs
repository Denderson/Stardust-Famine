using lsfUtils.CWTs;
using RWCustom;
using System.Collections.Generic;
using System.Linq;

namespace lsfUtils.RegionParams;

public static class CustomMusicParams
{
    public static void ApplyHooks()
    {
        On.Music.ProceduralMusic.ProceduralMusicInstruction.ctor += ProceduralMusicInstruction_ctor;
        On.Music.ProceduralMusic.ProceduralMusicInstruction.Track.AllowedInSubRegion += Track_AllowedInSubRegion;
    }

    private static bool Track_AllowedInSubRegion(On.Music.ProceduralMusic.ProceduralMusicInstruction.Track.orig_AllowedInSubRegion orig, Music.ProceduralMusic.ProceduralMusicInstruction.Track self, string subRegion)
    {
        var value = orig(self, subRegion);
        if (!ProcedualMusicTrackCWT.TryGetData(self, out var data)) return value;
        if (Custom.rainWorld.processManager?.currentMainLoop is RainWorldGame game)
        {
            foreach (ParamKey key in data.paramKeys)
            {
                if (!key.IsValid()) return false;
            }
        }
        else
        {
            Plugin.Log.LogMessage("Couldnt grab the game for track key checks!");
        }
        return value;
    }

    private static void ProceduralMusicInstruction_ctor(On.Music.ProceduralMusic.ProceduralMusicInstruction.orig_ctor orig, Music.ProceduralMusic.ProceduralMusicInstruction self, string name)
    {
        orig(self, name);
        if (self?.tracks == null || self?.tracks.Count <= 0) return;
        for (int i = 0; i < self.tracks.Count; i++)
        {
            var track = self.tracks[i];
            if (track?.subRegions == null || track.subRegions.Length <= 0) continue;
            if (!ProcedualMusicTrackCWT.TryGetData(self.tracks[i], out var data)) continue;

            List<string> subRegions = track.subRegions.ToList();
            for (int j = 0;  j < track.subRegions.Length; j++)
            {
                string subRegion = track.subRegions[j];
                if (string.IsNullOrEmpty(subRegion)) continue;
                if (subRegion.StartsWith("C-"))
                {
                    if (subRegion.Length <= 2) return;
                    if (!ParamKey.TryParse(subRegion.Substring(2), out var key)) continue;
                    if (key == null) continue;
                    data.paramKeys.Add(key.Value);
                    subRegions.Remove(subRegion);
                }
            }
            if (subRegions.Count > 0) track.subRegions = subRegions.ToArray();
            else track.subRegions = null;
        }
    }
}
