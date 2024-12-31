using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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
            Main.LogError(e.ToString());
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
        ImGui.SetNextWindowSize(new Vector2N(500, 400), ImGuiCond.FirstUseEver);
        ImGui.Begin("StashMan Debug");
        if (Settings.DuplicateStashError)
        {
            ImGui.Text("Duplicate stash name detected. Please fix the issue and retry.");
            if (ImGui.Button("Retry"))
            {
                Settings.DuplicateStashError = false;
                UpdateStash.InitStashTabNameCoRoutine();
            }
        }

        if (ImGui.BeginTable("StashTabsTable", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Type");
            ImGui.TableSetupColumn("Number of Items");
            ImGui.TableSetupColumn("Total Quantity");
            ImGui.TableSetupColumn("Last Sync");
            ImGui.TableHeadersRow();

            foreach (var tab in Main.Settings.StashData.Tabs.Where(t => t.Type != "Unknown1").OrderBy(t => t.Index).ToList())
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text(tab.Name);
                ImGui.TableSetColumnIndex(1);
                ImGui.Text(tab.Type);
                ImGui.TableSetColumnIndex(2);
                ImGui.Text(tab.Items.Count.ToString());
                ImGui.TableSetColumnIndex(3);
                ImGui.Text(tab.TotalItemQuantity.ToString());
                ImGui.TableSetColumnIndex(4);
                ImGui.Text(tab.LastGameSync.ToString("yy-MM-dd HH:mm:ss"));
                ImGui.TableSetColumnIndex(5);
            }

            ImGui.EndTable();
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