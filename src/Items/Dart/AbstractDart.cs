using Fisobs.Core;
using lsfUtils.Items.PoisonDart;
using System;
using System.Globalization;
using static lsfUtils.Plugin;
using static lsfUtils.Enums;
using lsfUtils.Items;
using lsfUtils.Items.Dart;

namespace lsfUtils.Items.Dart
{
    public class AbstractDart : AbstractPhysicalObject
    {
        public float poison;
        public DartType dartType;

        public AbstractDart(World world, Dart realizedObject, WorldCoordinate pos, EntityID ID, float poison = 0f) : base(world, AbstractPhysicalObjectType.Dart, realizedObject, pos, ID)
        {
            this.poison = poison;
            Log.LogMessage("Made abstract dart!");
        }

        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
            {
                if (dartType == DartType.Default)
                {
                    Log.LogMessage("Dart type is default!");
                    realizedObject = new Dart(this);
                    return;
                }
                if (dartType == DartType.Poison)
                {
                    Log.LogMessage("Dart type is poison!");
                    realizedObject = new PoisonDart.PoisonDart(this);
                    return;
                }
                Log.LogMessage("Dart type is none!");
            }
        }

        public override string ToString()
        {
            return this.SaveToString($"{poison},{dartType}");
        }
    }
}