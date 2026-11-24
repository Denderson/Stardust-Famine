using EffExt;
using lsfUtils.CWTs;
using RWCustom;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using UnityEngine;
using HUD;
using static lsfUtils.Plugin;
using IL;

namespace lsfUtils.DevtoolsEffects.Sunburn;

public class Sunburn
{
    public static void RegisterSunburn()
    {
        new EffectDefinitionBuilder("Sunburn")
            .SetCategory("lsfUtils")
            .Register();
    }

    public static bool HasEffect(Room room)
    {
        if (room?.roomSettings?.effects == null) return false;
        return room.roomSettings.GetEffect(Enums.EffectTypes.Sunburn) != null;
    }
    public static bool InSunlight(PhysicalObject obj)
    {
        //i have 0 clue of how to check for shade within the room so i just used the reverse of the room's light angle to check if it reaches the sky. not a good solution though, since rooms can have custom lightmaps
        if (obj.room == null || !HasEffect(obj.room) || obj.room.abstractRoom.skyExits < 1 || obj.abstractPhysicalObject.rippleLayer != 0 || obj is VoidSpawn) return false;

        Vector2 reverseLightAngle = new(obj.room.lightAngle.x * -1, obj.room.lightAngle.y);
        Vector2 corner = Custom.RectCollision(obj.firstChunk.pos, obj.firstChunk.pos + reverseLightAngle * 1000f, obj.room.RoomRect).GetCorner(FloatRect.CornerLabel.D);
        if (SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(obj.room, obj.firstChunk.pos, corner) != null)
        {
            //Log.LogMessage("collided with geometry");
            return false;
        }
        if (corner.y >= obj.room.PixelHeight - 5f || corner.x >= obj.room.PixelWidth - 5 || corner.x <= 5)
        {
            //Log.LogMessage("reached sky");
            return true;
        }
        return false;
    }

}
public class SunMeter : HudPart
{
    public Vector2 pos;
    public Vector2 lastPos;
    public int remainVisibleCounter;
    public float fade;
    public float lastFade;
    public HUDCircle[] circles;

    public SunMeter(HUD.HUD hud, FContainer fContainer) : base(hud)
    {
        this.pos = new Vector2(80f, 20f);
        this.lastPos = this.pos;
        this.circles = new HUDCircle[10];
        this.fade = 0f;
        this.lastFade = 0f;
        for (int i = 0; i < this.circles.Length; i++)
        {
            this.circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 6); //orange as a placeholder while the custom color doesn't work
            this.circles[i].fade = 0f;
            this.circles[i].lastFade = 0f;
            //this.circles[i].forceColor = new(1f, 0.56f, 0.39f);
        }
    }
    private bool Show
    {
        get
        {
            return this.hud.showKarmaFoodRain || this.hud.owner.RevealMap;
        }
    }
    public override void Update()
    {
        if (!CreatureCWT.TryGetData(this.hud.owner as Player, out var playerData)) return;
        float sunburn = playerData.fireBuildupAmount;
        if (this.hud.foodMeter != null)
        {
            this.pos.x = this.hud.foodMeter.pos.x;
            this.pos.y = this.hud.foodMeter.pos.y - 27f;
            if (sunburn <= 0.05f && this.hud.rainWorld.processManager.currentMainLoop is RainWorldGame && (this.hud.rainWorld.processManager.currentMainLoop as RainWorldGame).cameras[0].room != null && !Sunburn.InSunlight(this.hud.owner as Player) || playerData.onFire || (hud.owner as Player).injectedPoison > sunburn || (hud.owner as Player).Hypothermia > sunburn)
            {
                this.fade = Mathf.Lerp(this.fade, 0f, 0.1f);
            }
            else
            {
                this.fade = Mathf.Lerp(this.fade, this.Show ? this.hud.foodMeter.fade : 0f, ((this.Show && this.fade < this.hud.foodMeter.fade) || (!this.Show && this.fade > this.hud.foodMeter.fade)) ? 0.15f : 0.005f);
            }
        }

        this.lastPos = this.pos;
        this.lastFade = this.fade;
        if (this.hud.HideGeneralHud)
        {
        }
        float num = 1f / (float)this.circles.Length;
        for (int i = 0; i < this.circles.Length; i++)
        {
            float value = sunburn - num * (float)i;
            this.circles[i].Update();
            this.circles[i].thickness = Mathf.Lerp(6f, 1f, Mathf.InverseLerp(num, 0f, value));
            this.circles[i].fade = Mathf.Lerp(this.circles[i].fade, Mathf.InverseLerp(num * (float)i, num * ((float)i + 1f), this.fade), 0.1f);
            this.circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
            this.circles[i].snapRad = 0.45f;
            this.circles[i].snapThickness = 0.45f;
            this.circles[i].rad = Mathf.Lerp(0.1f, 5f, this.circles[i].fade);
        }
    }
    public Vector2 DrawPos(float timeStacker)
    {
        return Vector2.Lerp(this.lastPos, this.pos, timeStacker);
    }
    public override void Draw(float timeStacker)
    {
        for (int i = 0; i < this.circles.Length; i++)
        {
            this.circles[i].pos = this.pos + new Vector2((float)i * 21.6f, 0f);
            this.circles[i].Draw(timeStacker);
        }
    }
}
