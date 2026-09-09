using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.DevtoolsEffects.StardustRain;

public class StardustControllerData : PlacedObject.Data
{
    public Vector2 panelPos;
    public float lifeMult;
    public float speedMult;
    public float pushForce;
    public float swayDir;
    public bool spawnBelow;
    public bool strictWallCheck;
    public float extraWidth;
    public float hue1;
    public float hue2;
    public float lightness;

    public StardustControllerData(PlacedObject owner) : base(owner)
    {
        panelPos = new Vector2(0f, 20f);
        lifeMult = 1f;
        speedMult = 1f;
        pushForce = 1f;
        swayDir = 0f;
        spawnBelow = true;
        strictWallCheck = true;
        extraWidth = 100f;
        hue1 = 0.30f;
        hue2 = 0.15f;
        lightness = 0.5f;
    }

    public override string ToString()
    {
        return $"{panelPos.x}~{panelPos.y}~{lifeMult}~{speedMult}~{pushForce}~{swayDir}~{spawnBelow}~{strictWallCheck}~{extraWidth}~{hue1}~{hue2}~{lightness}";
    }

    public override void FromString(string s)
    {
        string[] bits = s.Split('~');
        if (bits.Length >= 6)
        {
            panelPos = new Vector2(float.Parse(bits[0]), float.Parse(bits[1]));
            lifeMult = float.Parse(bits[2]);
            speedMult = float.Parse(bits[3]);
            pushForce = float.Parse(bits[4]);
            swayDir = float.Parse(bits[5]);
        }
        if (bits.Length >= 7) spawnBelow = bool.Parse(bits[6]);
        if (bits.Length >= 8) strictWallCheck = bool.Parse(bits[7]);
        if (bits.Length >= 9) extraWidth = float.Parse(bits[8]);
        if (bits.Length >= 10) hue1 = float.Parse(bits[9]);
        if (bits.Length >= 11) hue2 = float.Parse(bits[10]);
        if (bits.Length >= 12) lightness = float.Parse(bits[11]);
    }
}
