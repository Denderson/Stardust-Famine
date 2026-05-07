using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.Creatures.Scavs.ScavSeer
{
    public class ScavSeer : Scavenger
    {
        public int haloActivationTime;
        public ScavSeer(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
            haloActivationTime = 0;
        }
        public override void InitiateGraphicsModule()
        {
            graphicsModule ??= new ScavSeerGraphics(this);
            graphicsModule.Reset();
        }
        public override void LoseAllGrasps()
        {
            ReleaseGrasp(0);
        }
    }
}
