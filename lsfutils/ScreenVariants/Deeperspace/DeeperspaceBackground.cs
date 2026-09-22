using lsfUtils.ScreenVariants;
using UnityEngine;

namespace lsfUtils.ScreenVariants.Deeperspace;

public class DeeperspaceBackground : ScreenVariant
{
    public RenderTexture overrideTex;
    public override string ShaderName => "DeeperspaceBackground";

    public DeeperspaceBackground(RoomCamera camera) : base(camera) { }

    public override void OnDispose()
    {
        if (overrideTex != null)
        {
            UnityEngine.Object.Destroy(overrideTex);
            overrideTex = null;
        }
    }
}