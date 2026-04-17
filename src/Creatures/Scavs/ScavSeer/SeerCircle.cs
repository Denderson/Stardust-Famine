using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using Unity.Mathematics;
using UnityEngine;

namespace lsfUtils.Creatures.Scavs.ScavSeer
{
    public class SeerPing : TemplarCircle
    {
        public HashSet<Creature> entitiesHit = new HashSet<Creature>();
        public bool hasPrimaryTarget = false;

        public SeerPing(PhysicalObject source, Vector2 pos, float rad, float radIncrement, float radAcceleration, int lifeTime)
            : base(source, pos, rad, radIncrement, radAcceleration, lifeTime, followSource: false)
        {
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (source?.room == null) return;
            for (int i = 0; i < room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                {
                    if (room.physicalObjects[i][j] is Creature creature && !creature.slatedForDeletetion && source.room == creature?.room && CollidesWithCreature(creature) && !IsCreatureIgnored(creature))
                    {
                        foreach (Scavenger scavenger in entitiesHit)
                        {
                            if (scavenger?.AI?.tracker != null)
                            {
                                scavenger.AI.tracker.AddTrackedCreature(creature.abstractCreature);
                                scavenger.AI.tracker.SeeCreature(creature.abstractCreature);
                            }
                        }
                        if (IsCreatureFriendly(creature))
                        {
                            // hit an ally
                            room.AddObject(new TemplarCircle(creature, creature.firstChunk.pos, 0f, 6f, 0f, 120, followSource: true)
                            {
                                maxThickness = 6f,
                                radDamping = 0.05f,
                                maxAlpha = 0.3f
                            });
                            // make ally see
                            if (creature.Template.TopAncestor().type == CreatureTemplate.Type.Scavenger && (creature as Scavenger).AI?.tracker != null)
                            {
                                foreach (Creature entity in entitiesHit)
                                {
                                    (creature as Scavenger).AI.tracker.AddTrackedCreature(entity.abstractCreature);
                                    (creature as Scavenger).AI.tracker.SeeCreature(entity.abstractCreature);
                                }
                            }
                        }
                        else 
                        {
                            // hit an enemy
                            for (int circleNum = 0; circleNum < 4; circleNum++)
                            {
                                room.AddObject(new TemplarCircle(creature, creature.firstChunk.pos, 0f, 6f, 0f, 120 - 25 * circleNum, followSource: true)
                                {
                                    maxThickness = 6f - circleNum,
                                    radDamping = 0.05f + circleNum * 0.05f,
                                    maxAlpha = 0.5f - circleNum * 0.1f
                                });
                            }
                            if (!hasPrimaryTarget && IsCreatureTarget(creature) && room.shortcuts != null && room.shortcuts.Length > 0 && room?.world?.offScreenDen?.creatures != null && room.world.offScreenDen.creatures.Count > 0)
                            {
                                hasPrimaryTarget = true;

                                float bestDistance = 0f;
                                WorldCoordinate destCoord = room.GetWorldCoordinate(creature.firstChunk.pos);
                                for (int currentShortcutIndex = 0; currentShortcutIndex < room.shortcuts.Length; currentShortcutIndex++)
                                {
                                    ShortcutData shortcutData = room.shortcuts[currentShortcutIndex];
                                    if (shortcutData.shortCutType == ShortcutData.Type.RoomExit && shortcutData.destinationCoord != null && shortcutData.startCoord != null)
                                    {
                                        // earlier in code is set to only work if the ping and creature are in the same room, no need to repeat that
                                        float newDistance = Custom.Dist(room.shortcuts[currentShortcutIndex].StartTile.ToVector2(), creature.firstChunk.pos);
                                        if (bestDistance == 0f || newDistance < bestDistance)
                                        {
                                            bestDistance = newDistance;
                                            // is this the right way to get the room that the pipe connects to? Or do I need to use startCoord instead
                                            destCoord = room.shortcuts[currentShortcutIndex].destinationCoord;
                                        }
                                    }
                                }
                                foreach (AbstractCreature flank in room.world.offScreenDen.creatures)
                                {
                                    if (flank.IsFlank())
                                    {
                                        // usually flanks are frozen so they dont go on missions, but once pinged they get sent to the room target is predicted to move to
                                        (flank.abstractAI as ScavengerAbstractAI).freeze = 0;
                                        (flank.abstractAI as ScavengerAbstractAI).SetDestination(destCoord);
                                        break;
                                    }
                                }
                            }

                            /*for (int currentNodeIndex = 0; currentNodeIndex < room.abstractRoom.nodes.Length; currentNodeIndex++)
                               {
                                   if (room.LocalCoordinateOfNode(currentNodeIndex) != null && room.LocalCoordinateOfNode(currentNodeIndex).Tile != null && room.abstractRoom.nodes[currentNodeIndex].type == AbstractRoomNode.Type.Exit)
                                   {
                                       float newDistance = Custom.Dist(room.LocalCoordinateOfNode(currentNodeIndex).Vec2(), room.GetWorldCoordinate(creature.firstChunk.pos).Vec2());
                                       if (bestDistance == 0f || newDistance < bestDistance)
                                       {
                                           bestDistance = newDistance;
                                           destRoom = room.abstractRoom.connections[currentNodeIndex];
                                       }
                                   }
                               }*/
                        }
                    }
                }
            }
        }

        public bool CollidesWithCreature(Creature creature)
        {
            return entitiesHit.Add(creature) && creature.firstChunk.vel.magnitude >= 2.5f && math.abs(Custom.Dist(pos, creature.firstChunk.pos) - rad) < 15;
        }

        public bool IsCreatureFriendly(Creature creature)
        {
            if (creature.Template.TopAncestor().type == CreatureTemplate.Type.Scavenger) return true;
            //if (creature.Template.TopAncestor().type == Enums.CreatureTemplateType.StarSpawn) return true;
            if (creature is Player player && room?.game?.GetStorySession?.creatureCommunities != null && room.game.GetStorySession.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, room.world.RegionNumber, player.playerState.playerNumber) > 0.5f) return true;
            return false;
        }

        public bool IsCreatureIgnored(Creature creature)
        {
            if (creature == null || creature == source) return true;
            CreatureTemplate.Type type = creature.Template.TopAncestor().type;
            if (type == CreatureTemplate.Type.Fly || type == CreatureTemplate.Type.Leech || type == CreatureTemplate.Type.Overseer || type == CreatureTemplate.Type.GarbageWorm) return true;
            return false;
        }

        public bool IsCreatureTarget(Creature creature)
        {
            CreatureTemplate.Type type = creature.Template.TopAncestor().type;
            if (type == CreatureTemplate.Type.LizardTemplate || type == CreatureTemplate.Type.Spider || type == CreatureTemplate.Type.DaddyLongLegs || type == CreatureTemplate.Type.Centipede) return true;
            if (creature is Player player && room?.game?.GetStorySession?.creatureCommunities != null && room.game.GetStorySession.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, room.world.RegionNumber, player.playerState.playerNumber) < -0.5f) return true;
            return false;
        }


    }

}
