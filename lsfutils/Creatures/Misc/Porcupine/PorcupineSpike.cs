using RWCustom;
using UnityEngine;
using lsfUtils.Items.Darts.Dart;

namespace lsfUtils.Creatures.Misc.Porcupine;

public static class SpikeShape
{
    public const int segmentCount = 6;
    public const float baseWidth = 3f;
    public const float referenceLength = 30f;
    public const float tipFogBlend = 0.6f;

    public static TriangleMesh MakeMesh()
    {
        TriangleMesh.Triangle[] triangles = new TriangleMesh.Triangle[(segmentCount - 1) * 2];
        for (int i = 0; i < segmentCount - 1; i++)
        {
            triangles[i * 2] = new TriangleMesh.Triangle(i * 2, i * 2 + 1, i * 2 + 2);
            triangles[i * 2 + 1] = new TriangleMesh.Triangle(i * 2 + 1, i * 2 + 2, i * 2 + 3);
        }
        return new TriangleMesh("Futile_White", triangles, true, false);
    }

    public static void ApplyPalette(TriangleMesh mesh, RoomPalette palette)
    {
        Color root = palette.blackColor;
        Color tip = Color.Lerp(palette.blackColor, palette.fogColor, tipFogBlend);

        for (int i = 0; i < segmentCount; i++)
        {
            Color color = Color.Lerp(root, tip, i / (float)(segmentCount - 1));
            mesh.verticeColors[i * 2] = color;
            mesh.verticeColors[i * 2 + 1] = color;
        }
    }

    public static void Draw(TriangleMesh mesh, Vector2 rootPosition, Vector2 direction, Vector2 curlDirection, float length, float curve, Vector2 cameraPosition)
    {
        direction.Normalize();

        for (int i = 0; i < segmentCount; i++)
        {
            float along = i / (float)(segmentCount - 1);

            Vector2 center = rootPosition + direction * length * along + curlDirection * curve * length * along * along;
            Vector2 tangent = (direction + curlDirection * 2f * curve * along).normalized;
            Vector2 sideways = Custom.PerpendicularVector(tangent);
            float halfWidth = baseWidth * (length / referenceLength) * (1f - along) * 0.5f;

            mesh.MoveVertice(i * 2, center - sideways * halfWidth - cameraPosition);
            mesh.MoveVertice(i * 2 + 1, center + sideways * halfWidth - cameraPosition);
        }
    }
}

public class AbstractPorcupineSpike : AbstractDart
{
    public float length;
    public float curve;

    public AbstractPorcupineSpike(World world, WorldCoordinate pos, EntityID ID, float length, float curve)
        : base(world, null, pos, ID)
    {
        this.length = length;
        this.curve = curve;
    }

    public override void Realize()
    {
        if (realizedObject != null) return;
        realizedObject = new PorcupineSpike(this);
    }
}

public class PorcupineSpike : Dart
{
    public AbstractPorcupineSpike AbstractSpike => abstractPhysicalObject as AbstractPorcupineSpike;

    public PorcupineSpike(AbstractPorcupineSpike abstractSpike) : base(abstractSpike) { }

    public override void InitiateSprites(RoomCamera.SpriteLeaser spriteLeaser, RoomCamera roomCamera)
    {
        spriteLeaser.sprites = new FSprite[1];
        spriteLeaser.sprites[0] = SpikeShape.MakeMesh();
        AddToContainer(spriteLeaser, roomCamera, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser spriteLeaser, RoomCamera roomCamera, float timeStacker, Vector2 cameraPosition)
    {
        if (slatedForDeletetion || room != roomCamera.room)
        {
            spriteLeaser.CleanSpritesAndRemove();
            return;
        }

        Vector2 center = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Vector2 direction = Vector3.Slerp(lastRotation, rotation, timeStacker);
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.up;
        direction.Normalize();

        float length = AbstractSpike.length;
        Vector2 rootPosition = center - direction * length * 0.5f;
        Vector2 curlDirection = Custom.PerpendicularVector(direction) * Mathf.Sign(AbstractSpike.curve);

        SpikeShape.Draw((TriangleMesh)spriteLeaser.sprites[0], rootPosition, direction, curlDirection, length, Mathf.Abs(AbstractSpike.curve), cameraPosition);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser spriteLeaser, RoomCamera roomCamera, RoomPalette palette)
    {
        SpikeShape.ApplyPalette((TriangleMesh)spriteLeaser.sprites[0], palette);
    }
}