using UnityEngine;

namespace lsfUtils.DevtoolsObjects.WaveLight
{
    public class WaveLight : UpdatableAndDeletable
    {
        public readonly PlacedObject placedObject;
        public WaveLightData Data => placedObject.data as WaveLightData;
        public float elapsedTime;

        public const float fadeRadius = 400f;
        public const float waveSpeed = 40f;
        public const float waveFrequency = 0.005f;
        public const float waveSharpness = 2f;
        public const float baseIntensity = 1.5f;

        public WaveLight(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
            this.room = room;
        }

        public Vector2 Pos => placedObject.pos;

        public Vector2 SourcePos => Pos + new Vector2(0f, Data.height * 0.5f);

        public override void Update(bool eu)
        {
            base.Update(eu);
            elapsedTime += 1f / 40f;
        }
    }
}