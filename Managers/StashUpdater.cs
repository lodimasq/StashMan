using System;
using System.Linq;
using StashMan.Models;

namespace StashMan.Managers;

public class StashUpdater(StashManager stashManager, ItemManager itemManager)
{
    public void RefreshStashData()
    {
        var currentTabDataFromMemory = ReadTabsFromMemory();
        if (currentTabDataFromMemory == null)
            return;

        foreach (var memoryTab in currentTabDataFromMemory)
        {
            var existingTab = stashManager.GetTabByName(memoryTab.Name);
            if (existingTab != null)
            {
                CheckAndUpdateTabProps(existingTab, memoryTab);
                CheckAndUpdateItemList(existingTab, memoryTab);
            }
            else
            {
                existingTab = stashManager.GetTabByIndex(memoryTab.Index);
                if (existingTab != null)
                {
                    stashManager.RenameTab(existingTab, memoryTab.Name);
                    CheckAndUpdateTabProps(existingTab, memoryTab);
                }
                else
                {
                    stashManager.AddTab(memoryTab.Index, memoryTab.Name, memoryTab.Type, memoryTab.GridSize);
                }
            }
        }

        var tabsToRemove = stashManager.Tabs
            .Where(et => currentTabDataFromMemory.All(mt => mt.Name != et.Name))
            .ToList();

        foreach (var tab in tabsToRemove)
        {
            stashManager.RemoveTab(tab);
        }

        stashManager.ReorderTabs();
    }

    private StashTab[] ReadTabsFromMemory()
    {
        var inventories = StashManCore.Main.GameController.Game.IngameState.IngameUi.StashElement?.Inventories;
        if (inventories != null)
        {
            return inventories
                .Where(inventory => !inventory.TabName.Contains("(Unavailable)"))
                .Select((inventory, index) =>
                    new StashTab(index, inventory.TabName,
                            inventory.Inventory?.InvType.ToString() ?? "Unknown",
                            inventory.Inventory?.TotalBoxesInInventoryRow ?? 0)
                        { Items = [] })
                .ToArray();
        }

        Console.WriteLine("[StashUpdater] Stash not visible or memory not accessible.");
        return null;
    }

    private void CheckAndUpdateTabProps(StashTab existingTab, StashTab memoryTab)
    {
        foreach (var prop in existingTab.GetType().GetProperties()
                     .Where(p => p.Name != "Items" && p.CanWrite && p.SetMethod != null))
        {
            if (prop.GetValue(existingTab) != prop.GetValue(memoryTab))
            {
                stashManager.UpdateTabProperty(existingTab, memoryTab, prop);
            }
        }
    }

    private void CheckAndUpdateItemList(StashTab existingTab, StashTab memoryTab)
    {
        foreach (var memItem in memoryTab.Items)
        {
            var existingItem = itemManager.GetItemByHash(existingTab, memItem.UniqueHash);
            if (existingItem == null)
            {
                itemManager.AddOrRecoverItem(existingTab, memItem);
            }
            else
            {
                itemManager.UpdateItem(existingTab, existingItem, memItem);
            }
        }

        var memoryHashSet = memoryTab.Items.Select(i => i.UniqueHash).ToHashSet();
        var toRemove = existingTab.Items
            .Where(ei => !memoryHashSet.Contains(ei.UniqueHash))
            .ToList();

        foreach (var rem in toRemove)
        {
            itemManager.RemoveItem(existingTab, rem);
        }
    }
}