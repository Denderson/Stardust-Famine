namespace lsfUtils.Creatures.Scavs.ScavFlank;

public class ScavFlank : Scavenger
{
    public bool reinforcedShield = true;
    public bool reinforcedBreak = false;
    public int shieldIframes = 0;

    public bool stayInOffscreen = true;
    public ScavFlank(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {

    }
    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new ScavengerGraphics(this);
        graphicsModule.Reset();
    }
    public override void LoseAllGrasps()
    {
        ReleaseGrasp(0);
    }
}
