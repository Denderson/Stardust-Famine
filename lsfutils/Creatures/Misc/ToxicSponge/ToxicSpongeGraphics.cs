using RWCustom;
using System;
using UnityEngine;
using Watcher;

namespace lsfUtils.Creatures.Misc.ToxicSponge;

public class ToxicSpongeGraphics : BarnacleGraphics
{
    private const int shellLayers = 8;
    private const float shellRadiusFactor = 1.5f;
    private const float domeHeight = 1f;
    private const float domeSharpness = 2.2f;
    private const float circleSpriteRadius = 8f;

    private static readonly Color shellBottomColor = new(0.30f, 0.38f, 0.18f);
    private static readonly Color shellTopColor = new(0.72f, 0.82f, 0.45f);

    public int shellIndex;

    private ToxicSponge Sponge => owner as ToxicSponge;

    public ToxicSpongeGraphics(ToxicSponge sponge) : base(sponge)
    {
        cones = [];
        totalSprites = firstConeSprite;
        shellIndex = -1;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);

        shellIndex = sLeaser.sprites.Length;
        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + shellLayers);

        for (int i = 0; i < shellLayers; i++)
        {
            sLeaser.sprites[shellIndex + i] = new FSprite("Futile_White")
            {
                shader = rCam.game.rainWorld.Shaders["JaggedCircle"]
            };
        }

        AddToContainer(sLeaser, rCam, null);
    }

    private Color LayerColor(int i)
    {
        Color color = Color.Lerp(shellBottomColor, shellTopColor, (i / (float)shellLayers) / (1f - 1f / shellLayers));
        return Color.Lerp(color, blackColor, darkness);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        if (shellIndex <= 0 || sLeaser.sprites.Length < shellIndex + shellLayers) return;

        bool visible = !(culled || sLeaser.deleteMeNextFrame) && Barnacle.hasShell;
        for (int i = 0; i < shellLayers; i++)
            sLeaser.sprites[shellIndex + i].isVisible = visible;
        if (!visible) return;

        float rollNow = Mathf.Lerp(lastRoll, roll, timeStacker);
        Vector2 headPos = Vector2.Lerp(Barnacle.bodyChunks[0].lastPos, Barnacle.bodyChunks[0].pos, timeStacker);
        Vector2 tailPos = Vector2.Lerp(Barnacle.bodyChunks[1].lastPos, Barnacle.bodyChunks[1].pos, timeStacker);
        Vector2 bodyDir = (headPos - tailPos).normalized;
        Quaternion rot = Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.right, bodyDir), Vector3.forward) * Quaternion.AngleAxis(rollNow * 90f + 90f, Vector3.right);

        Vector2 shellBottom = sLeaser.sprites[firstBodySprite].GetPosition() + camPos - bodyDir * 4f;

        
        float radius = innerRad * shellRadiusFactor;
        if (Sponge != null)
        {
            float chargeProgress = Mathf.Clamp01(Sponge.poisonBuildup / (float)ToxicSponge.poisonMaxTimer);
            float pulse = 1f + chargeProgress * 0.04f * Mathf.Sin(Sponge.poisonBuildup * 0.5f);
            radius *= pulse;
        }

        Vector3 axis = rot * new Vector3(0f, radius * domeHeight, 0f);
        Vector2 axis2D = new(axis.x, axis.y);
        float facing = Mathf.Clamp01(axis.z / (radius * domeHeight));
        Vector2 squashDir = axis2D.sqrMagnitude > 0.0001f ? axis2D.normalized : bodyDir;
        float rotation = Custom.VecToDeg(squashDir);
        float sideThickness = axis2D.magnitude / shellLayers * 1.6f;

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
            layer.color = LayerColor(i);
        }
    }
}