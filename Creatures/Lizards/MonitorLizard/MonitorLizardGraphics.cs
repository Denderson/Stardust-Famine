using LizardCosmetics;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Creatures.Lizards.MonitorLizard;

public class MonitorLizardGraphics : LizardGraphics
{
    public MonitorLizardGraphics(MonitorLizard ow) : base(ow)
    {
        overrideHeadGraphic = 2134688;

        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var spriteIndex = startOfExtraSprites + extraSprites;
        spriteIndex = AddCosmetic(spriteIndex, new LongShoulderScales(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new SpineSpikes(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new AxolotlGills(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new TailFin(this, spriteIndex));
        Random.state = state;
    }
}
