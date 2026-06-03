using EffExt;
using lsfUtils.CWTs;
using RWCustom;
using System.Linq;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Effects;

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

    public float ExpandSpeed => expandTimer > 0 ? 1f / expandTimer : 1f / 600f;
    public float RetractSpeed => retractTimer > 0 ? 1f / retractTimer : 1f / 200f;

    public CreepingDarknessUAD(World world)
    {
        expandTimer = 600;
        retractTimer = 200;
        expandIdleTimer = 80;
        retractIdleTimer = 120;
        simpleVersion = false;
        scavLantern = false;

        if (RegionCWT.TryGetCustomRegionParams(world?.region, out var p))
        {
            expandTimer = p.CreepingDarknessExpandTimer;
            retractTimer = p.CreepingDarknessRetractTimer;
            expandIdleTimer = p.CreepingDarknessExpandIdleTimer;
            retractIdleTimer = p.CreepingDarknessRetractIdleTimer;
            simpleVersion = p.CreepingDarknessSimpleVersion;
            scavLantern = p.CreepingDarknessScavLantern;
        }
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
        return room.roomSettings.GetEffect(Enums.EffectTypes.CreepingDarkness) != null;
    }

    public static bool TryGetUAD(World world, out CreepingDarknessUAD uad)
    {
        uad = null;
        if (!WorldCWT.TryGetData(world, out var worldData)) return false;
        uad = worldData.creepingDarkness;
        return uad != null;
    }
    public static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig(self);

        if (!HasEffect(self)) return;
        if (self.world == null) return;
        if (!WorldCWT.TryGetData(self.world, out var worldData)) return;
        if (worldData.creepingDarkness != null) return;

        worldData.creepingDarkness = new CreepingDarknessUAD(self.world);
        Log.LogMessage($"CreepingDarkness: UAD initialised for world '{self.world.region?.name}'.");
    }
    public static void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
    {
        orig(self);

        if (self.room == null || !HasEffect(self.room)) return;
        if (!TryGetUAD(self.room.world, out var uad)) return;

        if (uad.retractDarkness) uad.darknessProgress = Mathf.Max(0f, uad.darknessProgress - uad.RetractSpeed);
        else uad.darknessProgress = Mathf.Min(1f, uad.darknessProgress + uad.ExpandSpeed);

        self.sofBlackFade = uad.darknessProgress;
        self.effect_darkness = uad.darknessProgress;
        self.lightBloomAlpha = 1f - uad.darknessProgress;
    }
    public static void LightSource_Update(On.LightSource.orig_Update orig, LightSource self, bool eu)
    {
        orig(self, eu);

        if (self?.room == null || !HasEffect(self.room)) return;
        if (self.noGameplayImpact || self.slatedForDeletetion) return;
        if (self.Pos == null || self.tiedToObject == null || self.tiedToObject is not PhysicalObject) return;
        if (self.room.PlayersInRoom == null || self.room.PlayersInRoom.Count == 0) return;

        foreach (Player player in self.room.PlayersInRoom)
        {
            if (player?.bodyChunks == null || player.bodyChunks.Length == 0) continue;
            if (!Custom.DistLess(player.mainBodyChunk.pos, self.Pos, 100f)) continue;

            if (PlayerCWT.TryGetData(player, out var data)) data.darknessImmunity = 120;
            else Log.LogMessage("CreepingDarkness: Couldn't find PlayerCWT!");
        }
    }
    public static void Lantern_Update(On.Lantern.orig_Update orig, Lantern self, bool eu)
    {
        orig(self, eu);

        if (self?.room == null || !HasEffect(self.room)) return;
        if (!TryGetUAD(self.room.world, out var uad)) return;
        if (self.stick != null) return; // LanternStick handles this case

        foreach (Player player in self.room.PlayersInRoom)
        {
            if (player?.abstractCreature?.realizedCreature == null) continue;
            if (!Custom.DistLess(self.firstChunk.pos, player.firstChunk.pos, 100f)) continue;

            if (PlayerCWT.TryGetData(player, out var data)) data.darknessImmunity = 120;
        }

        if (uad.darknessProgress > 0.8f && LanternCWT.TryGetData(self, out var lanternData))
        {
            bool inLight = false;
            foreach (LightSource light in self.room.lightSources)
            {
                if (light?.pos != null && !light.noGameplayImpact && light.tiedToObject != self && Custom.DistLess(self.firstChunk.pos, light.pos, 100f))
                {
                    inLight = true;
                }
            }

            if (!inLight)
            {
                lanternData.health--;

                if (UnityEngine.Random.value < 0.05f)
                {
                    int sparkCount = (240 - lanternData.health) / 40;
                    for (int i = 0; i < sparkCount; i++)
                    {
                        Vector2 dir = Custom.RNV();
                        self.room.AddObject(new Spark(self.firstChunk.pos + dir * (UnityEngine.Random.value * 20f), dir * Mathf.Lerp(4f, 10f, UnityEngine.Random.value), new Color(1f, 0.2f, 0f), null, 4, 18));
                    }
                }

                if (lanternData.health <= 0)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 dir = Custom.RNV();
                        self.room.AddObject(new Spark(self.firstChunk.pos + dir * (UnityEngine.Random.value * 25f), dir * Mathf.Lerp(8f, 16f, UnityEngine.Random.value), new Color(1f, 0.2f, 0f), null, 8, 23));
                    }
                    self.Destroy();
                }
            }
            else
            {
                lanternData.health = 200;
            }
        }
    }
    public static void LanternStick_Update(On.LanternStick.orig_Update orig, LanternStick self, bool eu)
    {
        orig(self, eu);

        if (self?.room == null || !HasEffect(self.room)) return;
        if (self.lantern == null) return;

        foreach (Player player in self.room.PlayersInRoom)
        {
            if (player?.abstractCreature?.realizedCreature == null) continue;
            if (!Custom.DistLess(self.lantern.firstChunk.pos, player.firstChunk.pos, 100f)) continue;

            if (PlayerCWT.TryGetData(player, out var data)) data.darknessImmunity = 120;
        }
    }
}