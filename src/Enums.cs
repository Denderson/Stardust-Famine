using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils
{
    public class Enums
    {
        public static void Unregister<T>(ExtEnum<T> extEnum) where T : ExtEnum<T>
        {
            extEnum?.Unregister();
        }

        public class Colors
        {
            public static Color PoisonColor = new(0.31f, 0.46f, 0.10f);
        }

        public class CreatureTemplateType
        {
            public static CreatureTemplate.Type WeaverLizard = new(nameof(WeaverLizard), true);
            public static CreatureTemplate.Type FlameLizard = new(nameof(FlameLizard), true);
            public static CreatureTemplate.Type AirplaneLizard = new(nameof(AirplaneLizard), true);
            public static CreatureTemplate.Type RaspberryLizard = new(nameof(RaspberryLizard), true);

            public static CreatureTemplate.Type MonitorLizard = new(nameof(MonitorLizard), true);
            public static CreatureTemplate.Type StarNosedLizard = new(nameof(StarNosedLizard), true);
            public static CreatureTemplate.Type PoisonLizard = new(nameof(PoisonLizard), true);

            public static CreatureTemplate.Type ScavSeer = new(nameof(ScavSeer), true);
            public static CreatureTemplate.Type ScavFlank = new(nameof(ScavFlank), true);
            public static CreatureTemplate.Type ScavMessenger = new(nameof(ScavMessenger), true);

            public static CreatureTemplate.Type StarSpawn = new(nameof(StarSpawn), true);
            public static CreatureTemplate.Type StarNoodles = new(nameof(StarNoodles), true);
            public static CreatureTemplate.Type StarJelly = new(nameof(StarJelly), true);
            public static CreatureTemplate.Type StarSpider = new(nameof(StarSpider), true);
            public static CreatureTemplate.Type StarElder = new(nameof(StarElder), true);

            public static CreatureTemplate.Type PoisonSpider = new(nameof(PoisonSpider), true);

            public void UnregisterValues()
            {
                WeaverLizard?.Unregister();
                WeaverLizard = null;
                FlameLizard?.Unregister();
                FlameLizard = null;
                AirplaneLizard?.Unregister();
                AirplaneLizard = null;
                RaspberryLizard?.Unregister();
                RaspberryLizard = null;
                MonitorLizard?.Unregister();
                MonitorLizard = null;
                StarNosedLizard?.Unregister();
                StarNosedLizard = null;
                PoisonLizard?.Unregister();
                PoisonLizard = null;
                ScavSeer?.Unregister();
                ScavSeer = null;
                ScavFlank?.Unregister();
                ScavFlank = null;
                ScavMessenger?.Unregister();
                ScavMessenger = null;
                StarSpawn?.Unregister();
                StarSpawn = null;
                StarNoodles?.Unregister();
                StarNoodles = null;
                StarJelly?.Unregister();
                StarJelly = null;
                StarSpider?.Unregister();
                StarSpider = null;
                StarElder?.Unregister();
                StarElder = null;
                PoisonSpider?.Unregister();
                PoisonSpider = null;
            }
        }

        public class SandboxUnlockID
        {
            public static MultiplayerUnlocks.SandboxUnlockID RippleFlower = new(nameof(RippleFlower), true);
            public static MultiplayerUnlocks.SandboxUnlockID PoisonDart = new(nameof(PoisonDart), true);

            public static MultiplayerUnlocks.SandboxUnlockID WeaverLizard = new(nameof(WeaverLizard), true);
            public static MultiplayerUnlocks.SandboxUnlockID FlameLizard = new(nameof(FlameLizard), true);
            public static MultiplayerUnlocks.SandboxUnlockID AirplaneLizard = new(nameof(AirplaneLizard), true);
            public static MultiplayerUnlocks.SandboxUnlockID RaspberryLizard = new(nameof(RaspberryLizard), true);
            public static MultiplayerUnlocks.SandboxUnlockID MonitorLizard = new(nameof(MonitorLizard), true);
            public static MultiplayerUnlocks.SandboxUnlockID StarNosedLizard = new(nameof(StarNosedLizard), true);
            public static MultiplayerUnlocks.SandboxUnlockID PoisonLizard = new(nameof(PoisonLizard), true);

            public static MultiplayerUnlocks.SandboxUnlockID ScavSeer = new(nameof(ScavSeer), true);
            public static MultiplayerUnlocks.SandboxUnlockID ScavFlank = new(nameof(ScavFlank), true);
            public static MultiplayerUnlocks.SandboxUnlockID ScavMessenger = new(nameof(ScavMessenger), true);
            public static MultiplayerUnlocks.SandboxUnlockID StarSpawn = new(nameof(StarSpawn), true);
            public static MultiplayerUnlocks.SandboxUnlockID StarNoodles = new(nameof(StarNoodles), true);
            public static MultiplayerUnlocks.SandboxUnlockID StarJelly = new(nameof(StarJelly), true);
            public static MultiplayerUnlocks.SandboxUnlockID StarSpider = new(nameof(StarSpider), true);
            public static MultiplayerUnlocks.SandboxUnlockID StarElder = new(nameof(StarElder), true);
            public static MultiplayerUnlocks.SandboxUnlockID PoisonSpider = new(nameof(PoisonSpider), true);


            public void UnregisterValues()
            {
                RippleFlower?.Unregister();
                RippleFlower = null;

                WeaverLizard?.Unregister();
                WeaverLizard = null;
                FlameLizard?.Unregister();
                FlameLizard = null;
                AirplaneLizard?.Unregister();
                AirplaneLizard = null;
                RaspberryLizard?.Unregister();
                RaspberryLizard = null;
                MonitorLizard?.Unregister();
                MonitorLizard = null;
                StarNosedLizard?.Unregister();
                StarNosedLizard = null;
                PoisonLizard?.Unregister();
                PoisonLizard = null;
                ScavSeer?.Unregister();
                ScavSeer = null;
                ScavFlank?.Unregister();
                ScavFlank = null;
                ScavMessenger?.Unregister();
                ScavMessenger = null;
                StarSpawn?.Unregister();
                StarSpawn = null;
                StarNoodles?.Unregister();
                StarNoodles = null;
                StarJelly?.Unregister();
                StarJelly = null;
                StarSpider?.Unregister();
                StarSpider = null;
                StarElder?.Unregister();
                StarElder = null;
                PoisonSpider?.Unregister();
                PoisonSpider = null;
            }
        }

        public class AbstractPhysicalObjectType
        {
            public static AbstractPhysicalObject.AbstractObjectType RippleFlower = new(nameof(RippleFlower), true);
            public static AbstractPhysicalObject.AbstractObjectType PoisonDart = new(nameof(PoisonDart), true);

            public void UnregisterValues()
            {
                RippleFlower?.Unregister();
                RippleFlower = null;
                PoisonDart?.Unregister();
                PoisonDart = null;
            }
        }

        public class CreatureCommunityID
        {
            public static CreatureCommunities.CommunityID StarSpawn = new(nameof(StarSpawn), true);

            public void UnregisterValues()
            {
                StarSpawn?.Unregister();
                StarSpawn = null;
            }
        }
    }
}
