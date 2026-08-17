using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.BrownFruit
{
    public class BrownFruitFisob : Fisob
    {
        public BrownFruitFisob() : base(Enums.AbstractObjectType.BrownFruit)
        {
            Icon = new SimpleIcon(templarMaskIcon, RainWorld.GoldRGB);
            RegisterUnlock(Enums.SandboxUnlockID.BrownFruit, MultiplayerUnlocks.SandboxUnlockID.Slugcat);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock unlock)
        {
            BrownFruitAbstract BrownFruitAbstract = new(world, saveData.Pos, saveData.ID, -1, -1, null);

            Log.LogMessage("BrownFruitFisob: Exited parse");
            return BrownFruitAbstract;
        }
    }
}