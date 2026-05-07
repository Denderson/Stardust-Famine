using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
using IL;
using IL.LizardCosmetics;
using IL.Menu;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using Music;
using On;
using RWCustom;
using SlugBase;
using SlugBase.Features;
using SlugBase.SaveData;
using Stardust.SaveFile;
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


namespace Stardust.Conditionals
{
    public class ConditionalsCode
    {
        public static bool? StardustConditions(string text, RainWorldGame game)
        {
            string[] array;
            char sign;
            int condition = 0;
            int value = 0;
            text = text.ToLowerInvariant();
            if (!text.Contains("sf") || game == null || !game.IsStorySession)
            {
                return null;
            }

            if (text.Contains("="))
            {
                sign = '=';
                array = text.Split('=');
            }
            else if (text.Contains(">"))
            {
                sign = '>';
                array = text.Split('>');
            }
            else if (text.Contains('<'))
            {
                sign = '<';
                array = text.Split('<');
            }
            else
            {
                sign = '-';
                array = text.Split('-');
            }

            if (array.Length != 2 || !int.TryParse(array[1], out condition))
            {
                return null;
            }

            switch (array[0].ToLower())
            {
                case "sfcycles":
                    {
                        value = game.GetStorySession.saveState.cycleNumber;
                        break;
                    }
                case "sfechoes":
                    {
                        value = game.GetStorySession.saveState.EchoEncounters();
                        break;
                    }
                default: return null;
            }
            if (sign == '=' && value == condition || sign == '>' && value > condition || sign == '<' && value < condition || sign == '-' && value >= condition)
            {
                return true;
            }
            return false;
        }

        public static void RefreshSpawns(On.WorldLoader.orig_GeneratePopulation orig, WorldLoader self, bool fresh)
        {
            if (SharedMechanics(self.game?.StoryCharacter))
            {
                foreach (AbstractRoom abstractRoom in self.abstractRooms)
                {
                    if (!abstractRoom.shelter)
                    {
                        abstractRoom.creatures.Clear();
                        abstractRoom.entitiesInDens.Clear();
                        continue;
                    }
                    for (int num3 = abstractRoom.creatures.Count - 1; num3 >= 0; num3--)
                    {
                        if (!AbstractPhysicalObject.IsObjectImportant(abstractRoom.creatures[num3], self.world))
                        {
                            abstractRoom.creatures.RemoveAt(num3);
                        }
                    }
                    abstractRoom.entitiesInDens.Clear();
                }
                orig(self, true);
                // setting fresh to always be true, to make spawns load
                return;
            }
            orig(self, fresh);
        }

        public static string DeleteRespawnList(On.SaveState.orig_SaveToString orig, SaveState self)
        {
            if (SharedMechanics(self.saveStateNumber))
            {
                // makes a new empty list to remove the old one
                self.respawnCreatures = new List<int> { };
                self.waitRespawnCreatures = new List<int> { };
            }
            return orig(self);
        }
    }
}
