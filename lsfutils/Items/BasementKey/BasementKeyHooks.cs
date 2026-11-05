using RWCustom;
using System.Collections.Generic;

namespace lsfUtils.Items.BasementKey;

public static class BasementKeyHooks
{
    public static SlugcatStats.Name targetSlugcat = Enums.SlugcatStatsNames.____;
    public const string targetRoom = "SU_C04";
    public const int chosenNode = -1;
    public const int maxDelayAfterTeleport = 20;

    public static bool shouldTeleportOnLoad = false;
    public static bool shouldTeleport = false;
    public static int delayAfterTeleport = maxDelayAfterTeleport;

    public static Dictionary<int, PlayerInfo> infos = [];

    public struct PlayerInfo
    {
        public SlugcatStats.Name slugcatName;
        public string stomachItem;
        public List<string> handItems;

        public PlayerInfo(SlugcatStats.Name slugcatName, string stomachItem, List<string> handItems)
        {
            this.slugcatName = slugcatName;
            this.stomachItem = stomachItem;
            this.handItems = handItems;
        }
    }

    public static void ApplyHooks()
    {
        On.ShortcutHandler.Update += ShortcutHandler_Update;
        On.PlayerProgression.GetOrInitiateSaveState += PlayerProgression_GetOrInitiateSaveState;
        On.RainWorldGame.Update += RainWorldGame_Update;
        On.Player.SpitOutOfShortCut += Player_SpitOutOfShortCut;
    }

    public static void Player_SpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        orig(self, pos, newRoom, spitOutAllSticks);
        if (infos.Count <= 0) return;
        if (self?.abstractCreature == null || newRoom == null) return;
        int playerIndex = -1;
        for (int i = 0; i < newRoom.game.Players.Count; i++)
        {
            if (newRoom.game.Players[i] == self.abstractCreature)
            {
                playerIndex = i;
                break;
            }
        }
        if (playerIndex < 0) return;
        if (!infos.ContainsKey(playerIndex)) return;
        if (!infos.TryGetValue(playerIndex, out PlayerInfo value)) return;

        if (value.stomachItem != null)
        {
            if (Plugin.TryAddItem(self.abstractCreature, value.stomachItem, -1, false, out AbstractPhysicalObject stomachItem))
            {
                self.objectInStomach = stomachItem;
            }
        }

        if (value.handItems != null && value.handItems.Count > 0)
        {
            for (int i = 0; i < value.handItems.Count; i++)
            {
                if (string.IsNullOrEmpty(value.handItems[i])) continue;
                if (Plugin.TryAddItem(self.abstractCreature, value.handItems[i], i, true, out AbstractPhysicalObject handItem))
                {
                    handItem.RealizeInRoom();
                    self.SlugcatGrab(handItem.realizedObject, i);
                    Plugin.Log.LogMessage("Realised item!");
                }
            }
        }

        if (value.slugcatName != null)
        {
            self.SlugCatClass = value.slugcatName;
        }

