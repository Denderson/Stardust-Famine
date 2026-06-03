using lsfUtils.Ripplespace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.DevtoolsObjects.RippleZone
{
    public class RippleZone : UpdatableAndDeletable
    {
        private RippleZoneData data;
        public bool activated;

        public RippleZone(PlacedObject placedObject, Room room)
        {
            RippleZoneData maybedata = placedObject.data as RippleZoneData;
            if (maybedata == null)
            {
                throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(RippleZoneData)} instance");
            }
            data = maybedata;
            this.room = room;
            activated = false;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (activated) return;
            if (room != null && Input.GetKey(KeyCode.W))
            {
                AbstractCreature abstractCreature = new(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Centipede), null, room.GetWorldCoordinate(data.owner.pos), room.world.game.GetNewID());
                if (CWTs.AbstractCreatureCWT.TryGetData(abstractCreature, out var abstractobjectdata))
                {
                    Log.LogMessage("Ripplifying!!!");
                    activated = true;
                    abstractCreature.rippleBothSides = data.overrideRippleBoth;
                    abstractCreature.rippleLayer = data.overrideRippleLayer;
                    abstractobjectdata.isRippleHybrid = true;
                }
                room.abstractRoom.AddEntity(abstractCreature);
                abstractCreature.RealizeInRoom();
                abstractCreature.realizedCreature.RipplifyRealisedObject();
            }
        }
    }
}
