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
    public static class EchoImmune
    {
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
            var cursor = new ILCursor(il);
            int patchCount = 0;

            while (cursor.TryGotoNext(MoveType.After, x => x.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.ghostSedationImmune))))
            {
                VariableDefinition creatureLocal = null;
                var search = cursor.Clone();
                while (search.TryGotoPrev(x => x.MatchStloc(out _)))
                {
                    var local = il.Body.Variables[((VariableReference)search.Next.Operand).Index];
                    if (local.VariableType.FullName == typeof(AbstractCreature).FullName)
                    {
                        creatureLocal = local;
                        break;
                    }
                }

                if (creatureLocal == null)
                {
                    Log.LogWarning("EchoImmuneHooks: Could not find AbstractCreature local, skipping patch.");
                    continue;
                }

                cursor.Emit(OpCodes.Ldloc, creatureLocal);
                cursor.EmitDelegate<System.Func<bool, AbstractCreature, bool>>((vanillaImmune, ac) =>
                    vanillaImmune || (ac?.realizedCreature?.IsEchoImmune() ?? false));

                patchCount++;
            }

            if (patchCount == 0) Log.LogWarning("EchoImmuneHooks: No ghostSedationImmune fields were patched — the hook has no effect.");
            else if (patchCount != 2) Log.LogWarning($"EchoImmuneHooks: Expected 2 patches, got {patchCount}. Some immunity checks may be missing.");
        }
    }
}
