using System;
using lsfUtils.CWTs;
using MonoMod.RuntimeDetour;

namespace lsfUtils.DevtoolsObjects.FloatMud
{
    public static class FloatMudHooks
    {
        public const float floatMudGravityMultiplier = 0.35f;

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
                return floatMudGravityMultiplier;
            }
            return origVal;
        }

        public static float EffectiveRoomGravityForPlayer(Func<Player, float> orig, Player self)
        {
            float origVal = orig(self);
            if (self != null && CreatureCWT.TryGetData(self, out var data) && data.floatingMudTimer > 0)
            {
                return floatMudGravityMultiplier;
            }
            return origVal;
        }

        public static void Player_Update_CorrectGravityField(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (self.room == null) return;

            if (!CreatureCWT.TryGetData(self, out var data)) return;

            if (data.floatingMudTimer <= 0) return;

            self.gravity = floatMudGravityMultiplier;

            if (self.bodyMode == Player.BodyModeIndex.ZeroG && floatMudGravityMultiplier > 0.1f)
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

            if (self.room != null && CreatureCWT.TryGetData(self, out var data) && data.floatingMudTimer > 0 && floatMudGravityMultiplier > 0.1f)
            {
                if ((self.animation == Player.AnimationIndex.StandUp || self.animation == Player.AnimationIndex.DownOnFours) && self.bodyChunks[1].ContactPoint.y >= 0)
                {
                    self.animation = Player.AnimationIndex.None;
                    self.bodyMode = Player.BodyModeIndex.Default;
                }
            }
        }
        public static void Player_UpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
        {
            orig(self);
            var afterMode = self.bodyMode;

            if (self.room != null && CreatureCWT.TryGetData(self, out var data) && data.floatingMudTimer > 0 && floatMudGravityMultiplier > 0.1f)
            {
                if (afterMode == Player.BodyModeIndex.ZeroG)
                {
                    self.bodyMode = Player.BodyModeIndex.Default;
                }
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

            float origRoomGravity = -1f;
            bool patched = false;

            if (data.floatingMudTimer > 0)
            {
                origRoomGravity = self.room.gravity;
                self.room.gravity = floatMudGravityMultiplier;
                patched = true;
            }

            orig(self, eu);

            if (patched)
            {
                self.room.gravity = origRoomGravity;
                self.gravity = floatMudGravityMultiplier;
            }
        }
    }
}