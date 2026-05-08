using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Watcher;
using static Looker.Plugin;

namespace Looker
{
    public static class LizardCode
    {
        public static void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            Log.LogMessage("LIZARD CTOR!!");
            if (world?.game?.StoryCharacter != LookerEnums.looker)
            {
                orig(self, abstractCreature, world);
                return;
            }
            if (world.game.GetStorySession.saveState.GetBool(SaveFileCode.reachedThrone))
            {
                if (self?.LizardState != null)
                {
                    float randomValue = UnityEngine.Random.value;
                    if (randomValue > 0.95f)
                    {
                        self.LizardState.rotType = LizardState.RotType.Full;
                    }
                    else if (randomValue > 0.85f)
                    {
                        self.LizardState.rotType = LizardState.RotType.Opossum;
                    }
                    else if (randomValue > 0.70f)
                    {
                        self.LizardState.rotType = LizardState.RotType.Slight;
                    }
                }
                else
                {
                    Log.LogMessage("Couldnt grab self.LizardState!");
                }
            }

            orig(self, abstractCreature, world);

            Log.LogMessage("LOOKER LIZARD CTOR!!");
            if (world?.region?.name != null)
            {
                string regionName = world.region.name.ToLowerInvariant();
                if (regionName.Contains("wvwa"))
                {
                    self.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.87f, 0.1f, 0.6f), 1f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
                }
                if (regionName.Contains("warg") && (OptionsMenu.strongerLizardChance.Value == 1f || UnityEngine.Random.value < OptionsMenu.strongerLizardChance.Value))
                {
                    Log.LogMessage("Buffing The Surface lizards!!");
                    if (OptionsMenu.lizardsCanLeap.Value && self.jumpModule == null) self.jumpModule = new LizardJumpModule(self);
                    if (OptionsMenu.lizardsCanShield.Value && self.blizzardModule == null) self.blizzardModule = new LizardBlizzardModule(self);
                }
            }
            
            if (self.spawnDataEvil == 0)
            {
                self.spawnDataEvil = 0.3f * OptionsMenu.spawnFileDifficulty.Value;
            }
        }

    }
}
