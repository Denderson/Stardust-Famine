using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static lsfUtils.Plugin;

namespace lsfUtils.CreatureTags
{
    public static class CreatureFlagHooks
    {
        public static void ApplyHooks()
        {
            On.AbstractCreature.setCustomFlags += CreatureFlagHooks.AbstractCreature_setCustomFlags;
        }
        public static void AbstractCreature_setCustomFlags(On.AbstractCreature.orig_setCustomFlags orig, AbstractCreature self)
        {
            orig(self);
            foreach (string unrecognisedFlags in self.unrecognizedFlags)
            {
                Log.LogMessage("Reading unrecognised flags!");
                string value = unrecognisedFlags?.ToLowerInvariant();

                if (value == null)
                {
                    continue;
                }

                if (value.Contains("ripplehybrid"))
                {
                    Log.LogMessage("Ripplehybrid check!");
                    int rippleLayer = 1;
                    bool rippleBoth = false;
                    if (value.Contains(':') && value.Split(':').Length > 1 && int.TryParse(value.Split(':')[1], out rippleLayer))
                    {
                        Log.LogMessage("Ripplehybrid layer override!");
                        if (rippleLayer == -1)
                        {
                            rippleBoth = true;
                            rippleLayer = 0;
                        }
                    }
                    RippleHybridHooks.RipplifyAbstractObject(self, rippleLayer, rippleBoth);
                }

                if (value.Contains("poisonimmune"))
                {
                    Log.LogMessage("PoisonImmuneHooks check!");
                    PoisonImmuneHooks.SetupPoisonImmune(self);
                }

                if (value.Contains("echoimmune"))
                {
                    Log.LogMessage("EchoImmuneHooks check!");
                    EchoImmuneHooks.SetupEchoImmune(self);
                }

                if (value.Contains("starving"))
                {
                    Log.LogMessage("StarveTag check!");
                    EchoImmuneHooks.SetupEchoImmune(self);
                }

                if (value.Contains("faction"))
                {
                    if (value.Contains(':') && value.Split(':').Length > 1)
                    {
                        string faction = value.Split(':')[1];
                        if (!string.IsNullOrEmpty(faction))
                        {
                            Factions.SetupFaction(self, value.Split(':')[1]);
                        }
                        else Log.LogMessage("Invalid faction!");
                    }
                }
            }
        }
    }
}
