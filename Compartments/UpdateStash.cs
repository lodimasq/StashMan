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

internal static class UpdateStash
{
    private const string DefaultTabType = "Unknown";

    public static void InitStashTabNameCoRoutine()
    {
        TaskRunner.Run(StashTabNamesUpdater_Thread, StashTabsNameChecker);
    }

    private static void UpdateStashNames()
    {
        var inventories = Main.GameController.Game.IngameState.IngameUi.StashElement.StashTabContainer.Inventories;
        if (inventories == null) return;

        var newTabs = inventories.Select((inventory, index) =>
            new StashTab(index, inventory.TabName, inventory.Inventory?.InvType.ToString() ?? DefaultTabType)
                { Items = new List<StashItem>() }).ToList();
        
        newTabs.RemoveAll(t => t.Name.Contains("(Unavailable)"));

        var storedTabs = Main.Settings.StashData.Tabs;

        // 1) Quick check for duplicates in new tabs by name
        if (newTabs.GroupBy(t => t.Name).Any(g => g.Count() > 1))
        {
            Main.LogMessage("Duplicate stash names detected in new data.");
            Main.Settings.DuplicateStashError = true;
            return;
        }

        // 2) Build dictionaries from old data
        var oldTabsByName = storedTabs.ToDictionary(t => t.Name);
        var oldTabsByIndex = storedTabs.ToDictionary(t => t.Index);

        // 3) Process each new tab
        foreach (var newTab in newTabs)
        {
            // Try matching by NAME first
            if (oldTabsByName.TryGetValue(newTab.Name, out var oldTabByName))
            {
                // Update properties (Index, Type, Items, etc.)
                UpdateExistingTab(oldTabByName, newTab);
            }
            else
            {
                // If the name doesn't match, let's see if there's an old tab with the same INDEX
                if (oldTabsByIndex.TryGetValue(newTab.Index, out var oldTabByIndex))
                {
                    // We found a tab by the same INDEX but different name => it's a rename event
                    var oldName = oldTabByIndex.Name;
                    var newName = newTab.Name;

                    Main.LogMessage($"Detected rename from '{oldName}' to '{newName}' at index {newTab.Index}.");

                    // Remove oldTabByIndex from the name dictionary so we can re-insert with new name
                    oldTabsByName.Remove(oldName);

                    // Update the tab's name
                    oldTabByIndex.Name = newName;

                    // Update other properties if needed
                    UpdateExistingTab(oldTabByIndex, newTab);

                    // Now add it back to oldTabsByName with the new name
                    oldTabsByName[newName] = oldTabByIndex;
                }
                else
                {
                    // It's neither found by name nor index => treat as brand-new
                    storedTabs.Add(newTab);
                    Main.LogMessage($"New stash tab: {newTab.Name} (Index={newTab.Index}).");

                    // Also update dictionaries (optional, if we want to keep them “live”)
                    oldTabsByName[newTab.Name] = newTab;
                    oldTabsByIndex[newTab.Index] = newTab;
                }
            }
        }

        // 4) Remove old tabs that no longer exist in the new set
        var newIndexes = newTabs.Select(t => t.Index).ToHashSet();
        storedTabs.RemoveAll(oldTab => !newIndexes.Contains(oldTab.Index));
        
        // 5) re-sort the tabs by index
        storedTabs.Sort((a, b) => a.Index.CompareTo(b.Index));
    }

    private static void UpdateExistingTab(StashTab oldTab, StashTab newTab)
    {
        // If the old index is different, update it (though in theory we matched by index, so it might not differ)
        if (oldTab.Index != newTab.Index)
        {
            Main.LogMessage($"Tab '{oldTab.Name}' index changed from {oldTab.Index} to {newTab.Index}.");
            oldTab.Index = newTab.Index;
        }

        // Update type if the new type is known
        if (!string.IsNullOrEmpty(newTab.Type) && newTab.Type != "Unknown" && oldTab.Type != newTab.Type)
        {
            Main.LogMessage($"Tab '{oldTab.Name}' type changed from {oldTab.Type} to {newTab.Type}.");
            oldTab.Type = newTab.Type;
        }

        // Compare and update Items if they differ
        // ...
    }


    private static void InitStashTabs()
    {
        var inventories = Main.GameController.Game.IngameState?.IngameUi?.StashElement.StashTabContainer.Inventories;

        if (inventories == null) return;

        if (inventories.DistinctBy(t => t.TabName).Count() != inventories.Count)
        {
            Main.LogMessage("Duplicate stash names detected.");
            Main.Settings.DuplicateStashError = true;
        }

        var tabs = inventories.Select((inventory, index) =>
            new StashTab(index, inventory.TabName, inventory.Inventory?.InvType.ToString() ?? DefaultTabType)
                { Items = new List<StashItem>() }).ToList();
        
        tabs.RemoveAll(t => t.Name.Contains("(Unavailable)"));

        Main.Settings.StashData.Tabs = tabs;
        Main.LogMessage($"Initialized {tabs.Count} stash tabs.");
    }

    private static bool HaveTabsChanged(IList<string> stashNames, IList<string> newNames)
    {
        return !stashNames.SequenceEqual(newNames);
    }

    private static async SyncTask<bool> StashTabNamesUpdater_Thread()
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
                Main.LogMessage("Waiting for stash panel...");
                await Task.Delay(1000);
                continue;
            }

            var stashNames = Main.Settings.StashData?.GetAllStashNames() ?? new List<string>();
            if (stashNames.Count == 0)
            {
                try
                {
                    InitStashTabs();
                    Main.LogMessage("Stash tab names initialized.");
                    await Task.Delay(1000);
                    continue;
                }
                catch (Exception e)
                {
                    Main.LogError("Failed to initialize stash tabs: " + e);
                    throw;
                }
            }

            var newNames = stashPanel.Inventories.Select(t => t.TabName).ToList();
           
            // remove names with "(Unavailable)"
            newNames.RemoveAll(n => n.Contains("(Unavailable)"));

            if (HaveTabsChanged(stashNames, newNames))
            {   
                try
                {
                    UpdateStashNames();
                    Main.LogMessage("Stash tab names have changed.");
                    await Task.Delay(1000);
                    continue;
                }
                catch (Exception e)
                {
                    Main.LogError("Failed to update stash tabs: " + e);
                    throw;
                }
                
            }

            Main.LogMessage("Stash tab names have not changed.");
            await Task.Delay(1000);
        }
    }
}