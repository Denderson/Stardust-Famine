using System;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lsfUtils.ProcessingConditions;
using static Pom.Pom;
using DevInterface;
using static lsfUtils.Plugin;
using lsfUtils.Ripplespace;

namespace lsfUtils.ProcessingConditions
{
    public class RippleZoneData : ManagedData
    {
        public RippleZoneData(PlacedObject po) : base(po, new ManagedField[] { })
        {

        }
        [IntegerField("RippleLayer", -1, 9, 0, ManagedFieldWithPanel.ControlType.arrows, "RippleLayer: ")]
        public int overrideRippleLayer;


        [BooleanField("RippleBoth", false, ManagedFieldWithPanel.ControlType.button, "RippleBoth: ")]
        public bool overrideRippleBoth;


        [Vector2Field("Radius", defX: 80f, defY: 0f, Vector2Field.VectorReprType.circle)]
        public Vector2 radius;
    }

    internal class RippleZoneUAD : UpdatableAndDeletable
    {
        private RippleZoneData data;
        public bool activated;

        public RippleZoneUAD(PlacedObject placedObject, Room room)
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
                if (CWTs.AbstractPhysicalObjectCWT.TryGetData(abstractCreature, out var abstractobjectdata))
                {
                    Log.LogMessage("Ripplifying!!!");
                    activated = true;
                    abstractCreature.rippleBothSides = data.overrideRippleBoth;
                    abstractCreature.rippleLayer = data.overrideRippleLayer;
                    abstractobjectdata.isripplehybrid = true;
                }
                room.abstractRoom.AddEntity(abstractCreature);
                abstractCreature.RealizeInRoom();
                abstractCreature.realizedCreature.RipplifyRealisedObject();
            }
        }
    }
}