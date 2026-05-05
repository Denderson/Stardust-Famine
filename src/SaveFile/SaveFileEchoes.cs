using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using static Stardust.Plugin;
using static Stardust.SaveFile.SaveFileMain;

namespace Stardust.SaveFile
{
    public static class SaveFileEchoes
    {
        public static bool HalfwayEchoes(this SaveState data) => data.EchoEncounters() > maxEchoes / 2;
        public static int EchoEncounters(this DeathPersistentSaveData data)
        {
            return (data.maximumRippleLevel >= 1f) ? maxEchoes : math.clamp(data.karmaCap - 5, 0, maxEchoes);
        }

        public static int EchoEncounters(this SaveState save)
        {
            if (save?.deathPersistentSaveData != null && SharedMechanics(save.saveStateNumber))
                return save.deathPersistentSaveData.EchoEncounters();
            return 0;
        }

        public static bool Ripple(this SaveState data)
        {
            if (data?.deathPersistentSaveData != null)
                return data.deathPersistentSaveData.GetBool(rippleSequenceDone);
            return false;
        }

        public static int MinKarma(this DeathPersistentSaveData data) => (1 + data.EchoEncounters()) / 2;
        public static int MinKarma(this SaveState data) => (1 + data.EchoEncounters()) / 2;
        public static int MinKarma(this Menu.Menu menu)
        {
            return menu switch {
                SleepAndDeathScreen sads => sads.saveState.MinKarma(),
                KarmaLadderScreen kls => kls.saveState.MinKarma(),
                _ => 0
            };
        }
    }
}
