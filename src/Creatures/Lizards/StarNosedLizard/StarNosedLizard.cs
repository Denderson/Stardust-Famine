using UnityEngine;
using Watcher;
using RWCustom;

namespace lsfUtils.Creatures.Lizards.StarNosedLizard;

public class StarNosedLizard : Lizard
{
    public Vector2 smellPoint = Vector2.zero;
    public int smellRemaining = 0;
    public StarNosedLizard(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        Debug.Log("StarNosed Lizard ctor");
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new StarNosedLizardGraphics(this);

    public override void LoseAllGrasps() => ReleaseGrasp(0);

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (smellRemaining > 0)
        {
            smellRemaining--;
            if (smellRemaining == 0)
            {
                smellPoint = Vector2.zero;
            }
        }
    }
}
