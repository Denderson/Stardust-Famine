using System;
using System.Globalization;
using UnityEngine;
using lsfUtils.Items.BrownFruit;
using lsfUtils.Items.KarmaMask;
using lsfUtils.Items.RippleFlower;
using lsfUtils.Items.Darts.Dart;
using lsfUtils.Items.Darts.PoisonDart;
using static lsfUtils.Plugin;
using lsfUtils.Items.ExplosiveBoomerang;
using lsfUtils.Items.TorchSpears;
using static lsfUtils.Enums;
using lsfUtils.Items.KnotSpawnV2;

namespace lsfUtils.Items;

public static class ItemRegistry
{
    public static void RegisterAll()
    {
        RegisterDart();
        RegisterPoisonDart();
        RegisterBrownFruit();
        RegisterExplosiveBoomerang();
        RegisterSingularityBoomerang();
        RegisterKarmaMask();
        RegisterRippleFlower();
        RegisterTorchSpear();
        RegisterKnotSpawnV2();
    }

    public static void RegisterDart()
    {
        var entry = new ItemRegistryEntry(AbstractObjectType.Dart)
        {
            SaveParser = (world, objString, obj) =>
            {
                string[] array = objString.Split(["<oA>"], StringSplitOptions.None);

                float poison = 0f;
                if (array.Length > 3)
                {
                    string[] custom = array[3].Split(',');
                    if (custom.Length > 0)
                    {
                        float.TryParse(custom[0], NumberStyles.Any, CultureInfo.InvariantCulture, out poison);
                    }
                }

                return new AbstractDart(world, null, obj.pos, obj.ID, poison);
            }
        };

        ItemRegistryTemplate.Register(entry);
    }

    public static void RegisterPoisonDart()
    {
        var entry = new ItemRegistryEntry(AbstractObjectType.PoisonDart)
        {
            iconSprite = "atlases/Symbol_Dart",
            iconColor = Colors.PoisonLizardColor,
            unlockID = SandboxUnlockID.PoisonDart,

            SandboxFactory = (world, pos, id) => new PoisonDartAbstract(world, null, pos, id, 1f),

            SaveParser = (world, objString, obj) =>
            {
                string[] array = objString.Split(new[] { "<oA>" }, StringSplitOptions.None);

                float poison = 1f;
                if (array.Length > 3)
                {
                    string[] custom = array[3].Split(',');
                    if (custom.Length > 0)
                    {
                        float.TryParse(custom[0], NumberStyles.Any, CultureInfo.InvariantCulture, out poison);
                    }
                }

                return new PoisonDartAbstract(world, null, obj.pos, obj.ID, poison);
            },

            Grabability = (player, obj) =>
            {
                if (obj is PoisonDart dart)
                {
                    return dart.mode == Weapon.Mode.StuckInCreature && dart.pullOutTimer > 0 ? Player.ObjectGrabability.Drag : Player.ObjectGrabability.OneHand;
                }
                return Player.ObjectGrabability.OneHand;
            }
        };

        ItemRegistryTemplate.Register(entry);
    }

    public static void RegisterBrownFruit()
    {
        var entry = new ItemRegistryEntry(AbstractObjectType.BrownFruit)
        {
            iconSprite = templarMaskIcon,
            iconColor = RainWorld.GoldRGB,
            unlockID = SandboxUnlockID.BrownFruit,

            SandboxFactory = (world, pos, id) =>
                new BrownFruitAbstract(world, pos, id, -1, -1, null) { isConsumed = false },

            SaveParser = (world, objString, obj) =>
                new BrownFruitAbstract(world, obj.pos, obj.ID, -1, -1, null)
        };

        ItemRegistryTemplate.Register(entry);
    }

    public static void RegisterExplosiveBoomerang()
    {
        var entry = new ItemRegistryEntry(AbstractObjectType.ExplosiveBoomerang)
        {
            iconSprite = "Symbol_Boomerang",
            iconColor = new Color(1f, 0.4f, 0.3f),
            unlockID = SandboxUnlockID.ExplosiveBoomerang,

            SandboxFactory = (world, pos, id) => new AbstractExplosiveBoomerang(world, pos, id),
            SaveParser = (world, objString, obj) => new AbstractExplosiveBoomerang(world, obj.pos, obj.ID),

            ScavCollectScore = (scav, obj) => 6,
            ScavWeaponPickupScore = (scav, obj) => 6,
            Grabability = (player, obj) => Player.ObjectGrabability.OneHand
        };

        ItemRegistryTemplate.Register(entry);
    }

