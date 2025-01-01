using StashMan.Models;
using StashMan.Services;

namespace StashMan.Events.EventHandlers
{
    /// <summary>
    /// Demonstrates subscribing to stash item events to trigger price checks.
    /// </summary>
    public static class PriceEventHandler
    {
        private static bool _isRegistered = false;

        public static void Register()
        {
            if (_isRegistered) return;

            // For new or updated items, we might want to do a price check
            StashEventManager.OnItemAdded += OnItemAdded;
            StashEventManager.OnItemUpdated += OnItemUpdated;

            _isRegistered = true;
        }

        private static void OnItemAdded(StashTab tab, StashItem item)
        {
            // Decide if we want to price-check everything or only certain items
            // For demonstration, queue a price check for all
            PriceCheckService.EnqueuePriceCheck(item);
        }

        private static void OnItemUpdated(StashTab tab, StashItem oldItem, StashItem newItem)
        {
            // If the quantity or base name changed, we might re-check price
            // Simplistic example: always re-check
            PriceCheckService.EnqueuePriceCheck(newItem);
        }
    }
}