using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.Shared;
using StashMan.Classes;
using static StashMan.StashManCore;

namespace StashMan.Compartments;

internal class UpdateStash
{
    private const string DefaultTabType = "Unknown";

    public static void InitStashTabNameCoRoutine()
    {
        TaskRunner.Run(StashTabNamesUpdater_Thread, StashTabsNameChecker);
    }

    public static void UpdateStashTabs(StashElement stashPanel)
    {
        var stashNames = Main.Settings.StashData.GetAllStashNames();
        var newNames = stashPanel.AllStashNames;

        if (stashNames.SequenceEqual(newNames))
        {
            Main.LogMsg("No changes detected in stash tabs.");
            return;
        }

        // Create a lookup for efficient tab updates
        var stashTabsByName = Main.Settings.StashData.Tabs.ToDictionary(tab => tab.Name);

        var updatedTabs = new List<StashTab>();
        var seenNames = new HashSet<string>(); // Track duplicates during update
        var serverStashTabs = Main.GameController.IngameState.ServerData.PlayerStashTabs;

        for (var index = 0; index < newNames.Count; index++)
        {
            var name = newNames[index];

            // Detect duplicates in new names
            if (!seenNames.Add(name))
            {
                Main.LogMsg($"Duplicate stash name detected during update: {name}. Update aborted.");
                Main.Settings.DuplicateStashError = true;
                return;
            }

            if (stashTabsByName.TryGetValue(name, out var existingTab))
            {
                // Reorder existing tab
                existingTab.Index = index;
                updatedTabs.Add(existingTab);
            }
            else
            {
                // Add new tab
                updatedTabs.Add(new StashTab(index, name,
                    serverStashTabs.FirstOrDefault(tab => tab.Name == name)?.TabType.ToString() ?? DefaultTabType)
                {
                    Items = new List<StashItem>()
                });
            }
        }

        // Update stash data only if no duplicates were found
        Main.Settings.StashData.Tabs = updatedTabs;
        Main.LogMsg($"Updated stash tabs. Total: {updatedTabs.Count}");
    }


    public static void InitStashTabs(StashElement stashPanel)
    {
        var stashNames = stashPanel.AllStashNames;

        if (stashNames.Distinct().Count() != stashNames.Count)
        {
            Main.LogMsg("Duplicate stash names detected. Initialization aborted.");
            Main.Settings.StashData.Tabs.Clear();
            Main.Settings.DuplicateStashError = true;
            return;
        }


        var serverStashTabs = Main.GameController.IngameState.ServerData.PlayerStashTabs;
        var tabs = stashNames.Select((name, index) => new StashTab(index, name,
            serverStashTabs.FirstOrDefault(tab => tab.Name == name)?.TabType.ToString() ?? DefaultTabType)
        {
            Items = new List<StashItem>()
        }).ToList();

        Main.Settings.StashData.Tabs = tabs;
        Main.LogMsg($"Initialized {tabs.Count} stash tabs.");
    }

    private static bool HaveTabsChanged(IList<string> stashNames, IList<string> newNames)
    {
        return !stashNames.SequenceEqual(newNames);
    }

    public static async SyncTask<bool> StashTabNamesUpdater_Thread()
    {
        while (true)
        {
            if (Main.Settings.DuplicateStashError)
            {
                await Task.Delay(1000);
                continue;
            }

            var stashPanel = Main.GameController.Game.IngameState?.IngameUi?.StashElement;
            if (stashPanel == null || !stashPanel.IsVisibleLocal)
            {
                Main.LogMsg("Waiting for stash panel...");
                await Task.Delay(1000);
                continue;
            }

            var stashNames = Main.Settings.StashData.GetAllStashNames();
            if (stashNames.Count == 0)
            {
                InitStashTabs(stashPanel);
                continue;
            }

            var newNames = stashPanel.AllStashNames;

            if (HaveTabsChanged(stashNames, newNames))
            {
                UpdateStashTabs(stashPanel);
                continue;
            }

            await Task.Delay(1000);
        }
    }
}