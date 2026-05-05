using LizardCosmetics;
using lsfUtils.Creatures.Lizards.StarNosedLizard;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using UnityEngine;

public class NoseTendrils : Whiskers
{
    Vector2 smellDirection;
    bool noseActive;
    public NoseTendrils(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        smellDirection = Vector2.zero;
        noseActive = false;
    }

    public override void Update()
    {
        if (lGraphics?.lizard == null)
        {
            return;
        }
        noseActive = (lGraphics.lizard as StarNosedLizard).smellRemaining > 0;
        if (noseActive)
        {
            lGraphics.blackLizardLightUpHead = 0.5f;
        }
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < amount; j++)
            {
                whiskers[i, j].vel += whiskerDir(i, j, 1f) * whiskerProps[j, 2];

                if (smellDirection != Vector2.zero)
                {
                    whiskers[i, j].vel += Custom.DirVec(whiskers[i, j].pos, smellDirection) * 20f;
                }

                if (lGraphics.lizard.room.PointSubmerged(whiskers[i, j].pos))
                {
                    whiskers[i, j].vel *= 0.8f;
                }
                else
                {
                    whiskers[i, j].vel.y -= 0.6f;
                }

                whiskers[i, j].Update();

                whiskers[i, j].ConnectToPoint(AnchorPoint(i, j, 1f), whiskerProps[j, 0], push: false, 0f, lGraphics.lizard.mainBodyChunk.vel, 0f, 0f);
                if (!Custom.DistLess(lGraphics.head.pos, whiskers[i, j].pos, 200f))
                {
                    whiskers[i, j].pos = lGraphics.head.pos;
                }
                whiskerLightUp[j, i, 1] = whiskerLightUp[j, i, 0];
                if (noseActive)
                {
                    if (whiskerLightUp[j, i, 0] < Mathf.InverseLerp(0f, 0.3f, lGraphics.blackLizardLightUpHead))
                    {
                        whiskerLightUp[j, i, 0] = Mathf.Lerp(whiskerLightUp[j, i, 0], Mathf.InverseLerp(0f, 0.3f, lGraphics.blackLizardLightUpHead), 0.7f) + 0.05f;
                    }
                    else
                    {
                        whiskerLightUp[j, i, 0] -= 0.015f;
                    }
                    whiskerLightUp[j, i, 0] += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.03f * lGraphics.blackLizardLightUpHead;
                }
                else
                {
                    whiskerLightUp[j, i, 0] -= 0.015f;
                }
                whiskerLightUp[j, i, 0] = Mathf.Clamp(whiskerLightUp[j, i, 0], 0f, 1f);
            }
        }
        smellDirection = (lGraphics.lizard as StarNosedLizard).smellPoint;
    }

    public static void Whiskers_ctor(ILContext il)
    {
        var c = new ILCursor(il);

        try
        {
            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchCallOrCallvirt(typeof(UnityEngine.Random), "Range")
            ))
            {
                c.Emit(OpCodes.Ldc_I4_2);
                c.Emit(OpCodes.Mul);
            }
            else
            {
                UnityEngine.Debug.LogError("Whiskers_ctor IL hook failed: couldn't find Random.Range");
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Whiskers_ctor IL hook exception: " + e);
        }
    }
}

