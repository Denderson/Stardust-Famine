using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.CustomPushback
{
    public class CustomPushbackData(PlacedObject po) : ManagedData(po, [])
    {
        [FloatField("Strength", -100, 100, 10, 1f, ManagedFieldWithPanel.ControlType.slider, "Strength: ")]
        public float strength;

        [BooleanField("Directed", true, ManagedFieldWithPanel.ControlType.button, "Directed: ")]
        public bool directed;

        [BooleanField("Player-only", false, ManagedFieldWithPanel.ControlType.button, "Player-only: ")]
        public bool playerOnly;

        [Vector2Field("Radius", defX: 80f, defY: 0f, Vector2Field.VectorReprType.circle)]
        public Vector2 radius;
    }
}
