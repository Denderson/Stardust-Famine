using Stardust.SaveFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Stardust.Plugin;

namespace Stardust.Mechanics
{
    public static class SpawnCode
    {
        public static int Migration_MaxCapacity(Func<VoidSpawnMigrationStream, int> orig, VoidSpawnMigrationStream self)
        {
            int value = orig(self);
            if (SharedMechanics(self?.room?.game?.StoryCharacter) && self.room.game.GetStorySession.saveState.EchoEncounters() < 2)
                return 5;
            return value;
        }

        public static void StarspawnKillCode(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (SharedMechanics(self?.room?.game.StoryCharacter))
            {
                self.RippleSpawnInteractions();
                self.warpFatigueEffect -= 0.002f;
                self.warpFatigueEffect = Mathf.Clamp(self.warpFatigueEffect, 0f, 1f);
                self.rippleDeathIntensity -= 0.002f;
                self.rippleDeathIntensity = Mathf.Clamp(self.warpFatigueEffect, 0f, 1f);
            }
        }

        public static bool CanSee_VoidSpawn(Func<SaveState, bool> orig, SaveState self)
        {
            return orig(self) || SharedMechanics(self?.saveStateNumber) && self.EchoEncounters() > 0;
        }
    }
}
