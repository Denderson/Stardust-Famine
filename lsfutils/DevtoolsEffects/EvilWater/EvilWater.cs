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
    public const string delayKey = "delay";
    public const string speedKey = "speed";
    public const string temporaryKey = "temporary";

    public static void RegisterEvilWater()
    {
        new EffectDefinitionBuilder("EvilWater")
            .AddFloatField(delayKey, 0, 2f, 0.1f, 1f, "Delay")
            .AddFloatField(speedKey, 0f, 1f, 0.1f, 1f, "Speed")
            .AddBoolField(temporaryKey, true, "Temporary")
            .SetCategory("lsfUtils")
            .Register();
    }

    public static bool HasEffect(Room room)
    {
        if (room?.roomSettings?.effects == null) return false;
        return room.roomSettings.GetEffect(Enums.EffectTypes.EvilWater) != null;
    }
}