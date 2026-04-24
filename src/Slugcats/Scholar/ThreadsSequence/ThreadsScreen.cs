using System;
using System.Collections.Generic;
using Menu;
using RWCustom;
using SlugBase.SaveData;
using Stardust;
using UnityEngine;
using static Stardust.Plugin;

public class ThreadsScreen : global::Menu.Menu
{
    public SimpleButton exitButton;

    public List<SimpleButton> backupButtons;

    public MenuLabel messageLabel;

    public MenuLabel noticeLabel;

    public int counter;

    public bool active;

    public override bool ForceNoMouseMode
    {
        get
        {
            if (exitButton != null && !(exitButton.black > 0.5f))
            {
                return base.ForceNoMouseMode;
            }
            return true;
        }
    }

    public override bool FreezeMenuFunctions
    {
        get
        {
            if (!base.FreezeMenuFunctions)
            {
                return counter < 20;
            }
            return true;
        }
    }

    public ThreadsScreen(ProcessManager manager) : base(manager, Enums.ProcessIDs.threadsProcess)
    {
        pages.Add(new Page(this, null, "main", 0));
        if (manager.musicPlayer != null)
        {
            manager.musicPlayer.FadeOutAllSongs(30f);
        }
        mySoundLoopID = SoundID.MENU_Dream_LOOP;
        active = false;
        backupButtons = new List<SimpleButton>();
        

            /*messageLabel = new MenuLabel(this, pages[0], "Threads sequence test", new Vector2(base.manager.rainWorld.options.ScreenSize.x * 0.5f - 50f + (1366f - base.manager.rainWorld.options.ScreenSize.x) / 2f, base.manager.rainWorld.options.ScreenSize.y * 0.5f - 25f), new Vector2(100f, 30f), bigText: true);
            messageLabel.label.color = new Color(1f, 1f, 1f);
            messageLabel.label.alignment = FLabelAlignment.Center;
            noticeLabel = new MenuLabel(this, pages[0], Translate("[ Notice ]"), new Vector2(base.manager.rainWorld.options.ScreenSize.x * 0.5f - 50f + (1366f - base.manager.rainWorld.options.ScreenSize.x) / 2f, base.manager.rainWorld.options.ScreenSize.y * 0.5f + 50f), new Vector2(100f, 30f), bigText: true);
            noticeLabel.label.color = new Color(1f, 1f, 1f);
            noticeLabel.label.alignment = FLabelAlignment.Center;
            pages[0].subObjects.Add(messageLabel);
            pages[0].subObjects.Add(noticeLabel);*/
    }

    public override void Update()
    {
        base.Update();
        counter++;
        if (counter == 40)
        {
            PlaySound(SoundID.MENU_Dream_Init);
        }
        if (counter == 80)
        {
            active = true;
            exitButton = new SimpleButton(this, pages[0], Translate("EXIT"), "EXIT", new Vector2(manager.rainWorld.options.ScreenSize.x * 0.9f - 110f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 15f), new Vector2(110f, 30f));
            pages[0].subObjects.Add(exitButton);
            pages[0].lastSelectedObject = exitButton;
            exitButton.black = 1f;

            SaveState currentSaveState = manager.rainWorld.progression.GetOrInitiateSaveState(Enums.SlugcatStatsName.sfscholar, null, manager.menuSetup, saveAsDeathOrQuit: false);
            if (manager.rainWorld.progression.IsThereASavedGame(Enums.SlugcatStatsName.sfscholar))
            {
                for (int i = 0; i < 6; i++)
                {
                    if (currentSaveState?.deathPersistentSaveData?.GetBackup(i) != null)
                    {
                        Log.LogMessage("Backup " + i);
                        Log.LogMessage(currentSaveState?.deathPersistentSaveData?.GetBackup(i) != null);
                        SimpleButton newButton = new(this, pages[0], "BACKUP-" + i, "BACKUP-" + i, new Vector2(manager.rainWorld.options.ScreenSize.x * 0.5f + 120f * (i - 3), manager.rainWorld.options.ScreenSize.y * 0.5f), new Vector2(110f, 30f))
                        {
                            black = 1f
                        };
                        backupButtons.Add(newButton);
                        pages[0].subObjects.Add(newButton);
                    }
                }
            }
        }
        manager.fadeToBlack = Custom.LerpAndTick(manager.fadeToBlack, 0f, 0f, 0.0125f);
        if (active)
        {
            exitButton.buttonBehav.greyedOut = FreezeMenuFunctions;
            exitButton.black = Math.Max(0f, exitButton.black - 0.005f);
            foreach (SimpleButton button in backupButtons)
            {
                if (button != null)
                {
                    button.black = Math.Max(0f, button.black - 0.005f);
                    button.buttonBehav.greyedOut = FreezeMenuFunctions;
                }
            }
        }
    }

    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
    }

    public override void Singal(MenuObject sender, string message)
    {
        string Message = message.ToLowerInvariant();
        if (Message.Contains("backup"))
        {
            int backupNumber = Int32.Parse(message.Split('-')[1]);

            manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = Enums.SlugcatStatsName.sfscholar;

            Log.LogMessage("Loading backups");
            if (manager.rainWorld.progression.IsThereASavedGame(Enums.SlugcatStatsName.sfscholar))
            {

                SaveState save = manager.rainWorld.progression.GetOrInitiateSaveState(Enums.SlugcatStatsName.sfscholar, null, manager.menuSetup, saveAsDeathOrQuit: false);
                string saveToLoad = " ";
                if (save?.deathPersistentSaveData?.GetBackup(backupNumber) != null)
                {
                    Log.LogMessage("Backup " + backupNumber + " exists");
                    save.deathPersistentSaveData.SetInt(SaveFileCode.backupToUse, backupNumber);
                    saveToLoad = save.deathPersistentSaveData.GetBackup(backupNumber);
                    Log.LogMessage("Save to load: " + saveToLoad);
                    manager.rainWorld.progression.currentSaveState = save;
                    Log.LogMessage("Loading backup");
                    manager.rainWorld.progression.currentSaveState.LoadGame(saveToLoad, null);
                    Log.LogMessage(manager.rainWorld.progression.currentSaveState.SaveToString());
                    for (int i = 0; i < 6; i++)
                    {
                        //manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.GetSlugBaseData().Set<string>(SaveFileCode.backup + i, saveToLoad);
                    }
                }
                else Log.LogMessage("Backup " + backupNumber + " does not exist");
            }
            else manager.rainWorld.progression.currentSaveState = manager.rainWorld.progression.GetOrInitiateSaveState(Enums.SlugcatStatsName.sfscholar, null, manager.menuSetup, saveAsDeathOrQuit: false);
            if (manager.musicPlayer?.song != null) manager.musicPlayer.song.FadeOut(20f);
            manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
            PlaySound(SoundID.MENU_Dream_Button);
        }
        if (Message.Contains("exit"))
        {
            manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
            PlaySound(SoundID.MENU_Dream_Button);
        }
    }
}
