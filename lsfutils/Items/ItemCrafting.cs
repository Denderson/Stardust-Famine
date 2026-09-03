using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using UnityEngine;
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

    public static void InitGourmandCombos()
    {
        Log.LogMessage("Init gourmand combos!");
        SetCombo(AbstractObjectType.RippleFlower, AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractObjectType.KarmaMask);
        SetCombo(AbstractObjectType.KarmaMask, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractPhysicalObject.AbstractObjectType.VultureMask);

        SetCombo(AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractObjectType.RippleFlower, DLCSharedEnums.AbstractObjectType.SingularityBomb);

        if (ModManager.DLCShared)
        {
            SetCombo(AbstractObjectType.RippleFlower, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, AbstractObjectType.ExplosiveBoomerang);
            SetCombo(AbstractObjectType.RippleFlower, DLCSharedEnums.AbstractObjectType.SingularityBomb, AbstractObjectType.SingularityBoomerang);
            SetCombo(AbstractObjectType.ExplosiveBoomerang, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractObjectType.SingularityBoomerang);
            SetCombo(AbstractObjectType.SingularityBoomerang, AbstractObjectType.RippleFlower, AbstractObjectType.ExplosiveBoomerang);
        }

        SetCombo(CreatureTemplate.Type.VultureGrub, AbstractObjectType.RippleFlower, CreatureTemplateType.ClimbGrub);
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
        arrayOrig = GourmandCombos.craftingGrid_CrittersOnly;
        arrayNew = new GourmandCombos.CraftDat[cnt2, cnt2];
        l0 = arrayOrig.GetLength(0);
        l1 = arrayOrig.GetLength(1);
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                arrayNew[i, j] = arrayOrig[i, j];
        }
        GourmandCombos.craftingGrid_CrittersOnly = arrayNew;
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