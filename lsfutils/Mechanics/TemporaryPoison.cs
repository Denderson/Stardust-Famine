using lsfUtils.CWTs;
using UnityEngine;
using HUD;
using static lsfUtils.Plugin;

namespace lsfUtils.Mechanics;

public static class TemporaryPoison
{
    public static float recoverySpeed = 1f / 400f;
    public static void ApplyHooks()
    {
        On.Creature.Update += Creature_Update;
        On.Player.Update += Player_Update;
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (CreatureCWT.TryGetData(self, out var data) && data.cannotRecoverPoison > 0)
        {
            self.eyesClosedTime = Mathf.Max(20, self.eyesClosedTime);
        }
    }

    public static void Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        if (!CreatureCWT.TryGetData(self, out var data))
        {
            orig(self, eu);
            return;
            
        }
        float oldPoison = self.injectedPoison;
        self.injectedPoison = Mathf.Min(1f, self.injectedPoison + data.temporaryPoison);        
        orig(self, eu);               
        self.injectedPoison = oldPoison;
        if (data.cannotRecoverPoison > 0) data.cannotRecoverPoison--;
        else if (data.temporaryPoison > 0) data.temporaryPoison = Mathf.Max(data.temporaryPoison - recoverySpeed, 0);
    }
}

public class PoisonMeter : HudPart
{
    public Vector2 pos;
    public Vector2 lastPos;
    public int remainVisibleCounter;
    public float fade;
    public float lastFade;
    public HUDCircle[] circles;

    public PoisonMeter(HUD.HUD hud, FContainer fContainer) : base(hud)
    {
        this.pos = new Vector2(80f, 20f);
        this.lastPos = this.pos;
        this.circles = new HUDCircle[10];
        this.fade = 0f;
        this.lastFade = 0f;
        for (int i = 0; i < this.circles.Length; i++)
        {
            this.circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 2); //pure green as a placeholder while the custom color doesn't work
            this.circles[i].fade = 0f;
            this.circles[i].lastFade = 0f;
            //this.circles[i].forceColor = Enums.Colors.PoisonLizardColor;
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
        float poison = (hud.owner as Player).injectedPoison + playerData.temporaryPoison;
        if (this.hud.foodMeter != null)
        {
            this.pos.x = this.hud.foodMeter.pos.x;
            this.pos.y = this.hud.foodMeter.pos.y - 27f;
            if (poison <= 0.05f && playerData.temporaryPoison == 0f || playerData.fireAmount > 0f || playerData.fireBuildupAmount > poison || (hud.owner as Player).Hypothermia > poison)
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
            this.fade = 0f;
        }
        float num = 1f / (float)this.circles.Length;
        for (int i = 0; i < this.circles.Length; i++)
        {
            float value = poison - num * (float)i;
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
