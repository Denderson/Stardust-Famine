using lsfUtils.Creatures.Lizards;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using UnityEngine;
using Watcher;
using static lsfUtils.Plugin;

namespace lsfUtils.Creatures.Lizards
{
    public static class LizardCode
    {
        public static bool IsNotRedOrVariant(bool isNotRed, LizardAI self)
        {
            return isNotRed &&
                   self.creature.creatureTemplate.type != Enums.CreatureTemplateType.AirplaneLizard &&
                   self.creature.creatureTemplate.type != Enums.CreatureTemplateType.FlameLizard &&
                   self.creature.creatureTemplate.type != Enums.CreatureTemplateType.RaspberryLizard &&
                   self.creature.creatureTemplate.type != Enums.CreatureTemplateType.WeaverLizard;
        }

        public static void LizardAI_UpdateDynamicRelationship(ILContext il)
        {
            var c = new ILCursor(il);

            try
            {
                while (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdsfld<CreatureTemplate.Type>("RedLizard")))
                {
                    if (!c.TryGotoNext(MoveType.After,
                        x => x.MatchCallOrCallvirt<ExtEnum<CreatureTemplate.Type>>("op_Inequality")))
                    {
                        Log.LogError("Found RedLizard but no inequality call after!");
                        continue;
                    }
                    else
                    {
                        Log.LogError("Found RedLizard with inequality call!");
                    }

                        c.Emit(OpCodes.Ldarg_0);

                    c.EmitDelegate<Func<bool, LizardAI, bool>>((orig, self) =>
                    {
                        return IsNotRedOrVariant(orig, self);
                    });
                }
            }
            catch (Exception e)
            {
                Log.LogError(e);
            }
        }

