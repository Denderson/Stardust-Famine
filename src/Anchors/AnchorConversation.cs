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
        if (id == null) return null;
        if (id == deeperspaceAnchor) return SoundIDs.deeperspaceAnchor;
        if (id == ripplespaceAnchor) return SoundIDs.ripplespaceAnchor;
        if (id == carnalplaneAnchor) return SoundIDs.carnalplaneAnchor;
        if (id == karmaspaceAnchor) return SoundIDs.karmaspaceAnchor;
        if (id == mindspaceAnchor) return SoundIDs.mindspaceAnchor;
        if (id == weaverspaceAnchor) return SoundIDs.weaverspaceAnchor;
        if (id == intersticeAnchor) return SoundIDs.intersticeAnchor;
        return null;
    }

    private static int GetConversationFile(ID id)
    {
        return -1;
    }
}
