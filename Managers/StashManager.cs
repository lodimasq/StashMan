using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StashMan.Events;
using StashMan.Models;

namespace StashMan.Managers
{
    /// <summary>
    /// Manages CRUD operations for stash tabs:
    /// Adding, renaming, removing, and reordering.
    /// Fires appropriate events via StashEventManager.
    /// </summary>
    public class StashManager(Stash stash)
    {
        /// <summary>
        /// Tabs: List of all stash tabs.
        /// </summary>
        public List<StashTab> Tabs => stash.Tabs;

        /// <summary>
        /// Create and add a new tab to the stash.
        /// </summary>
        public StashTab AddTab(int index, string name, string type)
        {
            var newTab = new StashTab { Index = index, Name = name, Type = type };
            stash.Tabs.Add(newTab);

            // Optionally re-sort by index or keep them in the add order
            stash.Tabs = stash.Tabs.OrderBy(t => t.Index).ToList();

            StashEventManager.RaiseTabAdded(newTab);
            return newTab;
        }

        /// <summary>
        /// Rename an existing tab.
        /// </summary>
        public void RenameTab(StashTab tab, string newName)
        {
            var oldName = tab.Name;
            tab.Name = newName;
            StashEventManager.RaiseTabRenamed(tab, oldName);
        }

        /// <summary>
        /// Remove a tab from the stash.
        /// </summary>
        public void RemoveTab(StashTab tab)
        {
            stash.Tabs.Remove(tab);
            // TODO: Optionally move items to LostAndFound
            StashEventManager.RaiseTabRemoved(tab);
        }

        /// <summary>
        /// Reorder the entire stash tabs list (if needed).
        /// Then call the event to notify.
        /// </summary>
        public void ReorderTabs()
        {
            stash.Tabs = stash.Tabs.OrderBy(t => t.Index).ToList();
            StashEventManager.RaiseTabsReordered();
        }

        /// <summary>
        /// Update the property of an existing tab.
        /// </summary>
        public void UpdateTabProperty(StashTab existingTab, StashTab memoryTab, PropertyInfo prop)
        {
            try
            {
                prop.SetValue(existingTab, prop.GetValue(memoryTab));
            }
            catch (Exception e)
            {
                StashManCore.Main.LogError($"Error updating tab property {prop.Name}: {e.Message}");
            }
        }

        /// <summary>
        /// Retrieve an existing tab by name (case-sensitive or not).
        /// </summary>
        public StashTab GetTabByName(string name)
        {
            return stash.Tabs.FirstOrDefault(t => t.Name == name);
        }

        /// <summary>
        /// Retrieve an existing tab by index.
        /// </summary>
        public StashTab GetTabByIndex(int index)
        {
            return stash.Tabs.FirstOrDefault(t => t.Index == index);
        }
    }
}