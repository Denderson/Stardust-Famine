using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Looker
{
    public class SSOracleLooker : SSOracleBehavior.ConversationBehavior
    {
        
        bool startedConversation;

        public SSOracleLooker(SSOracleBehavior owner) : base(owner, Plugin.LookerEnums.lookerSubBehaviour, Plugin.LookerEnums.lookerConversation)
        {
            owner.TurnOffSSMusic(abruptEnd: true);
        }

        public override void Update()
        {
            base.Update();
            if (base.inActionCounter > 15 && !startedConversation && owner.conversation == null)
            {
                owner.InitateConversation(convoID, this);
                startedConversation = true;
            }
        }
    }
}
