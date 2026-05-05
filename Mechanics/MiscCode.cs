using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.Mechanics
{
    public static class MiscCode
    {
        public static float LocustSystem_SwarmScore_Creature(On.LocustSystem.orig_SwarmScore_Creature orig, LocustSystem self, Creature crit)
        {
            if (crit != null && crit.Submersion > 0.5)
            {
                return 0f;
            }
            return orig(self, crit);
        }
    }
}
