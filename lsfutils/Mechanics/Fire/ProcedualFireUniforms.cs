using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Mechanics.Fire;

public class ProceduralFireUniforms
{
    public static Color coreColor;
    public static Color edgeColor;
    public void SetColors(Color core, Color edge)
    {
        coreColor = core;
        edgeColor = edge;
    }

    public void PushGlobals()
    {
        Shader.SetGlobalColor("_FireCoreColor", coreColor);
        Shader.SetGlobalColor("_FireEdgeColor", edgeColor);
    }
}