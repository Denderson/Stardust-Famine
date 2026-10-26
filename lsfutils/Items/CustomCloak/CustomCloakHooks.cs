using lsfUtils.CWTs;
using lsfUtils.SaveFile;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using static lsfUtils.Enums;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.CustomCloak;

public static class CustomCloakHooks
{
    public static void ApplyHooks()
    {
        On.MoreSlugcats.MoonCloak.InitiateSprites += MoreSlugcats_MoonCloak_InitiateSprites;
        On.MoreSlugcats.MoonCloak.Color += MoreSlugcats_MoonCloak_Color;
        On.PlayerGraphics.Gown.InitiateSprite += PlayerGraphics_Gown_InitiateSprite;
        On.PlayerGraphics.Gown.Color += PlayerGraphics_Gown_Color;

        On.SaveState.LoadGame += SaveState_LoadGame;
        On.Player.SlugcatGrab += Player_SlugcatGrab;

        On.Player.ctor += Player_ctor;
    }

    private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (PlayerCWT.TryGetData(self, out var data) && SaveStateCWT.TryGetData(world?.game?.GetStorySession?.saveState, out var saveData))
        {
            data.gownColor = saveData.gownColor;
            data.gownType = saveData.gownType;
        }
    }

    private static void Player_SlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
    {
        if (self?.room?.game?.GetStorySession?.saveState?.deathPersistentSaveData != null && obj is CustomCloak cloak && cloak.abstractPhysicalObject is CustomCloakAbstract cloakAbstract && PlayerCWT.TryGetData(self, out var data))
        {
            AbstractPhysicalObject abstractPhysicalObject = obj.abstractPhysicalObject;
            abstractPhysicalObject.realizedObject.RemoveFromRoom();
            abstractPhysicalObject.Room.RemoveEntity(abstractPhysicalObject);
            self.switchHandsCounter = 0;
            self.wantToPickUp = 0;
            self.noPickUpOnRelease = 20;
            self.room.game.GetStorySession.saveState.wearingCloak = true;
            data.gownColor = cloakAbstract.cloakColor;
            data.gownType = cloakAbstract.cloakType;
            self.room.game.GetStorySession.saveState.deathPersistentSaveData.Set("gownColor", cloakAbstract.cloakColor);
            self.room.game.GetStorySession.saveState.deathPersistentSaveData.Set("gownType", cloakAbstract.cloakType);
            return;
        }
        orig(self, obj, graspUsed);

    }

    private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
        orig(self, str, game);
        if (self == null) return;
        if (!SaveStateCWT.TryGetData(self, out var data)) return;

        if (!self.wearingCloak)
        {
            self.deathPersistentSaveData.Set("gownColor", CloakColor.Default);
            self.deathPersistentSaveData.Set("gownType", CloakColor.Default);
        }  
        
        data.gownColor = self.deathPersistentSaveData.GetGownColor("gownColor");
        data.gownType = self.deathPersistentSaveData.GetGownType("gownType");
    }

    public static void MoreSlugcats_MoonCloak_InitiateSprites(On.MoreSlugcats.MoonCloak.orig_InitiateSprites orig, MoonCloak self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (self is not CustomCloak customCloak || customCloak.abstractPhysicalObject is not CustomCloakAbstract cloakAbstract)
        {
            orig(self, sLeaser, rCam);
            return;
        }

        sLeaser.sprites = new FSprite[1];
        if (cloakAbstract.cloakType == CloakType.Broken)
        {
            Log.LogMessage("Broken cloak!");
            sLeaser.sprites[0] = TriangleMesh.MakeGridMesh("MoonCloakTex", self.divs - 1);
        }
        else
        {
            Log.LogMessage("Normal cloak!");
            sLeaser.sprites[0] = TriangleMesh.MakeGridMesh("Futile_White", self.divs - 1);
        }

        for (int i = 0; i < self.divs; i++)
        {
            for (int j = 0; j < self.divs; j++)
            {
                (sLeaser.sprites[0] as TriangleMesh).verticeColors[j * self.divs + i] = self.Color((float)i / (float)(self.divs - 1));
            }
        }
        self.AddToContainer(sLeaser, rCam, null);
    }
    public static Color MoreSlugcats_MoonCloak_Color(On.MoreSlugcats.MoonCloak.orig_Color orig, MoonCloak self, float f)
    {
        if (self is not CustomCloak customCloak || customCloak.abstractPhysicalObject is not CustomCloakAbstract cloakAbstract)
        {
            return orig(self, f);
        }
        return GetGownColor(cloakAbstract.cloakColor, f);
    }
    public static void PlayerGraphics_Gown_InitiateSprite(On.PlayerGraphics.Gown.orig_InitiateSprite orig, PlayerGraphics.Gown self, int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (!PlayerCWT.TryGetData(self?.owner?.player, out var data))
        {
            orig(self, sprite, sLeaser, rCam); 
            return;
        }

        if (data.gownType == CloakType.Broken) sLeaser.sprites[sprite] = TriangleMesh.MakeGridMesh("MoonCloakTex", self.divs - 1);
        else sLeaser.sprites[sprite] = TriangleMesh.MakeGridMesh("Futile_White", self.divs - 1);

        for (int i = 0; i < self.divs; i++)
        {
            for (int j = 0; j < self.divs; j++)
            {
                self.clothPoints[i, j, 0] = self.owner.player.firstChunk.pos;
                self.clothPoints[i, j, 1] = self.owner.player.firstChunk.pos;
                self.clothPoints[i, j, 2] = new Vector2(0f, 0f);
            }
        }
    }
    public static Color PlayerGraphics_Gown_Color(On.PlayerGraphics.Gown.orig_Color orig, PlayerGraphics.Gown self, float f)
    {
        if (!PlayerCWT.TryGetData(self?.owner?.player, out var data))
        {
            return orig(self, f);
        }

        return GetGownColor(data.gownColor, f);
    }

    public static Color GetGownColor(CloakColor presetColor, float f)
    {
        return presetColor switch
        {
            CloakColor.Pebbles => Custom.HSL2RGB(Mathf.Lerp(0.08f, 0.02f, Mathf.Pow(f, 2f)), Mathf.Lerp(1f, 0.8f, f), 0.5f),
            CloakColor.CollapsePebbles => Color.Lerp(Custom.HSL2RGB(Mathf.Lerp(0.08f, 0.02f, Mathf.Pow(f, 2f)), Mathf.Lerp(1f, 0.8f, f), 0.5f), Color.white, 0.3f),
            CloakColor.Sliver => Custom.HSL2RGB(Mathf.Lerp(0f, 0.2f, f), Mathf.Lerp(0.5f, 1f, f), Mathf.Lerp(0.4f, 0.75f, f)),
            CloakColor.Rot => Custom.HSL2RGB(0.73f, Mathf.Lerp(0f, 0.77f, Mathf.Pow(f, 1.1f)), Mathf.Lerp(0.01f, 0.47f, Mathf.Pow(f, 6f))),
            CloakColor.PastMoon => Custom.HSL2RGB(Mathf.Lerp(0.08f, 0.02f, Mathf.Pow(f, 2f)), Mathf.Lerp(0.6f, 0.4f, f), 0.4f),
            CloakColor.Default => Custom.HSL2RGB(Mathf.Lerp(0.38f, 0.32f, Mathf.Pow(f, 2f)), Mathf.Lerp(0f, 0.1f, Mathf.Pow(f, 1.1f)), Mathf.Lerp(0.7f, 0.3f, Mathf.Pow(f, 6f))),
            _ => Custom.HSL2RGB(Mathf.Lerp(0.38f, 0.32f, Mathf.Pow(f, 2f)), Mathf.Lerp(0f, 0.1f, Mathf.Pow(f, 1.1f)), Mathf.Lerp(0.7f, 0.3f, Mathf.Pow(f, 6f))),
        };
    }
}