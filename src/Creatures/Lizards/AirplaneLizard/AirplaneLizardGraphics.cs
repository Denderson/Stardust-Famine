using LizardCosmetics;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Creatures.Lizards.AirplaneLizard;

public class AirplaneLizardGraphics : LizardGraphics
{
    public AirplaneLizardGraphics(AirplaneLizard ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var spriteIndex = startOfExtraSprites + extraSprites;
        spriteIndex = AddCosmetic(spriteIndex, new LongShoulderScales(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new SpineSpikes(this, spriteIndex));
        //spriteIndex = AddCosmetic(spriteIndex, new WingScales(this, spriteIndex));

        spriteIndex = Random.value < 0.5f ? AddCosmetic(spriteIndex, new TailTuft(this, spriteIndex)) : AddCosmetic(spriteIndex, new TailFin(this, spriteIndex));
        Random.state = state;
    }
}