        public static CreatureTemplate On_LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
        {
            CreatureTemplate temp;
            LizardBreedParams breedParams;
            if (type == Enums.CreatureTemplateType.WeaverLizard)
            {
                temp = orig(CreatureTemplate.Type.RedLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                breedParams = (temp.breedParameters as LizardBreedParams)!;
                temp.type = type;
                temp.name = "WeaverLizard";
                breedParams.template = type;
                breedParams.baseSpeed = 8f;
                breedParams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[3] = new(1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[4] = new(1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[5] = new(.1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[6] = new(.1f, 1f, 1f, 1f);
                breedParams.standardColor = RainWorld.GoldRGB;
                breedParams.biteDelay = 2;
                breedParams.biteInFront = 20f;
                breedParams.biteRadBonus = 10f;
                breedParams.biteHomingSpeed = 4f;
                breedParams.biteChance = 1f;
                breedParams.attemptBiteRadius = 20f;
                breedParams.getFreeBiteChance = 1f;
                breedParams.biteDamage = 5f;
                breedParams.biteDamageChance = 1f;
                breedParams.toughness = 4f;
                breedParams.stunToughness = 5f;
                breedParams.regainFootingCounter = 1;
                breedParams.bodyMass = 13f;
                breedParams.bodySizeFac = 1.4f;
                breedParams.floorLeverage = 8f;
                breedParams.maxMusclePower = 14f;
                breedParams.wiggleSpeed = .5f;
                breedParams.wiggleDelay = 15;
                breedParams.bodyStiffnes = .3f;
                breedParams.swimSpeed = 1.5f;
                breedParams.idleCounterSubtractWhenCloseToIdlePos = 10;
                breedParams.danger = 1f;
                breedParams.aggressionCurveExponent = .7f;
                breedParams.headShieldAngle = 100f;
                temp.visualRadius = 2300f;
                temp.waterVision = .2f;
                temp.throughSurfaceVision = .2f;
                breedParams.perfectVisionAngle = Mathf.Lerp(1f, -1f, 4f / 9f);
                breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 7f / 9f);
                breedParams.biteDominance = 1f;
                breedParams.limbSize = 1.5f;
                breedParams.limbThickness = 1f;
                breedParams.stepLength = 0.8f;
                breedParams.liftFeet = .3f;
                breedParams.feetDown = .5f;
                breedParams.noGripSpeed = .25f;
                breedParams.limbSpeed = 9f;
                breedParams.limbQuickness = .8f;
                breedParams.limbGripDelay = 1;
                breedParams.smoothenLegMovement = true;
                breedParams.legPairDisplacement = .2f;
                breedParams.walkBob = 3f;
                breedParams.tailSegments = 16;
                breedParams.tailStiffness = 400f;
                breedParams.tailStiffnessDecline = .25f;
                breedParams.tailLengthFactor = 2.5f;
                breedParams.tailColorationStart = 0.1f;
                breedParams.tailColorationExponent = 1.2f;
                breedParams.headSize = 1.4f;
                breedParams.neckStiffness = .37f;
                breedParams.jawOpenAngle = 150f;
                breedParams.jawOpenLowerJawFac = .7666667f;
                breedParams.jawOpenMoveJawsApart = 25f;
                breedParams.headGraphics = new int[5];
                breedParams.framesBetweenLookFocusChange = 20;
                breedParams.tamingDifficulty = 10f;
                breedParams.canExitLounge = false;
                breedParams.canExitLoungeWarmUp = true;
                breedParams.findLoungeDirection = 0.5f;
                breedParams.preLoungeCrouch = 25;
                breedParams.loungeMaximumFrames = 20;
                breedParams.loungePropulsionFrames = 10;
                breedParams.loungeJumpyness = 0.5f;
                breedParams.riskOfDoubleLoungeDelay = 0.1f;
                breedParams.loungeTendensy = 0.2f;
                breedParams.loungeDistance = 300f;


                temp.movementBasedVision = .3f;
                temp.waterPathingResistance = 3f;
                temp.dangerousToPlayer = breedParams.danger;
                temp.doPreBakedPathing = false;
                temp.requireAImap = true;
                temp.preBakedPathingAncestor = pinkTemplate;
                temp.meatPoints = 7;
                temp.baseDamageResistance = breedParams.toughness * 2f;
                temp.baseStunResistance = breedParams.stunToughness;
                temp.damageRestistances[(int)Creature.DamageType.Bite, 0] = 2.5f;
                temp.damageRestistances[(int)Creature.DamageType.Bite, 1] = 3f;
                temp.wormGrassImmune = true;
                temp.meatPoints = 9;
                temp.bodySize = 3f;
                return temp;
            }
            if (type == Enums.CreatureTemplateType.FlameLizard)
            {
                temp = orig(CreatureTemplate.Type.RedLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                breedParams = (temp.breedParameters as LizardBreedParams)!;
                temp.type = type;
                temp.name = "FlameLizard";
                breedParams.template = type;
                breedParams.baseSpeed = 5f;
                breedParams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[3] = new(1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[4] = new(1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[5] = new(.1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[6] = new(.1f, 1f, 1f, 1f);
                breedParams.standardColor = Color.red;
                breedParams.biteDelay = 20;
                breedParams.biteChance = 1f;
                breedParams.attemptBiteRadius = 10f;
                breedParams.getFreeBiteChance = 1f;
                breedParams.biteDamage = 1f;
                breedParams.biteDamageChance = 1f;
                breedParams.toughness = 5f;
                breedParams.stunToughness = 1.5f;
                breedParams.regainFootingCounter = 1;
                breedParams.bodyMass = 6f;
                breedParams.bodySizeFac = 1.3f;
                breedParams.floorLeverage = 8f;
                breedParams.maxMusclePower = 14f;
                breedParams.wiggleSpeed = .25f;
                breedParams.wiggleDelay = 25;
                breedParams.bodyStiffnes = .5f;
                breedParams.swimSpeed = 0.3f;
                breedParams.idleCounterSubtractWhenCloseToIdlePos = 10;
                breedParams.danger = 0.8f;
                breedParams.aggressionCurveExponent = .7f;
                breedParams.headShieldAngle = 100f;
                temp.visualRadius = 2300f;
                temp.waterVision = .2f;
                temp.throughSurfaceVision = .25f;
                breedParams.perfectVisionAngle = Mathf.Lerp(1f, -1f, 4f / 9f);
                breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 7f / 9f);
                breedParams.biteDominance = 1f;
                breedParams.limbSize = 1.3f;
                breedParams.limbThickness = 1.5f;
                breedParams.stepLength = 0.8f;
                breedParams.liftFeet = .3f;
                breedParams.feetDown = .5f;
                breedParams.noGripSpeed = .25f;
                breedParams.limbSpeed = 9f;
                breedParams.limbQuickness = .8f;
                breedParams.limbGripDelay = 1;
                breedParams.smoothenLegMovement = true;
                breedParams.legPairDisplacement = .2f;
                breedParams.walkBob = 1f;
                breedParams.tailSegments = 8;
                breedParams.tailStiffness = 400f;
                breedParams.tailStiffnessDecline = .25f;
                breedParams.tailLengthFactor = 1.5f;
                breedParams.tailColorationStart = 0.1f;
                breedParams.tailColorationExponent = 2f;
                breedParams.headSize = 1.4f;
                breedParams.neckStiffness = .5f;
                breedParams.jawOpenAngle = 150f;
                breedParams.jawOpenLowerJawFac = .7666667f;
                breedParams.jawOpenMoveJawsApart = 25f;
                breedParams.headGraphics = new int[5];
                breedParams.framesBetweenLookFocusChange = 60;
                breedParams.tamingDifficulty = 10f;


                temp.movementBasedVision = .0f;
                temp.waterPathingResistance = 10f;
                temp.dangerousToPlayer = breedParams.danger;
                temp.doPreBakedPathing = false;
                temp.requireAImap = true;
                temp.preBakedPathingAncestor = pinkTemplate;
                temp.meatPoints = 8;
                temp.baseDamageResistance = breedParams.toughness * 2f;
                temp.baseStunResistance = breedParams.stunToughness;
                temp.damageRestistances[(int)Creature.DamageType.Explosion, 1] = 2f;
                temp.meatPoints = 9;
                temp.bodySize = 4f;
                return temp;
            }
            if (type == Enums.CreatureTemplateType.AirplaneLizard)
            {
                temp = orig(CreatureTemplate.Type.PinkLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                breedParams = (temp.breedParameters as LizardBreedParams)!;
                temp.type = type;
                temp.name = "AirplaneLizard";
                breedParams.template = type;
                breedParams.baseSpeed = 10f;
                breedParams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[3] = new(1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[4] = new(1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[5] = new(.1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[6] = new(.1f, 1f, 1f, 1f);
                breedParams.standardColor = new Color(0.53f, 0.00f, 0.78f);
                breedParams.biteDelay = 2;
                breedParams.biteChance = 1f;
                breedParams.attemptBiteRadius = 200f;
                breedParams.biteHomingSpeed = 4f;
                breedParams.getFreeBiteChance = 1f;
                breedParams.biteDamage = 1f;
                breedParams.biteDamageChance = 1f;
                breedParams.toughness = 5f;
                breedParams.stunToughness = 3f;
                breedParams.regainFootingCounter = 1;
                breedParams.bodyMass = 9f;
                breedParams.bodySizeFac = 1.5f;
                breedParams.floorLeverage = 8f;
                breedParams.maxMusclePower = 14f;
                breedParams.wiggleSpeed = .25f;
                breedParams.wiggleDelay = 25;
                breedParams.bodyStiffnes = .5f;
                breedParams.swimSpeed = 1.5f;
                breedParams.idleCounterSubtractWhenCloseToIdlePos = 10;
                breedParams.danger = 1f;
                breedParams.aggressionCurveExponent = .7f;
                breedParams.headShieldAngle = 100f;
                temp.visualRadius = 2300f;
                temp.waterVision = .2f;
                temp.throughSurfaceVision = .25f;
                breedParams.perfectVisionAngle = Mathf.Lerp(1f, -1f, 4f / 9f);
                breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 7f / 9f);
                breedParams.biteDominance = 1f;
                breedParams.limbSize = 1.3f;
                breedParams.limbThickness = 1.5f;
                breedParams.stepLength = 0.8f;
                breedParams.liftFeet = .3f;
                breedParams.feetDown = .5f;
                breedParams.noGripSpeed = .25f;
                breedParams.limbSpeed = 11f;
                breedParams.limbQuickness = .8f;
                breedParams.limbGripDelay = 1;
                breedParams.smoothenLegMovement = true;
                breedParams.legPairDisplacement = .2f;
                breedParams.walkBob = 0.5f;
                breedParams.tailSegments = 8;
                breedParams.tailStiffness = 400f;
                breedParams.tailStiffnessDecline = .25f;
                breedParams.tailLengthFactor = 1.5f;
                breedParams.tailColorationStart = 0.1f;
                breedParams.tailColorationExponent = 2f;
                breedParams.headSize = 1.5f;
                breedParams.neckStiffness = .5f;
                breedParams.jawOpenAngle = 150f;
                breedParams.jawOpenLowerJawFac = .7666667f;
                breedParams.jawOpenMoveJawsApart = 25f;
                breedParams.headGraphics = new int[5];
                breedParams.framesBetweenLookFocusChange = 20;
                breedParams.canExitLounge = false;
                breedParams.canExitLoungeWarmUp = false;
                breedParams.findLoungeDirection = 0.5f;
                breedParams.loungeDistance = 400f;
                breedParams.preLoungeCrouch = 20;
                breedParams.preLoungeCrouchMovement = -0.2f;
                breedParams.loungeSpeed = 15f;
                breedParams.loungeMaximumFrames = 40;
                breedParams.loungePropulsionFrames = 10;
                breedParams.loungeJumpyness = 1.2f;
                breedParams.loungeDelay = 200;
                breedParams.riskOfDoubleLoungeDelay = 0.3f;
                breedParams.postLoungeStun = 100;
                breedParams.loungeTendensy = 0.85f;
                breedParams.tamingDifficulty = 3f;


                temp.movementBasedVision = .0f;
                temp.waterPathingResistance = 10f;
                temp.dangerousToPlayer = breedParams.danger;
                temp.doPreBakedPathing = false;
                temp.requireAImap = true;
                temp.preBakedPathingAncestor = pinkTemplate;
                temp.meatPoints = 8;
                temp.baseDamageResistance = breedParams.toughness * 2f;
                temp.baseStunResistance = breedParams.stunToughness;
                temp.meatPoints = 9;
                temp.bodySize = 7f;
                return temp;
            }
            if (type == Enums.CreatureTemplateType.RaspberryLizard)
            {
                temp = orig(CreatureTemplate.Type.YellowLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                breedParams = (temp.breedParameters as LizardBreedParams)!;
                temp.type = type;
                temp.name = "RaspberryLizard";
                breedParams.template = type;
                breedParams.baseSpeed = 5.5f;
                breedParams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[3] = new(1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[4] = new(1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[5] = new(.1f, 1f, 1f, 1f);
                breedParams.terrainSpeeds[6] = new(.1f, 1f, 1f, 1f);
                breedParams.standardColor = new Color(0.8f, 0f, 0f);
                breedParams.biteDelay = 20;
                breedParams.biteInFront = 20f;
                breedParams.biteRadBonus = 10f;
                breedParams.biteHomingSpeed = 4f;
                breedParams.biteChance = 1f;
                breedParams.attemptBiteRadius = 20f;
                breedParams.getFreeBiteChance = 1f;
                breedParams.biteDamage = 2.2f;
                breedParams.biteDamageChance = 0.75f;
                breedParams.toughness = 2f;
                //if (!AbstractCreature.superSizeMe)
                //{
                //    breedParams.stunToughness = 2.5f; //
                //    breedParams.swimSpeed = 1.2f; //
                //    temp.waterVision = .2f; //
                //    temp.throughSurfaceVision = .2f; //
                //}
                //else
                //{
                //    breedParams.stunToughness = 60f; //
                //    breedParams.swimSpeed = 4f; //
                //    temp.waterVision = 2.5f; //
                //    temp.throughSurfaceVision = 2.5f; //
                //    temp.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
                //    temp.canSwim = true;
                //}
                breedParams.regainFootingCounter = 1;
                breedParams.bodyMass = 8f;
                breedParams.bodySizeFac = 1.4f;
                breedParams.floorLeverage = 8f;
                breedParams.maxMusclePower = 14f;
                breedParams.wiggleSpeed = .5f;
                breedParams.wiggleDelay = 15;
                breedParams.bodyStiffnes = .2f;
                breedParams.idleCounterSubtractWhenCloseToIdlePos = 10;
                breedParams.danger = 1f;
                breedParams.aggressionCurveExponent = .7f;
                breedParams.headShieldAngle = 100f;
                temp.visualRadius = 2300f;
                breedParams.perfectVisionAngle = Mathf.Lerp(1f, -1f, 4f / 9f);
                breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 7f / 9f);
                breedParams.biteDominance = 1f;
                breedParams.limbSize = 1.2f;
                breedParams.limbThickness = 1f;
                breedParams.stepLength = 0.8f;
                breedParams.liftFeet = .4f;
                breedParams.feetDown = .4f;
                breedParams.noGripSpeed = .25f;
                breedParams.limbSpeed = 9f;
                breedParams.limbQuickness = .6f;
                breedParams.limbGripDelay = 1;
                breedParams.smoothenLegMovement = true;
                breedParams.legPairDisplacement = .2f;
                breedParams.walkBob = 3f;
                breedParams.tailSegments = 6;
                breedParams.tailStiffness = 350f;
                breedParams.tailStiffnessDecline = .25f;
                breedParams.tailLengthFactor = 1.5f;
                breedParams.tailColorationStart = 0.45f;
                breedParams.tailColorationExponent = 1f;
                breedParams.headSize = 1.2f;
                breedParams.neckStiffness = .37f;
                breedParams.jawOpenAngle = 150f; //
                breedParams.jawOpenLowerJawFac = .7666667f;
                breedParams.jawOpenMoveJawsApart = 25f;
                breedParams.headGraphics = new int[5]; //
                breedParams.framesBetweenLookFocusChange = 20; //
                breedParams.tamingDifficulty = 7f;
                breedParams.canExitLounge = false;
                breedParams.canExitLoungeWarmUp = true;
                breedParams.findLoungeDirection = 0.5f; //
                breedParams.preLoungeCrouch = 25; //
                breedParams.loungeMaximumFrames = 20; //
                breedParams.loungePropulsionFrames = 10;
                breedParams.loungeJumpyness = 0.5f;
                breedParams.riskOfDoubleLoungeDelay = 0.3f;
                breedParams.loungeTendensy = 0.1f;
                breedParams.loungeDistance = 200f;


                temp.movementBasedVision = .3f;
                temp.waterPathingResistance = 3f;
                temp.dangerousToPlayer = breedParams.danger;
                temp.doPreBakedPathing = false;
                temp.requireAImap = true;
                temp.preBakedPathingAncestor = pinkTemplate;
                temp.meatPoints = 6;
                temp.baseDamageResistance = breedParams.toughness * 1.5f;
                temp.baseStunResistance = breedParams.stunToughness;
                temp.damageRestistances[(int)Creature.DamageType.Bite, 0] = 2.5f;
                temp.damageRestistances[(int)Creature.DamageType.Bite, 1] = 3f;
                temp.wormGrassImmune = false;
                temp.meatPoints = 6;
                temp.bodySize = 3f;
                return temp;
            }
            if (type == Enums.CreatureTemplateType.MonitorLizard)
            {
                temp = orig(CreatureTemplate.Type.Salamander, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                breedParams = (temp.breedParameters as LizardBreedParams)!;
                temp.type = type;
                temp.name = "MonitorLizard";
                breedParams.template = type;

                var temp2 = orig(WatcherEnums.CreatureTemplateType.PeachLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                temp.pathingPreferencesTiles = temp2.pathingPreferencesTiles;
                temp.pathingPreferencesConnections = temp2.pathingPreferencesConnections;
                temp.maxAccessibleTerrain = temp2.maxAccessibleTerrain;

                breedParams.tongue = true;
                breedParams.tongueAttackRange = 400f;
                breedParams.tongueWarmUp = 120;
                breedParams.tongueSegments = 10;
                breedParams.tongueChance = 0.3f;

                breedParams.bodyLengthFac = 1.3f;
                breedParams.baseSpeed = 3.5f;
                breedParams.bodyMass = 2f;
                breedParams.bodySizeFac = 1f;
                breedParams.floorLeverage = 0.35f;
                breedParams.maxMusclePower = 4.8f;
                breedParams.bodyStiffnes = 0.25f;
                breedParams.limbSize = 1.1f;
                breedParams.limbThickness = 1f;
                breedParams.limbSpeed = 3f;
                breedParams.limbQuickness = 0.6f;
                breedParams.limbGripDelay = 1;
                breedParams.smoothenLegMovement = true;
                breedParams.legPairDisplacement = 0.2f;
                breedParams.walkBob = 4f;
                breedParams.tailSegments = 6;
                breedParams.tailStiffness = 300f;
                breedParams.tailStiffnessDecline = 0.2f;
                breedParams.tailLengthFactor = 1.2f;
                breedParams.tailColorationStart = 0.3f;
                breedParams.tailColorationExponent = 2f;

                breedParams.standardColor = Color.yellow;
                breedParams.toughness = 2f;
                breedParams.swimSpeed = 1f;
                breedParams.idleCounterSubtractWhenCloseToIdlePos = 100;
                breedParams.danger = 0.5f;
                breedParams.headSize = 1.2f;
                breedParams.tamingDifficulty = 3f;
                breedParams.headGraphics = new int[5] { 0, 0, 0, 2, 2 };
                breedParams.headShieldAngle = 100f;
                breedParams.neckStiffness = 0.2f;

                breedParams.jawOpenLowerJawFac = 0.5f;
                temp.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
                temp.canSwim = true;
                temp.waterPathingResistance = 1f;
                temp.dangerousToPlayer = breedParams.danger;
                temp.doPreBakedPathing = false;
                temp.requireAImap = true;
                temp.preBakedPathingAncestor = pinkTemplate;
                temp.baseDamageResistance = breedParams.toughness * 2f;
                temp.baseStunResistance = breedParams.stunToughness;
                temp.meatPoints = 5;
                temp.bodySize = 1f;
                return temp;
            }
            if (type == Enums.CreatureTemplateType.StarNosedLizard)
            {
                temp = orig(CreatureTemplate.Type.BlackLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                breedParams = (temp.breedParameters as LizardBreedParams)!;
                temp.type = type;
                temp.name = "StarNosedLizard";

                breedParams.template = type;

                var temp2 = orig(WatcherEnums.CreatureTemplateType.IndigoLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                temp.pathingPreferencesTiles = temp2.pathingPreferencesTiles;
                temp.pathingPreferencesConnections = temp2.pathingPreferencesConnections;
                temp.maxAccessibleTerrain = temp2.maxAccessibleTerrain;

                breedParams.tongue = false;
                breedParams.bodyLengthFac = 1f;
                breedParams.baseSpeed = 3f;
                breedParams.bodyMass = 2f;
                breedParams.bodySizeFac = 1f;
                breedParams.floorLeverage = 0.35f;
                breedParams.maxMusclePower = 4.8f;
                breedParams.bodyStiffnes = 0.25f;
                breedParams.limbSize = 1.3f;
                breedParams.limbThickness = 1.4f;
                breedParams.limbSpeed = 3f;
                breedParams.limbQuickness = 0.6f;
                breedParams.limbGripDelay = 1;
                breedParams.smoothenLegMovement = true;
                breedParams.legPairDisplacement = 0.2f;
                breedParams.walkBob = 4f;
                breedParams.tailSegments = 5;
                breedParams.tailStiffness = 300f;
                breedParams.tailStiffnessDecline = 0.1f;
                breedParams.tailLengthFactor = 1f;
                breedParams.tailColorationStart = 0.3f;
                breedParams.tailColorationExponent = 2f;
                breedParams.standardColor = new Color(0.1f, 0.1f, 0.1f);
                breedParams.toughness = 1.5f;
                breedParams.swimSpeed = 0.5f;
                breedParams.idleCounterSubtractWhenCloseToIdlePos = 100;
                breedParams.danger = 0.3f;
                breedParams.tamingDifficulty = 3f;
                breedParams.headGraphics = new int[5] { 0, 0, 0, 0, 0 };
                breedParams.headSize = 1.3f;
                breedParams.headShieldAngle = 100f;
                breedParams.neckStiffness = 0.2f;

                temp.canSwim = false;
                temp.dangerousToPlayer = breedParams.danger;
                temp.doPreBakedPathing = false;
                temp.requireAImap = true;
                temp.preBakedPathingAncestor = pinkTemplate;
                temp.baseDamageResistance = breedParams.toughness * 2f;
                temp.baseStunResistance = breedParams.stunToughness;
                temp.meatPoints = 3;
                temp.bodySize = 1f;
                return temp;
            }
            if (type == Enums.CreatureTemplateType.PoisonLizard)
            {
                temp = orig(CreatureTemplate.Type.PinkLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                breedParams = (temp.breedParameters as LizardBreedParams)!;
                temp.type = type;
                temp.name = "MonitorLizard";
                breedParams.template = type;

                breedParams.tongue = true;
                breedParams.tongueAttackRange = 200f;
                breedParams.tongueWarmUp = 180;
                breedParams.tongueSegments = 10;
                breedParams.tongueChance = 0.1f;
                breedParams.bodyLengthFac = 1.3f;
                breedParams.baseSpeed = 6.5f;
                breedParams.shakePrey = 200;
                breedParams.biteDamageChance = 0.001f;
                breedParams.bodyMass = 3f;
                breedParams.bodySizeFac = 1.2f;
                breedParams.floorLeverage = 0.35f;
                breedParams.maxMusclePower = 3.8f;
                breedParams.bodyStiffnes = 0.25f;
                breedParams.limbSize = 1f;
                breedParams.limbThickness = 0.7f;
                breedParams.limbSpeed = 3f;
                breedParams.limbQuickness = 0.6f;
                breedParams.limbGripDelay = 1;
                breedParams.smoothenLegMovement = true;
                breedParams.legPairDisplacement = 0.2f;
                breedParams.walkBob = 5f;
                breedParams.tailSegments = 6;
                breedParams.tailStiffness = 300f;
                breedParams.tailStiffnessDecline = 0.2f;
                breedParams.tailLengthFactor = 1f;
                breedParams.tailColorationStart = 0.3f;
                breedParams.tailColorationExponent = 2f;
                breedParams.standardColor = Enums.Colors.PoisonColor;
                breedParams.toughness = 3f; //
                breedParams.swimSpeed = 1f; //
                breedParams.idleCounterSubtractWhenCloseToIdlePos = 100; //
                breedParams.danger = 0.5f;
                breedParams.headSize = 1f;
                breedParams.tamingDifficulty = 3f;
                breedParams.headShieldAngle = 100f;
                breedParams.neckStiffness = 0.2f;
                breedParams.shakePrey = 200;

                breedParams.jawOpenLowerJawFac = 0.5f;
                temp.waterPathingResistance = 2f;
                temp.dangerousToPlayer = breedParams.danger;
                temp.doPreBakedPathing = false;
                temp.requireAImap = true;
                temp.preBakedPathingAncestor = pinkTemplate;
                temp.baseDamageResistance = breedParams.toughness * 2f;
                temp.baseStunResistance = breedParams.stunToughness;
                temp.meatPoints = 5;
                temp.bodySize = 1f;
                return temp;
            }
            return orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
        }

        public static bool Lizard_Swimmer(Func<Lizard, bool> orig, Lizard self)
        {
            return orig(self) || self.Template.type == Enums.CreatureTemplateType.MonitorLizard || self.Template.type == Enums.CreatureTemplateType.RaspberryLizard;
        }

        public static void LizardTongue_ctor(On.LizardTongue.orig_ctor orig, LizardTongue self, Lizard lizard)
        {
            orig(self, lizard);
            if (lizard?.Template?.type == Enums.CreatureTemplateType.MonitorLizard)
            {
                self.range = 400f;
                self.elasticRange = 0.1f;
                self.lashOutSpeed = 35f;
                self.reelInSpeed = 0.006f;
                self.chunkDrag = 0f;
                self.terrainDrag = 0f;
                self.dragElasticity = 0.05f;
                self.emptyElasticity = 0.01f;
                self.involuntaryReleaseChance = 0.000250f;
                self.voluntaryReleaseChance = 0.8f;
                self.totR = self.range * 1.1f;
            }
            if (lizard?.Template?.type == Enums.CreatureTemplateType.PoisonLizard)
            {
                self.range = 300f;
                self.elasticRange = 0.2f;
                self.lashOutSpeed = 50f;
                self.reelInSpeed = 0.002f;
                self.chunkDrag = 0f;
                self.terrainDrag = 0f;
                self.dragElasticity = 0.05f;
                self.emptyElasticity = 0.01f;
                self.involuntaryReleaseChance = 0.005f;
                self.voluntaryReleaseChance = 1f;
                self.totR = self.range * 1.1f;
            }
        }

        public static SoundID On_LizardVoice_GetMyVoiceTrigger(On.LizardVoice.orig_GetMyVoiceTrigger orig, LizardVoice self)
        {
            CreatureTemplate.Type lizardType = self?.lizard?.Template?.type;
            if (lizardType == null)
            {
                Log.LogMessage("Couldnt get lizard type for voice!");
                return orig(self);
            }

            string text;
            string[] array;

            if (lizardType == Enums.CreatureTemplateType.FlameLizard)
            {
                text = "Pink";
                array = ["A", "B", "C", "D", "E"];
            }
            else if (lizardType == Enums.CreatureTemplateType.AirplaneLizard)
            {
                text = "Green";
                array = ["A"];
            }
            else if (lizardType == Enums.CreatureTemplateType.WeaverLizard)
            {
                if (ModManager.MMF && MMF.cfgExtraLizardSounds.Value)
                {
                    text = "Red";
                    array = ["A"];
                }
                else
                {
                    text = "Pink";
                    array = ["A", "B", "C", "D", "E"];
                }
            }
            else if (lizardType == Enums.CreatureTemplateType.RaspberryLizard)
            {
                if (ModManager.MMF && MMF.cfgExtraLizardSounds.Value)
                {
                    text = "Yellow";
                    array = ["A"];
                }
                else
                {
                    text = "Pink";
                    array = ["A", "B", "C", "D", "E"];
                }
            }
            else if (lizardType == Enums.CreatureTemplateType.MonitorLizard)
            {
                if (ModManager.MMF && MMF.cfgExtraLizardSounds.Value)
                {
                    text = "Eel";
                    array = ["A", "B"];
                }
                else
                {
                    text = "Green";
                    array = ["A"];
                }
            }
            else if (lizardType == Enums.CreatureTemplateType.PoisonLizard)
            {
                text = "Green";
                array = ["A"];
            }
            else if (lizardType == Enums.CreatureTemplateType.StarNosedLizard)
            {
                if (ModManager.MMF && MMF.cfgExtraLizardSounds.Value)
                {
                    text = "Black";
                    array = ["A"];
                }
                else
                {
                    text = "Pink";
                    array = ["A", "B", "C", "D", "E"];
                }
            }
            else
            {
                return orig(self);
            }

            List<SoundID> list = [];
            for (int i = 0; i < array.Length; i++)
            {
                SoundID soundID = SoundID.None;
                string text2 = "Lizard_Voice_" + text + "_" + array[i];
                if (ExtEnum<SoundID>.values.entries.Contains(text2))
                {
                    soundID = new SoundID(text2);
                }
                if (soundID != SoundID.None && soundID.Index != -1 && self.lizard.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                {
                    list.Add(soundID);
                }
            }
            if (list.Count == 0)
            {
                return SoundID.None;
            }
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static void LizardAI_ctor(On.LizardAI.orig_ctor orig, LizardAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);

            if (self?.lizard?.Template?.type != null)
            {
                if (self.lizard.Template.type == Enums.CreatureTemplateType.StarNosedLizard)
                {
                    self.AddModule(new SuperHearing(self, self.tracker, 500f));
                }
                if (self.lizard.Template.type == Enums.CreatureTemplateType.MonitorLizard || self.lizard.Template.type == Enums.CreatureTemplateType.PoisonLizard)
                {
                    self.lurkTracker = new LizardAI.LurkTracker(self, self.lizard);
                    self.AddModule(self.lurkTracker);
                    self.utilityComparer.AddComparedModule(self.lurkTracker, null, Mathf.Lerp(0.4f, 0.3f, creature.personality.energy), 1f);
                }
            }
        }
    }
}
