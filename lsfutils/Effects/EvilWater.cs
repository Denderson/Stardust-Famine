using EffExt;
using lsfUtils.CWTs;
using System;
using System.Linq;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Effects.EvilWater;

public class EvilWater
{
    public static void RegisterEvilWater()
    {
        try
        {
            new EffectDefinitionBuilder("EvilWater")
                .AddFloatField("placeholder", 0f, 1f, 0.1f, 1f, "placeholder")
                //.AddIntField("delay", 0, 80, 40, "Delay until poison")
                //.AddFloatField("speed", 0f, 1f, 0.1f, 1f, "Poison speed")
                .SetUADFactory((room, data, firstTimeRealized) => new EvilWaterEffectUAD(data))
                .SetCategory("lsfUtils")
                .Register();
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Error on eff examples init {ex}");
        }
    }

    public static void InitialiseEvilWater(On.Water.orig_ctor orig, Water self, Room room, int waterLevel)
    {
        orig(self, room, waterLevel);
        if (room.roomSettings.GetEffectAmount(Enums.EffectTypes.EvilWater) > 0f && CWTs.WaterCWT.TryGetData(self, out var data))
        {
            data.isPoisonous = true;
            // setting to a CWT so I dont need to grab room effects every tick
        }
    }

    public static void EvilWaterLogic(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        // checks to fail code and call vanilla
        if (CreatureCWT.TryGetData(self, out var data))
        {
            orig(self, eu);
            return;
        }
        if (!WaterCWT.TryGetData(self.room.waterObject, out var waterdata))
        {
            orig(self, eu);
            return;
        }
        if (self?.room == null)
        {
            orig(self, eu);
            return;
        }

        float oldPoison = self.injectedPoison;
        self.injectedPoison = Mathf.Min(1f, self.injectedPoison + data.temporaryPoison);
        orig(self, eu);
        self.injectedPoison = oldPoison;

        if (self.Submersion > 0.5f && waterdata.isPoisonous)
        {
            if (data.timeInEvilWater < (int)(secondsUntilPoisonStartsFallingOffAfterExitingPoisonWater * 40)) data.timeInEvilWater++;
        }
        else
        {
            if (data.timeInEvilWater > 0)
            {
                data.timeInEvilWater--;
            }
        }
        if (data.timeInEvilWater > (int)(secondsUntilPoisonBuildupAfterEnteringPoisonWater * 40))
        {
            // buildup poison while submerged
            data.temporaryPoison = Mathf.Min(1f, data.temporaryPoison + (float)(1 / (40 * secondsUntilFullPoisonFromEvilWater)));
        }
        else
        {
            // poison decrease while not submerged
            data.temporaryPoison = Mathf.Max(0f, data.temporaryPoison - (float)(1 / (40 * secondsUntilFullPoisonFromEvilWater)));
        }
    }

    public float OverridePoison(Func<Creature, float> orig, Creature self)
    {
        float result = orig(self);
        if (self != null && CWTs.CreatureCWT.TryGetData(self, out var data))
        {
            return Mathf.Max(result, data.temporaryPoison);
        }
        return result;
    }
}

public class EvilWaterEffectUAD : UpdatableAndDeletable
{
    public EffectExtraData EffectData { get; }

    public EvilWaterEffectUAD(EffectExtraData effectData)
    {
        EffectData = effectData;
    }

    public override void Update(bool eu)
    {
        // add update here? idk
        // not really needed since hooking Water.ctor is possible and how its done in vanilla
        // but oh well
    }
}

