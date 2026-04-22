using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static lsfUtils.Plugin;

namespace lsfUtils.Ripplespace
{
    public static class RippleHybridHooks
    {
        public static void RipplifyRealisedObject(this PhysicalObject physicalObject, int rippleLayer = 0, bool rippleBoth = false)
        {
            if (physicalObject?.graphicsModule == null)
            {
                Log.LogMessage("No graphics module to ripplify!!");
                return;
            }
            RipplifyAbstractObject(physicalObject.abstractPhysicalObject, rippleLayer, rippleBoth);

            Watcher.RippleHybridVFX.RippleSide rippleHybridVFX;
            if (rippleBoth)
            {
                rippleHybridVFX = Watcher.RippleHybridVFX.RippleSide.Both;
            }
            else if (rippleLayer != 0)
            {
                rippleHybridVFX = Watcher.RippleHybridVFX.RippleSide.Ripple;
            }
            else
            {
                rippleHybridVFX = Watcher.RippleHybridVFX.RippleSide.Normal;
            }
            physicalObject.graphicsModule.ActivateRippleHybrid(1, rippleHybridVFX);
            Log.LogMessage("Ripplified!!!");
        }

        public static void RipplifyAbstractObject(this AbstractPhysicalObject abstractPhysicalObject, int rippleLayer = 0, bool rippleBoth = false)
        {
            Log.LogMessage("Ripplifying abstract!");
            abstractPhysicalObject.rippleLayer = rippleLayer;
            abstractPhysicalObject.rippleBothSides = rippleBoth;
            if (abstractPhysicalObject is AbstractCreature)
            {
                (abstractPhysicalObject as AbstractCreature).rippleLayer = 1;
                (abstractPhysicalObject as AbstractCreature).rippleBothSides = true;
                (abstractPhysicalObject as AbstractCreature).rippleCreature = true;
            }
            if (CWTs.AbstractPhysicalObjectCWT.TryGetData(abstractPhysicalObject, out var data))
            {
                data.isripplehybrid = true;
            }
        }

        public static void AbstractCreature_setCustomFlags(On.AbstractCreature.orig_setCustomFlags orig, AbstractCreature self)
        {
            orig(self);
            foreach (string unrecognisedFlags in self.unrecognizedFlags)
            {
                Log.LogMessage("Reading unrecognised flags!");
                string value = unrecognisedFlags.ToLowerInvariant();
                if (value != null && value.Contains("ripplehybrid"))
                {
                    Log.LogMessage("Ripplehybrid check!");
                    int rippleLayer = 1;
                    bool rippleBoth = false;
                    if (value.Contains(':') && value.Split(':').Length > 1 && int.TryParse(value.Split(':')[1], out rippleLayer))
                    {
                        Log.LogMessage("Ripplehybrid layer override!");
                        if (rippleLayer == -1)
                        {
                            rippleBoth = true;
                            rippleLayer = 0;
                        }
                    }
                    RippleHybridHooks.RipplifyAbstractObject(self, rippleLayer, rippleBoth);
                }
            }
        }

        public static void PhysicalObject_InitiateGraphicsModule(On.PhysicalObject.orig_InitiateGraphicsModule orig, PhysicalObject self)
        {
            orig(self);
            if (self?.abstractPhysicalObject == null)
            {
                return;
            }
            if (!CWTs.AbstractPhysicalObjectCWT.TryGetData(self.abstractPhysicalObject, out var data))
            {
                Log.LogMessage("No CWT!");
                return;
            }
            if (data.isripplehybrid)
            {
                self.RipplifyRealisedObject(self.abstractPhysicalObject.rippleLayer, self.abstractPhysicalObject.rippleBothSides);
                Log.LogMessage("Is rippleHybrid!");
                return;
            }
        }

        public static void SpriteLeaser_ctor(On.RoomCamera.SpriteLeaser.orig_ctor orig, RoomCamera.SpriteLeaser self, IDrawable obj, RoomCamera rCam)
        {
            orig(self, obj, rCam);
            if (obj is PhysicalObject physicalObject && physicalObject?.abstractPhysicalObject != null && CWTs.AbstractPhysicalObjectCWT.TryGetData(physicalObject.abstractPhysicalObject, out var data) && data.isripplehybrid)
            {
                Log.LogMessage("Ripple shader starting!");
                if (self.sprites == null || self.sprites.Length == 0)
                {
                    return;
                }
                foreach (FSprite fSprite in self.sprites)
                {
                    if (fSprite != null)
                    {
                        int rippleLayer = (physicalObject.abstractPhysicalObject.rippleBothSides) ? -1 : physicalObject.abstractPhysicalObject.rippleLayer;
                        fSprite.shader = RainWorld.TryGetRippleMaskedShaderVariant(rippleLayer, fSprite.shader.name);
                    }
                }
            }
        }
    }
}
