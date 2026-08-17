using lsfUtils.CWTs;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.DevtoolsObjects.LocalGravity
{
    public static class LocalGravityHooks
    {
        public static void ApplyHooks()
        {
            On.PhysicalObject.Update += PhysicalObject_Update;
            On.Player.Update += Player_Update_CorrectGravityField;
            On.Player.UpdateBodyMode += Player_UpdateBodyMode;
            On.Player.UpdateAnimation += Player_UpdateAnimation;
            On.Player.Update += Player_Update;
            new Hook(typeof(PhysicalObject).GetProperty(nameof(PhysicalObject.EffectiveRoomGravity)).GetGetMethod(), typeof(LocalGravityHooks).GetMethod(nameof(EffectiveRoomGravity)));
            new Hook(typeof(Player).GetProperty(nameof(Player.EffectiveRoomGravity)).GetGetMethod(), typeof(LocalGravityHooks).GetMethod(nameof(EffectiveRoomGravityForPlayer)));
        }

        public static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
        {
            if (!PhysicalObjectCWT.TryGetData(self, out var physicalobjectdata))
            {
                Log.LogMessage("Couldnt grab PhysicalObjectCWT from physicalobject update!");
                orig(self, eu);
                return;
            }

            physicalobjectdata.shouldOverrideGravity = false;
            physicalobjectdata.overrideGravity = float.NegativeInfinity;

            if (self?.room != null && RoomCWT.TryGetData(self.room, out var roomdata) && roomdata?.localGravities != null && roomdata.localGravities.Count > 0)
            {

                foreach (LocalGravity localGravity in roomdata.localGravities)
                {
                    bool inRange = localGravity.InRange(self.firstChunk.pos);

                    if (inRange)
                    {
                        physicalobjectdata.shouldOverrideGravity = true;
                        physicalobjectdata.overrideGravity = Mathf.Max(physicalobjectdata.overrideGravity, localGravity.data.gravity);
                    }
                }
            }

            orig(self, eu);
        }

        public static float EffectiveRoomGravity(Func<PhysicalObject, float> orig, PhysicalObject self)
        {
            float origVal = orig(self);
            if (self != null && PhysicalObjectCWT.TryGetData(self, out var data) && data.shouldOverrideGravity)
            {
                return data.overrideGravity;
            }
            return origVal;
        }

        public static float EffectiveRoomGravityForPlayer(Func<Player, float> orig, Player self)
        {
            float origVal = orig(self);
            if (self != null && PhysicalObjectCWT.TryGetData(self, out var data) && data.shouldOverrideGravity)
            {
                return data.overrideGravity;
            }
            return origVal;
        }

        public static void Player_Update_CorrectGravityField(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (self.room == null) return;

            if (!PhysicalObjectCWT.TryGetData(self, out var data))
            {
                return;
            }

            if (!data.shouldOverrideGravity)
            {
                return;
            }

            self.gravity = data.overrideGravity;

            if (self.bodyMode == Player.BodyModeIndex.ZeroG && data.overrideGravity > 0.1f)
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

            if (self.room != null && PhysicalObjectCWT.TryGetData(self, out var data) && data.shouldOverrideGravity && data.overrideGravity > 0.1f)
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

            if (self.room != null && PhysicalObjectCWT.TryGetData(self, out var data) && data.shouldOverrideGravity && data.overrideGravity > 0.1f)
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

            if (!PhysicalObjectCWT.TryGetData(self, out var data))
            {
                orig(self, eu);
                return;
            }

            float origRoomGravity = -1f;
            bool patched = false;

            if (data.shouldOverrideGravity)
            {
                origRoomGravity = self.room.gravity;
                self.room.gravity = data.overrideGravity;
                patched = true;
            }

            orig(self, eu);

            if (patched)
            {
                self.room.gravity = origRoomGravity;
                self.gravity = data.overrideGravity;
            }
        }
    }
}
