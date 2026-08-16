using System;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pom.Pom;
using DevInterface;
using static lsfUtils.Plugin;

namespace lsfUtils.DevtoolsObjects.RippleZone
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
}