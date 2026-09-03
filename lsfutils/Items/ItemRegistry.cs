using System;
using System.Globalization;
using UnityEngine;
using lsfUtils.Items.BrownFruit;
using lsfUtils.Items.KarmaMask;
using lsfUtils.Items.RippleFlower;
using lsfUtils.Items.Normal.TorchSpears;
using lsfUtils.Items.Darts.Dart;
using lsfUtils.Items.Darts.PoisonDart;
using lsfUtils.Items.Normal.ExplosiveBoomerang;
using static lsfUtils.Plugin;

namespace lsfUtils.Items
{
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
        }

        public static void RegisterDart()
        {
            var entry = new ItemRegistryEntry(Enums.AbstractObjectType.Dart)
            {
                SaveParser = (world, objString, obj) =>
                {
                    string[] array = objString.Split(new[] { "<oA>" }, StringSplitOptions.None);

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
            var entry = new ItemRegistryEntry(Enums.AbstractObjectType.PoisonDart)
            {
                IconSprite = "atlases/Symbol_Dart",
                IconColor = Enums.Colors.PoisonColor,
                UnlockID = Enums.SandboxUnlockID.PoisonDart,
                Points = 15,

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
                        return dart.mode == Weapon.Mode.StuckInCreature && dart.pullOutTimer > 0
                            ? Player.ObjectGrabability.Drag
                            : Player.ObjectGrabability.OneHand;
                    }
                    return null;
                }
            };

            ItemRegistryTemplate.Register(entry);
        }

        public static void RegisterBrownFruit()
        {
            var entry = new ItemRegistryEntry(Enums.AbstractObjectType.BrownFruit)
            {
                IconSprite = templarMaskIcon,
                IconColor = RainWorld.GoldRGB,
                UnlockID = Enums.SandboxUnlockID.BrownFruit,

                SandboxFactory = (world, pos, id) =>
                    new BrownFruitAbstract(world, pos, id, -1, -1, null) { isConsumed = false },

                SaveParser = (world, objString, obj) =>
                    new BrownFruitAbstract(world, obj.pos, obj.ID, -1, -1, null)
            };

            ItemRegistryTemplate.Register(entry);
        }

        public static void RegisterExplosiveBoomerang()
        {
            var entry = new ItemRegistryEntry(Enums.AbstractObjectType.ExplosiveBoomerang)
            {
                IconSprite = "Symbol_Boomerang",
                IconColor = new Color(1f, 0.4f, 0.3f),
                UnlockID = Enums.SandboxUnlockID.ExplosiveBoomerang,
                Points = 20,

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
            var entry = new ItemRegistryEntry(Enums.AbstractObjectType.SingularityBoomerang)
            {
                IconSprite = "Symbol_Boomerang",
                IconColor = new Color(0.2f, 0.2f, 1f),
                UnlockID = Enums.SandboxUnlockID.SingularityBoomerang,
                Points = 20,

                SandboxFactory = (world, pos, id) => new AbstractExplosiveBoomerang(world, pos, id, true),
                SaveParser = (world, objString, obj) => new AbstractExplosiveBoomerang(world, obj.pos, obj.ID, true),

                ScavCollectScore = (scav, obj) => 10,
                ScavWeaponPickupScore = (scav, obj) => 10,
                Grabability = (player, obj) =>
                {
                    if (obj is Normal.ExplosiveBoomerang.ExplosiveBoomerang boom)
                    {
                        return boom.mode == Weapon.Mode.Thrown ? Player.ObjectGrabability.CantGrab : Player.ObjectGrabability.OneHand;
                    }
                    return null;
                }
            };

            ItemRegistryTemplate.Register(entry);
        }

        public static void RegisterKarmaMask()
        {
            var entry = new ItemRegistryEntry(Enums.AbstractObjectType.KarmaMask)
            {
                IconSprite = templarMaskIcon,
                IconColor = RainWorld.GoldRGB,
                UnlockID = Enums.SandboxUnlockID.KarmaMask,

                SandboxFactory = (world, pos, id) =>
                    new KarmaMaskAbstract(world, pos, id, -1, -1, null) { isConsumed = false },

                SaveParser = (world, objString, obj) =>
                    new KarmaMaskAbstract(world, obj.pos, obj.ID, -1, -1, null) { rippleBothSides = true }
            };

            ItemRegistryTemplate.Register(entry);
        }

        public static void RegisterRippleFlower()
        {
            var entry = new ItemRegistryEntry(Enums.AbstractObjectType.RippleFlower)
            {
                IconSprite = "Kill_Scavenger",
                IconColor = Color.blue,
                UnlockID = Enums.SandboxUnlockID.RippleFlower,

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
                    string[] array = objString.Split(new[] { "<oA>" }, StringSplitOptions.None);

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
            var entry = new ItemRegistryEntry(Enums.AbstractObjectType.TorchSpear)
            {
                IconSprite = "Symbol_Boomerang",
                IconColor = new Color(0.5f, 0.5f, 0.5f),
                UnlockID = Enums.SandboxUnlockID.TorchSpear,
                Points = 20,

                SandboxFactory = (world, pos, id) => new TorchSpearAbstract(world, pos, id),

                ScavCollectScore = (scav, obj) => obj is TorchSpear spear ? spear.isLit ? 4 : 3 : 3,
                ScavWeaponPickupScore = (scav, obj) => obj is TorchSpear spear ? spear.isLit ? 4 : 3 : 3
            };

            ItemRegistryTemplate.Register(entry);
        }
    }
}
