using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using UnityEngine;
using Watcher;
using Menu;

namespace lsfUtils.RegionParams
{
    internal class ConditionalWorld
    {
        public static void ApplyHooks()
        {
            On.Watcher.WarpMap.WarpRegionIcon.AddGraphics += WarpMap_WarpRegionIcon_AddGraphics;
        }

        private static void WarpMap_WarpRegionIcon_AddGraphics(On.Watcher.WarpMap.WarpRegionIcon.orig_AddGraphics orig, Watcher.WarpMap.WarpRegionIcon self)
        {
            orig(self);
            if (RWCustom.Custom.rainWorld.processManager?.currentMainLoop is not RainWorldGame game) 
            {
                Plugin.Log.LogMessage("idk what i'm doing");
                return; 
            }
            if (game == null)
            {
                Plugin.Log.LogMessage("at least it not working doesn't break anything");
                return;
            }
            SlugcatStats.Timeline timeline = game.GetStorySession.saveState.currentTimelinePosition;

            string timelineIcon = "warp-" + self.region + "-" + timeline;
            if (!self.visited)
            {
                timelineIcon = "warp-unknown-" + timeline;
            }
            if (!Futile.atlasManager.DoesContainElementWithName(timelineIcon))
            {
                Plugin.Log.LogMessage("icon not found in atlas");
                Texture2D texture2D = new Texture2D(0, 0);
                string path = AssetManager.ResolveFilePath("illustrations/" + timelineIcon + ".png");
                if (File.Exists(path))
                {
                    Plugin.Log.LogMessage("Loading timeline icon: " + timelineIcon);
                    texture2D.LoadImage(File.ReadAllBytes(path));
                }
                texture2D.filterMode = FilterMode.Point;
                Futile.atlasManager.LoadAtlasFromTexture(timelineIcon, texture2D, false);
            }
            Plugin.Log.LogMessage("it's never gtten to this one lmao");
            self.RefreshGraphics();
            self.ClearSprites();
            self.regionIcon = new FSprite(timelineIcon, true);
            self.regionIcon.SetAnchor(0.5f, 0.5f);
            self.map.container.AddChild(self.regionIcon);
        }
    }
}
