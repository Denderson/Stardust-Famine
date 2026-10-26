using UnityEngine;

namespace lsfUtils;

public class Enums
{
    public static void Unregister<T>(ExtEnum<T> extEnum) where T : ExtEnum<T>
    {
        extEnum?.Unregister();
    }
    
    public static void RegisterAll()
    {
        EffectTypes.RegisterValues();
        CreatureTemplateType.RegisterValues();
        SandboxUnlockID.RegisterValues();
        AbstractObjectType.RegisterValues();
        CreatureCommunityID.RegisterValues();
    }
    
    public static void UnregisterAll()
    {
        EffectTypes.UnregisterValues();
        CreatureTemplateType.UnregisterValues();
        SandboxUnlockID.UnregisterValues();
        AbstractObjectType.UnregisterValues();
        CreatureCommunityID.UnregisterValues();
    }
    
    public enum DartType { Default, Poison, Quill, Sharpnel }

    public enum DarknessPhase { None, Increasing, Decreasing }

    public enum CloakType { Default, Broken }
    public enum CloakColor { Default, Pebbles, CollapsePebbles, Sliver, Rot, PastMoon }

    public class Colors
    {
        public static Color AirplaneLizardColor = new(0.53f, 0.00f, 0.78f);
        public static Color FlameLizardColor = new(0.41f, 0.04f, 0.04f);
        public static Color MonitorLizardColor = new(0.89f, 0.98f, 0.37f);
        public static Color PoisonLizardColor = new(0.31f, 0.46f, 0.10f);
        public static Color RaspberryLizardColor = new(0.8f, 0f, 0.26f);
        public static Color StarNosedLizardColor = new(0.16f, 0.16f, 0.19f);
        public static Color WeaverLizardColor = RainWorld.SaturatedGold;
        public static Color ClimbGrubColor = new(1f, 0.6f, 0.8f);
        public static Color KnotSpawnColor = RainWorld.SaturatedGold;
    }
    
    public class EffectTypes
    {
        public static RoomSettings.RoomEffect.Type EvilWater = new(nameof(EvilWater), true);
        public static RoomSettings.RoomEffect.Type CreepingDarkness = new(nameof(CreepingDarkness), true);
        public static RoomSettings.RoomEffect.Type LookerMechanicOverride = new(nameof(LookerMechanicOverride), true);

        public static void RegisterValues()
        {
            EvilWater ??= new(nameof(EvilWater), true);
            CreepingDarkness ??= new(nameof(CreepingDarkness), true);
            LookerMechanicOverride ??= new(nameof(LookerMechanicOverride), true);
        }

        public static void UnregisterValues()
        {
            EvilWater?.Unregister();
            EvilWater = null;
            CreepingDarkness?.Unregister();
            CreepingDarkness = null;
            LookerMechanicOverride?.Unregister();
            LookerMechanicOverride = null;
        }
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
        public static CreatureTemplate.Type ClimbGrub = new(nameof(ClimbGrub), true);

        public static CreatureTemplate.Type Porcupine = new(nameof(Porcupine), true);

        public static void RegisterValues()
        {
            WeaverLizard ??= new(nameof(WeaverLizard), true);
            FlameLizard ??= new(nameof(FlameLizard), true);
            AirplaneLizard ??= new(nameof(AirplaneLizard), true);
            RaspberryLizard ??= new(nameof(RaspberryLizard), true);
            MonitorLizard ??= new(nameof(MonitorLizard), true);
            StarNosedLizard ??= new(nameof(StarNosedLizard), true);
            PoisonLizard ??= new(nameof(PoisonLizard), true);
            ScavSeer ??= new(nameof(ScavSeer), true);
            ScavFlank ??= new(nameof(ScavFlank), true);
            ScavMessenger ??= new(nameof(ScavMessenger), true);
            StarSpawn ??= new(nameof(StarSpawn), true);
            StarNoodles ??= new(nameof(StarNoodles), true);
            StarJelly ??= new(nameof(StarJelly), true);
            StarSpider ??= new(nameof(StarSpider), true);
            StarElder ??= new(nameof(StarElder), true);
            PoisonSpider ??= new(nameof(PoisonSpider), true);
            ClimbGrub ??= new(nameof(ClimbGrub), true);
            Porcupine ??= new(nameof(Porcupine), true);
        }

