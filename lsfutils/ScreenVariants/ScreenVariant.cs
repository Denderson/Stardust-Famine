using UnityEngine;

public abstract class ScreenVariant
{
    public RoomCamera camera;
    public FSprite backgroundSprite;
    public FShader shader;
    public bool active;
    public Texture2D paletteTexture;
    public Texture2D gameplayPaletteTexture;
    public Texture2D fadeTexture;
    public Texture2D gameplayFadeTexture;

    public abstract string ShaderName { get; }

    public virtual float FadeInRate => 3f;
    public virtual float FadeOutRate => 5f;

    private float fadeTimer = 0f;

    public bool IsVisible => backgroundSprite != null && backgroundSprite.isVisible;

    private float Alpha
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
        fadeTimer = 0f;
        EnsureSprite();
        EnsureOverrideTexture();
        OnActivate();
    }

    public virtual void Deactivate()
    {
        if (!active) return;
        active = false;
        fadeTimer = 0f;
        OnDeactivate();
    }

    public virtual void DrawUpdate(float timeStacker)
    {
        if (backgroundSprite == null) return;

        fadeTimer += Time.deltaTime;

        if (active) Alpha = Mathf.Clamp01(fadeTimer / FadeInRate);
        else Alpha = Mathf.Clamp01(1f - (fadeTimer / FadeOutRate));

        backgroundSprite.isVisible = Alpha > 0.001f;

        if (!backgroundSprite.isVisible) return;

        Shader.SetGlobalVector("_screenSize", new Vector2(Futile.screen.pixelWidth, Futile.screen.pixelHeight));

        OnDrawUpdate(timeStacker);
    }

    public virtual void EnsureSprite()
    {
        if (backgroundSprite != null) return;

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

    public virtual void OnActivate() { }
    public virtual void OnDeactivate() { }
    public virtual void OnDrawUpdate(float timeStacker) { }
    public virtual void OnDispose() { }

    public virtual void EnsureOverrideTexture()
    {
        paletteTexture ??= new Texture2D(32, 8, TextureFormat.ARGB32, false);
    }

    public virtual void EnsureGameplayOverrideTexture()
    {
        gameplayPaletteTexture ??= new Texture2D(32, 8, TextureFormat.ARGB32, false);
    }
    public virtual void LoadFadeTex() { }
    public virtual void LoadGameplayFadeTex() { }

    public virtual void Dispose()
    {
        if (backgroundSprite != null)
        {
            backgroundSprite.RemoveFromContainer();
            backgroundSprite = null;
        }

        if (paletteTexture != null) { UnityEngine.Object.Destroy(paletteTexture); paletteTexture = null; }
        if (gameplayPaletteTexture != null) { UnityEngine.Object.Destroy(gameplayPaletteTexture); gameplayPaletteTexture = null; }
        if (fadeTexture != null) { UnityEngine.Object.Destroy(fadeTexture); fadeTexture = null; }
        if (gameplayFadeTexture != null) { UnityEngine.Object.Destroy(gameplayFadeTexture); gameplayFadeTexture = null; }

        active = false;
        OnDispose();
    }
}