using DevInterface;
using lsfUtils.Items.KarmaMask;
using static lsfUtils.Plugin;
using static Pom.Pom;
using RWCustom;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.PoisonSteam
{
    public class ManagedPoisonSteam : ManagedObjectType
    {
        public class PoisonSteamData : PlacedObject.HarmfulSteamData
        {
            public PoisonSteamData(PlacedObject po) : base(po) 
            {
                this.panelPos = Custom.DegToVec(30f) * 10f;
                this.duration = 0.2f;
                this.frequency = 0.5f;
                this.lifetime = 0.3f;
                this.handlePos = new Vector2(0f, 100f);
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
}