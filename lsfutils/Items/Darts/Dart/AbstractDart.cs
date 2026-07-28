using Fisobs.Core;
using lsfUtils.Items.Darts.Dart;
using static lsfUtils.Enums;
using static lsfUtils.Plugin;

public class AbstractDart : AbstractPhysicalObject
{
    public float poison;
    public DartType dartType;
    public Dart realisedDart;

    public AbstractDart(World world, Dart realizedObject, WorldCoordinate pos, EntityID ID, float poison = 0f) : base(world, lsfUtils.Enums.AbstractObjectType.Dart, realizedObject, pos, ID)
    {
        this.poison = poison;
        this.dartType = DartType.Default;
        Log.LogMessage("Made abstract dart!");
    }

    public override void Realize()
    {
        base.Realize();

        if (realizedObject == null)
        {
            if (dartType == DartType.Default)
            {
                Log.LogMessage("Dart type is Default, creating Dart");
                realizedObject = new Dart(this);
                return;
            }

            if (dartType == DartType.Poison)
            {
                Log.LogMessage("Dart type is Poison, creating PoisonDart");
                realizedObject = new lsfUtils.Items.Darts.PoisonDart.PoisonDart(this);
                return;
            }

            Log.LogMessage("Dart type unknown, creating default Dart");
            realizedObject = new Dart(this);
        }
    }

    public override string ToString()
    {
        return this.SaveToString($"{poison},{dartType}");
    }
}