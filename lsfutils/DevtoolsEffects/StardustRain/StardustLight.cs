using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.DevtoolsEffects.StardustRain;

public class StardustLight : UpdatableAndDeletable, IDrawable // jerma dont question this
{
    public Vector2 pos;
    public Vector2 prevPos;
    public Vector2? nextPos;
    public float radius;
    public float prevRadius;
    public float? nextRadius;
    public float fade;
    public float prevFade;
    public float? nextFade;
    public float paletteDarkness = 1f;
    public UpdatableAndDeletable ownerObj;
    public bool isFlat;
    private Color col;
    public float colAlpha;
    public bool needsShaderReset;

    public Color color
    {
        get { return col; }
        set
        {
            col = value;
            colAlpha = 0f;
            if (col.r > colAlpha) colAlpha = col.r;
            if (col.g > colAlpha) colAlpha = col.g;
            if (col.b > colAlpha) colAlpha = col.b;

            if (colAlpha == 0f)
            {
                col = Color.white;
                return;
            }
            col /= colAlpha;
        }
    }

    public float Lightness
    {
        get
        {
            if (colAlpha == 0f || fade == 0f) return 0f;
            return (col.r + col.g + col.b) / 3f * colAlpha * fade;
        }
    }

    public StardustLight(Vector2 startPos, Color color, UpdatableAndDeletable ownerObj)
    {
        pos = startPos;
        prevPos = startPos;
        //cs1717
        this.color = color;
        this.ownerObj = ownerObj;
    }

    public override void Update(bool evenUpdate)
    {
        base.Update(evenUpdate);

        prevPos = pos;
        if (nextPos.HasValue)
        {
            pos = nextPos.Value;
            nextPos = null;
        }

        prevRadius = radius;
        if (nextRadius.HasValue)
        {
            radius = nextRadius.Value;
            nextRadius = null;
        }

        prevFade = fade;
        if (nextFade.HasValue)
        {
            fade = nextFade.Value;
            nextFade = null;
        }

        if (ownerObj != null && (ownerObj.slatedForDeletetion || ownerObj.room != room))
        {
            if (fade == 0f) Destroy();
            else
            {
                nextFade = 0f;
                nextRadius = 0f;
            }
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sprites, RoomCamera cam)
    {
        sprites.sprites = new FSprite[1];
        sprites.sprites[0] = new FSprite("Futile_White", true);
        sprites.sprites[0].shader = cam.room.game.rainWorld.Shaders[isFlat ? "FlatLight" : "LightSource"];
        sprites.sprites[0].color = color;

        AddToContainer(sprites, cam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sprites, RoomCamera cam, float lerpTime, Vector2 camOffset) // should have no issues adapting to stardust's particle color
    {
        sprites.sprites[0].x = Mathf.Floor(Mathf.Lerp(prevPos.x, pos.x, lerpTime) - camOffset.x) + 0.5f;
        sprites.sprites[0].y = Mathf.Floor(Mathf.Lerp(prevPos.y, pos.y, lerpTime) - camOffset.y) + 0.5f;

        sprites.sprites[0].color = color;
        sprites.sprites[0].scale = Mathf.Lerp(prevRadius, radius, lerpTime) / 8f;

        sprites.sprites[0].alpha = Mathf.Lerp(prevFade, fade, lerpTime) * Mathf.Lerp(1f, cam.room.Darkness(pos), paletteDarkness) * colAlpha;

        if (needsShaderReset)
        {
            sprites.sprites[0].shader = cam.room.game.rainWorld.Shaders[isFlat ? "FlatLight" : "LightSource"];
            needsShaderReset = false;
        }
        if (slatedForDeletetion || room != cam.room)
        {
            sprites.CleanSpritesAndRemove();
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sprites, RoomCamera cam, RoomPalette pal)
    {
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sprites, RoomCamera cam, FContainer container)
    {
        cam.ReturnFContainer("ForegroundLights").AddChild(sprites.sprites[0]);
    }
}