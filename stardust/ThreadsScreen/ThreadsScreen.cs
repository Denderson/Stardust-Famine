using System;
using System.Collections.Generic;
using Menu;
using RWCustom;
using SlugBase.SaveData;
using Stardust.SaveFile;
using UnityEngine;
using static Stardust.Plugin;

namespace Stardust.ThreadsScreen;

public class ThreadsScreen : Menu.Menu
{
    public SimpleButton exitButton;

    public List<HoldButton> backupButtons;

    public MenuLabel messageLabel;

    public MenuLabel noticeLabel;

    public ThreadSpiral threadSpiral;

    public ThreadFog threadFog;

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
        manager.musicPlayer?.FadeOutAllSongs(30f);
        mySoundLoopID = SoundID.MENU_Dream_LOOP;
        active = false;
        backupButtons = [];
    }

    public override void Update()
    {
        base.Update();
        if (counter < 80) counter++;
        if (counter == 10)
        {
            PlaySound(SoundID.MENU_Dream_Init);
        }
        if (counter == 20)
        {
            active = true;
            exitButton = new SimpleButton(this, pages[0], Translate("EXIT"), "EXIT", new Vector2(manager.rainWorld.options.ScreenSize.x * 0.9f - 110f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 25f), new Vector2(110f, 30f));
            pages[0].subObjects.Add(exitButton);
            pages[0].lastSelectedObject = exitButton;
            exitButton.black = 1f;

            threadFog = new ThreadFog(this, pages[0], new Vector2(manager.rainWorld.options.ScreenSize.x * 0.5f, manager.rainWorld.options.ScreenSize.y * 0.5f));
            pages[0].subObjects.Add(threadFog);

            threadSpiral = new ThreadSpiral(this, pages[0], new Vector2(manager.rainWorld.options.ScreenSize.x * 0.5f, manager.rainWorld.options.ScreenSize.y * 0.5f), 50f);
            pages[0].subObjects.Add(threadSpiral);
            

            SaveState currentSaveState = manager.rainWorld.progression.GetOrInitiateSaveState(Enums.SlugcatStatsName.sfscholar, null, manager.menuSetup, saveAsDeathOrQuit: false);
            if (manager.rainWorld.progression.IsThereASavedGame(Enums.SlugcatStatsName.sfscholar))
            {
                for (int i = 0; i < 6; i++)
                {
                    //if (currentSaveState?.deathPersistentSaveData?.GetBackup(i) != null)
                    /*{
                        Log.LogMessage($"Backup {i}");
                        Log.LogMessage(currentSaveState?.deathPersistentSaveData?.GetBackup(i) != null);

                        Vector2 slotPos;
                        if (i == 0)
                        {
                            slotPos = threadSpiral.pos; // manual center placement for slot 1
                        }
                        else
                        {
                            slotPos = threadSpiral.pos + GetPosOnSpiral(BackupPositions[i - 1]) * threadSpiral.sprite.width;
                        }

                        HoldButton holdButton = new(this, pages[0], Translate("RETRY<LINE>EXPEDITION").Replace("<LINE>", "\n"), "RETRY", slotPos, 100f);
                        pages[0].subObjects.Add(holdButton);
                        backupButtons.Add(holdButton);
                        Log.LogMessage($"Spawning {i} backup on {slotPos.x},{slotPos.y}");
                    }*/
                }
            }
        }
        manager.fadeToBlack = Custom.LerpAndTick(manager.fadeToBlack, 0f, 0f, 0.0125f);
        if (active)
        {
            exitButton.buttonBehav.greyedOut = FreezeMenuFunctions;
            exitButton.black = Math.Max(0f, exitButton.black - 0.005f);
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
            int backupNumber = int.Parse(message.Split('-')[1]);

            manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = Enums.SlugcatStatsName.sfscholar;

            Log.LogMessage("Loading backups");
            if (manager.rainWorld.progression.IsThereASavedGame(Enums.SlugcatStatsName.sfscholar))
            {
                SaveState save = manager.rainWorld.progression.GetOrInitiateSaveState(Enums.SlugcatStatsName.sfscholar, null, manager.menuSetup, saveAsDeathOrQuit: false);
                manager.rainWorld.progression.currentSaveState = save;
                if (save.deathPersistentSaveData.GetBackup(backupNumber) != null)
                {
                    Log.LogMessage($"Backup {backupNumber} exists");
                    save.LoadBackupAsMain(backupNumber);
                    manager.rainWorld.progression.SaveWorldStateAndProgression(malnourished: false);
                }
                else
                {
                    Log.LogMessage($"Backup {backupNumber} does not exist");
                }
            }
            else
            {
                manager.rainWorld.progression.currentSaveState = manager.rainWorld.progression.GetOrInitiateSaveState(Enums.SlugcatStatsName.sfscholar, null, manager.menuSetup, saveAsDeathOrQuit: false);
            }
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

    public static Vector2 GetPosOnSpiral(float armPos, float lineSpacing = 0.03f)
    {
        float r = lineSpacing * armPos;
        return new Vector2(r * Mathf.Cos(armPos), r * Mathf.Sin(armPos));
    }

    private const float QuarterTurn = Mathf.PI * 0.5f;

    private static readonly float[] BackupPositions =
    [
        QuarterTurn * 3f,
        QuarterTurn * 4f,
        QuarterTurn * 5f,
        QuarterTurn * 6f,
        QuarterTurn * 7f,
        QuarterTurn * 8f,
        QuarterTurn * 9f,
        QuarterTurn * 10f,
    ];
}