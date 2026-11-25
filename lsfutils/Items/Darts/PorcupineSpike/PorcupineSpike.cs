using UnityEngine;
using lsfUtils.Items.Darts.Dart;
using RWCustom;

namespace lsfUtils.Items.Darts.PorcupineSpike;

public class PorcupineSpike : Dart.Dart
{
    public AbstractPorcupineSpike AbstractSpike => abstractPhysicalObject as AbstractPorcupineSpike;

    public PorcupineSpike(AbstractPorcupineSpike abstractSpike) : base(abstractSpike) 
    {

    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser spriteLeaser, RoomCamera roomCamera)
    {
        spriteLeaser.sprites = new FSprite[1];
        spriteLeaser.sprites[0] = PorcupineSpikeGraphics.MakeMesh();
        AddToContainer(spriteLeaser, roomCamera, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser spriteLeaser, RoomCamera roomCamera, float timeStacker, Vector2 cameraPosition)
    {
        if (slatedForDeletetion || room != roomCamera.room)
        {
            spriteLeaser.CleanSpritesAndRemove();
            return;
        }

        Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Vector2 dir = Vector3.Slerp(lastRotation, rotation, timeStacker);
        if (dir.sqrMagnitude < 0.001f) dir = Vector2.up;
        dir.Normalize();

        float length = AbstractSpike.length;
        Vector2 rootPos = pos - dir * length * 0.5f;
        Vector2 curveDir = Custom.PerpendicularVector(dir) * Mathf.Sign(AbstractSpike.curve);

        PorcupineSpikeGraphics.Draw((TriangleMesh)spriteLeaser.sprites[0], rootPos, dir, curveDir, length, Mathf.Abs(AbstractSpike.curve), cameraPosition);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser spriteLeaser, RoomCamera roomCamera, RoomPalette palette)
    {
        PorcupineSpikeGraphics.ApplyPalette((TriangleMesh)spriteLeaser.sprites[0], palette);
    }
}
