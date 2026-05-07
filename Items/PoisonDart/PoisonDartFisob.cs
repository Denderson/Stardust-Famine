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

namespace lsfUtils.Items.PoisonDart
{
    public class PoisonDartFisob : Fisob
    {
        public PoisonDartFisob() : base(Enums.AbstractPhysicalObjectType.PoisonDart)
        {
            Icon = new SimpleIcon("atlases/Symbol_Dart", Enums.Colors.PoisonColor);
            SandboxPerformanceCost = new SandboxPerformanceCost(0.35f, 0f);
            RegisterUnlock(Enums.SandboxUnlockID.PoisonDart, MultiplayerUnlocks.SandboxUnlockID.Slugcat, 15);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock unlock)
        {
            Plugin.Log.LogMessage("Entered parse");
            string[] array = saveData.CustomData.Split(';');
            if (array.Length < 1)
            {
                array = new string[1];
            }
            float remainingPoison = float.TryParse(array[0], out float result) ? result : 1f;
            PoisonDartAbstract PoisonDartAbstract = new(world, null, saveData.Pos, saveData.ID, remainingPoison);
            Plugin.Log.LogMessage("Exited parse");
            return PoisonDartAbstract;
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
