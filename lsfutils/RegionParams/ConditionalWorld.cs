using lsfUtils.CWTs;
using System.IO;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.RegionParams;

internal class ConditionalWorld
{
    public static void ApplyHooks()
    {
        On.Watcher.WarpMap.WarpRegionIcon.AddGraphics += WarpMap_WarpRegionIcon_AddGraphics;
        On.Watcher.WarpMap.WarpRegionIcon.ctor += WarpRegionIcon_ctor;
    }

    private static void WarpRegionIcon_ctor(On.Watcher.WarpMap.WarpRegionIcon.orig_ctor orig, Watcher.WarpMap.WarpRegionIcon self, Watcher.WarpMap map, int index, string region, bool infected, bool hasVoidWeaver, bool hasSpinningTopEncounter, bool visited)
    {
        orig(self, map, index, region, infected, hasVoidWeaver, hasSpinningTopEncounter, visited);
        if (!WarpRegionIconCWT.TryGetData(self, out var data)) return;
        data.map = map;
    }

    private static void WarpMap_WarpRegionIcon_AddGraphics(On.Watcher.WarpMap.WarpRegionIcon.orig_AddGraphics orig, Watcher.WarpMap.WarpRegionIcon self)
    {
        orig(self);
        if (!WarpRegionIconCWT.TryGetData(self, out var data)) return;
        if (data.map == null) 
        {
            Log.LogMessage("idk what i'm doing");
            return; 
        }
        SaveState state = data.map.GetSaveState();
        if (state == null)
        {
            Log.LogMessage("at least it not working doesn't break anything");
            return;
        }
        SlugcatStats.Timeline timeline = state.currentTimelinePosition;

        string timelineIcon = "warp-" + self.region + "-" + timeline;
        if (!self.visited)
        {
            timelineIcon = "warp-unknown-" + timeline;
        }

        if (!Futile.atlasManager.DoesContainElementWithName(timelineIcon))
        {
            Log.LogMessage("icon not found in atlas");
            Texture2D texture2D = new Texture2D(0, 0);
            string path = AssetManager.ResolveFilePath("illustrations/" + timelineIcon + ".png");
            if (File.Exists(path))
            {
                Log.LogMessage("Loading timeline icon: " + timelineIcon);
                texture2D.LoadImage(File.ReadAllBytes(path));
            }
            texture2D.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture(timelineIcon, texture2D, false);
        }
        Log.LogMessage("it's never gtten to this one lmao");
        self.RefreshGraphics();
        self.ClearSprites();
        self.regionIcon = new FSprite(timelineIcon, true);
        self.regionIcon.SetAnchor(0.5f, 0.5f);
        self.map.container.AddChild(self.regionIcon);
    }
}