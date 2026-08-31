using Fisobs.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Items.Normal.TorchSpears
{
    public class TorchSpearAbstract : AbstractSpear
    {
        public bool isLit = true;

        public TorchSpearAbstract(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID, bool explosive, float hue)  : base(world, realizedObject, pos, ID, explosive, hue)
        {
            type = Enums.AbstractObjectType.TorchSpear;
        }

        public TorchSpearAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, null, pos, ID, false, 0f)
        {
            type = Enums.AbstractObjectType.TorchSpear;
        }

        public override string ToString()
        {
            string baseString = base.ToString();
            return baseString + "<oA>" + (isLit ? "1" : "0");
        }
    }
}
