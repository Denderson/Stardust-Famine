using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static lsfUtils.Plugin;

namespace lsfUtils.Ripplespace
{
    public static class RippleHybrid
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
            if (abstractPhysicalObject is AbstractCreature absractCreature)
            {
                absractCreature.rippleLayer = 1;
                absractCreature.rippleBothSides = true;
                absractCreature.rippleCreature = true;
                if (CWTs.AbstractCreatureCWT.TryGetData(absractCreature, out var data))
                {
                    data.isRippleHybrid = true;
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
            if (self.abstractPhysicalObject is not AbstractCreature abstractCreature)
            {
                return;
            }
            if (!CWTs.AbstractCreatureCWT.TryGetData(abstractCreature, out var data))
            {
                Log.LogMessage("No CWT!");
                return;
            }
            if (data.isRippleHybrid)
            {
                self.RipplifyRealisedObject(self.abstractPhysicalObject.rippleLayer, self.abstractPhysicalObject.rippleBothSides);
                Log.LogMessage("Is rippleHybrid!");
                return;
            }
        }

        public static void SpriteLeaser_ctor(On.RoomCamera.SpriteLeaser.orig_ctor orig, RoomCamera.SpriteLeaser self, IDrawable obj, RoomCamera rCam)
        {
            orig(self, obj, rCam);
            if (obj is Creature creature && creature?.abstractCreature != null && CWTs.AbstractCreatureCWT.TryGetData(creature.abstractCreature, out var data) && data.isRippleHybrid)
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
                        int rippleLayer = (creature.abstractCreature.rippleBothSides) ? -1 : creature.abstractCreature.rippleLayer;
                        fSprite.shader = RainWorld.TryGetRippleMaskedShaderVariant(rippleLayer, fSprite.shader.name);
                    }
                }
            }
        }
    }
}
