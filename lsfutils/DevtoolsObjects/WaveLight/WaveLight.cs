using RWCustom;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.WaveLight
{
    public class WaveLight : UpdatableAndDeletable, IDrawable
    {
        public readonly PlacedObject placedObject;
        public WaveLightData Data => placedObject.data as WaveLightData;
        public float elapsedTime;

        public const int meshIndex = 0;
        public const int topLeftDotIndex = 1;
        public const int topRightDotIndex = 2;
        public const int bottomRightDotIndex = 3;
        public const int bottomLeftDotIndex = 4;
        public const int spritecount = 5;

        public const float fadeRadius = 400f;
        public const float waveSpeed = 40f;
        public const float waveFrequency = 0.005f;
        public const float baseIntensity = 1.5f;

        public WaveLight(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
            this.room = room;
        }

        public Vector2 Pos => placedObject.pos;

        public override void Update(bool eu)
        {
            base.Update(eu);
            elapsedTime += 1f / 40f;
        }

        public static float Smoothstep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[spritecount];

            TriangleMesh mesh = TriangleMesh.MakeGridMesh("Futile_White", 1);
            mesh.customColor = true;
            sLeaser.sprites[meshIndex] = mesh;

            for (int i = topLeftDotIndex; i <= bottomLeftDotIndex; i++) sLeaser.sprites[i] = new FSprite("Circle20") { scale = 0.15f, color = Color.white };

            AddToContainer(sLeaser, rCam, null);
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {

        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < sLeaser.sprites.Length; i++) sLeaser.sprites[i].isVisible = true;

            float halfMinWidth = Data.minWidth * 0.5f;
            float halfMaxWidth = Data.maxWidth * 0.5f;
            float halfHeight = Data.height * 0.5f;

            Vector2 screenPos = Pos - camPos;
            Vector2 topLeft = screenPos + new Vector2(-halfMinWidth, halfHeight);
            Vector2 topRight = screenPos + new Vector2(halfMinWidth, halfHeight);
            Vector2 bottomLeft = screenPos + new Vector2(-halfMaxWidth, -halfHeight);
            Vector2 bottomRight = screenPos + new Vector2(halfMaxWidth, -halfHeight);

            TriangleMesh mesh = (TriangleMesh)sLeaser.sprites[meshIndex];
            mesh.MoveVertice(0, bottomLeft);
            mesh.MoveVertice(1, bottomRight);
            mesh.MoveVertice(2, topLeft);
            mesh.MoveVertice(3, topRight);

            float intensity = SampleRippleIntensityAt(Pos);
            Color vertColor = new Color(1f, 1f, 1f, Mathf.Clamp01(intensity * 0.5f));
            for (int i = 0; i < mesh.verticeColors.Length; i++)
                mesh.verticeColors[i] = vertColor;
            mesh.Refresh();

            sLeaser.sprites[topLeftDotIndex].SetPosition(topLeft);
            sLeaser.sprites[topRightDotIndex].SetPosition(topRight);
            sLeaser.sprites[bottomRightDotIndex].SetPosition(bottomRight);
            sLeaser.sprites[bottomLeftDotIndex].SetPosition(bottomLeft);

            float dotScale = 0.15f * (1f + 0.2f * intensity);
            for (int i = topLeftDotIndex; i <= bottomLeftDotIndex; i++)
            {
                sLeaser.sprites[i].scale = dotScale;
                sLeaser.sprites[i].color = Color.red;
            }
        }

        public float SampleRippleIntensityAt(Vector2 worldPos)
        {
            float distanceFromLight = Vector2.Distance(worldPos, Pos);

            float envelope = 1f - Smoothstep(0f, fadeRadius, distanceFromLight);
            envelope *= envelope;

            float wave = Mathf.Sin(distanceFromLight * waveFrequency * 6.2831f - elapsedTime * (waveSpeed * waveFrequency * 6.2831f));
            wave = 0.5f + 0.5f * wave;
            wave = Mathf.Lerp(0.35f, 1f, wave);

            float core = 1f - Smoothstep(0f, fadeRadius * 0.25f, distanceFromLight);

            float intensity = envelope * wave * baseIntensity;
            return Mathf.Max(intensity, core * envelope * 0.9f);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Foreground");
            for (int i = 0; i < sLeaser.sprites.Length; i++) newContatiner.AddChild(sLeaser.sprites[i]);
        }
    }
}