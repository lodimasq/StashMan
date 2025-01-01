using System;
using StashMan.Events;
using StashMan.Models;

namespace StashMan.Events.EventHandlers
{
    /// <summary>
    /// A trivial example of subscribing to StashEventManager to log 
    /// or display info whenever a tab or item changes.
    /// </summary>
    public static class LoggingEventHandler
    {
        private static bool _isRegistered = false;

        /// <summary>
        /// Call this once (e.g. in plugin init) to register the event handlers.
        /// </summary>
        public static void Register()
        {
            if (_isRegistered) return;

            StashEventManager.OnTabAdded += LogTabAdded;
            StashEventManager.OnTabRenamed += LogTabRenamed;
            StashEventManager.OnTabRemoved += LogTabRemoved;
            StashEventManager.OnTabsReordered += LogTabsReordered;

            StashEventManager.OnItemAdded += LogItemAdded;
            StashEventManager.OnItemUpdated += LogItemUpdated;
            StashEventManager.OnItemRemoved += LogItemRemoved;
            StashEventManager.OnItemRestored += LogItemRestored;

            _isRegistered = true;
        }

        private static void LogTabAdded(StashTab tab)
        {
            Console.WriteLine($"[LoggingEventHandler] Tab added: '{tab.Name}', index={tab.Index}.");
        }

        private static void LogTabRenamed(StashTab tab, string oldName)
        {
            Console.WriteLine($"[LoggingEventHandler] Tab renamed from '{oldName}' to '{tab.Name}'.");
        }

        private static void LogTabRemoved(StashTab tab)
        {
            Console.WriteLine($"[LoggingEventHandler] Tab removed: '{tab.Name}' (index={tab.Index}).");
        }

        private static void LogTabsReordered()
        {
            Console.WriteLine("[LoggingEventHandler] Tabs reordered.");
        }

        private static void LogItemAdded(StashTab tab, StashItem item)
        {
            Console.WriteLine($"[LoggingEventHandler] Item added to '{tab.Name}': {item.BaseName} x{item.Quantity}");
        }

        private static void LogItemUpdated(StashTab tab, StashItem oldItem, StashItem newItem)
        {
            Console.WriteLine($"[LoggingEventHandler] Item updated in '{tab.Name}': {oldItem.BaseName}, " +
                              $"qty {oldItem.Quantity} -> {newItem.Quantity}");
        }

        private static void LogItemRemoved(StashTab tab, StashItem item)
        {
            Console.WriteLine($"[LoggingEventHandler] Item removed from '{tab.Name}': {item.BaseName}");
        }

        private static void LogItemRestored(StashItem item)
        {
            Console.WriteLine($"[LoggingEventHandler] Item restored from LostAndFound: {item.BaseName}");
        }
    }
}