        public static void UnregisterValues()
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
            ClimbGrub?.Unregister();
            ClimbGrub = null;
            Porcupine?.Unregister();
            Porcupine = null;
        }
    }

    public class SandboxUnlockID
    {
        public static MultiplayerUnlocks.SandboxUnlockID RippleFlower = new(nameof(RippleFlower), true);
        public static MultiplayerUnlocks.SandboxUnlockID PoisonDart = new(nameof(PoisonDart), true);
        public static MultiplayerUnlocks.SandboxUnlockID KarmaMask = new(nameof(KarmaMask), true);
        public static MultiplayerUnlocks.SandboxUnlockID ExplosiveBoomerang = new(nameof(ExplosiveBoomerang), true);
        public static MultiplayerUnlocks.SandboxUnlockID SingularityBoomerang = new(nameof(SingularityBoomerang), true);

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
        public static MultiplayerUnlocks.SandboxUnlockID ClimbGrub = new(nameof(ClimbGrub), true);
        public static MultiplayerUnlocks.SandboxUnlockID Porcupine = new(nameof(Porcupine), true);

        public static MultiplayerUnlocks.SandboxUnlockID BrownFruit = new(nameof(BrownFruit), true);
        public static MultiplayerUnlocks.SandboxUnlockID TorchSpear = new(nameof(TorchSpear), true);

        public static MultiplayerUnlocks.SandboxUnlockID KnotSpawnV2 = new(nameof(KnotSpawnV2), true);
        public static MultiplayerUnlocks.SandboxUnlockID CustomCloak = new(nameof(CustomCloak), true);

        public static void RegisterValues()
        {
            RippleFlower ??= new(nameof(RippleFlower), true);
            PoisonDart ??= new(nameof(PoisonDart), true);
            KarmaMask ??= new(nameof(KarmaMask), true);
            ExplosiveBoomerang ??= new(nameof(ExplosiveBoomerang), true);
            SingularityBoomerang ??= new(nameof(SingularityBoomerang), true);

            WeaverLizard ??= new(nameof(WeaverLizard), true);
            FlameLizard ??= new(nameof(FlameLizard), true);
            AirplaneLizard ??= new(nameof(AirplaneLizard), true);
            RaspberryLizard ??= new(nameof(RaspberryLizard), true);
            MonitorLizard ??= new(nameof(MonitorLizard), true);
            StarNosedLizard ??= new(nameof(StarNosedLizard), true);
            PoisonLizard ??= new(nameof(PoisonLizard), true);

            ScavSeer ??= new(nameof(ScavSeer), true);
            ScavFlank ??= new(nameof(ScavFlank), true);
            ScavMessenger ??= new(nameof(ScavMessenger), true);

            StarSpawn ??= new(nameof(StarSpawn), true);
            StarNoodles ??= new(nameof(StarNoodles), true);
            StarJelly ??= new(nameof(StarJelly), true);
            StarSpider ??= new(nameof(StarSpider), true);
            StarElder ??= new(nameof(StarElder), true);

            PoisonSpider ??= new(nameof(PoisonSpider), true);
            ClimbGrub ??= new(nameof(ClimbGrub), true);
            Porcupine ??= new(nameof(Porcupine), true);

            BrownFruit ??= new(nameof(BrownFruit), true);
            TorchSpear ??= new(nameof(TorchSpear), true);

            KnotSpawnV2 ??= new(nameof(KnotSpawnV2), true);
            CustomCloak ??= new(nameof(CustomCloak), true);
        }

        public static void UnregisterValues()
        {
            RippleFlower?.Unregister();
            RippleFlower = null;
            PoisonDart?.Unregister();
            PoisonDart = null;
            KarmaMask?.Unregister();
            KarmaMask = null;
            ExplosiveBoomerang?.Unregister();
            ExplosiveBoomerang = null;
            SingularityBoomerang?.Unregister();
            SingularityBoomerang = null;

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
            ClimbGrub?.Unregister();
            ClimbGrub = null;
            Porcupine?.Unregister();
            Porcupine = null;

            BrownFruit?.Unregister();
            BrownFruit = null;
            TorchSpear?.Unregister();
            TorchSpear = null;

            KnotSpawnV2?.Unregister();
            KnotSpawnV2 = null;
            CustomCloak?.Unregister();
            CustomCloak = null;
        }
    }

    public class AbstractObjectType
    {
        public static AbstractPhysicalObject.AbstractObjectType RippleFlower = new(nameof(RippleFlower), true);
        public static AbstractPhysicalObject.AbstractObjectType Dart = new(nameof(Dart), true);
        public static AbstractPhysicalObject.AbstractObjectType PoisonDart = new(nameof(PoisonDart), true);
        public static AbstractPhysicalObject.AbstractObjectType KarmaMask = new(nameof(KarmaMask), true);
        public static AbstractPhysicalObject.AbstractObjectType ExplosiveBoomerang = new(nameof(ExplosiveBoomerang), true);
        public static AbstractPhysicalObject.AbstractObjectType SingularityBoomerang = new(nameof(SingularityBoomerang), true);
        public static AbstractPhysicalObject.AbstractObjectType BrownFruit = new(nameof(BrownFruit), true);
        public static AbstractPhysicalObject.AbstractObjectType TorchSpear = new(nameof(TorchSpear), true);
        public static AbstractPhysicalObject.AbstractObjectType KnotSpawnV2 = new(nameof(KnotSpawnV2), true);
        public static AbstractPhysicalObject.AbstractObjectType CustomCloak = new(nameof(CustomCloak), true);

        public static void RegisterValues()
        {
            RippleFlower ??= new(nameof(RippleFlower), true);
            Dart ??= new(nameof(Dart), true);
            PoisonDart ??= new(nameof(PoisonDart), true);
            KarmaMask ??= new(nameof(KarmaMask), true);
            ExplosiveBoomerang ??= new(nameof(ExplosiveBoomerang), true);
            SingularityBoomerang ??= new(nameof(SingularityBoomerang), true);
            BrownFruit ??= new(nameof(BrownFruit), true);
            TorchSpear ??= new(nameof(TorchSpear), true);
            KnotSpawnV2 ??= new(nameof(KnotSpawnV2), true);
            CustomCloak ??= new(nameof(CustomCloak), true);
        }

        public static void UnregisterValues()
        {
            RippleFlower?.Unregister();
            RippleFlower = null;
            Dart?.Unregister();
            Dart = null;
            PoisonDart?.Unregister();
            PoisonDart = null;
            KarmaMask?.Unregister();
            KarmaMask = null;
            ExplosiveBoomerang?.Unregister();
            ExplosiveBoomerang = null;
            SingularityBoomerang?.Unregister();
            SingularityBoomerang = null;
            BrownFruit?.Unregister();
            BrownFruit = null;
            TorchSpear?.Unregister();
            TorchSpear = null;
            KnotSpawnV2?.Unregister();
            KnotSpawnV2 = null;
            CustomCloak?.Unregister();
            CustomCloak = null;
        }
    }

    public class CreatureCommunityID
    {
        public static CreatureCommunities.CommunityID StarSpawn = new(nameof(StarSpawn), true);

        public static void RegisterValues()
        {
            StarSpawn ??= new(nameof(StarSpawn), true);
        }

        public static void UnregisterValues()
        {
            StarSpawn?.Unregister();
            StarSpawn = null;
        }
    }
}