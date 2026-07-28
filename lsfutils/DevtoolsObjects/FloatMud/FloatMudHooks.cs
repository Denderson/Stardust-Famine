using System;
using lsfUtils.CWTs;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.FloatMud
{
    public static class FloatMudHooks
    {
        public const float floatMudGravityMultiplier = 0.30f;
        public const int maxFloatingMudTimer = 2400;

        private static float EffectiveGravityMultiplier(int timer)
        {
            float strength = Mathf.Clamp01(timer / (float)maxFloatingMudTimer);
            return Mathf.Lerp(1f, floatMudGravityMultiplier, strength);
        }

        public static void Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
        {
            orig(self, eu);
            if (orig == null) return;
            if (!CreatureCWT.TryGetData(self, out var data)) return;
            if (data.floatingMudTimer > 0) data.floatingMudTimer--;
        }

        public static float EffectiveRoomGravity(Func<PhysicalObject, float> orig, PhysicalObject self)
        {
            float origVal = orig(self);
            if (self != null && self is Creature creature && CreatureCWT.TryGetData(creature, out var data) && data.floatingMudTimer > 0)
            {
                return Mathf.Lerp(origVal, floatMudGravityMultiplier, Mathf.Clamp01(data.floatingMudTimer / (float)maxFloatingMudTimer));
            }
            return origVal;
        }

        public static float EffectiveRoomGravityForPlayer(Func<Player, float> orig, Player self)
        {
            float origVal = orig(self);
            if (self != null && CreatureCWT.TryGetData(self, out var data) && data.floatingMudTimer > 0)
            {
                return Mathf.Lerp(origVal, floatMudGravityMultiplier, Mathf.Clamp01(data.floatingMudTimer / (float)maxFloatingMudTimer));
            }
            return origVal;
        }

        public static void Player_Update_CorrectGravityField(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (self.room == null) return;
            if (!CreatureCWT.TryGetData(self, out var data)) return;
            if (data.floatingMudTimer <= 0) return;

            float effectiveGravity = EffectiveGravityMultiplier(data.floatingMudTimer);
            self.gravity = effectiveGravity;

            if (self.bodyMode == Player.BodyModeIndex.ZeroG && effectiveGravity > 0.1f)
            {
                self.bodyMode = Player.BodyModeIndex.Default;

                if (self.animation == Player.AnimationIndex.ZeroGSwim || self.animation == Player.AnimationIndex.ZeroGPoleGrab)
                {
                    self.animation = Player.AnimationIndex.None;
                }
            }
        }

        public static void Player_UpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self)
        {
            orig(self);

            if (self.room == null || !CreatureCWT.TryGetData(self, out var data) || data.floatingMudTimer <= 0) return;

            float effectiveGravity = EffectiveGravityMultiplier(data.floatingMudTimer);
            if (effectiveGravity <= 0.1f) return;

            if ((self.animation == Player.AnimationIndex.StandUp || self.animation == Player.AnimationIndex.DownOnFours) && self.bodyChunks[1].ContactPoint.y >= 0)
            {
                self.animation = Player.AnimationIndex.None;
                self.bodyMode = Player.BodyModeIndex.Default;
            }
        }

        public static void Player_UpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
        {
            orig(self);
            var afterMode = self.bodyMode;

            if (self.room == null || !CreatureCWT.TryGetData(self, out var data) || data.floatingMudTimer <= 0) return;

            float effectiveGravity = EffectiveGravityMultiplier(data.floatingMudTimer);
            if (effectiveGravity <= 0.1f) return;

            if (afterMode == Player.BodyModeIndex.ZeroG)
            {
                self.bodyMode = Player.BodyModeIndex.Default;
            }
        }

        public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            if (self?.room == null)
            {
                orig(self, eu);
                return;
            }

            if (!CreatureCWT.TryGetData(self, out var data))
            {
                orig(self, eu);
                return;
            }

            if (data.floatingMudTimer <= 0)
            {
                orig(self, eu);
                return;
            }

            Room targetRoom = self.room;
            float origRoomGravity = targetRoom.gravity;
            float effectiveGravity = EffectiveGravityMultiplier(data.floatingMudTimer);
            targetRoom.gravity = effectiveGravity;

            try
            {
                orig(self, eu);
            }
            finally
            {
                targetRoom.gravity = origRoomGravity;
                self.gravity = effectiveGravity;
            }
        }

        public static void MudPit_ApplyPalette(On.MudPit.orig_ApplyPalette orig, MudPit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self is FloatMud)
            {
                self.color = (self as FloatMud).GetMudColor();
                self.black = palette.blackColor;
                sLeaser.sprites[1].color = self.color;
                TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
                for (int i = 0; i < triangleMesh.verticeColors.Length; i++)
                {
                    triangleMesh.verticeColors[i] = self.color;
                }
            }
        }

        public static void MudPit_SpawnBubbles(On.MudPit.orig_SpawnBubbles orig, MudPit self)
        {
            if (self is not FloatMud) orig(self);
        }
    }
}