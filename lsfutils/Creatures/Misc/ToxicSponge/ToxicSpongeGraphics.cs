using Newtonsoft.Json.Linq;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;
using Watcher;

namespace lsfUtils.Creatures.Misc.ToxicSponge;

public class ToxicSpongeGraphics : BarnacleGraphics
{
    public const int shellLayers = 8;
    public const float shellRadiusFactor = 1.5f;
    public const float domeSharpness = 1.5f;
    public const float circleSpriteRadius = 8f;
    public const float minShellSize = 0.9f;
    public const float maxShellSize = 1.10f;
    public const float shellLift = 0.2f;
    public const float axisScale = 0.9f / (1f - 1f / shellLayers);
    public const float topT = (shellLayers - 1f) / shellLayers;
    public const float shellBottomLight = 0.35f;
    public const float shellTopLight = 0.85f;
    public const float coralFog = 0.3f;
    public const int minClusters = 5;
    public const int maxClusters = 7;
    public const int minTubesPerCluster = 2;
    public const int maxTubesPerCluster = 4;
    public const float maxClusterDistance = 0.8f;
    public const float maxTubeDistance = 0.95f;
    public const float clusterRadius = 0.3f;
    public const float minClusterSpacing = 0.5f;
    public const float minTubeSpacing = 0.15f;
    public const int tubeRows = 8;
    public const float minTubeLength = 0.7f;
    public const float maxTubeLength = 1.3f;
    public const float minTubeSize = 0.9f;
    public const float maxTubeSize = 1.1f;
    public const float tubeWidthFactor = 0.3f;
    public const float lowerTubeWidthMultiplier = 1.35f;
    public const float lowerTubeLength = 0.25f;
    public const float upwardBlend = 0.35f;
    public const float holeWidth = 0.85f;
    public const float tubeDarkness = 0.5f;
    public const float minTwistAmount = 0.05f;
    public const float maxTwistAmount = 0.15f;
    public const float minTwistFrequency = 0.75f;
    public const float maxTwistFrequency = 1.75f;
    public const int maxDotsPerTube = 2;
    public const float dotSize = 0.3f;
    public const float dotSpread = 0.5f;
    public const int spritesPerTube = 2 + maxDotsPerTube;

    public Color shellBottomColor = new(0.28f, 0.28f, 0.28f);
    public Color shellTopColor = new(0.65f, 0.65f, 0.65f);
    public Color paletteFogColor = new(0.5f, 0.5f, 0.5f);

    public struct Tube
    {
        public Vector2 spot;
        public float length;
        public float hue;
        public float saturation;
        public float twistAmount;
        public float twistFrequency;
        public float twistPhase;
        public int dotCount;
        public float dotPhase;
    }

    public int shellIndex;
    public int tubeIndex;

    public float shellSize;
    public Tube[] tubes;
    public bool[] tubeBehind;

    public ToxicSponge Sponge => owner as ToxicSponge;
    public int TubeSpriteCount => tubes.Length * spritesPerTube;

    public ToxicSpongeGraphics(ToxicSponge sponge) : base(sponge)
    {
        cones = [];
        totalSprites = firstConeSprite;
        shellIndex = -1;
        tubeIndex = -1;

        UnityEngine.Random.State oldState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(sponge.abstractCreature.ID.RandomSeed);
        shellSize = UnityEngine.Random.Range(minShellSize, maxShellSize);
        tubes = CreateTubes();
        UnityEngine.Random.state = oldState;
    }

