using lsfUtils.CWTs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
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

        public override void Destroy()
        {
            base.Destroy();
            if (RoomCWT.TryGetData(room, out var roomdata) && roomdata.localGravities != null && roomdata.localGravities.Count > 0 && roomdata.localGravities.Contains(this))
            {
                roomdata.localGravities.Remove(this);
            }
        }

        public bool InRange(Vector2 pos)
        {
            return Custom.DistLess(pos, this.pos, data.radius.magnitude);
        }
    }
}