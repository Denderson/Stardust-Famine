using DevInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.DevtoolsEffects.StardustRain
{
    public static class StardustRainHooks // hooks wise everything okay
    {
        public static void Apply()
        {
            On.RoomCamera.Update += RoomCamera_Update;
            On.RoomSettings.RoomEffect.GetSliderCount += RoomEffect_GetSliderCount;
            On.RoomSettings.RoomEffect.GetSliderName += RoomEffect_GetSliderName;
            On.RoomSettings.RoomEffect.GetSliderDefault += RoomEffect_GetSliderDefault;
            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
        }

        private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject obj)
        {
            orig(obj);
            if (obj.type == StardustEnums.StardustController)
            {
                obj.data = new StardustControllerData(obj);
            }
        }

        private static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type objType, PlacedObject placedThing)
        {
            orig(self, objType, placedThing);
            if (objType == StardustEnums.StardustController)
            {
                if (placedThing == null) placedThing = self.RoomSettings.placedObjects[self.RoomSettings.placedObjects.Count - 1];

                PlacedObjectRepresentation prevRep = (PlacedObjectRepresentation)self.tempNodes.Pop();
                self.subNodes.Pop();
                prevRep.ClearSprites();

                var freshRep = new StardustControllerRepresentation(self.owner, objType.ToString() + "_Rep", self, placedThing, objType.ToString());
                self.tempNodes.Add(freshRep);
                self.subNodes.Add(freshRep);
            }
        }

        private static int RoomEffect_GetSliderCount(On.RoomSettings.RoomEffect.orig_GetSliderCount orig, RoomSettings.RoomEffect.Type fxType)
        {
            if (fxType == StardustEnums.StardustFall) return 1;
            return orig(fxType);
        }

        private static string RoomEffect_GetSliderName(On.RoomSettings.RoomEffect.orig_GetSliderName orig, RoomSettings.RoomEffect.Type fxType, int idx)
        {
            if (fxType == StardustEnums.StardustFall && idx == 0) return "Stardust Intensity";
            return orig(fxType, idx);
        }

        private static float RoomEffect_GetSliderDefault(On.RoomSettings.RoomEffect.orig_GetSliderDefault orig, RoomSettings.RoomEffect.Type fxType, int idx)
        {
            if (fxType == StardustEnums.StardustFall && idx == 0) return 0.5f;
            return orig(fxType, idx);
        }

        private static void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera cam)
        {
            orig(cam);

            if (cam.room == null) return;

            float dustAmt = cam.room.roomSettings.GetEffectAmount(StardustEnums.StardustFall);

            if (dustAmt > 0f)
            {
                bool gotDust = false;
                foreach (var thing in cam.room.updateList)
                {
                    if (thing is StardustFallGraphics) gotDust = true;
                }

                if (!gotDust)
                {
                    cam.room.AddObject(new StardustFallGraphics(cam, dustAmt));
                }
            }
        }
    }
}
