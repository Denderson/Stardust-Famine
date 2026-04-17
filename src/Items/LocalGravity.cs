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

    internal class LocalGravityUAD : UpdatableAndDeletable
    {
        private LocalGravityData data;

        public LocalGravityUAD(PlacedObject placedObject, Room room)
        {
            LocalGravityData maybedata = placedObject.data as LocalGravityData;
            if (maybedata == null)
            {
                throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(LocalGravityData)} instance");
            }
            data = maybedata;
            this.room = room;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (data != null && room != null && room.physicalObjects != null && room.physicalObjects.Length > 0) 
            {
                for (int i = 0; i < room.physicalObjects.Length; i++)
                {
                    for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    {
                        if (PhysicalObjectCWT.TryGetData(room.physicalObjects[i][j], out var cwtdata))
                        {
                            float dist = Custom.Dist(room.physicalObjects[i][j].firstChunk.pos, data.owner.pos);
                            if (dist < data.radius.magnitude)
                            {
                                cwtdata.shouldOverrideGravity = true;
                                cwtdata.overrideGravity = data.gravity;
                                room.physicalObjects[i][j].SetLocalGravity(data.gravity);
                                if (room.physicalObjects[i][j] is Player)
                                {
                                    (room.physicalObjects[i][j] as Player).customPlayerGravity = data.gravity;
                                    (room.physicalObjects[i][j] as Player).gravity = data.gravity;
                                }
                            }
                            else if (dist < (data.radius.magnitude + 20))
                            {
                                cwtdata.shouldOverrideGravity = false;
                                room.physicalObjects[i][j].SetLocalGravity(room.gravity);
                                if (room.physicalObjects[i][j] is Player)
                                {
                                    (room.physicalObjects[i][j] as Player).customPlayerGravity = room.gravity;
                                    (room.physicalObjects[i][j] as Player).gravity = room.gravity;
                                }
                            }
                        }
                    }
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