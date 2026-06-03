using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fisobs.Core;
using UnityEngine;
using static lsfUtils.Enums;
using static lsfUtils.Items.Darts.Dart.Dart;
using static lsfUtils.Items.Darts.Dart.AbstractDart;
using static lsfUtils.Plugin;
using lsfUtils.Items.Darts.Dart;

namespace lsfUtils.Items.Darts.PoisonDart
{
    public class PoisonDartAbstract : AbstractDart
    {
        public PoisonDartAbstract(World world, Dart.Dart realizedObject, WorldCoordinate pos, EntityID ID, float poison) : base(world, realizedObject, pos, ID, poison)
        {
            Log.LogMessage("Making an abstract poison dart!");
            type = AbstractPhysicalObjectType.PoisonDart;
            dartType = DartType.Poison;
        }
    }
}
