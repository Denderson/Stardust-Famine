using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fisobs.Core;
using UnityEngine;
using static lsfUtils.Enums;
using lsfUtils.Items;
using lsfUtils.Items.Dart;
using static lsfUtils.Items.Dart.Dart;
using static lsfUtils.Items.Dart.AbstractDart;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.PoisonDart
{
    public class PoisonDartAbstract : AbstractDart
    {
        public PoisonDartAbstract(World world, Dart.Dart realizedObject, WorldCoordinate pos, EntityID ID, float poison) : base(world, realizedObject, pos, ID, poison)
        {
            Log.LogMessage("Making an abstract poison dart!");
            type = Enums.AbstractPhysicalObjectType.PoisonDart;
            dartType = Enums.DartType.Poison;
        }
    }
}
