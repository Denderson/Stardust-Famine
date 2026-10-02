using lsfUtils.CWTs;
using RWCustom;
using Unity.Mathematics;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.PoisonSteam;

public class PoisonSteam : HarmfulSteam
{
    private readonly PlacedObject myPObj;

    public static float poisonApplyRate = 1f / 160f;

    public PoisonSteam(PlacedObject pObj, Room room) : base(pObj, room)
    {
        myPObj = pObj;
        this.room = room;
        if (SteamSmokeCWT.TryGetData(steam, out var data)) data.isPoisonSmoke = true;
    }
    public override void Update(bool eu)
    {
        base.Update(eu); // do vanilla code of Update first

        for (int i = 0; i < steam.particles.Count; i++) // for every steam particle...
        {
            if (steam.particles[i].life > dangerRange) // if its during the time it would be dangerous...
            {
                for (int j = 0; j < room.physicalObjects.Length; j++) // check all physical object types...
                {
                    for (int k = 0; k < room.physicalObjects[j].Count; k++) // check all physical objects in those types...
                    {
                        if (room.physicalObjects[j][k] is not Creature creature) continue; // if they are not a Creature, skip it. Otherwise, the physical object as a Creature is named creature now
                        if (!CreatureCWT.TryGetData(creature, out var data)) continue; // get the CreatureCWT entry of that creature. If it doesnt exist for some reason, skip it.
                        bool alreadyPoisoned = false;

                        for (int l = 0; l < creature.bodyChunks.Length; l++) // check every body chunk of that creature...
                        {
                            if (alreadyPoisoned) continue; // if it already got poison this tick, skip the rest of checks.
                            Vector2 a = creature.bodyChunks[l].ContactPoint.ToVector2(); // get the contact point of that chunk
                            Vector2 b = creature.bodyChunks[l].pos + a * (creature.bodyChunks[l].rad + 30f); // uhhh idk really, get some position that is meant to be the "collision box" for the steam???
                            if (Vector2.Distance(steam.particles[i].pos, b) < 10f && creature.abstractCreature.rippleLayer == 0) // check if the steam particle is 10 or less pixels from the collision box, and on same rippleLayer
                            {
                                alreadyPoisoned = true; // mark the creature as touching the poison steam this tick
                                data.cannotRecoverPoison = 120;
                                data.temporaryPoison += poisonApplyRate;
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }
}
