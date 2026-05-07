using System;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using static Pom.Pom;
using lsfUtils.CWTs;
using static lsfUtils.Plugin;
using SlugBase.Features;

namespace lsfUtils.Items
{
    public class LocalGravityData : ManagedData
    {
        public LocalGravityData(PlacedObject po) : base(po, new ManagedField[] { })
        {

        }
        [FloatField("Gravity%", 0, 1, 1, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Gravity%: ")]
        public float gravity;

        [Vector2Field("Radius", defX: 80f, defY: 0f, Vector2Field.VectorReprType.circle)]
        public Vector2 radius;
    }

    public class LocalGravity : UpdatableAndDeletable
    {
        public LocalGravityData data;
        Vector2 pos;
        float range;

        public LocalGravity(PlacedObject placedObject, Room room)
        {
            LocalGravityData maybedata = placedObject.data as LocalGravityData;
            if (maybedata == null)
            {
                throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(LocalGravityData)} instance");
            }
            this.data = maybedata;
            this.pos = placedObject.pos;
            this.range = maybedata.radius.magnitude;
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
            return Custom.DistLess(pos, this.pos, this.range);
        }

        public static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
        {
            orig(self, eu);
            if (!PhysicalObjectCWT.TryGetData(self, out var physicalobjectdata))
            {
                Log.LogMessage("Couldnt grab PhysicalObjectCWT from physicalobject update!");
                return;
            }
            physicalobjectdata.shouldOverrideGravity = false;
            physicalobjectdata.overrideGravity = -1f;
            if (self?.room != null)
            {
                if (!RoomCWT.TryGetData(self.room, out var roomdata))
                {
                    Log.LogMessage("Couldnt grab RoomCWT from physicalobject update!");
                    return;
                }
                if (roomdata == null || roomdata.localGravities == null || roomdata.localGravities.Count < 1)
                {
                    return;
                }
                foreach (LocalGravity localGravity in roomdata.localGravities)
                {
                    if (localGravity.InRange(self.firstChunk.pos))
                    {
                        physicalobjectdata.shouldOverrideGravity = true;
                        physicalobjectdata.overrideGravity = Mathf.Max(physicalobjectdata.overrideGravity, localGravity.data.gravity);
                    }
                }
            }
        }

        public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self?.room != null)
            {
                if (!PhysicalObjectCWT.TryGetData(self, out var data))
                {
                    Log.LogMessage("Couldnt get PlayerCWT from player update!");
                    return;
                }
                if (data.shouldOverrideGravity)
                {
                    self.customPlayerGravity = data.overrideGravity;
                    self.gravity = data.overrideGravity;
                }
            }
        }

        public static float EffectiveRoomGravity(Func<PhysicalObject, float> orig, PhysicalObject self)
        {
            if (self != null && PhysicalObjectCWT.TryGetData(self, out var data) && data.shouldOverrideGravity)
            {
                return data.overrideGravity;
            }
            return orig(self);
        }

        public static float EffectiveRoomGravityForPlayer(Func<Player, float> orig, Player self)
        {
            if (self != null && PhysicalObjectCWT.TryGetData(self, out var data) && data.shouldOverrideGravity)
            {
                return data.overrideGravity;
            }
            return orig(self);
        }
    }
}