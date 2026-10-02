using DevInterface;
using static Pom.Pom;
using RWCustom;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.PoisonSteam;

public class ManagedPoisonSteam : ManagedObjectType
{
    public class PoisonSteamData : PlacedObject.HarmfulSteamData
    {
        public PoisonSteamData(PlacedObject po) : base(po) 
        {

        }
    }

    public ManagedPoisonSteam() : base("PoisonSteam", "lsfUtils", typeof(PoisonSteam), typeof(PoisonSteamData), typeof(HarmfulSteamRepresentation)) { }

    public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
    {
        PoisonSteam steam = new(placedObject, room)
        {
            room = room
        };
        return steam;
    }
}