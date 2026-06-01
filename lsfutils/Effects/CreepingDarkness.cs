using EffExt;
using lsfUtils.CWTs;
using System;
using System.Linq;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Effects;

public class CreepingDarkness
{
    public const float secondsUntilFullDarkness = 15;
    public const float secondsUntilNoDarkness = 5;
    public const int ticksInIdleWhenFull = 80;
    public const int ticksInIdleWhenNone = 120;

    public const string simpleDarknessKey = "simpleDarkness";

    public static void RegisterEvilWater()
    {
        try
        {
            new EffectDefinitionBuilder("CreepingDarkness")
                .AddFloatField("delay", 0, 2f, 0.1f, 1f, "Delay")
                .AddFloatField("speed", 0f, 1f, 0.1f, 1f, "Speed")
                .AddBoolField(simpleDarknessKey, false, "SimpleDarkness")
                .SetUADFactory((room, data, firstTimeRealized) => new EvilWaterEffectUAD(data))
                .SetCategory("lsfUtils")
                .Register();
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Error on eff examples init {ex}");
        }
    }
}

public class CreepingDarknessUAD : UpdatableAndDeletable
{
    public EffectExtraData EffectData { get; }

    public bool simpleDarkness;

    public CreepingDarknessUAD(EffectExtraData effectData)
    {
        EffectData = effectData;
        simpleDarkness = EffectData.GetBool(CreepingDarkness.simpleDarknessKey);
    }

    public override void Update(bool eu)
    {

    }
}