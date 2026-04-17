using LizardCosmetics;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Creatures.Lizards.PoisonLizard;

public class PoisonLizardGraphics : LizardGraphics
{
    public PoisonLizardGraphics(PoisonLizard ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var spriteIndex = startOfExtraSprites + extraSprites;
        //spriteIndex = AddCosmetic(spriteIndex, new LongShoulderScales(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new LongShoulderScales(this, spriteIndex));
        //spriteIndex = AddCosmetic(spriteIndex, new ShortBodyScales(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new SpineSpikes(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new AxolotlGills(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new TailFin(this, spriteIndex));
        Random.state = state;
    }
}
