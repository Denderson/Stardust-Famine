using LizardCosmetics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Watcher;
using static lsfUtils.Plugin;

namespace lsfUtils.Creatures.Lizards.RaspberryLizard
{
    public static class RaspberryLizardHooks
    {
        public static void Antennae_ctor(ILContext il)
        {
            var c = new ILCursor(il);

            try
            {
                if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt(typeof(UnityEngine.Random), "get_value"),
                x => x.MatchStfld<Antennae>("length")))
                {
                    c.Emit(OpCodes.Ldarg_1);

                    c.EmitDelegate<Func<float, LizardGraphics, float>>((origValue, lg) =>
                    {
                        if (lg?.lizard is RaspberryLizard) return 1f;
                        return origValue;
                    });
                }
                else
                {
                    Log.LogMessage("Antennae_ctor IL hook failed: Random.value not found!");
                }
            }
            catch (Exception e) { Log.LogMessage(e); }
        }

        public static PathCost LizardPather_HeuristicForCell(On.LizardPather.orig_HeuristicForCell orig, LizardPather self, PathFinder.PathingCell cell, PathCost costToGoal)
        {
            if (self?.creature?.creatureTemplate?.type == Enums.CreatureTemplateType.RaspberryLizard)
            {
                return costToGoal;
            }
            return orig(self, cell, costToGoal);
        }

        public static PathCost LizardAI_TravelPreference(On.LizardAI.orig_TravelPreference orig, LizardAI self, MovementConnection connection, PathCost cost)
        {
            if (self?.creature?.creatureTemplate?.type == Enums.CreatureTemplateType.RaspberryLizard)
            {
                return self.yellowAI.TravelPreference(connection, cost);
            }
            return orig(self, connection, cost);
        }

        public static void LizardAI_ctor(On.LizardAI.orig_ctor orig, LizardAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            if (creature?.creatureTemplate?.type == Enums.CreatureTemplateType.RaspberryLizard)
            {
                if (self.yellowAI == null)
                {
                    self.yellowAI = new YellowAI(self);
                    self.AddModule(self.yellowAI);
                }
                if (self.redSpitAI == null)
                {
                    self.redSpitAI = new LizardAI.LizardSpitTracker(self);
                    self.AddModule(self.redSpitAI);
                }
                self.pathFinder.stepsPerFrame = 30;
            }
        }
    }
}
