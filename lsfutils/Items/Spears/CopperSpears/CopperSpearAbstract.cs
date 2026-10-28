namespace lsfUtils.Items.Spears.CopperSpears
{
    public class CopperSpearAbstract : AbstractSpear
    {
        public int oxidization = 0;
        public CopperSpearAbstract(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID, bool explosive, float hue) : base(world, realizedObject, pos, ID, explosive, hue)
        {
            type = Enums.AbstractObjectType.CopperSpear;
        }

        public CopperSpearAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, null, pos, ID, false, 0f)
        {
            type = Enums.AbstractObjectType.CopperSpear;
        }

        public override string ToString()
        {
            string baseString = base.ToString();
            return baseString + "<oA>" + oxidization;
        }
    }
}
