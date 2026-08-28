using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.DevtoolsObjects.WaveLight
{
    public static class WaveLightHooks
    {
        private static Material overlayMaterial;
        private static RenderTexture baseLightmapSnapshot;
        private static RenderTexture trackedLightmapRef;

        public static void Apply()
        {
            On.RoomCamera.ApproximateLightmap += RoomCamera_ApproximateLightmap;
            On.RoomCamera.DrawUpdate += RoomCamera_DrawUpdate;
        }
        public static void Undo()
        {
            On.RoomCamera.ApproximateLightmap -= RoomCamera_ApproximateLightmap;
            On.RoomCamera.DrawUpdate -= RoomCamera_DrawUpdate;

            if (baseLightmapSnapshot != null)
            {
                baseLightmapSnapshot.Release();
                baseLightmapSnapshot = null;
            }
            trackedLightmapRef = null;
            overlayMaterial = null;
        }

        private static void RoomCamera_ApproximateLightmap(On.RoomCamera.orig_ApproximateLightmap orig, RoomCamera self)
        {
            Log.LogMessage("ApproximateLightmap hook!");
            orig(self);

            RenderTexture lightmapTexture = Shader.GetGlobalTexture("_Lightmap") as RenderTexture;
            if (lightmapTexture == null)
            {
                Log.LogMessage("_Lightmap is null!");
                return;
            }
            RefreshSnapshot(lightmapTexture);
        }

        private static void RoomCamera_DrawUpdate(On.RoomCamera.orig_DrawUpdate orig, RoomCamera self, float timeStacker, float timeSpeed)
        {
            orig(self, timeStacker, timeSpeed);

            if (self?.room?.updateList == null) return;

            bool roomHasWaveLight = false;
            foreach (UpdatableAndDeletable updatable in self.room.updateList)
            {
                if (updatable is WaveLight)
                {
                    roomHasWaveLight = true;
                    break;
                }
            }
            if (!roomHasWaveLight) return;

            RenderTexture lightmapTexture = Shader.GetGlobalTexture("_Lightmap") as RenderTexture;
            if (lightmapTexture == null)
            {
                Log.LogMessage("_Lightmap missing! room.generateLightmap = " + self.room.generateLightmap);
                return;
            }

            if (lightmapTexture != trackedLightmapRef)
            {
                Log.LogMessage("_Lightmap reference changed! Refreshing snapshot");
                RefreshSnapshot(lightmapTexture);
            }

            if (overlayMaterial == null)
            {
                if (self.game?.rainWorld?.Shaders != null && self.game.rainWorld.Shaders.TryGetValue("WaveLightOverlay", out var shaderAsset) && shaderAsset?.shader != null)
                {
                    overlayMaterial = new Material(shaderAsset.shader);
                    Log.LogMessage("overlayMaterial created");
                }
                else
                {
                    Log.LogMessage("WaveLightOverlay shader missing!");
                    return;
                }
            }

            Vector2 cameraOrigin = self.CamPos(self.currentCameraPosition);

            Graphics.CopyTexture(baseLightmapSnapshot, lightmapTexture);

            overlayMaterial.SetVector("_LightmapSize", new Vector2(lightmapTexture.width, lightmapTexture.height));
            overlayMaterial.SetFloat("_EdgeSoftness", 4f);

            foreach (UpdatableAndDeletable updatable in self.room.updateList)
            {
                if (updatable is not WaveLight waveLight || waveLight.Data == null) continue;

                Vector2 sourcePositionInPixels = waveLight.SourcePos - cameraOrigin;
                overlayMaterial.SetVector("_SourcePosPixels", sourcePositionInPixels);
                overlayMaterial.SetFloat("_MinWidth", waveLight.Data.minWidth);
                overlayMaterial.SetFloat("_MaxWidth", waveLight.Data.maxWidth);
                overlayMaterial.SetFloat("_Height", waveLight.Data.height);
                overlayMaterial.SetFloat("_FadeRadius", WaveLight.fadeRadius);
                overlayMaterial.SetFloat("_WaveSpeed", WaveLight.waveSpeed);
                overlayMaterial.SetFloat("_WaveFreq", WaveLight.waveFrequency);
                overlayMaterial.SetFloat("_WaveSharpness", WaveLight.waveSharpness);
                overlayMaterial.SetFloat("_BaseIntensity", WaveLight.baseIntensity);
                overlayMaterial.SetFloat("_TimeOffset", waveLight.elapsedTime);

                Graphics.Blit(null, lightmapTexture, overlayMaterial);
            }
        }

        private static void RefreshSnapshot(RenderTexture lightmapTexture)
        {
            Log.LogMessage("baseLightmapSnapshot remade from current _Lightmap");
            if (baseLightmapSnapshot == null)
            {
                baseLightmapSnapshot = new RenderTexture(lightmapTexture.width, lightmapTexture.height, 0, lightmapTexture.format)
                {
                    filterMode = FilterMode.Bilinear
                };
            }
            else if (baseLightmapSnapshot.width != lightmapTexture.width || baseLightmapSnapshot.height != lightmapTexture.height)
            {
                Log.LogMessage("Size mismatch, recreating snapshot texture");
                baseLightmapSnapshot.Release();
                baseLightmapSnapshot = new RenderTexture(lightmapTexture.width, lightmapTexture.height, 0, lightmapTexture.format)
                {
                    filterMode = FilterMode.Bilinear
                };
            }
            Graphics.CopyTexture(lightmapTexture, baseLightmapSnapshot);
            trackedLightmapRef = lightmapTexture;
        }
    }
}