using lsfUtils.DevtoolsObjects.RippleTunnel;
using Pom;
using UnityEngine;
using static Pom.Pom;

namespace Stardust.PlacedObjects
{
    public class RippleTunnelData(PlacedObject po) : ManagedData(po, [])
    {
        public RippleTunnel obj;

        [IntegerField("LayerA", 0, 10, 0, ManagedFieldWithPanel.ControlType.arrows, "Layer A: ")]
        public int layerA;

        [IntegerField("LayerB", 0, 10, 1, ManagedFieldWithPanel.ControlType.arrows, "Layer B: ")]
        public int layerB;

        [Vector2Field("Radius", defX: 80f, defY: 0f, Vector2Field.VectorReprType.circle)]
        public Vector2 radius;

        [IntegerField("TransportFrames", 10, 300, 60, ManagedFieldWithPanel.ControlType.slider, "Hold frames: ")]
        public int transportFrames;
    }
}