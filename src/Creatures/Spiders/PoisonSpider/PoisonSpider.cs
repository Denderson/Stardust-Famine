using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Creatures.Spiders.PoisonSpider
{
    public class PoisonSpider : BigSpider
    {
        public PoisonSpider(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
            spitter = true;
            bodyChunks = new BodyChunk[2];
            bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 2f * (1f / 3f));
            bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 9f, 2f * (2f / 3f));
            bodyChunkConnections = new BodyChunkConnection[1];
            bodyChunkConnections[0] = new BodyChunkConnection(bodyChunks[0], bodyChunks[1], 25f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
            grabChunks = new BodyChunk[2, 4];
            yellowCol = Color.Lerp(Enums.Colors.PoisonColor, Custom.HSL2RGB(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), UnityEngine.Random.value * 0.2f);
        }
        public override Color ShortCutColor()
        {
            return abstractCreature.IsVoided() ? RainWorld.SaturatedGold : Enums.Colors.PoisonColor;
        }
        public override void LoseAllGrasps()
        {
            ReleaseGrasp(0);
        }
    }
}
