using MoreSlugcats;
using RWCustom;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.BrownFruit
{
    public class BrownFruit : DangleFruit
    {
        public BrownFruitAbstract BrownFruitAbstract => abstractPhysicalObject as BrownFruitAbstract;

        public BrownFruit(BrownFruitAbstract abstr, World world) : base(abstr)
        {
            abstractPhysicalObject = abstr;
            abstr.realizedObject = this;
        }
    }
}