        infos.Remove(playerIndex);
    }

    public static void ShortcutHandler_Update(On.ShortcutHandler.orig_Update orig, ShortcutHandler self)
    {
        if (!shouldTeleportOnLoad)
        {
            for (int i = 0; i < self.transportVessels.Count; i++)
            {
                var vessel = self.transportVessels[i];

                if (vessel?.creature is not Player player) continue;
                if (vessel.lastPositions == null) continue;
                if (vessel.wait > 0) continue;

                bool holdsKey = false;
                foreach (Creature.Grasp grasp in player.grasps)
                {
                    if (grasp?.grabbed?.abstractPhysicalObject?.type == AbstractPhysicalObject.AbstractObjectType.DataPearl) holdsKey = true;
                }
                if (player.objectInStomach?.type == AbstractPhysicalObject.AbstractObjectType.DataPearl) holdsKey = true;

                if (!holdsKey) continue;

                Room room = vessel.room?.realizedRoom;
                if (room == null) continue;

                IntVector2 next = ShortcutHandler.NextShortcutPosition(vessel.pos, vessel.lastPos, room);
                if (next == vessel.pos) continue;
                if (room.GetTile(next).shortCut != 2) continue;

                int currentDen = room.exitAndDenIndex.IndexfOf(next);

                List<int> destinationDens = [];
                for (int j = 0; j < room.exitAndDenIndex.Length; j++)
                {
                    if (j == currentDen) continue;
                    if (room.GetTile(room.exitAndDenIndex[j]).shortCut == 2) destinationDens.Add(j);
                }

                if (false) //destinationDens.Count != 0)
                {
                    int destination = destinationDens[UnityEngine.Random.Range(0, destinationDens.Count)];
                    vessel.creature.abstractCreature.pos.abstractNode = destination;
                    vessel.SetAllPositions(room.exitAndDenIndex[destination]);
                }
                else if (self.game != null && self.game.IsArenaSession)
                {
                    vessel.wait = 1000;
                    
                    PlayerInfo playerInfo = new();
                    int playerIndex = -1;
                    for (int j = 0; j < room.game.Players.Count; j++)
                    {
                        if (room.game.Players[j] == player.abstractCreature)
                        {
                            playerIndex = j;
                            break;
                        }
                    }
                    List<string> handItems = new List<string>();
                    foreach (Creature.Grasp grasp in player.grasps)
                    {
                        if (grasp?.grabbed != null)
                        {
                            handItems.Add(grasp.grabbed.abstractPhysicalObject.type.ToString());
                        }
                    }
                    playerInfo.handItems = handItems;
                    if (player.objectInStomach != null) playerInfo.stomachItem = player.objectInStomach.type.ToString();
                    playerInfo.slugcatName = player.slugcatStats.name;
                    infos.Add(playerIndex, playerInfo);
                    EnterBasement(self.game);
                    break;
                }
            }
        }
        orig(self);
    }

    public static void EnterBasement(RainWorldGame game)
    {
        if (shouldTeleportOnLoad) return;
        shouldTeleportOnLoad = true;
        ProcessManager manager = game.manager;
        if (manager == null) return;
        manager.arenaSitting = null;
        manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = targetSlugcat;
        manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.Load;
        manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
    }

    public static SaveState PlayerProgression_GetOrInitiateSaveState(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, SlugcatStats.Name saveStateNumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveAsDeathOrQuit)
    {
        SaveState saveState = orig(self, saveStateNumber, game, setup, saveAsDeathOrQuit);
        if (shouldTeleportOnLoad && saveState != null)
        {
            shouldTeleportOnLoad = false;
            shouldTeleport = true;
            saveState.denPosition = targetRoom;
        }
        return saveState;
    }

    public static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

        if (!shouldTeleport) return;
        if (self?.world == null || !self.IsStorySession || self.Players == null  || self.Players.Count == 0) return;


        delayAfterTeleport--;
        if (delayAfterTeleport > 0) return;

        AbstractRoom abstractRoom = self.world.GetAbstractRoom(targetRoom);
        if (abstractRoom == null)
        {
            shouldTeleport = false;
            Plugin.Log.LogMessage("Aren't you forgetting something importmant?");
            return;
        }

        if (abstractRoom.realizedRoom == null)
        {
            self.world.ActivateRoom(abstractRoom);
            return;
        }

        if (abstractRoom.realizedRoom.shortcuts == null) return;

        shouldTeleport = false;
        delayAfterTeleport = maxDelayAfterTeleport;

        int node = GetNode(abstractRoom);
        if (node == -1) return;

        for (int i = 0; i < self.Players.Count; i++)
        {
            if (self.Players[i]?.realizedCreature is not Player player) continue;
            PlayerInfo playerInfo = new();
            foreach (Creature.Grasp grasp in player.grasps)
            {
                if (grasp?.grabbed != null)
                {
                    playerInfo.handItems.Add(grasp.grabbed.abstractPhysicalObject.type.ToString());
                    grasp.grabbed.RemoveFromRoom();
                }
            }
            if (player.objectInStomach != null) playerInfo.stomachItem = player.objectInStomach.type.ToString();
            playerInfo.slugcatName = player.slugcatStats.name;
            player.RemoveFromRoom();
            self.shortcuts.CreatureEnterFromAbstractRoom(player, abstractRoom, node);
        }
    }

    public static int GetNode(AbstractRoom room)
    {
        if (chosenNode >= 0 && chosenNode < room.nodes.Length) return chosenNode;
        for (int i = 0; i < room.nodes.Length; i++)
        {
            if (room.nodes[i].type == AbstractRoomNode.Type.Exit) return i; // just placeholder for testing stuff for now
        }
        return -1;
    }
}