using LizardCosmetics;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Creatures.Lizards.WeaverLizard;

public class WeaverLizardGraphics : LizardGraphics
{
    public WeaverLizardGraphics(WeaverLizard ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var spriteIndex = startOfExtraSprites + extraSprites;
        spriteIndex = AddCosmetic(spriteIndex, new LongShoulderScales(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new LongShoulderScales(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new LongShoulderScales(this, spriteIndex));
        //spriteIndex = AddCosmetic(spriteIndex, new ShortBodyScales(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new SpineSpikes(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new LongHeadScales(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new TailTuft(this, spriteIndex));
        Random.state = state;
    }
}
