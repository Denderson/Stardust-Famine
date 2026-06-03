using MoreSlugcats;
using RWCustom;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.KarmaMask
{
    public class KarmaMask : VultureMask
    {
        public KarmaMaskAbstract KarmaMaskAbstract => abstractPhysicalObject as KarmaMaskAbstract;

        public KarmaMask(KarmaMaskAbstract abstr) : base(abstr, null)
        {
            Log.LogMessage("Spawning karma mask!");
            abstractPhysicalObject.rippleBothSides = true;
            if (CWTs.VultureMaskCWT.TryGetData(this, out var data))
            {
                data.isKarmaMask = true;
            }
            else
            {
                Log.LogMessage("Couldn't get VultureMask CWT from KarmaMaskObject ctor!");
            }
        }

        public static bool IsKarmaMask(VultureMask mask)
        {
            if (mask == null) return false;
            if (!CWTs.VultureMaskCWT.TryGetData(mask, out var data)) return false;
            return data.isKarmaMask;
        }
    }
}