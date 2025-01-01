using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.Elements.InventoryElements;
using ExileCore2.Shared;
using ExileCore2.Shared.Static;
using Microsoft.VisualBasic.Logging;
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

        var newTabs = inventories
            .Where(inventory => !inventory.TabName.Contains("(Unavailable)"))
            .Select((inventory, index) =>
                new StashTab(index, inventory.TabName, inventory.Inventory?.InvType.ToString() ?? DefaultTabType,
                        inventory.Inventory?.TotalBoxesInInventoryRow ?? 0)
                    { Items = [] })
            .ToList();

        var storedTabs = Main.Settings.StashData.Tabs;

        // Quick check for duplicates in new tabs by name
        if (newTabs.GroupBy(t => t.Name).Any(g => g.Count() > 1))
        {
            Main.LogMessage("Duplicate stash names detected in new data.");
            Main.Settings.DuplicateStashError = true;
            return;
        }

        // Build dictionaries from old data
        var oldTabsByName = storedTabs.ToDictionary(t => t.Name);
        var oldTabsByIndex = storedTabs.ToDictionary(t => t.Index);

        // Process each new tab
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

        oldTab.LastUpdatedDateTime = DateTime.Now;
    }


    private static void InitStashTabs()
    {
        var inventories = Main.GameController.Game.IngameState?.IngameUi?.StashElement?.StashTabContainer?.Inventories;
        if (inventories == null) return;

        // Check for duplicate stash names
        if (inventories.DistinctBy(t => t.TabName).Count() != inventories.Count)
        {
            Main.LogMessage("Duplicate stash names detected.");
            Main.Settings.DuplicateStashError = true;
        }

        var tabs = inventories
            // Skip any stash tab labeled "(Unavailable)"
            .Where(inventory => !inventory.TabName.Contains("(Unavailable)"))
            .Select((inventory, index) =>
            {
                // Build an array of items for this stash tab, ensuring it's never null
                var visibleItems = inventory.Inventory?.VisibleInventoryItems?
                                       .Select(item =>
                                       {
                                           // If we can’t safely get ‘Base’ or ‘Stack’ components, skip or handle them
                                           var baseComponent =
                                               item.Entity.GetComponent<ExileCore2.PoEMemory.Components.Base>();
                                           var stackComponent =
                                               item.Entity.GetComponent<ExileCore2.PoEMemory.Components.Stack>();

                                           if (baseComponent?.Info?.BaseItemTypeDat == null || stackComponent == null)
                                               return null;

                                           var rect = item.GetClientRectCache;

                                           return new StashItem(
                                               baseComponent.Info.BaseItemTypeDat.BaseName,
                                               baseComponent.Info.BaseItemTypeDat.ClassName,
                                               price: 0,
                                               quantity: stackComponent.Size,
                                               isFullStack: stackComponent.FullStack,
                                               new ItemPosition(
                                                   item.ItemHeight,
                                                   item.ItemWidth,
                                                   item.Height,
                                                   item.Width,
                                                   rect.TopLeft,
                                                   rect.BottomRight
                                               )
                                           );
                                       })
                                       // Filter out any nulls returned by the selector
                                       .Where(x => x != null)
                                       // Convert to array
                                       .ToArray()
                                   // If everything was null or the chain was null, fallback to an empty array
                                   ?? [];

                return new StashTab(
                    index,
                    inventory.TabName,
                    inventory.Inventory?.InvType.ToString() ?? DefaultTabType,
                    inventory.Inventory?.TotalBoxesInInventoryRow ?? 0
                )
                {
                    // Safely assign the non-null array to the Items list
                    Items = [..visibleItems]
                };
            })
            // Finally, build the list of all stash tabs
            .ToList();

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
                // Main.LogMessage("Waiting for stash panel...");
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

            var newNames = stashPanel.Inventories
                .Select(t => t.TabName)
                .Where(name => !name.Contains("(Unavailable)"))
                .ToList();

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

            // Main.LogMessage("Stash tab names have not changed.");
            await Task.Delay(1000);
        }
    }
}