using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.RippleFlower
{

    public class RippleFlowerFisob : Fisob
    {

        public RippleFlowerFisob() : base(Enums.AbstractPhysicalObjectType.RippleFlower)
        {
            Icon = new SimpleIcon("Kill_Scavenger", UnityEngine.Color.blue);
            SandboxPerformanceCost = new SandboxPerformanceCost(0.35f, 0f);
            RegisterUnlock(Enums.SandboxUnlockID.RippleFlower, MultiplayerUnlocks.SandboxUnlockID.Slugcat);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock unlock)
        {

            RippleFlowerAbstract rippleFlowerAbstract = new(world, saveData.Pos, saveData.ID, -1, -1, null)
            {
                flowerRippleLayer = int.TryParse(saveData.CustomData, out var layer) ? layer : 0
            };
            if (unlock is SandboxUnlock u)
            {
                rippleFlowerAbstract.flowerRippleLayer = -1;
            }

            if (rippleFlowerAbstract.flowerRippleLayer == -1)
            {
                rippleFlowerAbstract.rippleBothSides = true;
            }
            else
            {
                rippleFlowerAbstract.rippleLayer = rippleFlowerAbstract.flowerRippleLayer;
            }

            Log.LogMessage("Exited parse");
            return rippleFlowerAbstract;
        }
    }

}
