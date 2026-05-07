using HUD;
using Watcher;
using static Stardust.Plugin;
using static Stardust.Enums;
using static Stardust.Enums.ConversationIDs;

namespace Stardust.Anchors;

public class AnchorConversation : Conversation
{
    public SoundID Voiceline { get; private set; }

    public AnchorConversation(IOwnAConversation interfaceOwner, ID id, DialogBox dialogBox) : base(interfaceOwner, id, dialogBox)
    {
        Voiceline = GetVoiceline(id);
        AddEvents();
    }

    public override void AddEvents()
    {
        LoadEventsFromFile(GetConversationFile(id));
    }

    private static SoundID GetVoiceline(ID id)
    {
        if (id == deeperspaceAnchor) return SoundIDs.deeperspaceAnchor;
        else if (id == ripplespaceAnchor) return SoundIDs.ripplespaceAnchor;
        else if (id == carnalplaneAnchor) return SoundIDs.carnalplaneAnchor;
        else if (id == karmaspaceAnchor) return SoundIDs.karmaspaceAnchor;
        else if (id == mindspaceAnchor) return SoundIDs.mindspaceAnchor;
        else if (id == weaverspaceAnchor) return SoundIDs.weaverspaceAnchor;
        else if (id == intersticeAnchor) return SoundIDs.intersticeAnchor;
        return null;
    }

    private static int GetConversationFile(ID id)
    {
        return -1;
    }
}
