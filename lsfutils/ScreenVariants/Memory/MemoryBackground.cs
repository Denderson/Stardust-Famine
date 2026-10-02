using UnityEngine;

namespace lsfUtils.ScreenVariants.Memory;

public class MemoryBackground : ScreenVariant
{
    public override string ShaderName => "Futile/MemoryBackground";

    public Vector2 pos;

    public MemoryBackground(RoomCamera camera) : base(camera) 
    {
        pos = Vector2.one;
    }

    public override void OnDrawUpdate(float timeStacker)
    {
        Shader.SetGlobalVector("POS", pos);
    }
}