    public static void RegisterSingularityBoomerang()
    {
        var entry = new ItemRegistryEntry(AbstractObjectType.SingularityBoomerang)
        {
            iconSprite = "Symbol_Boomerang",
            iconColor = new Color(0.2f, 0.2f, 1f),
            unlockID = SandboxUnlockID.SingularityBoomerang,

            SandboxFactory = (world, pos, id) => new AbstractExplosiveBoomerang(world, pos, id, true),
            SaveParser = (world, objString, obj) => new AbstractExplosiveBoomerang(world, obj.pos, obj.ID, true),

            ScavCollectScore = (scav, obj) => 10,
            ScavWeaponPickupScore = (scav, obj) => 10,
            Grabability = (player, obj) =>
            {
                if (obj is ExplosiveBoomerang.ExplosiveBoomerang boom)
                {
                    return boom.mode == Weapon.Mode.Thrown ? Player.ObjectGrabability.CantGrab : Player.ObjectGrabability.OneHand;
                }
                return Player.ObjectGrabability.OneHand;
            }
        };

        ItemRegistryTemplate.Register(entry);
    }

    public static void RegisterKarmaMask()
    {
        var entry = new ItemRegistryEntry(AbstractObjectType.KarmaMask)
        {
            iconSprite = templarMaskIcon,
            iconColor = RainWorld.GoldRGB,
            unlockID = SandboxUnlockID.KarmaMask,

            SandboxFactory = (world, pos, id) => new KarmaMaskAbstract(world, pos, id, -1, -1, null) { isConsumed = false },
            SaveParser = (world, objString, obj) => new KarmaMaskAbstract(world, obj.pos, obj.ID, -1, -1, null) { rippleBothSides = true }
        };

        ItemRegistryTemplate.Register(entry);
    }

    public static void RegisterRippleFlower()
    {
        var entry = new ItemRegistryEntry(AbstractObjectType.RippleFlower)
        {
            iconSprite = "Kill_Scavenger",
            iconColor = Color.blue,
            unlockID = SandboxUnlockID.RippleFlower,

            SandboxFactory = (world, pos, id) =>
            {
                var flower = new RippleFlowerAbstract(world, pos, id, -1, -1, null)
                {
                    isConsumed = false,
                    flowerRippleLayer = -1,
                    rippleBothSides = true
                };
                return flower;
            },

            SaveParser = (world, objString, obj) =>
            {
                string[] array = objString.Split(["<oA>"], StringSplitOptions.None);

                int layer = 0;
                if (array.Length > 3) int.TryParse(array[3], out layer);

                var flower = new RippleFlowerAbstract(world, obj.pos, obj.ID, -1, -1, null)
                {
                    flowerRippleLayer = layer
                };

                if (layer == -1) flower.rippleBothSides = true;
                else flower.rippleLayer = layer;

                return flower;
            }
        };

        ItemRegistryTemplate.Register(entry);
    }

    public static void RegisterTorchSpear()
    {
        var entry = new ItemRegistryEntry(AbstractObjectType.TorchSpear)
        {
            iconSprite = "Symbol_Spear",
            iconColor = new Color(0.5f, 0.5f, 0.5f),
            unlockID = SandboxUnlockID.TorchSpear,

            SandboxFactory = (world, pos, id) => new TorchSpearAbstract(world, pos, id),

            ScavCollectScore = (scav, obj) => obj is TorchSpear spear ? spear.isLit ? 4 : 3 : 3,
            ScavWeaponPickupScore = (scav, obj) => obj is TorchSpear spear ? spear.isLit ? 4 : 3 : 3
        };

        ItemRegistryTemplate.Register(entry);
    }

    public static void RegisterKnotSpawnV2()
    {
        var entry = new ItemRegistryEntry(AbstractObjectType.KnotSpawnV2)
        {
            iconSprite = "karma9-9",
            iconColor = Colors.KnotSpawnColor,
            unlockID = SandboxUnlockID.KnotSpawnV2,

            SandboxFactory = (world, pos, id) => new KnotSpawnV2Abstract(world, pos, id, -1, -1, null) { isConsumed = false },
            SaveParser = (world, objString, obj) => new KnotSpawnV2Abstract(world, obj.pos, obj.ID, -1, -1, null) { rippleBothSides = true }
        };

        ItemRegistryTemplate.Register(entry);
    }
}
