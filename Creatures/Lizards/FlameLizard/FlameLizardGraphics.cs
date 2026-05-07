using LizardCosmetics;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Creatures.Lizards.FlameLizard;

public class FlameLizardGraphics : LizardGraphics
{
    public FlameLizardGraphics(FlameLizard ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var spriteIndex = startOfExtraSprites + extraSprites;
        spriteIndex = AddCosmetic(spriteIndex, new ShortBodyScales(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new SpineSpikes(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new LongHeadScales(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new AxolotlGills(this, spriteIndex));

        spriteIndex = Random.value < 0.5f ? AddCosmetic(spriteIndex, new TailTuft(this, spriteIndex)) : AddCosmetic(spriteIndex, new TailFin(this, spriteIndex));
        Random.state = state;
    }
}
