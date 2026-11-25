using lsfUtils.Items.Darts.Dart;

namespace lsfUtils.Items.Darts.PorcupineSpike;

public class AbstractPorcupineSpike : AbstractDart
{
    public float length;
    public float curve;

    public AbstractPorcupineSpike(World world, WorldCoordinate pos, EntityID ID, float length, float curve) : base(world, null, pos, ID)
    {
        this.length = length;
        this.curve = curve;
    }

    public override void Realize()
    {
        if (realizedObject != null) return;
        realizedObject = new PorcupineSpike(this);
    }
}
