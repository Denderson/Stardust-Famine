using CoralBrain;
using LizardCosmetics;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Creatures.Lizards.StarNosedLizard;

public class StarNosedLizardGraphics : LizardGraphics
{
    public StarNosedLizardGraphics(StarNosedLizard ow) : base(ow)
    {
        // saves the old RNG
        var state = UnityEngine.Random.state;

        // temporarily sets RNG to be as if the seed was the lizards ID
        UnityEngine.Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);

        // activates the RNG that depends on lizard ID, making a lizard with some ID always get the same result
        if (UnityEngine.Random.value < 0.5f)
        {
            overrideHeadGraphic = 2134689;
        }
        else
        {
            overrideHeadGraphic = 11;
        }

        var spriteIndex = startOfExtraSprites + extraSprites;
        spriteIndex = AddCosmetic(spriteIndex, new SpineSpikes(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new NoseTendrils(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new TailFin(this, spriteIndex));

        // returns RNG to the saved value
        UnityEngine.Random.state = state;
    }
}
