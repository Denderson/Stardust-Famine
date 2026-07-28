using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.Darts.PoisonDart
{
    public class PoisonDartFisob : Fisob
    {
        public PoisonDartFisob() : base(lsfUtils.Enums.AbstractObjectType.PoisonDart)
        {
            Icon = new SimpleIcon("atlases/Symbol_Dart", lsfUtils.Enums.Colors.PoisonColor);
            SandboxPerformanceCost = new SandboxPerformanceCost(0.35f, 0f);
            RegisterUnlock(lsfUtils.Enums.SandboxUnlockID.PoisonDart, MultiplayerUnlocks.SandboxUnlockID.Slugcat, 15);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock unlock)
        {
            Log.LogMessage("PoisonDartFisob.Parse called");

            string[] array = saveData.CustomData.Split(';');
            if (array.Length < 1)
            {
                array = ["1"];
            }

            float remainingPoison = float.TryParse(array[0], out float result) ? result : 1f;
            PoisonDartAbstract abstractDart = new(world, null, saveData.Pos, saveData.ID, remainingPoison);
            Log.LogMessage("PoisonDartFisob.Parse complete, poison: " + remainingPoison);
            return abstractDart;
        }

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            if (forObject is PoisonDart dart)
            {
                return new PoisonDartProperties(dart);
            }
            return null;
        }
    }

}
