using EffExt;
using lsfUtils.CWTs;
using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.DevtoolsEffects.EvilWater;

public class EvilWater
{

    public static void RegisterEvilWater()
    {
        new EffectDefinitionBuilder("EvilWater")
            .SetCategory("lsfUtils")
            .Register();
    }

    public static bool HasEffect(Room room)
    {
        if (room?.roomSettings?.effects == null) return false;
        return room.roomSettings.GetEffect(Enums.EffectTypes.EvilWater) != null;
    }
}