using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.KarmaMask
{
    public class KarmaMaskFisob : Fisob
    {
        public KarmaMaskFisob() : base(Enums.AbstractPhysicalObjectType.KarmaMask)
        {
            Icon = new SimpleIcon(templarMaskIcon, RainWorld.GoldRGB);
            SandboxPerformanceCost = new SandboxPerformanceCost(0.2f, 0f);
            RegisterUnlock(Enums.SandboxUnlockID.KarmaMask, MultiplayerUnlocks.SandboxUnlockID.Slugcat);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock unlock)
        {
            KarmaMaskAbstract karmaMaskAbstract = new(world, saveData.Pos, saveData.ID, -1, -1, null);

            if (unlock is SandboxUnlock)
            {
                karmaMaskAbstract.rippleBothSides = true;
            }

            Log.LogMessage("KarmaMaskFisob: Exited parse");
            return karmaMaskAbstract;
        }
    }
}