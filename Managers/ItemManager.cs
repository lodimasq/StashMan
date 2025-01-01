using System.Linq;
using StashMan.Events;
using StashMan.Models;

namespace StashMan.Managers
{
    /// <summary>
    /// Manages CRUD operations for items within a tab, and recycles them into the LostAndFound if removed.
    /// Triggers events on creation, update, removal, and potential restore.
    /// </summary>
    public class ItemManager
    {
        /// <summary>
        /// Add an item to a stash tab, checking LostAndFound first to see if it can be "restored."
        /// </summary>
        public void AddOrRecoverItem(StashTab tab, StashItem newItem)
        {
            // Ensure unique hash
            if (string.IsNullOrEmpty(newItem.UniqueHash))
            {
                newItem.UniqueHash = ItemHasher.GenerateHash(newItem);
            }

            // Try to recover from LostAndFound
            var recovered = LostAndFound.TryRecover(newItem.UniqueHash);
            if (recovered != null)
            {
                // Merge or overwrite fields if needed
                recovered.Position = newItem.Position;
                recovered.Quantity = newItem.Quantity;
                recovered.IsFullStack = newItem.IsFullStack;
                recovered.PoEItemId = newItem.PoEItemId;

                // Price & validity stay whatever was in the old record or reset if you prefer
                recovered.IsPriceValid = false; 

                tab.Items.Add(recovered);
                StashEventManager.RaiseItemRestored(recovered);
            }
            else
            {
                // Brand-new item
                tab.Items.Add(newItem);
                StashEventManager.RaiseItemAdded(tab, newItem);
            }
        }

        /// <summary>
        /// Update an existing item in a stash tab, e.g. quantity or position changed.
        /// </summary>
        public void UpdateItem(StashTab tab, StashItem oldItem, StashItem newData)
        {
            var index = tab.Items.IndexOf(oldItem);
            if (index < 0) return; // Not found in this tab

            // If you only want to update certain fields, do so:
            var updatedItem = new StashItem
            {
                BaseName = oldItem.BaseName,
                ClassName = oldItem.ClassName,
                PoEItemId = oldItem.PoEItemId,
                UniqueHash = oldItem.UniqueHash, // preserve
                Price = oldItem.Price, // or reset
                IsPriceValid = false, // or keep
                Quantity = newData.Quantity,
                IsFullStack = newData.IsFullStack,
                Position = newData.Position
            };

            tab.Items[index] = updatedItem;

            StashEventManager.RaiseItemUpdated(tab, oldItem, updatedItem);
        }

        /// <summary>
        /// Remove an item from the stash tab, placing it in LostAndFound.
        /// </summary>
        public void RemoveItem(StashTab tab, StashItem item)
        {
            if (!tab.Items.Remove(item)) return; // item not found

            LostAndFound.StoreItem(item);
            StashEventManager.RaiseItemRemoved(tab, item);
        }

        /// <summary>
        /// Quick helper to find an item by unique hash in a tab.
        /// </summary>
        public StashItem GetItemByHash(StashTab tab, string uniqueHash)
        {
            return tab.Items.FirstOrDefault(i => i.UniqueHash == uniqueHash);
        }
    }
}
