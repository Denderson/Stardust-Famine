using UnityEngine;
using static Stardust.Plugin;

namespace Stardust.RippleLayers
{
    public class DeeperspaceData
    {
        private FSprite backgroundSprite;

        public bool IsActive { get; private set; }

        private float alpha
        {
            get => backgroundSprite?.alpha ?? 0f;
            set { if (backgroundSprite != null) backgroundSprite.alpha = value; }
        }

        public DeeperspaceData(RoomCamera camera)
        {
            RainWorld rw = camera.game.rainWorld;

            if (!rw.Shaders.ContainsKey("deeperspaceBackground"))
            {
                Log.LogMessage("deeperspaceBackground shader not found!");
                return;
            }

            backgroundSprite = new FSprite("pixel")
            {
                shader = rw.Shaders["deeperspaceBackground"],
                scaleX = Futile.screen.pixelWidth,
                scaleY = Futile.screen.pixelHeight,
                anchorX = 0f,
                anchorY = 0f,
                alpha = 0f,
                isVisible = false
            };

            // Bloom is above gameplay and below HUD, so using that
            camera.ReturnFContainer("Bloom").AddChild(backgroundSprite);
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void DrawUpdate(float timeStacker)
        {
            if (backgroundSprite == null) return;

            alpha = IsActive ? Mathf.MoveTowards(alpha, 1f, Time.deltaTime * 3f) : Mathf.MoveTowards(alpha, 0f, Time.deltaTime * 5f);

            backgroundSprite.isVisible = alpha > 0.001f;

            if (backgroundSprite.isVisible)
            {
                Shader.SetGlobalVector("_screenSize", new Vector2(Futile.screen.pixelWidth, Futile.screen.pixelHeight));
            }
        }

        public void Dispose()
        {
            backgroundSprite?.RemoveFromContainer();
            backgroundSprite = null;
        }
    }
}