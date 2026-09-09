using BepInEx;
using RWCustom;
using System;
using UnityEngine;
using DevInterface;

namespace lsfUtils.DevtoolsEffects.StardustRain;

public class StardustEnums // im combining both of these stuff together so i dont need to suffer making two classes
{
    public static RoomSettings.RoomEffect.Type StardustFall;
    public static PlacedObject.Type StardustController;

    public static void RegisterValues()
    {
        if (StardustFall == null) StardustFall = new RoomSettings.RoomEffect.Type("StardustFall", true);
        if (StardustController == null) StardustController = new PlacedObject.Type("StardustController", true);
    }

    public static void UnregisterValues()
    {
        if (StardustFall != null)
        {
            StardustFall.Unregister();
            StardustController.Unregister();
            StardustFall = null;
            StardustController = null;
        }
    }
}