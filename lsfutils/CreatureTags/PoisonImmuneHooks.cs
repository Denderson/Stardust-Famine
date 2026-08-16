using lsfUtils.CWTs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.CreatureTags
{
    public static class PoisonImmuneHooks
    {
        public static void ApplyHooks()
        {
            On.Creature.InjectPoison += PoisonImmuneHooks.Creature_InjectPoison;
            On.Creature.Update += PoisonImmuneHooks.Creature_Update;
        }
        public static void SetupPoisonImmune(this AbstractCreature abstractCreature)
        {
            if (abstractCreature == null)
            {
                return;
            }
            if (!AbstractCreatureCWT.TryGetData(abstractCreature, out var data))
            {
                Log.LogMessage("Couldnt get AbstractCreatureCWT!");
                return;
            }
            data.isPoisonImmune = true;
        }

        public static bool IsPoisonImmune(this Creature creature)
        {
            if (creature?.abstractCreature == null) return false;

            if (!AbstractCreatureCWT.TryGetData(creature.abstractCreature, out var data))
                return false;

            return data.isPoisonImmune;
        }

        public static void Creature_InjectPoison(On.Creature.orig_InjectPoison orig, Creature self, float amount, Color poisonColor)
        {
            if (self != null && self.IsPoisonImmune())
            {
                return;
            }
            orig(self, amount, poisonColor);
        }

        public static void Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
        {
            if (self != null && self.IsPoisonImmune())
            {
                self.injectedPoison = 0f;
            }    
            orig(self, eu);

        }
    }
}
