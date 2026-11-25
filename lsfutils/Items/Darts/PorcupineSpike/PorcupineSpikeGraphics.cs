using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Items.Darts.PorcupineSpike;

public static class PorcupineSpikeGraphics
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

    public static void Draw(TriangleMesh mesh, Vector2 rootPos, Vector2 dir, Vector2 curveDir, float length, float curve, Vector2 cameraPosition)
    {
        dir.Normalize();

        for (int i = 0; i < segmentCount; i++)
        {
            float along = i / (float)(segmentCount - 1);

            Vector2 center = rootPos + dir * length * along + curveDir * curve * length * along * along;
            Vector2 tangent = (dir + curveDir * 2f * curve * along).normalized;
            Vector2 sideways = Custom.PerpendicularVector(tangent);
            float halfWidth = baseWidth * (length / referenceLength) * (1f - along) * 0.5f;

            mesh.MoveVertice(i * 2, center - sideways * halfWidth - cameraPosition);
            mesh.MoveVertice(i * 2 + 1, center + sideways * halfWidth - cameraPosition);
        }
    }
}
