using lsfUtils.DevtoolsObjects.WaveLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.WaveLight
{
    public static class WaveLightHooks
    {
        public static Material overlayMaterial;

        public static void Apply()
        {
            //On.RoomCamera.ApproximateLightmap += RoomCamera_ApproximateLightmap; // Edits the lightmap using the WaveLightOverlay shader
        }

        public static void RoomCamera_ApproximateLightmap(On.RoomCamera.orig_ApproximateLightmap orig, RoomCamera self)
        {
            orig(self);

            if (self.room == null) return;
            if (overlayMaterial == null) overlayMaterial = new Material(self.game.rainWorld.Shaders["WaveLightOverlay"].shader);

            RenderTexture lightmapTexture = Shader.GetGlobalTexture("_Lightmap") as RenderTexture;
            if (lightmapTexture == null) return;

            Vector2 cameraOrigin = self.CamPos(self.currentCameraPosition);

            foreach (UpdatableAndDeletable updatable in self.room.updateList)
            {
                if (updatable is not WaveLight waveLight) continue;

                Vector2 lightPositionInPixels = waveLight.Pos - cameraOrigin;

                overlayMaterial.SetVector("_LightPosPixels", lightPositionInPixels);
                overlayMaterial.SetFloat("_FadeRadius", WaveLight.fadeRadius);
                overlayMaterial.SetFloat("_WaveSpeed", WaveLight.waveSpeed);
                overlayMaterial.SetFloat("_WaveFreq", WaveLight.waveFrequency);
                overlayMaterial.SetFloat("_BaseIntensity", WaveLight.baseIntensity);
                overlayMaterial.SetFloat("_Time01", waveLight.elapsedTime);

                Graphics.Blit(null, lightmapTexture, overlayMaterial);
            }
        }
    }
}