    public static Tube[] CreateTubes()
    {
        List<Tube> result = new();
        List<Vector2> clusterCenters = new();
        int clusterCount = UnityEngine.Random.Range(minClusters, maxClusters + 1);

        for (int c = 0; c < clusterCount; c++)
        {
            Vector2 clusterCenter;
            if (c == 0) clusterCenter = Vector2.zero;
            else clusterCenter = PickSpot(Vector2.zero, maxClusterDistance, maxClusterDistance, clusterCenters, minClusterSpacing);
            clusterCenters.Add(clusterCenter);

            float hue = UnityEngine.Random.value;
            float saturation = UnityEngine.Random.Range(0.65f, 0.9f);
            float clusterLength = UnityEngine.Random.Range(minTubeLength, maxTubeLength);

            List<Vector2> placed = new();
            int count = UnityEngine.Random.Range(minTubesPerCluster, maxTubesPerCluster + 1);

            for (int k = 0; k < count; k++)
            {
                Vector2 spot = PickSpot(clusterCenter, clusterRadius, maxTubeDistance, placed, minTubeSpacing);
                placed.Add(spot);

                Tube tube = new()
                {
                    spot = spot,
                    length = clusterLength * UnityEngine.Random.Range(minTubeSize, maxTubeSize),
                    hue = hue,
                    saturation = saturation,
                    twistAmount = UnityEngine.Random.Range(minTwistAmount, maxTwistAmount),
                    twistFrequency = UnityEngine.Random.Range(minTwistFrequency, maxTwistFrequency),
                    twistPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f),
                    dotCount = UnityEngine.Random.Range(1, maxDotsPerTube + 1),
                    dotPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f)
                };
                result.Add(tube);
            }
        }

        return result.ToArray();
    }

    public static Vector2 PickSpot(Vector2 center, float spread, float maxDistance, List<Vector2> placed, float minDistance)
    {
        Vector2 spot = center;
        for (int i = 0; i < 20; i++)
        {
            spot = center + UnityEngine.Random.insideUnitCircle * spread;
            if (spot.magnitude > maxDistance) spot = spot.normalized * maxDistance;

            bool farEnough = true;
            for (int j = 0; j < placed.Count; j++)
            {
                if (Vector2.Distance(placed[j], spot) < minDistance) farEnough = false;
            }
            if (farEnough) return spot;
        }
        return spot;
    }

    public void GetSurface(Vector2 spot, float radius, out Vector3 localPos, out Vector3 localNormal)
    {
        float d = Mathf.Clamp01(spot.magnitude);
        float t = Mathf.Pow(1f - Mathf.Pow(d, domeSharpness), 1f / domeSharpness);

        if (t >= topT)
        {
            localPos = new Vector3(spot.x * radius, topT * axisScale * radius, spot.y * radius);
            localNormal = Vector3.up;
            return;
        }

        Vector2 dir = spot.normalized;
        float normalUp = Mathf.Pow(t / d, domeSharpness - 1f);
        localPos = new Vector3(dir.x * d * radius, t * axisScale * radius, dir.y * d * radius);
        localNormal = new Vector3(dir.x * axisScale, normalUp, dir.y * axisScale).normalized;
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);

        paletteFogColor = palette.fogColor;
        shellBottomColor = Color.Lerp(palette.blackColor, palette.fogColor, shellBottomLight);
        shellTopColor = Color.Lerp(palette.blackColor, palette.fogColor, shellTopLight);
    }

    public Color TubeShade(Color color)
    {
        return Color.Lerp(color, blackColor, darkness * tubeDarkness);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);

        shellIndex = sLeaser.sprites.Length;
        tubeIndex = shellIndex + shellLayers;
        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + shellLayers + TubeSpriteCount);

        for (int i = 0; i < shellLayers; i++)
        {
            sLeaser.sprites[shellIndex + i] = CreateCircle(rCam);
        }

        TriangleMesh.Triangle[] triangles = new TriangleMesh.Triangle[(tubeRows - 1) * 2];
        for (int i = 0; i < tubeRows - 1; i++)
        {
            triangles[i * 2] = new TriangleMesh.Triangle(i * 2, i * 2 + 1, i * 2 + 2);
            triangles[i * 2 + 1] = new TriangleMesh.Triangle(i * 2 + 1, i * 2 + 2, i * 2 + 3);
        }

        for (int i = 0; i < tubes.Length; i++)
        {
            int index = tubeIndex + i * spritesPerTube;
            sLeaser.sprites[index] = new TriangleMesh("Futile_White", triangles, true);

            for (int k = 1; k < spritesPerTube; k++)
            {
                sLeaser.sprites[index + k] = CreateCircle(rCam);
            }
        }

        tubeBehind = new bool[tubes.Length];

        AddToContainer(sLeaser, rCam, null);
    }

    public static FSprite CreateCircle(RoomCamera rCam)
    {
        return new FSprite("Futile_White")
        {
            shader = rCam.game.rainWorld.Shaders["JaggedCircle"]
        };
    }

    public void SetTubeBehind(RoomCamera.SpriteLeaser sLeaser, int tubeNumber, bool behind)
    {
        if (tubeBehind[tubeNumber] == behind) return;

        int spriteIndex = tubeIndex + tubeNumber * spritesPerTube;
        FSprite body = sLeaser.sprites[spriteIndex];
        FSprite firstLayer = sLeaser.sprites[shellIndex];
        FSprite lastLayer = sLeaser.sprites[shellIndex + shellLayers - 1];

        if (body.container == null || body.container != firstLayer.container) return;

        tubeBehind[tubeNumber] = behind;
        if (behind) body.MoveBehindOtherNode(firstLayer);
        else body.MoveInFrontOfOtherNode(lastLayer);

        for (int k = 1; k < spritesPerTube; k++)
        {
            sLeaser.sprites[spriteIndex + k].MoveInFrontOfOtherNode(sLeaser.sprites[spriteIndex + k - 1]);
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        if (shellIndex <= 0 || tubeIndex <= 0 || sLeaser.sprites.Length < tubeIndex + TubeSpriteCount) return;

        bool visible = !(culled || sLeaser.deleteMeNextFrame) && Barnacle.hasShell;
        for (int i = 0; i < shellLayers; i++)
            sLeaser.sprites[shellIndex + i].isVisible = visible;
        for (int i = 0; i < TubeSpriteCount; i++)
            sLeaser.sprites[tubeIndex + i].isVisible = visible;
        if (!visible) return;

        float rollNow = Mathf.Lerp(lastRoll, roll, timeStacker);
        Vector2 headPos = Vector2.Lerp(Barnacle.bodyChunks[0].lastPos, Barnacle.bodyChunks[0].pos, timeStacker);
        Vector2 tailPos = Vector2.Lerp(Barnacle.bodyChunks[1].lastPos, Barnacle.bodyChunks[1].pos, timeStacker);
        Vector2 bodyDir = (headPos - tailPos).normalized;
        Quaternion rot = Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.right, bodyDir), Vector3.forward) * Quaternion.AngleAxis(rollNow * 90f + 90f, Vector3.right);

        Vector2 shellBottom = sLeaser.sprites[firstBodySprite].GetPosition() + camPos - bodyDir * 4f;

        float radius = innerRad * shellRadiusFactor * shellSize;
        if (Sponge != null)
        {
            float chargeProgress = Mathf.Clamp01(Sponge.poisonBuildup / (float)ToxicSponge.poisonMaxTimer);
            radius *= 1f + chargeProgress * 0.04f * Mathf.Sin(Sponge.poisonBuildup * 0.5f);
        }

        Vector3 axis = rot * new Vector3(0f, radius, 0f);
        Vector2 axis2D = new(axis.x, axis.y);
        float facing = Mathf.Clamp01(axis.z / radius);
        Vector2 squashDir = axis2D.sqrMagnitude > 0.0001f ? axis2D.normalized : bodyDir;
        float rotation = Custom.VecToDeg(squashDir);
        float sideThickness = axis2D.magnitude / shellLayers * 1.6f;

        shellBottom += axis2D * shellLift;

        for (int i = 0; i < shellLayers; i++)
        {
            float t = i / (float)shellLayers;
            float r = radius * Mathf.Pow(1f - Mathf.Pow(t, domeSharpness), 1f / domeSharpness);
            float minor = Mathf.Max(r * facing, sideThickness + r * 0.08f);

            FSprite layer = sLeaser.sprites[shellIndex + i];
            layer.SetPosition(shellBottom + axis2D * t / (1f - 1f / shellLayers) * 0.9f - camPos);
            layer.rotation = rotation;
            layer.scaleX = r / circleSpriteRadius;
            layer.scaleY = minor / circleSpriteRadius;
            layer.color = Color.Lerp(Color.Lerp(shellBottomColor, shellTopColor, (i / (float)shellLayers) / (1f - 1f / shellLayers)), blackColor, darkness);
        }

        for (int i = 0; i < tubes.Length; i++)
        {
            DrawTube(sLeaser, i, rot, shellBottom, camPos, squashDir, radius);
        }
    }

    public static Vector2 GetTubePoint(Tube tube, Vector2 basePos, Vector2 tipPos, Vector2 sidewaysDir, float length, float s)
    {
        float wave = Mathf.Sin(s * Mathf.PI * 2f * tube.twistFrequency + tube.twistPhase);
        return Vector2.Lerp(basePos, tipPos, s) + sidewaysDir * wave * tube.twistAmount * length * s;
    }

    public void DrawTube(RoomCamera.SpriteLeaser sLeaser, int tubeNumber, Quaternion rot, Vector2 shellBottom, Vector2 camPos, Vector2 fallbackDir, float radius)
    {
        Tube tube = tubes[tubeNumber];
        int spriteIndex = tubeIndex + tubeNumber * spritesPerTube;
        TriangleMesh body = sLeaser.sprites[spriteIndex] as TriangleMesh;
        FSprite mouth = sLeaser.sprites[spriteIndex + 1];
        if (body == null) return;

        GetSurface(tube.spot, radius, out Vector3 localPos, out Vector3 localNormal);
        Vector3 surfaceNormal = rot * localNormal;
        Vector3 surfacePos = rot * localPos;

        SetTubeBehind(sLeaser, tubeNumber, surfaceNormal.z < 0f);

        Vector3 tubeDir = Vector3.Lerp(surfacePos.normalized, rot * Vector3.up, upwardBlend).normalized;

        float length = radius * tube.length;
        float halfWidth = length * tubeWidthFactor * 0.5f;

        Vector2 basePos = shellBottom + new Vector2(surfacePos.x, surfacePos.y);
        Vector2 tipPos = basePos + new Vector2(tubeDir.x, tubeDir.y) * length;
        Vector2 toTip = tipPos - basePos;
        Vector2 dir = toTip.sqrMagnitude > 0.0001f ? toTip.normalized : fallbackDir;
        Vector2 perp = Custom.PerpendicularVector(dir);

        Vector2 tipPoint = basePos;
        for (int i = 0; i < tubeRows; i++)
        {
            float s = i / (float)(tubeRows - 1);
            Vector2 point = GetTubePoint(tube, basePos, tipPos, perp, length, s);
            float width = halfWidth * Mathf.Lerp(lowerTubeWidthMultiplier, 1f, Mathf.Clamp01(s / lowerTubeLength));

            body.MoveVertice(i * 2, point - perp * width - camPos);
            body.MoveVertice(i * 2 + 1, point + perp * width - camPos);

            Color rowColor = TubeShade(Color.Lerp(Color.HSVToRGB(tube.hue, tube.saturation, Mathf.Lerp(0.75f, 1f, s)), paletteFogColor, coralFog));
            body.verticeColors[i * 2] = rowColor;
            body.verticeColors[i * 2 + 1] = rowColor;

            tipPoint = point;
        }

        for (int d = 0; d < maxDotsPerTube; d++)
        {
            FSprite dot = sLeaser.sprites[spriteIndex + 2 + d];
            if (d >= tube.dotCount)
            {
                dot.isVisible = false;
                continue;
            }

            float s = (d + 1f) / (tube.dotCount + 1f);
            float side = Mathf.Sin(tube.dotPhase + d * 2.4f) * dotSpread * halfWidth;

            dot.SetPosition(GetTubePoint(tube, basePos, tipPos, perp, length, s) + perp * side - camPos);
            dot.scale = halfWidth * dotSize / circleSpriteRadius;
            dot.color = blackColor;
        }

        if (tubeDir.z <= 0f)
        {
            mouth.isVisible = false;
            return;
        }

        float mouthRadius = halfWidth * holeWidth;
        mouth.SetPosition(tipPoint - camPos);
        mouth.rotation = Custom.VecToDeg(dir);
        mouth.scaleX = mouthRadius / circleSpriteRadius;
        mouth.scaleY = Mathf.Max(mouthRadius * tubeDir.z, mouthRadius * 0.3f) / circleSpriteRadius;
        mouth.color = TubeShade(Color.HSVToRGB(tube.hue, 1f, 0.3f));
    }
}