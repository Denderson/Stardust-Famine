using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using Music;
using RWCustom;
using SlugBase;
using SlugBase.Features;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;
using Watcher;
using static SlugBase.Features.FeatureTypes;
using static Stardust.Plugin;


namespace Stardust.Slugcats.Scholar
{
    public static class ScholarCode
    {


        public static void Teleport(Player player)
        {
            for (int i = 0; i < player.room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < player.room.physicalObjects[i].Count; j++)
                {
                    if (player.room.physicalObjects[i][j] is KnotSpawn memory && memory.abstractPhysicalObject.ID == new EntityID(-1, 1))
                    {
                        Vector2 vel = player.mainBodyChunk.vel;
                        player.SuperHardSetPosition(memory.firstChunk.pos);
                        player.mainBodyChunk.vel = new Vector2(vel.x * 1.3f, vel.y * 1.3f);
                        memory.room.AddObject(new ShockWave(new Vector2(memory.abstractPhysicalObject.pos.x * 20f, memory.abstractPhysicalObject.pos.y * 20f), 120f, 0.1f, 15, true));
                        memory.room.AddObject(new VoidParticle(memory.abstractPhysicalObject.pos.Vec2(), Custom.RNV(), 40));
                        memory.Destroy();
                        return;
                    }
                }
            }
            AbstractPhysicalObject abstractCreature = new AbstractPhysicalObject(player.room.world, WatcherEnums.AbstractObjectType.KnotSpawn, null, player.room.GetWorldCoordinate(player.mainBodyChunk.pos), new EntityID(-1, 1));
            player.room.abstractRoom.AddEntity(abstractCreature);
            abstractCreature.RealizeInRoom();
            player.room.AddObject(new ShockWave(new Vector2((float)abstractCreature.pos.x * 20f, (float)abstractCreature.pos.y * 20f), 120f, 0.1f, 15, true));
            //TeleportPoint telepoint = new(player);
            //player.room.AddObject(telepoint);
        }

        public static void RemoveTeleports(Room room)
        {
            for (int i = 0; i < room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                {
                    if (room.physicalObjects[i][j] is KnotSpawn knot && knot.abstractPhysicalObject.ID == new EntityID(-1, 1))
                    {
                        knot.room.AddObject(new VoidParticle(knot.abstractPhysicalObject.pos.Vec2(), Custom.RNV(), 40));
                        knot.Destroy();
                        return;
                    }
                }
            }
        }
    }
}
