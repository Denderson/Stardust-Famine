using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.LocalGravity
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
}
