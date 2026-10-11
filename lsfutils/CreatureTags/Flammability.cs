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
    internal static class Flammability
    {
        public static void SetupCustomFlammability(AbstractCreature abstractCreature, int flammability, char? sign)
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
            data.customFlammability = true;
            data.flammabilityValue = flammability;
            data.flammabilitySign = sign;
        }

        public static bool IsCustomFlammable(this Creature creature)
        {
            if (creature?.abstractCreature == null) return false;
            if (!AbstractCreatureCWT.TryGetData(creature.abstractCreature, out var data))
                return false;
            return data.customFlammability;
        }

        public static bool IsCustomFlammable(this AbstractCreature creature)
        {
            if (!AbstractCreatureCWT.TryGetData(creature, out var data)) return false;

            return data.customFlammability;
        }
    }
}
