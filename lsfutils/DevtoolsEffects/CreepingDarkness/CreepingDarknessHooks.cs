using lsfUtils.CWTs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static lsfUtils.Plugin;
using static lsfUtils.DevtoolsEffects.CreepingDarkness.CreepingDarkness;
using RWCustom;

namespace lsfUtils.DevtoolsEffects.CreepingDarkness
{
    public static class CreepingDarknessHooks
    {
        public static void ApplyHooks()
        {
            On.Room.Loaded += CreepingDarknessHooks.Room_Loaded;
            On.RoomCamera.Update += CreepingDarknessHooks.RoomCamera_Update;
            On.LightSource.Update += CreepingDarknessHooks.LightSource_Update;
            On.Lantern.Update += CreepingDarknessHooks.Lantern_Update;
            On.Player.Update += CreepingDarknessHooks.Player_Update;
            On.RainWorldGame.Update += CreepingDarknessHooks.RainWorldGame_Update;
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

                    if (Random.value < 0.05f)
                    {
                        int sparkCount = (240 - lanternData.health) / 40;
                        for (int i = 0; i < sparkCount; i++)
                        {
                            Vector2 dir = Custom.RNV();
                            self.room.AddObject(new Spark(self.firstChunk.pos + dir * (Random.value * 20f), dir * Mathf.Lerp(4f, 10f, Random.value), new Color(1f, 0.2f, 0f), null, 4, 18));
                        }
                    }

                    if (lanternData.health <= 0)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            Vector2 dir = Custom.RNV();
                            self.room.AddObject(new Spark(self.firstChunk.pos + dir * (Random.value * 25f), dir * Mathf.Lerp(8f, 16f, Random.value), new Color(1f, 0.2f, 0f), null, 8, 23));
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
}
