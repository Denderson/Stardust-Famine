using EffExt;
using lsfUtils.CWTs;
using RWCustom;
using System.Linq;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.DevtoolsEffects.CreepingDarkness;

public class CreepingDarknessUAD
{
    public float darknessProgress = 0f;
    public bool retractDarkness = false;

    public int expandTimer;
    public int retractTimer;
    public int expandIdleTimer;
    public int retractIdleTimer;
    public bool simpleVersion;
    public bool scavLantern;

    public enum DarknessState { Expanding, FullIdle, Retracting, EmptyIdle }
    public DarknessState state = DarknessState.Expanding;
    public int idleCounter = 0;

    public float ExpandSpeed => expandTimer > 0 ? 1f / expandTimer : 1f / 600f;
    public float RetractSpeed => retractTimer > 0 ? 1f / retractTimer : 1f / 200f;

    public CreepingDarknessUAD(World world)
    {
        expandTimer = 600;
        retractTimer = 200;
        expandIdleTimer = 80;
        retractIdleTimer = 120;
        simpleVersion = false;

        if (RegionCWT.TryGetCustomRegionParams(world?.region, out var p))
        {
            expandTimer = p.CreepingDarknessExpandTimer;
            retractTimer = p.CreepingDarknessRetractTimer;
            expandIdleTimer = p.CreepingDarknessExpandIdleTimer;
            retractIdleTimer = p.CreepingDarknessRetractIdleTimer;
            simpleVersion = p.CreepingDarknessSimpleVersion;
        }
    }

    public void Tick()
    {
        switch (state)
        {
            case DarknessState.Expanding:
                {
                    darknessProgress = Mathf.Min(1f, darknessProgress + ExpandSpeed);
                    if (darknessProgress >= 1f)
                    {
                        state = DarknessState.FullIdle;
                        idleCounter = expandIdleTimer;
                    }
                    break;
                }

            case DarknessState.FullIdle:
                {
                    idleCounter--;
                    if (idleCounter <= 0)
                    {
                        state = DarknessState.Retracting;
                    }
                    break;
                }
                

            case DarknessState.Retracting:
                {
                    darknessProgress = Mathf.Max(0f, darknessProgress - RetractSpeed);
                    if (darknessProgress <= 0f)
                    {
                        state = DarknessState.EmptyIdle;
                        idleCounter = retractIdleTimer;
                    }
                    break;
                }

            case DarknessState.EmptyIdle:
                {
                    idleCounter--;
                    if (idleCounter <= 0)
                    {
                        state = DarknessState.Expanding;
                    }
                    break;
                }
                
        }

        retractDarkness = state == DarknessState.Retracting;
    }
}

public class CreepingDarkness
{
    public static void RegisterCreepingDarkness()
    {
        new EffectDefinitionBuilder("CreepingDarkness")
            .SetCategory("lsfUtils")
            .Register();
    }

    public static bool HasEffect(Room room)
    {
        if (room?.roomSettings?.effects == null) return false;
        return TryGetUAD(room.world, out var _);
    }

    public static bool TryGetUAD(World world, out CreepingDarknessUAD uad)
    {
        uad = null;
        if (!WorldCWT.TryGetData(world, out var worldData)) return false;
        uad = worldData.creepingDarkness;
        return uad != null;
    }
}