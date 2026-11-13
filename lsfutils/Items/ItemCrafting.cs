using lsfUtils.CWTs;
using lsfUtils.Items.ExplosiveBoomerang;
using lsfUtils.Items.Spears.CopperSpears;
using lsfUtils.Items.Spears.TorchSpears;
using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watcher;
using static lsfUtils.Enums;
using static lsfUtils.Plugin;

namespace lsfUtils.Items;

public static class ItemCrafting
{

    public static void On_GourmandCombos_InitCraftingLibrary(On.MoreSlugcats.GourmandCombos.orig_InitCraftingLibrary orig)
    {
        orig();
        InitGourmandCombos();
    }

    public static AbstractPhysicalObject.AbstractObjectType On_Player_CraftingResults(On.Player.orig_CraftingResults orig, Player self)
    {
        if (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
        {
            if (self.FoodInStomach > 0)
            {
                Creature.Grasp[] grasps = self.grasps;
                for (int i = 0; i < grasps.Length; i++)
                {
                    if (grasps[i] != null && grasps[i].grabbed is IPlayerEdible && (grasps[i].grabbed as IPlayerEdible).Edible)
                    {
                        return null;
                    }
                }
                if (grasps[0] != null && grasps[0].grabbed is Boomerang && grasps[0].grabbed.abstractPhysicalObject.type != Enums.AbstractObjectType.ExplosiveBoomerang)
                {
                    return WatcherEnums.AbstractObjectType.Boomerang;
                }
                if (grasps[0] == null && grasps[1] != null && grasps[1].grabbed is Boomerang && grasps[1].grabbed.abstractPhysicalObject.type != Enums.AbstractObjectType.ExplosiveBoomerang&& self.objectInStomach == null)
                {
                    return WatcherEnums.AbstractObjectType.Boomerang;
                }
            }
        }
        return orig(self);
    }
    public static void On_Player_SpitUpCraftedObject(On.Player.orig_SpitUpCraftedObject orig, Player self)
    {
        if (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
        {
            for (int i = 0; i < self.grasps.Length; i++)
            {
                if (self.grasps[i] != null)
                {
                    AbstractPhysicalObject abstractPhysicalObject = self.grasps[i].grabbed.abstractPhysicalObject;
                    if (abstractPhysicalObject is TorchSpearAbstract)
                    {
                        if ((abstractPhysicalObject as TorchSpearAbstract).isLit)
                        {
                            var room = self.room;
                            var pos = self.mainBodyChunk.pos;
                            if (self?.room == null || pos == null) return;
                            room.AddObject(new Explosion(room, self, pos, 7, 350f, 6.2f, 2f, 280f, 0.4f, self, 0.7f, 160f, 1f));
                            room.AddObject(new Explosion.ExplosionLight(pos, 330f, 1f, 3, Color.white));
                            room.AddObject(new ExplosionSpikes(room, pos, 14, 40f, 9f, 7f, 170f, Color.white));
                            room.PlaySound(SoundID.Bomb_Explode, pos, 0.8f, 0.4f);
                            self.Stun(200);
                            self.SubtractFood(1);
                            if (self.FreeHand() != -1)
                            {
                                self.SlugcatGrab(abstractPhysicalObject.realizedObject, self.FreeHand());
                            }
                            self.pyroJumpCounter = MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value - 1;
                            return;
                        }
                        else
                        {
                            self.SubtractFood(1);
                            (abstractPhysicalObject.realizedObject as TorchSpear).Relight(0);
                            return;
                        }
                    }
                    else if (abstractPhysicalObject is CopperSpearAbstract)
                    {
                        CopperSpear spear = (abstractPhysicalObject as CopperSpearAbstract).realizedObject as CopperSpear;
                        if (spear.intensity > 0f)
                        {
                            self.Stun((int)(spear.intensity * 80));
                            self.room.AddObject(new CreatureSpasmer(self, true, (int)(spear.intensity * 80)));
                            self.room.PlaySound(SoundID.Zapper_Zap, spear.firstChunk.pos, 1f, 1.5f, abstractPhysicalObject);
                            self.room.AddObject(new Explosion.ExplosionLight(spear.firstChunk.pos, 40f * spear.intensity, 1f, 8, spear.GetElectricityColor()));
                            if (spear.intensity > 2f) self.Die();
                            spear.Discharge();
                            return;
                        }
                        else
                        {
                            self.ReleaseGrasp(i);
                            abstractPhysicalObject.realizedObject.RemoveFromRoom();
                            self.room.abstractRoom.RemoveEntity(abstractPhysicalObject);
                            self.SubtractFood(1);
                            AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), true);
                            self.room.abstractRoom.AddEntity(abstractSpear);
                            abstractSpear.RealizeInRoom();
                            if (self.FreeHand() != -1)
                            {
                                self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                            }
                            return;
                        }
                    }
                    else if (abstractPhysicalObject.type == WatcherEnums.AbstractObjectType.Boomerang)
                    {
                        self.ReleaseGrasp(i);
                        abstractPhysicalObject.realizedObject.RemoveFromRoom();
                        self.room.abstractRoom.RemoveEntity(abstractPhysicalObject);
                        self.SubtractFood(1);
                        AbstractExplosiveBoomerang absBoom = new AbstractExplosiveBoomerang(self.room.world, self.abstractCreature.pos, self.room.game.GetNewID(), false);
                        self.room.abstractRoom.AddEntity(absBoom);
                        absBoom.RealizeInRoom();
                        if (self.FreeHand() != -1)
                        {
                            self.SlugcatGrab(absBoom.realizedObject, self.FreeHand());
                        }
                        return;

                    }
                }
            }
        }
        orig(self);
    }
    public static void On_Player_Die(On.Player.orig_Die orig, Player self)
    {
        bool isDead = self.dead;
        orig(self);
        if (self.isSlugpup && self.room != null && self.abstractCreature.ID == SpecialId && !isDead)
        {
            AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(self.abstractCreature.Room.world, DLCSharedEnums.AbstractObjectType.SingularityBomb, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.abstractCreature.Room.world.game.GetNewID());
            self.abstractCreature.Room.AddEntity(abstractPhysicalObject);
            abstractPhysicalObject.RealizeInRoom();
            (abstractPhysicalObject.realizedObject as SingularityBomb).Explode();
        }
    }

