using Music;
using RWCustom;
using Stardust.Anchors;
using Stardust.CWTs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Stardust.Enums;
using static Stardust.Plugin;


namespace Stardust.Anchors;
public static class AnchorHooks
{
    public static void SpawnAnchor(this World world)
    {
        if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode)
        {
            return;
        }
        if (Custom.rainWorld.safariMode)
        {
            return;
        }
        if (!WorldCWT.TryGetData(world, out var data))
        {
            Log.LogMessage("Couldnt get WorldCWT value!");
            return;
        }

        string filePath = $"world/{world.region.name}/anchor.txt";
        if (!File.Exists(AssetManager.ResolveFilePath(filePath)))
        {
            Log.LogMessage("Anchor file doesnt exist!");
            return;
        }

        string[] array = File.ReadAllLines(AssetManager.ResolveFilePath(filePath));
        if (array == null || array.Length < 2)
        {
            Log.LogMessage("Anchor file is empty!");
            return;
        }

        AnchorID anchorID = AnchorIDFromString(array[0]);
        if (!AnchorData.Active(world.game, anchorID))
        {
            Log.LogMessage("Anchor isnt active!");
            return;
        }

        string anchorSpotRoom = null;
        Dictionary<string, int> anchorPresenceRooms = [];

        for (int i = 1; i < array.Length; i++)
        {
            if (!array[i].StartsWith("//") && array[i].Length > 0)
            {
                Log.LogMessage($"Reading line: {i}");
                if (array[i].Contains(':'))
                {
                    string[] splitLine = array[i].Split(':');
                    if (splitLine.Length != 2)
                    {
                        Log.LogMessage($"Incorrect formatting in Anchor file, line {i}");
                        continue;
                    }
                    if (splitLine[1].ToLowerInvariant().StartsWith("spot"))
                    {
                        anchorSpotRoom = splitLine[0];
                    }
                    else if (int.TryParse(splitLine[1], out int value))
                    {
                        anchorPresenceRooms.Add(splitLine[0], value);
                    }
                }
            }
        }
        if (anchorSpotRoom != null && anchorSpotRoom.Length > 0)
        {
            Log.LogMessage("Spawning Anchor!");
            data.hasAnchorWorldPresence = true;
            data.anchorWorldPresence = new AnchorWorldPresence(world, anchorID, anchorSpotRoom, anchorPresenceRooms);
            return;
        }
        Log.LogMessage("No anchor to load!");
        data.hasAnchorWorldPresence = false;
    }

    public static bool AnchorPresenceInRoom(this AbstractRoom abstractRoom, out float intensity, out AnchorWorldPresence presence)
    {
        intensity = 0f;
        presence = null;
        if (abstractRoom?.world == null)
        {
            return false;
        }
        if (!CWTs.WorldCWT.TryGetData(abstractRoom.world, out var data))
        {
            Log.LogMessage("Couldnt get WorldCWT!");
            return false;
        }
        if (data?.anchorWorldPresence == null)
        {
            return false;
        }
        presence = data.anchorWorldPresence;
        intensity = data.anchorWorldPresence.AnchorMode(abstractRoom);
        return intensity > 0f;
    }

    public static AnchorID AnchorIDFromString(string id)
    {
        if (id == null)
        {
            Log.LogMessage("AnchorID is null!");
            return AnchorID.None;
        }
        id = id.Trim().ToLowerInvariant();
        switch (id)
        {
            case "deeperspace": return AnchorID.Deeperspace;
            case "ripplespace": return AnchorID.Ripplespace;
            case "carnalplane": return AnchorID.Carnalplane;
            case "karmaspace": return AnchorID.Karmaspace;
            case "mindspace": return AnchorID.Karmaspace;
            case "weaverspace": return AnchorID.Weaverspace;
            default:
                {
                    Log.LogMessage("Couldnt parse the AnchorID from string!");
                    return AnchorID.None;
                }
        }
    }

    public static void PlayerThreatTracker_Update(On.Music.PlayerThreatTracker.orig_Update orig, PlayerThreatTracker self)
    {
        orig(self);
        if (!Stardust.CWTs.PlayerThreatTrackerCWT.TryGetData(self, out var data))
        {
            Log.LogMessage("Couldnt assign CWT!");
            return;
        }
        if (self?.musicPlayer.manager.currentMainLoop == null || self.musicPlayer.manager.currentMainLoop.ID != ProcessManager.ProcessID.Game || !(self.musicPlayer.manager.currentMainLoop as RainWorldGame).IsStorySession)
        {
            return;
        }
        if (self.playerNumber >= (self.musicPlayer.manager.currentMainLoop as RainWorldGame).Players.Count || !((self.musicPlayer.manager.currentMainLoop as RainWorldGame).Players[self.playerNumber].realizedCreature is Player { room: not null } player))
        {
            return;
        }
        if (player.room.game.GameOverModeActive)
        {
            return;
        }

        string songName = null;
        if (player?.room?.abstractRoom != null && player.room.abstractRoom.AnchorPresenceInRoom(out float intensity, out AnchorWorldPresence presence) && presence != null && presence.anchorID != AnchorID.None)
        {
            songName = presence.songName;
            data.anchorMode = true;
            data.anchorIntensity = intensity;
        }
        else
        {
            data.anchorMode = false;
            data.anchorIntensity = 0f;
        }
        if (data.anchorMode)
        {
            self.recommendedDroneVolume = 0f;
            self.musicPlayer.FadeOutAllNonGhostSongs(120f);
            if (self.musicPlayer.song == null || !(self.musicPlayer.song is GhostSong) && songName != null)
            {
                self.musicPlayer.RequestGhostSong(songName);
            }
        }
    }

    public static void GhostSong_Update(On.Music.GhostSong.orig_Update orig, GhostSong self)
    {
        if (self?.musicPlayer?.threatTracker != null && CWTs.PlayerThreatTrackerCWT.TryGetData(self.musicPlayer.threatTracker, out var data) && data.anchorMode)
        {
            float oldGhostMode = self.musicPlayer.threatTracker.ghostMode;
            self.musicPlayer.threatTracker.ghostMode = data.anchorIntensity;
            orig(self);
            self.musicPlayer.threatTracker.ghostMode = oldGhostMode;
            return;
        }
        orig(self);
    }

    public static void MusicPlayer_GameRequestsSong(On.Music.MusicPlayer.orig_GameRequestsSong orig, MusicPlayer self, MusicEvent musicEvent)
    {
        if (self?.threatTracker != null && CWTs.PlayerThreatTrackerCWT.TryGetData(self.threatTracker, out var data) && data.anchorMode)
        {
            Log.LogMessage("Not loading song due to Anchor presence!");
            return;
        }
        orig(self, musicEvent);

    }



    public static void World_LoadWorldForFastTravel_Timeline_List1_Int32Array_Int32Array_Int32Array(On.World.orig_LoadWorldForFastTravel_Timeline_List1_Int32Array_Int32Array_Int32Array orig, World self, SlugcatStats.Timeline timelinePosition, List<AbstractRoom> abstractRoomsList, int[] swarmRooms, int[] shelters, int[] gates)
    {
        orig(self, timelinePosition, abstractRoomsList, swarmRooms, shelters, gates);
        if (self?.game != null && !self.singleRoomWorld && self.game.session is StoryGameSession && SharedMechanics(self.game.GetStorySession.saveStateNumber)) // you can switch to simply Bitter if we decide to exclude them from Scholar later on
        {
            self.SpawnAnchor();
        }
    }
}