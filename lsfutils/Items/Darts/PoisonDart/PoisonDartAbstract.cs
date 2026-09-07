using lsfUtils.Items.Darts.Dart;
using static lsfUtils.Enums;
using static lsfUtils.Plugin;

public class PoisonDartAbstract : AbstractDart
{
    public PoisonDartAbstract(World world, Dart realizedObject, WorldCoordinate pos, EntityID ID, float poison) : base(world, realizedObject, pos, ID, poison)
    {
        Log.LogMessage("Making an abstract poison dart with poison: " + poison);
        type = lsfUtils.Enums.AbstractObjectType.PoisonDart;
        dartType = DartType.Poison;
    }
}