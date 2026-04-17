using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Creatures.Spawn
{
    public class StarJelly : StarSpawn
    {
        public StarJelly(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {

        }
        public override void InitiateGraphicsModule()
        {
            graphicsModule ??= new StarSpawnGraphics(this);
            graphicsModule.Reset();
        }
        public override void LoseAllGrasps()
        {
            ReleaseGrasp(0);
        }
    }

    public class StarNoodle : StarSpawn
    {
        public StarNoodle(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {

        }
        public override void InitiateGraphicsModule()
        {
            graphicsModule ??= new StarSpawnGraphics(this);
            graphicsModule.Reset();
        }
        public override void LoseAllGrasps()
        {
            ReleaseGrasp(0);
        }
    }

    public class StarAmoeba : StarSpawn
    {
        public StarAmoeba(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {

        }
        public override void InitiateGraphicsModule()
        {
            graphicsModule ??= new StarSpawnGraphics(this);
            graphicsModule.Reset();
        }
        public override void LoseAllGrasps()
        {
            ReleaseGrasp(0);
        }
    }
}
