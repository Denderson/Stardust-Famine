using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using Watcher;
using static Stardust.Plugin;
using Stardust.SaveFile;

namespace Stardust
{
    public static class GhostCode
    {
        public static bool NoEchoPriming(On.GhostWorldPresence.orig_SpawnGhost orig, GhostWorldPresence.GhostID ghostID, int karma, int karmaCap, int ghostPreviouslyEncountered, bool playingAsRed)
        {
            if (SharedMechanics(Custom.rainWorld.progression.currentSaveState.saveStateNumber))
                return orig(ghostID, karmaCap, karmaCap, ghostPreviouslyEncountered, true);
            return orig(ghostID, karma, karmaCap, ghostPreviouslyEncountered, playingAsRed);
        }

        public static int DynamicNumberOfFlakes(On.GoldFlakes.orig_NumberOfFlakes orig, GoldFlakes self, float ghostMode)
        {
            if (SharedMechanics(self?.room?.game?.StoryCharacter))
                return (int)(orig(self, ghostMode) * (0.5f + self.room.game.GetStorySession.saveState.EchoEncounters() * 0.3f));
            return orig(self, ghostMode);
        }

        public static void MinKarmaOnEchoScreen(On.Menu.GhostEncounterScreen.orig_GetDataFromGame orig, Menu.GhostEncounterScreen self, Menu.KarmaLadderScreen.SleepDeathScreenDataPackage package)
        {
            if (SharedMechanics(package.saveState.saveStateNumber))
                package.karma.x = Mathf.Min(package.karma.x, package.saveState.deathPersistentSaveData.MinKarma());
            orig(self, package);
        }

        public static void RippleDepthsNearEchoes(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);
            if (SharedMechanics(self.game?.StoryCharacter))
            {
                float num = 0f;
                for (int i = 0; i < self.cameraPositions.Length; i++)
                    if (self.world.worldGhost != null)
                        num = Mathf.Max(num, self.world.worldGhost.GhostMode(self, i));
                if (num > 0.3)
                    RippleDepths.SpawnRippleDephts(self, Mathf.InverseLerp(0.9f, 0.45f, num));
            }
        }

        public static void CheckpointAfterEcho(On.Ghost.orig_FadeOutFinished orig, Ghost self)
        {
            if (self?.room?.game?.GetStorySession?.saveStateNumber == Enums.SlugcatStatsName.sfscholar)
                RainWorldGame.ForceSaveNewDenLocation(self.room.game, self.room.abstractRoom.name, saveWorldStates: false);
            orig(self);
            return;
        }
    }
}
