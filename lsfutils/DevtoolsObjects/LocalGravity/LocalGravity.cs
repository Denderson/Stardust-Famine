using lsfUtils.CWTs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.LocalGravity
{

    public class LocalGravity : UpdatableAndDeletable
    {
        public LocalGravityData data;
        Vector2 pos;

        public LocalGravity(PlacedObject placedObject, Room room)
        {
            LocalGravityData maybedata = placedObject.data as LocalGravityData;
            if (maybedata == null)
            {
                throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(LocalGravityData)} instance");
            }
            data = maybedata;
            pos = placedObject.pos;
            this.room = room;
            if (RoomCWT.TryGetData(room, out var roomdata))
            {
                roomdata.localGravities.Add(this);
            }
            else
            {
                Log.LogMessage("Couldnt grab RoomCWT from orig!");
            }
        }

        public bool InRange(Vector2 pos)
        {
            return Custom.DistLess(pos, this.pos, data.radius.magnitude);
        }

        public static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
        {
            if (!PhysicalObjectCWT.TryGetData(self, out var physicalobjectdata))
            {
                Log.LogMessage("Couldnt grab PhysicalObjectCWT from physicalobject update!");
                orig(self, eu);
                return;
            }

            // Reset from last frame
            physicalobjectdata.shouldOverrideGravity = false;
            physicalobjectdata.overrideGravity = float.NegativeInfinity;

            if (self?.room != null && RoomCWT.TryGetData(self.room, out var roomdata) && roomdata?.localGravities != null && roomdata.localGravities.Count > 0)
            {

                foreach (LocalGravity localGravity in roomdata.localGravities)
                {
                    float dist = Vector2.Distance(self.firstChunk.pos, localGravity.pos);
                    float radius = localGravity.data.radius.magnitude;
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

            // After animation update, check if in a normal gravity zone
            if (self.room != null && PhysicalObjectCWT.TryGetData(self, out var data) && data.shouldOverrideGravity && data.overrideGravity > 0.1f)
            {
                // If stuck in StandUp/DownOnFours with no floor contact, reset animation
                if ((self.animation == Player.AnimationIndex.StandUp || self.animation == Player.AnimationIndex.DownOnFours)
                    && self.bodyChunks[1].ContactPoint.y >= 0)  // not on floor
                {
                    self.animation = Player.AnimationIndex.None;
                    self.bodyMode = Player.BodyModeIndex.Default;
                }
            }
        }
        public static void Player_UpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
        {
            var beforeMode = self.bodyMode;
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