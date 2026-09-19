/*using System;
using System.Collections.Generic;
using System.IO;
using HUD;
using Menu;
using RWCustom;
using UnityEngine;

public class ThreadIcon : CircularMenuObject, SelectableMenuObject, ButtonMenuObject
{
    public FSprite regionIcon;

    public string region;

    public float ringFade;

    public float fadeCounter;

    private float maxFade = 1f;

    private float fadeIn = -0.25f;

    private Vector3 _missingPos;

    public bool hasSignalled;

    public bool active;

    public ButtonBehavior buttonBehav;

    public MenuLabel menuLabel;

    public FSprite[] circleSprites;

    public float filled;

    public float lastFilled;

    public float fillTime;

    public int buttonReleasedCounter;

    private float pulse;

    private float lastPulse;

    public string signalText;

    public bool controlledFromOutside;

    public bool held;

    public bool lastHeld;

    public MenuMicrophone.MenuSoundLoop soundLoop;

    public FSprite circleSprite;

    public int CircleColor
    {
        get
        {
            if (Selected)
            {
                return 4;
            }
            return 0;
        }
    }

    public bool IsMouseOverMe => base.MouseOver;

    public bool CurrentlySelectableMouse
    {
        get
        {
            if (!active) return false;
            if (hasSignalled) return false;
            return true;
        }
    }

    public bool CurrentlySelectableNonMouse => true;

    public ButtonBehavior GetButtonBehavior => buttonBehav;

    public ThreadIcon(Menu.Menu menu, MenuObject owner, Vector2 pos, float rad, string region) : base(menu, owner, pos, rad)
    {
        this.region = region;
        AddGraphics();
    }

    public void AddGraphics()
    {
        ClearSprites();
        circleSprite = new FSprite();
        string text = "warp-" + region;
        if (!Futile.atlasManager.DoesContainElementWithName(text))
        {
            Texture2D val = new Texture2D(0, 0);
            string path = AssetManager.ResolveFilePath("illustrations/" + text + ".png");
            if (File.Exists(path))
            {
                ImageConversion.LoadImage(val, File.ReadAllBytes(path));
            }
            val.filterMode = 0;
            Futile.atlasManager.LoadAtlasFromTexture(text, (Texture)(object)val, textureFromAsset: false);
        }
        regionIcon = new FSprite(text);
        regionIcon.SetAnchor(0.5f, 0.5f);
        UpdateGraphics();
    }

    public override void Update()
    {
        base.Update();
        if (map.visible)
        {
            Vector3 pos = Pos;
            Vector2 val = RotateAroundCircle(new Vector2(pos.x, pos.z), (0f - map.panVel.x) / 60f);
            pos.x = val.x;
            pos.z = val.y;
            Vector2 val2 = RotateAroundCircle(new Vector2(pos.y, pos.z), (0f - map.panVel.y) / 60f);
            pos.y = val2.x;
            pos.z = val2.y;
            Pos = pos;
        }
    }

    public void Draw(float timeStacker)
    {
        base.Draw(timeStacker);
        circle.Draw(timeStacker);
        selectionCircle.Draw(timeStacker);
    }

    public void UpdateGraphics()
    {
        circle.Update();
        Vector3 val = Pos * map.warpMapDiameter / 2f;
        circle.pos = new Vector2(val.x, val.y) + map.hud.rainWorld.screenSize / 2f;
        circle.rad = 60f;
        circle.thickness = 4f;
        float num2 = 1f;
        maxFade = Mathf.Lerp(maxFade, active ? 1f : 0.25f, 0.08f);
        num2 *= maxFade;
        float num3 = map.fade * num2;
        circle.color = CircleColor;
        selectionCircle.Update();
        selectionCircle.pos = circle.pos;
        selectionCircle.rad = 80;
        selectionCircle.thickness = circle.thickness;
        selectionCircle.fade = (Selected ? num3 : 0f);
        selectionCircle.color = circle.color;
        if (regionIcon != null)
        {
            regionIcon.x = Mathf.Lerp(selectionCircle.pos.x, selectionCircle.lastPos.x, 0.5f);
            regionIcon.y = Mathf.Lerp(selectionCircle.pos.y, selectionCircle.lastPos.y, 0.5f);
            regionIcon.alpha = num3;
        }
    }

    public void RefreshGraphics()
    {
        circle.ClearSprite();
        menu.container.AddChild(circle.sprite);
        selectionCircle.ClearSprite();
        menu.container.AddChild(selectionCircle.sprite);
        if (regionIcon != null)
        {
            regionIcon.RemoveFromContainer();
            menu.container.AddChild(regionIcon);
        }
    }

    public void ClearSprites()
    {
        circle?.ClearSprite();
        selectionCircle?.ClearSprite();
        regionIcon?.RemoveFromContainer();
        regionIcon = null;
    }

    public void Clicked()
    {

    }
}*/