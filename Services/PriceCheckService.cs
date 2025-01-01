using StashMan.Models;

namespace StashMan.Services
{
    /// <summary>
    /// Scaffold for a service that handles price checks for newly added or updated items.
    /// </summary>
    public static class PriceCheckService
    {
        // Hook to stash events or call from item manager
        // e.g., "public static void EnqueueCheck(StashItem item)"

        public static void EnqueuePriceCheck(StashItem item)
        {
            // Example stub:
            // - Add to a queue
            // - Kick off an async process
            // - Or call external API, etc.
        }

        public static void PerformPriceCheck(StashItem item)
        {
            // For demonstration:
            // 1. Hit external API with item data
            // 2. Parse response
            // 3. item.Price = result
            // 4. item.IsPriceValid = true
        }
    }
}