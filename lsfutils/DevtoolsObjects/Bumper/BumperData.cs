using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.Bumper
{
    public class BumperData(PlacedObject po) : ManagedData(po, [])
    {
        [Vector2Field("Radius", defX: 40f, defY: 0f, Vector2Field.VectorReprType.circle)]
        public Vector2 radius;

        [FloatField("Force", 1f, 80f, 24f, 1f, ManagedFieldWithPanel.ControlType.slider, "Force: ")]
        public float force;

        [FloatField("Bounciness", 0f, 2f, 0.5f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "Bounciness: ")]
        public float bounciness;
    }
}
