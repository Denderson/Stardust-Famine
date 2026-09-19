using lsfUtils.ScreenVariants;
using UnityEngine;

namespace lsfUtils.ScreenVariants.Deeperspace;

public class DeeperspaceBackground : ScreenVariant
{
    public RenderTexture overrideTex;
    public override string ShaderName => "DeeperspaceBackground";

    public DeeperspaceBackground(RoomCamera camera) : base(camera) { }

    public override Texture GetOverrideTexture()
    {
        overrideTex ??= new RenderTexture(Futile.screen.pixelWidth, Futile.screen.pixelHeight, 0);
        return overrideTex;
    }

    public override void OnDispose()
    {
        if (overrideTex != null)
        {
            UnityEngine.Object.Destroy(overrideTex);
            overrideTex = null;
        }
    }
}