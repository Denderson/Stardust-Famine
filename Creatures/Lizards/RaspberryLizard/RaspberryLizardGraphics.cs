using LizardCosmetics;
using System.Collections.Generic;
using UnityEngine;
using Watcher;
using static lsfUtils.Plugin;

namespace lsfUtils.Creatures.Lizards.RaspberryLizard;

public class RaspberryLizardGraphics : LizardGraphics
{
    public RaspberryLizardGraphics(RaspberryLizard ow) : base(ow)
    {

        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var spriteIndex = startOfExtraSprites + extraSprites;
        if (lizard?.abstractCreature != null)
        {
            if (lizard.abstractCreature.superSizeMe)
            {
                spriteIndex = AddCosmetic(spriteIndex, new PeachHeadStripes(this, spriteIndex));
                spriteIndex = AddCosmetic(spriteIndex, new TailFin(this, spriteIndex));
                spriteIndex = AddCosmetic(spriteIndex, new PeachBackFin(this, spriteIndex));
                spriteIndex = AddCosmetic(spriteIndex, new LongHeadScales(this, spriteIndex));
            }
            else
            {
                spriteIndex = AddCosmetic(spriteIndex, new Antennae(this, spriteIndex));
                spriteIndex = AddCosmetic(spriteIndex, new ShortBodyScales(this, spriteIndex));
            }
        }
        else
        {
            Log.LogMessage("Raspberry lizard > abstract creature doesnt exist!!");
        }
        spriteIndex = AddCosmetic(spriteIndex, new LongShoulderScales(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new SpineSpikes(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new TailTuft(this, spriteIndex));

        Random.state = state;
    }
}
