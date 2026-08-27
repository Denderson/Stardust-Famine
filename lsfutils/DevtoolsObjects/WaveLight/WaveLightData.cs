using UnityEngine;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.WaveLight
{
    public class WaveLightData(PlacedObject po) : ManagedData(po, [])
    {
        [FloatField("MinWidth", 10f, 1000f, 100f, 1f, ManagedFieldWithPanel.ControlType.text, "Min width (top): ")]
        public float minWidth;

        [FloatField("MaxWidth", 10f, 1000f, 200f, 1f, ManagedFieldWithPanel.ControlType.text, "Max width (bottom): ")]
        public float maxWidth;

        [FloatField("Height", 10f, 1000f, 150f, 1f, ManagedFieldWithPanel.ControlType.text, "Height: ")]
        public float height;
    }
}