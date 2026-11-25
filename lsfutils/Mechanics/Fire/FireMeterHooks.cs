using lsfUtils.CWTs;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lsfUtils.DevtoolsEffects.Sunburn;

namespace lsfUtils.Mechanics.Fire;

internal class FireMeterHooks
{
    public static void ApplyHooks()
    {
        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
        On.HUD.HUDCircle.Draw += HUDCircle_Draw;
        On.MoreSlugcats.HypothermiaMeter.Update += HypothermiaMeter_Update;
        On.MoreSlugcats.HypothermiaMeter.ctor += HypothermiaMeter_ctor;
    }

    

    private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        self.AddPart(new SunMeter(self, self.fContainers[1]));
        self.AddPart(new FireMeter(self, self.fContainers[1]));
        self.AddPart(new PoisonMeter(self, self.fContainers[1]));
        orig(self, cam);
    }
    private static void HUDCircle_Draw(On.HUD.HUDCircle.orig_Draw orig, HUD.HUDCircle self, float timeStacker)
    {
        //Plugin.Log.LogMessage(self.color);
        orig(self, timeStacker);
        //Plugin.Log.LogMessage(self.color + " " + self.sprite.color);
    }
    private static void HypothermiaMeter_Update(On.MoreSlugcats.HypothermiaMeter.orig_Update orig, MoreSlugcats.HypothermiaMeter self)
    {
        if (!CreatureCWT.TryGetData(self.hud.owner as Player, out var playerData))
        {
            orig(self);
            return; 
        }
        if (playerData.onFire || playerData.fireBuildupAmount > (self.hud.owner as Player).Hypothermia)
        {
            self.fade = Mathf.Lerp(self.fade, 0f, 0.1f);
            if (self.hud.HideGeneralHud) self.fade = 0f;
            float num = 1f / (float)self.circles.Length;
            for (int i = 0; i < self.circles.Length; i++)
            {
                self.circles[i].Update();
                self.circles[i].fade = Mathf.Lerp(self.circles[i].fade, Mathf.InverseLerp(num * (float)i, num * ((float)i + 1f), self.fade), 0.1f);
                self.circles[i].rad = Mathf.Lerp(0.1f, 5f, self.circles[i].fade);
            }
                return;
        }
        orig(self);
    }

    public static void UpdateHUDColors()
    {
        List<Color> fadableColors = [];
        Plugin.Log.LogMessage("Custom hud colors begin");
        for (int i = 0; i < RWCustom.Custom.FadableVectorCircleColors.Length; i++)
        {
            fadableColors.Add(RWCustom.Custom.FadableVectorCircleColors[i]);
            Plugin.Log.LogMessage(i + " " + RWCustom.Custom.FadableVectorCircleColors[i]);
        }
        if (!fadableColors.Contains(new Color(1f, 0.39f, 0.39f)))
        {
            fadableColors.Add(new(1f, 0.84f, 0.39f));           //Sunburn
            fadableColors.Add(new(1f, 0.39f, 0.39f));           //Fire
            fadableColors.Add(Enums.Colors.PoisonLizardColor);  //Poison
            fadableColors.Add(new(0.5f, 1f, 1f));               //Hypothermia
        }

        RWCustom.Custom.FadableVectorCircleColors = fadableColors.ToArray();

        for (int i = 0; i < RWCustom.Custom.FadableVectorCircleColors.Length; i++)
        {
            Plugin.Log.LogMessage(i + " " + RWCustom.Custom.FadableVectorCircleColors[i]);
        }
        Plugin.Log.LogMessage("Finished applying custom hud colors!");

    }
    private static void HypothermiaMeter_ctor(On.MoreSlugcats.HypothermiaMeter.orig_ctor orig, MoreSlugcats.HypothermiaMeter self, HUD.HUD hud, FContainer fContainer)
    {
        orig(self, hud, fContainer);
        /*for (int i = 0; i < self.circles.Length; i++)
        {
            self.circles[i] = new HUD.HUDCircle(hud, HUD.HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 3); //pure blue as a placeholder while the custom color doesn't work
        }*/
        //for when we add the custom colors to meters
    }
}

