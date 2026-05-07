using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.Creatures.Scavs.ScavMessenger
{
    public class ScavMessenger : Scavenger
    {
        public int retreatCounter;
        public float message;
        public ScavMessenger(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
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

        public bool WantsToRetreat()
        {
            return retreatCounter >= 100;
        }
    }
}
