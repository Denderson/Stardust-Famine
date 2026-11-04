using RWCustom;
using System.Collections.Generic;

namespace lsfUtils.Items.BasementKey;

public static class BasementKeyHooks
{
    public static void ApplyHooks()
    {
        On.ShortcutHandler.Update += ShortcutHandler_Update;
    }

    private static void ShortcutHandler_Update(On.ShortcutHandler.orig_Update orig, ShortcutHandler self)
    {
        for (int i = 0; i < self.transportVessels.Count; i++)
        {
            var vessel = self.transportVessels[i];

            if (vessel?.creature is not Player player) continue;
            if (vessel.lastPositions == null) continue;
            if (vessel.wait > 0) continue;

            bool holdingKey = false;
            foreach (Creature.Grasp grasp in player.grasps)
            {
                if (grasp?.grabbed?.abstractPhysicalObject != null && grasp.grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.DataPearl) holdingKey = true; // placeholder for now
            }
            if (!holdingKey) continue;

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

            if (destinationDens.Count != 0)
            {
                int destination = destinationDens[UnityEngine.Random.Range(0, destinationDens.Count)];
                vessel.creature.abstractCreature.pos.abstractNode = destination;
                vessel.SetAllPositions(room.exitAndDenIndex[destination]);
            }
            else
            {
                // SOMEHOW load another room (and possibly whole Story Mode) from within arena
            }
        }
        orig(self);
    }
}