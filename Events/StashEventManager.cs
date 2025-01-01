using System;
using StashMan.Models;

namespace StashMan.Events
{
    /// <summary>
    /// Central static event aggregator for stash & item events.
    /// External modules can subscribe to these events to perform further actions
    /// (e.g., pricing, logging).
    /// </summary>
    public static class StashEventManager
    {
        // --- TAB EVENTS ---
        public static event Action<StashTab> OnTabAdded;
        public static event Action<StashTab, string> OnTabRenamed; // (tab, oldName)
        public static event Action<StashTab> OnTabRemoved;
        public static event Action OnTabsReordered; // Or pass old/new lists

        // --- ITEM EVENTS ---
        public static event Action<StashTab, StashItem> OnItemAdded;
        public static event Action<StashTab, StashItem, StashItem> OnItemUpdated; 
        public static event Action<StashTab, StashItem> OnItemRemoved;
        public static event Action<StashItem> OnItemRestored;

        // --- RAISE METHODS ---
        public static void RaiseTabAdded(StashTab tab) => OnTabAdded?.Invoke(tab);
        public static void RaiseTabRenamed(StashTab tab, string oldName) 
            => OnTabRenamed?.Invoke(tab, oldName);
        public static void RaiseTabRemoved(StashTab tab) => OnTabRemoved?.Invoke(tab);
        public static void RaiseTabsReordered() => OnTabsReordered?.Invoke();

        public static void RaiseItemAdded(StashTab tab, StashItem item)
            => OnItemAdded?.Invoke(tab, item);
        public static void RaiseItemUpdated(StashTab tab, StashItem oldItem, StashItem newItem)
            => OnItemUpdated?.Invoke(tab, oldItem, newItem);
        public static void RaiseItemRemoved(StashTab tab, StashItem item)
            => OnItemRemoved?.Invoke(tab, item);
        public static void RaiseItemRestored(StashItem item)
            => OnItemRestored?.Invoke(item);
        
    }
}