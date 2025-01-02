using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.Elements.InventoryElements;
using StashMan.Models;
using static StashMan.StashManCore;

namespace StashMan.Managers;

public class StashUpdater(StashManager stashManager, ItemManager itemManager)
{
    public void RefreshStashData()
    {
        var currentTabDataFromMemory = ReadTabsFromMemory();
        if (currentTabDataFromMemory == null)
        {
            Main.LogError("[StashUpdater] No stash data found in memory.");
            return;
        }

        foreach (var memoryTab in currentTabDataFromMemory)
        {
            try
            {
                ProcessMemoryTab(memoryTab);
            }
            catch (Exception ex)
            {
                Main.LogError($"[StashUpdater] Error processing tab '{memoryTab.Name}': {ex.Message}");
            }
        }

        RemoveObsoleteTabs(currentTabDataFromMemory);
        stashManager.ReorderTabs();
    }

    private void ProcessMemoryTab(StashTab memoryTab)
    {
        var existingTab = stashManager.GetTabByName(memoryTab.Name) ?? stashManager.GetTabByIndex(memoryTab.Index);

        if (existingTab != null)
        {
            if (existingTab.Name != memoryTab.Name)
                stashManager.RenameTab(existingTab, memoryTab.Name);

            CheckAndUpdateTabProps(existingTab, memoryTab);

            // visible inventory items bugged for currency tab
            if (memoryTab.Type != "CurrencyStash" || memoryTab.IsVisible == true)
            {
                CheckAndUpdateItemList(existingTab, memoryTab);
            }
        }
        else
        {
            stashManager.AddTab(memoryTab.Index, memoryTab.Name, memoryTab.Type);
        }
    }

    private void RemoveObsoleteTabs(IEnumerable<StashTab> currentTabDataFromMemory)
    {
        var tabsToRemove = stashManager.Tabs
            .Where(et => currentTabDataFromMemory.All(mt => mt.Name != et.Name))
            .ToList();

        foreach (var tab in tabsToRemove)
        {
            try
            {
                stashManager.RemoveTab(tab);
            }
            catch (Exception ex)
            {
                Main.LogError($"[StashUpdater] Error removing tab '{tab.Name}': {ex.Message}");
            }
        }
    }

    private static List<StashTab> ReadTabsFromMemory()
    {
        var inventories = Main.GameController.Game.IngameState.IngameUi.StashElement?.Inventories;
        if (inventories == null)
        {
            Main.LogError("[StashUpdater] Stash not visible or memory not accessible.");
            return null;
        }

        return inventories
            .Where(inventory => !inventory.TabName.Contains("(Unavailable)"))
            .Select((inventory, index) =>
            {
                try
                {
                    return new StashTab
                    {
                        Index = index,
                        Name = inventory.TabName,
                        Type = inventory.Inventory?.InvType.ToString() ?? "Unknown",
                        IsVisible = inventory.Inventory?.IsVisible ?? false,
                        Items = inventory.Inventory?.VisibleInventoryItems?.Select(CreateStashItem)
                            .Where(item => item != null).ToList() ?? []
                    };
                }
                catch (Exception ex)
                {
                    Main.LogError($"[StashUpdater] Error reading tab '{inventory.TabName}': {ex.Message}");
                    return null;
                }
            })
            .Where(tab => tab != null)
            .ToList();
    }

    private static StashItem CreateStashItem(NormalInventoryItem item)
    {
        try
        {
            var baseComponent = item.Entity.GetComponent<Base>();
            var stackComponent = item.Entity.GetComponent<Stack>();

            var newItem = new StashItem
            {
                BaseName = baseComponent.Info.BaseItemTypeDat.BaseName,
                ClassName = baseComponent.Info.BaseItemTypeDat.ClassName,
                Quantity = stackComponent?.Size ?? 1,
                Position = new ItemPosition
                {
                    GridHeight = item.ItemHeight,
                    GridWidth = item.ItemWidth,
                    Height = item.Height,
                    Width = item.Width,
                    TopLeft = item.GetClientRect().TopLeft,
                    BottomRight = item.GetClientRect().BottomRight
                }
            };
            newItem.UniqueHash = ItemHasher.GenerateHash(newItem);
            return newItem;
        }
        catch (Exception ex)
        {
            Main.LogError($"[StashUpdater] Error creating item: {ex.Message}");
            return null;
        }
    }

    private void CheckAndUpdateTabProps(StashTab existingTab, StashTab memoryTab)
    {
        foreach (var prop in existingTab.GetType().GetProperties()
                     .Where(p => p.CanWrite && p.Name != "Items" && p.Name != "LastUpdatedDateTime"))
        {
            try
            {
                if (!Equals(prop.GetValue(existingTab), prop.GetValue(memoryTab)))
                {
                    stashManager.UpdateTabProperty(existingTab, memoryTab, prop);
                }
            }
            catch (Exception ex)
            {
                Main.LogError(
                    $"[StashUpdater] Error updating property '{prop.Name}' for tab '{existingTab.Name}': {ex.Message}");
            }
        }
    }


    private void CheckAndUpdateItemList(StashTab existingTab, StashTab memoryTab)
    {
        foreach (var memItem in memoryTab.Items)
        {
            try
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
            catch (Exception ex)
            {
                Main.LogError(
                    $"[StashUpdater] Error processing item '{memItem.BaseName}' in tab '{existingTab.Name}': {ex.Message}");
            }
        }

        var memoryHashSet = memoryTab.Items.Select(i => i.UniqueHash).ToHashSet();
        var toRemove = existingTab.Items
            .Where(ei => !memoryHashSet.Contains(ei.UniqueHash))
            .ToList();

        foreach (var rem in toRemove)
        {
            try
            {
                itemManager.RemoveItem(existingTab, rem);
            }
            catch (Exception ex)
            {
                Main.LogError(
                    $"[StashUpdater] Error removing item '{rem.BaseName}' from tab '{existingTab.Name}': {ex.Message}");
            }
        }
    }
}