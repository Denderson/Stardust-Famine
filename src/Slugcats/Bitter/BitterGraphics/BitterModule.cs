using Mono.Cecil.Metadata;
using RWCustom;
using SlugBase.DataTypes;
using System.Collections;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using System.Xml;
using UnityEngine;
using static Stardust.Plugin;

namespace Stardust.Slugcats.Bitter.BitterGraphics
{
    public static class BitterModule
    {
        private static readonly ConditionalWeakTable<Player, BitterData> BitterCWT = new ConditionalWeakTable<Player, BitterData>();

        public static bool IsBitter(this Player player)
        {
            return player.SlugCatClass == Enums.SlugcatStatsName.bitter;
        }

        public static bool IsBitter(this PlayerGraphics player)
        {
            return player.player.IsBitter();
        }
            
        public static BitterData GetBitterData(this Player player)
        {
            return BitterCWT.GetValue(player, (_) => new BitterData(player));
        }

        public static void Follow(this FSprite sprite, FSprite follow)
        {
            sprite.SetPosition(follow.GetPosition());
            sprite.rotation = follow.rotation;
            sprite.scaleX = follow.scaleX;
            sprite.scaleY = follow.scaleY;
            sprite.isVisible = follow.isVisible;
            sprite.alpha = follow.alpha;
            sprite.anchorX = follow.anchorX;
            sprite.anchorY = follow.anchorY;
        }
    }
}