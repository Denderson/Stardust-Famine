using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.ShaderUniforms;

public class AltBoxWormBodyUniforms
{
    public static Color altBoxWormColor = new(0.5f, 0.5f, 0.5f);
    public void SetColors(Color color)
    {
        altBoxWormColor = color;
    }

    public void PushGlobals()
    {
        Shader.SetGlobalColor("_fireBoxWormBody", altBoxWormColor);
    }
}