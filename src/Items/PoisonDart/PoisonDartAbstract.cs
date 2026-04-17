using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fisobs.Core;
using UnityEngine;

namespace lsfUtils.Items.PoisonDart
{
    public class PoisonDartAbstract : AbstractSpear
    {
        public float remainingPoison;

        public PoisonDartAbstract(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID) : base(world, realizedObject, pos, ID, explosive: false)
        {
            type = Enums.AbstractPhysicalObjectType.PoisonDart;
            remainingPoison = 1.5f;
        }

        public PoisonDartAbstract(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID, float remainingPoison) : this(world, realizedObject, pos, ID)
        {
            type = Enums.AbstractPhysicalObjectType.PoisonDart;
            this.remainingPoison = remainingPoison;
        }

        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
            {
                realizedObject = new PoisonDart(this, remainingPoison);
            }
        }

        public override string ToString()
        {
            return this.SaveToString($"{remainingPoison}");
        }
    }

}
