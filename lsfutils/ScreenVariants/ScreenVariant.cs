using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.ScreenVariants;

public abstract class ScreenVariant
{
    public RoomCamera camera;
    public FSprite backgroundSprite;
    public FShader shader;
    public bool active;

    public abstract string ShaderName { get; }

    public virtual float FadeInRate => 3f;

    public virtual float FadeOutRate => 5f;

    public bool IsVisible => backgroundSprite != null && backgroundSprite.isVisible;

    public virtual string OverrideTexPropertyName => $"_{ShaderName}_OverrideTex";

    public virtual string BlendFactorPropertyName => $"_{ShaderName}_BlendFactor";

    public float Alpha
    {
        get => backgroundSprite?.alpha ?? 0f;
        set { if (backgroundSprite != null) backgroundSprite.alpha = value; }
    }

    public ScreenVariant(RoomCamera camera)
    {
        this.camera = camera;
    }

    public virtual void Activate()
    {
        if (active) return;
        active = true;

        EnsureSprite();
        OnActivate();
    }

    public virtual void Deactivate()
    {
        if (!active) return;
        active = false;
        OnDeactivate();
    }

    public virtual void DrawUpdate(float timeStacker)
    {
        if (backgroundSprite == null) return;

        Alpha = active ? Mathf.MoveTowards(Alpha, 1f, Time.deltaTime * FadeInRate) : Mathf.MoveTowards(Alpha, 0f, Time.deltaTime * FadeOutRate);
        backgroundSprite.isVisible = Alpha > 0.001f;

        if (!backgroundSprite.isVisible) return;

        Shader.SetGlobalVector("_screenSize", new Vector2(Futile.screen.pixelWidth, Futile.screen.pixelHeight));

        Texture overrideTex = GetOverrideTexture();
        if (overrideTex != null) Shader.SetGlobalTexture(OverrideTexPropertyName, overrideTex);

        float blendFactor = GetBlendFactor();
        Shader.SetGlobalFloat(BlendFactorPropertyName, blendFactor);

        OnDrawUpdate(timeStacker);
    }

    public virtual void Dispose()
    {
        if (backgroundSprite != null)
        {
            backgroundSprite.RemoveFromContainer();
            backgroundSprite = null;
        }
        OnDispose();
    }

    public virtual void EnsureSprite()
    {
        if (backgroundSprite != null) return;
        if (!RWCustom.Custom.rainWorld.Shaders.TryGetValue(ShaderName, out shader))
        {
            active = false;
            return;
        }
        shader ??= RWCustom.Custom.rainWorld.Shaders[ShaderName];

        backgroundSprite = new FSprite("Futile_White")
        {
            shader = shader,
            scaleX = Futile.screen.pixelWidth,
            scaleY = Futile.screen.pixelHeight,
            anchorX = 0f,
            anchorY = 0f,
            alpha = 0f,
            isVisible = false
        };
        camera.ReturnFContainer("Bloom").AddChild(backgroundSprite);
    }

    public abstract Texture GetOverrideTexture();

    public virtual float GetBlendFactor() => Alpha;

    public virtual void OnActivate() { }

    public virtual void OnDeactivate() { }

    public virtual void OnDrawUpdate(float timeStacker) { }

    public virtual void OnDispose() { }
}