    public static bool IsHydrogenSlugpup(Player player)
    {
        if (player == null) return false;
        if (player.isSlugpup) return false;
        if (player.abstractCreature.ID != SpecialId) return false;
        return true;
    }

    public static string On_PlayerGraphics_DefaultFaceSprite_float_int(On.PlayerGraphics.orig_DefaultFaceSprite_float_int orig, PlayerGraphics self, float eyeScale, int imgIndex)
    {
        if (IsHydrogenSlugpup(self.player))
        {
            return self._cachedFaceSpriteNames[0, 4, imgIndex];
        }
        return orig(self, eyeScale, imgIndex);
    }
    public static string On_PlayerGraphics_DefaultFaceSprite_float(On.PlayerGraphics.orig_DefaultFaceSprite_float orig, PlayerGraphics self, float eyeScale)
    {
        if (IsHydrogenSlugpup(self.player))
        {
            return "FaceE";
        }
        return orig(self, eyeScale);
    }
    public static void On_PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (IsHydrogenSlugpup(self.player))
        {
            sLeaser.sprites[9].color = Color.red;
        }
    }
    public static void On_PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (IsHydrogenSlugpup(self.player))
        {
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                if (i != 9)
                {
                    sLeaser.sprites[i].color = new Color(0.01f, 0.01f, 0.01f);
                }
            }
        }
    }
    public static void On_AbstractRoom_RealizeRoom(On.AbstractRoom.orig_RealizeRoom orig, AbstractRoom self, World world, RainWorldGame game)
    {
        bool currentlyRealising = (self != null && self.realizedRoom == null);
        orig(self, world, game);
        if (!game.IsStorySession) return;
        if (game.wasAnArtificerDream) return;
        if (!currentlyRealising || self.realizedRoom == null) return;
        if (!RegionCWT.TryGetCustomRegionParams(self?.realizedRoom?.world?.region, out var customRegionParams)) return;
        if ((int)(Random.value * 100) < customRegionParams.InvedSlugpupChance && self.shelter)
        {
            int num = (game.IsStorySession && game.StoryCharacter == SlugcatStats.Name.Red) ? 5 : 25;
            if (ModManager.MSC && game.GetStorySession.slugPupMaxCount > 0 && game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceLastSlugpup >= num && self.name != game.GetStorySession.saveState.denPosition)
            {
                AbstractCreature abstractCreature = new(world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, new WorldCoordinate(self.index, -1, -1, 0), SpecialId);
                self.AddEntity(abstractCreature);
                (abstractCreature.state as PlayerNPCState).foodInStomach = 1;
                game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceLastSlugpup = -game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceLastSlugpup;
            }
        }
    }
    public static int On_World_SpawnPupNPCs(On.World.orig_SpawnPupNPCs orig, World self)
    {
        var value = orig(self);
        var modifiedValue = SpawnHydrogenPupNPCs(self);
        return Mathf.Max(value, modifiedValue);
    }

    public static int SpawnHydrogenPupNPCs(World self)
    {
        if (self.game.world.singleRoomWorld || self.game.rainWorld.safariMode || self.game.wasAnArtificerDream || !self.game.IsStorySession || self.game.GetStorySession.Players.Count <= 0)
        {
            return 0;
        }
        int num = 0;
        int slugpupsSpawned = 0;
        AbstractRoom room = self.game.GetStorySession.Players[0].Room;
        foreach (AbstractCreature abstractCreature in room.creatures)
        {
            if (abstractCreature.state.alive && abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            {
                num++;
                slugpupsSpawned++;
            }
        }
        Random.State state = Random.state;
        self.game.GetStorySession.SetRandomSeedToCycleSeed(self.region.regionNumber);
        if (Random.value >= self.region.regionParams.slugPupSpawnChance && self.game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && self.game.GetStorySession.saveState.forcePupsNextCycle != 1)
        {
            Random.state = state;
            return num;
        }
        if (num < self.game.GetStorySession.slugPupMaxCount)
        {
            List<AbstractRoom> list = new List<AbstractRoom>();
            foreach (AbstractRoom abstractRoom in self.abstractRooms)
            {
                if (abstractRoom != room && abstractRoom.shelter && abstractRoom.name != "SU_S05" && !self.game.world.DisabledMapRooms.Contains(abstractRoom.name, System.StringComparer.InvariantCultureIgnoreCase))
                {
                    list.Add(abstractRoom);
                }
            }
            int num3 = 1;
            AbstractRoom abstractRoom2;
            if (self.game.GetStorySession.saveState.forcePupsNextCycle == 1)
            {
                abstractRoom2 = room;
                num3 = self.game.GetStorySession.slugPupMaxCount - num;
                self.game.GetStorySession.saveState.forcePupsNextCycle = 2;
            }
            else
            {
                if (list.Count == 0)
                {
                    Custom.LogWarning(new string[]
                    {
                        "No shelters for pup spawns"
                    });
                    return num;
                }
                if (list.Count == 1)
                {
                    Custom.LogWarning(new string[]
                    {
                        "only a SINGLE shelter for pup spawns"
                    });
                    abstractRoom2 = list[0];
                }
                else
                {
                    abstractRoom2 = list[Random.Range(0, list.Count)];
                }
            }
            for (int j = 0; j < num3; j++)
            {
                AbstractCreature abstractCreature2 = new AbstractCreature(self, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, new WorldCoordinate(abstractRoom2.index, -1, -1, 0), SpecialId);
                abstractRoom2.AddEntity(abstractCreature2);
                if (abstractRoom2.realizedRoom != null)
                {
                    abstractCreature2.RealizeInRoom();
                }
                (abstractCreature2.state as PlayerNPCState).foodInStomach = 1;
                num++;
                Custom.Log(new string[]
                {
                    string.Format("Created slugpup! {0} at {1} ({2})", abstractCreature2, abstractRoom2.name, abstractRoom2.index)
                });
            }
        }
        Random.state = state;
        return slugpupsSpawned;
    }

    public static void InitGourmandCombos()
    {
        Log.LogMessage("Init gourmand combos!");
        SetCombo(AbstractPhysicalObject.AbstractObjectType.Spear, AbstractObjectType.RippleFlower, AbstractPhysicalObject.AbstractObjectType.Spear); // the result isn't actually Spear, the logic code is in ItemRegistryCore

        SetCombo(AbstractObjectType.RippleFlower, AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractObjectType.KarmaMask);
        SetCombo(AbstractObjectType.KarmaMask, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractPhysicalObject.AbstractObjectType.VultureMask);

        SetCombo(AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractObjectType.RippleFlower, DLCSharedEnums.AbstractObjectType.SingularityBomb);

        if (ModManager.Watcher)
        {
            SetCombo(AbstractPhysicalObject.AbstractObjectType.Rock, AbstractPhysicalObject.AbstractObjectType.DataPearl, WatcherEnums.AbstractObjectType.Boomerang);

            SetCombo(AbstractObjectType.RippleFlower, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, AbstractObjectType.ExplosiveBoomerang);
            SetCombo(WatcherEnums.AbstractObjectType.Boomerang, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, AbstractObjectType.ExplosiveBoomerang);
            SetCombo(AbstractObjectType.RippleFlower, DLCSharedEnums.AbstractObjectType.SingularityBomb, AbstractObjectType.SingularityBoomerang);
            SetCombo(WatcherEnums.AbstractObjectType.Boomerang, DLCSharedEnums.AbstractObjectType.SingularityBomb, AbstractObjectType.SingularityBoomerang);
            SetCombo(AbstractObjectType.ExplosiveBoomerang, AbstractObjectType.RippleFlower, AbstractObjectType.SingularityBoomerang);
            SetCombo(AbstractObjectType.SingularityBoomerang, AbstractObjectType.RippleFlower, AbstractObjectType.ExplosiveBoomerang);
            SetCombo(AbstractObjectType.ExplosiveBoomerang, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
            SetCombo(AbstractObjectType.SingularityBoomerang, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, DLCSharedEnums.AbstractObjectType.SingularityBomb);
        }
        

        SetCombo(CreatureTemplate.Type.VultureGrub, AbstractObjectType.RippleFlower, CreatureTemplateType.ClimbGrub);
        SetCombo(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC);

        Log.LogMessage("Gourmand combos done!");
    }

    internal static void ResizeGourmandCombos()
    {
        _ = GourmandCombos.craftingGrid_CritterObjects;
        var cnt = GourmandCombos.objectsLibrary.Count;
        GourmandCombos.objectsLibrary[AbstractObjectType.RippleFlower] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.ExplosiveBoomerang] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.SingularityBoomerang] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.KarmaMask] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.Dart] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.PoisonDart] = cnt;
        ++cnt;
        if (!GourmandCombos.objectsLibrary.ContainsKey(AbstractPhysicalObject.AbstractObjectType.Spear))
        {
            GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Spear] = cnt;
            ++cnt;
        }
        if (ModManager.Watcher && !GourmandCombos.objectsLibrary.ContainsKey(WatcherEnums.AbstractObjectType.Boomerang))
        {
            GourmandCombos.objectsLibrary[WatcherEnums.AbstractObjectType.Boomerang] = cnt;
            ++cnt;
        }
        var arrayOrig = GourmandCombos.craftingGrid_ObjectsOnly;
        var arrayNew = new GourmandCombos.CraftDat[cnt, cnt];
        int l0 = arrayOrig.GetLength(0), l1 = arrayOrig.GetLength(1), i, j;
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                arrayNew[i, j] = arrayOrig[i, j];
        }
        GourmandCombos.craftingGrid_ObjectsOnly = arrayNew;
        var cnt2 = GourmandCombos.critsLibrary.Count;
        GourmandCombos.critsLibrary[CreatureTemplateType.ClimbGrub] = cnt2;
        ++cnt2;
        if (ModManager.MSC && !GourmandCombos.critsLibrary.ContainsKey(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC))
        {
            GourmandCombos.critsLibrary[MoreSlugcatsEnums.CreatureTemplateType.SlugNPC] = cnt2;
            ++cnt2;
        }
        arrayOrig = GourmandCombos.craftingGrid_CrittersOnly;
        arrayNew = new GourmandCombos.CraftDat[cnt2, cnt2];
        l0 = arrayOrig.GetLength(0);
        l1 = arrayOrig.GetLength(1);
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                arrayNew[i, j] = arrayOrig[i, j];
        }
        GourmandCombos.craftingGrid_CritterObjects = arrayNew;
        arrayOrig = GourmandCombos.craftingGrid_CritterObjects;
        l0 = arrayOrig.GetLength(0);
        l1 = arrayOrig.GetLength(1);
        arrayNew = new GourmandCombos.CraftDat[cnt2, cnt];
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                arrayNew[i, j] = arrayOrig[i, j];
        }
        GourmandCombos.craftingGrid_CritterObjects = arrayNew;
    }

    public static void SetCombo(CreatureTemplate.Type a, CreatureTemplate.Type b, CreatureTemplate.Type result) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[a], GourmandCombos.critsLibrary[b], 2, null, result);

    public static void SetCombo(CreatureTemplate.Type a, CreatureTemplate.Type b, AbstractPhysicalObject.AbstractObjectType result) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[a], GourmandCombos.critsLibrary[b], 2, result, null);

    public static void SetCombo(AbstractPhysicalObject.AbstractObjectType a, CreatureTemplate.Type b, AbstractPhysicalObject.AbstractObjectType result) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[b], GourmandCombos.objectsLibrary[a], 1, result, null);

    public static void SetCombo(CreatureTemplate.Type a, AbstractPhysicalObject.AbstractObjectType b, AbstractPhysicalObject.AbstractObjectType result) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[a], GourmandCombos.objectsLibrary[b], 1, result, null);

    public static void SetCombo(AbstractPhysicalObject.AbstractObjectType a, CreatureTemplate.Type b, CreatureTemplate.Type result) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[b], GourmandCombos.objectsLibrary[a], 1, null, result);

    public static void SetCombo(CreatureTemplate.Type a, AbstractPhysicalObject.AbstractObjectType b, CreatureTemplate.Type result) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[a], GourmandCombos.objectsLibrary[b], 1, null, result);

    public static void SetCombo(AbstractPhysicalObject.AbstractObjectType a, AbstractPhysicalObject.AbstractObjectType b, CreatureTemplate.Type result) => GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[a], GourmandCombos.objectsLibrary[b], 0, null, result);

    public static void SetCombo(AbstractPhysicalObject.AbstractObjectType a, AbstractPhysicalObject.AbstractObjectType b, AbstractPhysicalObject.AbstractObjectType result) => GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[a], GourmandCombos.objectsLibrary[b], 0, result, null);

    public static void SetCombo(CreatureTemplate.Type a, CreatureTemplate.Type b) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[a], GourmandCombos.critsLibrary[b], 2, null, null);

    public static void SetCombo(AbstractPhysicalObject.AbstractObjectType a, CreatureTemplate.Type b) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[b], GourmandCombos.objectsLibrary[a], 1, null, null);

    public static void SetCombo(CreatureTemplate.Type a, AbstractPhysicalObject.AbstractObjectType b) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[a], GourmandCombos.objectsLibrary[b], 1, null, null);

    public static void SetCombo(AbstractPhysicalObject.AbstractObjectType a, AbstractPhysicalObject.AbstractObjectType b) => GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[a], GourmandCombos.objectsLibrary[b], 0, null, null);
}