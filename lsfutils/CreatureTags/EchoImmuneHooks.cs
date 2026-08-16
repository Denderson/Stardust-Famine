using lsfUtils.CWTs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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
    public static class EchoImmuneHooks
    {
        public static void ApplyHooks()
        {
            IL.GhostCreatureSedater.Update += EchoImmuneHooks.GhostCreatureSedater_Update;
        }
        public static void SetupEchoImmune(this AbstractCreature abstractCreature)
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
            data.isEchoImmune = true;
        }

        public static bool IsEchoImmune(this Creature creature)
        {
            if (creature?.abstractCreature == null) return false;

            if (!AbstractCreatureCWT.TryGetData(creature.abstractCreature, out var data))
                return false;

            return data.isEchoImmune;
        }

        public static void GhostCreatureSedater_Update(ILContext il)
        {
            try
            {
                var cursor = new ILCursor(il);
                int patchCount = 0;
                while (cursor.TryGotoNext(MoveType.Before, x => x.MatchLdfld(out var f) && f.Name == nameof(CreatureTemplate.ghostSedationImmune), x => x.MatchBrtrue(out _)))
                {
                    cursor.Index++;
                    cursor.Emit(OpCodes.Ldloc_3);
                    cursor.EmitDelegate<Func<bool, AbstractCreature, bool>>((vanillaImmune, abstractCreature) =>  vanillaImmune || (abstractCreature?.realizedCreature?.IsEchoImmune() ?? false));
                    patchCount++;
                }
                Log.LogMessage($"ghostSedationImmune check patch count: {patchCount}");
            }
            catch (Exception ex)
            {
                Log.LogMessage($"ghostSedationImmune error! {ex}");
            }
        }
    }
}
