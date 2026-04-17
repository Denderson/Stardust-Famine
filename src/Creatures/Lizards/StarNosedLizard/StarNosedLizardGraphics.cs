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
        overrideHeadGraphic = 2134689;

        var state = UnityEngine.Random.state;
        UnityEngine.Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);

        var spriteIndex = startOfExtraSprites + extraSprites;
        spriteIndex = AddCosmetic(spriteIndex, new SpineSpikes(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new NoseTendrils(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new TailFin(this, spriteIndex));

        UnityEngine.Random.state = state;
    }
}
