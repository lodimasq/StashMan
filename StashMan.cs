using System;
using System.Collections.Generic;
using System.Diagnostics;
using ExileCore2;
using ImGuiNET;
using StashMan.Classes;
using StashMan.Compartments;
using StashMan.Filter;
using Vector2N = System.Numerics.Vector2;

namespace StashMan;

public class StashManCore : BaseSettingsPlugin<StashManSettings>
{
    public const string CoroutineName = "Drop To Stash";
    public const string StashTabsNameChecker = "Stash Tabs Name Checker";
    public static StashManCore Main;

    public static List<string> RenamedAllStashNames;
    public readonly Stopwatch DebugTimer = new();
    public Vector2N ClickWindowOffset;

    public List<CustomFilter> currentFilter;
    public List<FilterResult> DropItems;
    public Action FilterTabs;
    public bool IsFilterEditorTab;
    public List<ListIndexNode> SettingsListNodes;
    public string[] StashTabNamesByIndex;
    public int VisibleStashIndex = -1;

    public StashManCore()
    {
        Name = "StashMan";
    }

    public override bool Initialise()
    {
        Main = this;
        Settings.Enable.OnValueChanged += (sender, b) =>
        {
            if (b)
                StashTabNameCoRoutine.InitStashTabNameCoRoutine();
            else
                TaskRunner.Stop(StashTabsNameChecker);

            Utility.SetupOrClose();
        };


        StashieEditorHandler.FileSaveName = Settings.ConfigLastSaved;
        StashieEditorHandler.SelectedFileName = Settings.ConfigLastSaved;

        StashTabNameCoRoutine.InitStashTabNameCoRoutine();
        Utility.SetupOrClose();

        Input.RegisterKey(Settings.DropHotkey);

        Settings.DropHotkey.OnValueChanged += () => { Input.RegisterKey(Settings.DropHotkey); };
        Settings.FilterFile.OnValueSelected = _ => FilterManager.LoadCustomFilters();

        return true;
    }

    public override void Render()
    {
        if (!Settings.Enable.Value) return;

        // Draw a debug window

        ImGui.SetNextWindowSize(new Vector2N(300, 200), ImGuiCond.FirstUseEver);
        ImGui.Begin("StashMan Debug");
        ImGui.Text(Settings.AllStashNames.Count > 0
            ? $"Stash Names: {string.Join(", ", Settings.AllStashNames)}"
            : "No Stash Names");
    }

    public override void DrawSettings()
    {
        base.DrawSettings();

        Settings.ConfigLastSaved = StashieEditorHandler.FileSaveName;
        Settings.ConfigLastSelected = StashieEditorHandler.SelectedFileName;
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
        if (area.IsHideout || area.IsTown)
            StashTabNameCoRoutine.InitStashTabNameCoRoutine();
        else
            TaskRunner.Stop(StashTabsNameChecker);
    }

    public override void Tick()
    {
        if (!StashingRequirementsMet())
        {
            TaskRunner.Stop("Stashie_DropItemsToStash");
            return;
        }

        if (!Settings.DropHotkey.PressedOnce())
            return;

        if (TaskRunner.Has("Stashie_DropItemsToStash"))
            ActionCoRoutine.StopCoroutine("Stashie_DropItemsToStash");
        else
            ActionCoRoutine.StartDropItemsToStashCoroutine();
    }

    public bool StashingRequirementsMet()
    {
        return GameController.Game.IngameState.IngameUi.InventoryPanel.IsVisible &&
               GameController.Game.IngameState.IngameUi.StashElement.IsVisibleLocal;
    }
}