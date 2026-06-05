using MoreSlugcats;
using RWCustom;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.KarmaMask
{
    public class KarmaMask : VultureMask
    {
        LightSource lightSource;
        public KarmaMaskAbstract KarmaMaskAbstract => abstractPhysicalObject as KarmaMaskAbstract;

        public KarmaMask(KarmaMaskAbstract abstr, World world) : base(abstr, world)
        {
            Log.LogMessage("Spawning karma mask!");
            abstractPhysicalObject.rippleBothSides = true;
            lightSource = null;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (lightSource == null)
            {
                lightSource = new LightSource(firstChunk.pos, environmentalLight: false, RainWorld.GoldRGB, this)
                {
                    affectedByPaletteDarkness = 0.5f
                };
                room.AddObject(lightSource);
            }
            else
            {
                lightSource.setPos = firstChunk.pos;
                lightSource.setRad = 100f;
                lightSource.setAlpha = 1f;
                if (lightSource.slatedForDeletetion || lightSource.room != this.room)
                {
                    lightSource = null;
                }
            }
        }
    }
}