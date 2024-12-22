using System;
using System.Collections.Generic;
using System.Diagnostics;
using ExileCore2;
using ImGuiNET;
using StashMan.Classes;
using StashMan.Compartments;
using Vector2N = System.Numerics.Vector2;

namespace StashMan;

public class StashManCore : BaseSettingsPlugin<StashManSettings>
{
    public const string CoroutineName = "Drop To Stash";
    public const string StashTabsNameChecker = "Stash Tabs Name Checker";
    public static StashManCore Main;

    public readonly Stopwatch DebugTimer = new();

    public StashManCore()
    {
        Name = "StashMan";
    }

    public override bool Initialise()
    {
        Main = this;
        Settings.StashData.Tabs.Clear();
        Settings.Enable.OnValueChanged += (sender, b) =>
        {
            if (b)
                UpdateStash.InitStashTabNameCoRoutine();
            else
                TaskRunner.Stop(StashTabsNameChecker);

            // Utility.SetupOrClose();
        };

        UpdateStash.InitStashTabNameCoRoutine();
        // Utility.SetupOrClose();

        return true;
    }

    public override void OnUnload()
    {
        try
        {
            TaskRunner.Stop(StashTabsNameChecker);
        }
        catch (Exception e)
        {
            
        }
        base.OnUnload();
    }

    public override void Render()
    {
        if (!Settings.Enable.Value) return;

        // Draw a debug window

        ImGui.SetNextWindowSize(new Vector2N(300, 200), ImGuiCond.FirstUseEver);
        ImGui.Begin("StashMan Debug");
        ImGui.Text(TaskRunner.Has(StashTabsNameChecker)
            ? "Stash Tabs Name Checker is running."
            : "Stash Tabs Name Checker is not running.");
        //list all stash names
        if (Settings.StashData.GetAllStashNames() != null)
            foreach (var name in Settings.StashData.GetAllStashNames())
                ImGui.Text(name);

        //if duplicate stash error show text and button to retry
        if (Settings.DuplicateStashError)
        {
            ImGui.Text("Duplicate stash names found. Please retry.");
            if (ImGui.Button("Retry"))
            {
                Settings.DuplicateStashError = false;
            }
        }

        ImGui.End();
    }

    public override void DrawSettings()
    {
        base.DrawSettings();
    }

    public override void ReceiveEvent(string eventId, object args)
    {
        if (!Settings.Enable.Value) return;

        switch (eventId)
        {
            case "switch_to_tab":
                ActionsHandler.HandleSwitchToTabEvent(args);
                break;

            case "start_stashie":
                if (TaskRunner.Has(CoroutineName)) ActionCoRoutine.StartDropItemsToStashCoroutine();

                break;
        }
    }

    public override void AreaChange(AreaInstance area)
    {
        // if (area.IsHideout || area.IsTown)
        //     UpdateStash.InitStashTabNameCoRoutine();
        // else
        //     TaskRunner.Stop(StashTabsNameChecker);
    }

    public override void Tick()
    {
    }

    public bool StashingRequirementsMet()
    {
        return GameController.Game.IngameState.IngameUi.InventoryPanel.IsVisible &&
               GameController.Game.IngameState.IngameUi.StashElement.IsVisibleLocal;
    }
}