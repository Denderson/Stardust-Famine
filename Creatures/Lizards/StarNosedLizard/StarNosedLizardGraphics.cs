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
        if (UnityEngine.Random.value < 0.16f)
        {
            overrideHeadGraphic = 2134689;
        }
        else if (UnityEngine.Random.value < 0.33f)
        {
            overrideHeadGraphic = 2134690;
        }
        else if (UnityEngine.Random.value < 0.5f)
        {
            overrideHeadGraphic = 2134691;
        }
        else if (UnityEngine.Random.value < 0.66f)
        {
            overrideHeadGraphic = 2134692;
        }
        else if (UnityEngine.Random.value < 0.83f)
        {
            overrideHeadGraphic = 2134693;
        }
        else
        {
            overrideHeadGraphic = 2134694;
        }

        var spriteIndex = startOfExtraSprites + extraSprites;
        spriteIndex = AddCosmetic(spriteIndex, new SpineSpikes(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new NoseTendrils(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new TailFin(this, spriteIndex));

        // returns RNG to the saved value
        UnityEngine.Random.state = state;
    }
}
