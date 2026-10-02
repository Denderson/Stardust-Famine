/*using EffExt;
using lsfUtils.CWTs;
using RWCustom;
using System.Linq;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.DevtoolsEffects.Summer;

public class SummerUAD
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

    public SummerUAD(World world)
    {
        expandTimer = 600;
        retractTimer = 200;
        expandIdleTimer = 80;
        retractIdleTimer = 120;
        simpleVersion = false;

        if (RegionCWT.TryGetCustomRegionParams(world?.region, out var p))
        {
            expandTimer = p.SummerExpandTimer;
            retractTimer = p.SummerRetractTimer;
            expandIdleTimer = p.SummerExpandIdleTimer;
            retractIdleTimer = p.SummerRetractIdleTimer;
            simpleVersion = p.SummerSimpleVersion;
        }
    }

    public void Tick()
    {

    }
}

public class Summer
{
    public static void RegisterSummer()
    {
        new EffectDefinitionBuilder("Summer")
            .SetCategory("lsfUtils")
            .Register();
    }

    public static bool HasEffect(Room room)
    {
        if (room?.roomSettings?.effects == null) return false;
        return TryGetUAD(room.world, out var _);
    }

    public static bool TryGetUAD(World world, out SummerUAD uad)
    {
        uad = null;
        if (!WorldCWT.TryGetData(world, out var worldData)) return false;
        uad = worldData.Summer;
        return uad != null;
    }
}
*/