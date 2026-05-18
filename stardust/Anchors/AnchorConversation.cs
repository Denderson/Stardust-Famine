using HUD;
using Stardust.CWTs;
using System.Collections.Generic;
using UnityEngine;
using Watcher;
using static Stardust.Enums;
using static Stardust.Enums.ConversationIDs;
using static Stardust.Plugin;

namespace Stardust.Anchors;

public class AnchorDialogueBoxes
{
    public readonly List<DialogBox> Dialogs = new();

    public DialogBox MainBox => Dialogs.Count > 0 ? Dialogs[0] : null;

    public DialogBox CreateBox(HUD.HUD hud, Vector2 positionOffset)
    {
        DialogBox box = new(hud);
        if (DialogBoxCWT.TryGetData(box, out var data))
        {
            data.isAnchorBox = true;
            data.positionOffset = positionOffset;
        }

        hud.parts.Add(box);
        Dialogs.Add(box);

        return box;
    }

    public void RemoveBox(HUD.HUD hud, DialogBox box)
    {
        if (box == null) return;

        box.ClearSprites();
        hud.parts.Remove(box);
        Dialogs.Remove(box);
    }

    public void Clear(HUD.HUD hud)
    {
        for (int i = 0; i < Dialogs.Count; i++)
        {
            DialogBox box = Dialogs[i];
            box.ClearSprites();
            hud.parts.Remove(box);
        }
        Dialogs.Clear();
    }
}

public class AnchorConversation : Conversation
{
    public AnchorDialogueBoxes DialogueBoxes { get; set; }

    public SoundID Voiceline { get; private set; }

    public AnchorConversation(
        IOwnAConversation interfaceOwner,
        ID id,
        DialogBox dialogBox)

        : base(interfaceOwner, id, dialogBox)
    {
        DialogueBoxes = new();

        Voiceline = GetVoiceline(id);

        AddEvents();

        TestDialogueSystem(dialogBox.hud);
    }

    public override void AddEvents()
    {
        LoadEventsFromFile(GetConversationFile(id));
    }
    private void TestDialogueSystem(HUD.HUD hud)
    {
        DialogueBoxes.Dialogs.Add(dialogBox);

        DialogBox leftBox = DialogueBoxes.CreateBox(hud, new Vector2(-400f, 120f));
        leftBox.NewMessage("Greetings",0);

        DialogBox rightBox = DialogueBoxes.CreateBox(hud, new Vector2(400f, -50f));

        rightBox.NewMessage("Hello", 0);

        DialogBox topBox = DialogueBoxes.CreateBox(hud, new Vector2(0f, 300f));

        topBox.NewMessage("Howdy", 0);
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