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
        if (self.stick != null) return;

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

    public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (self?.room == null) return;
        if (!PlayerCWT.TryGetData(self, out var data)) return;
        if (!TryGetUAD(self.room.world, out var uad)) return;

        if (data.darknessImmunity > 0)
        {
            data.darknessImmunity--;
            if (uad.simpleVersion)
            {
                uad.state = CreepingDarknessUAD.DarknessState.Retracting;
            }
        }

        if (uad.darknessProgress > 0.8)
        {
            self.eyesClosedTime = 10;
            self.slowMovementStun = 40;
            if (uad.darknessProgress >= 1 && data.darknessImmunity <= 0)
            {
                if (!self.dead)
                {
                    self.Die();
                }
            }
        }
    }

    public static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);
        if (self?.world != null && !self.GamePaused && self.processActive && WorldCWT.TryGetData(self.world, out var data) && data.creepingDarkness != null)
        {
            data.creepingDarkness.Tick();
        }
    }
}