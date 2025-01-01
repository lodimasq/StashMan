using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace StashMan.Models
{
    /// <summary>
    /// Holds position/size data for a stash item.
    /// You can store both grid-based dimensions and pixel-based rect corners if needed.
    /// </summary>
    public class ItemPosition
    {
        public int GridHeight { get; set; }
        public int GridWidth { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }
        public Vector2 TopLeft { get; set; }
        public Vector2 BottomRight { get; set; }

        public ItemPosition()
        {
        }

        public ItemPosition(int gridHeight, int gridWidth, float height, float width, Vector2 topLeft,
            Vector2 bottomRight)
        {
            GridHeight = gridHeight;
            GridWidth = gridWidth;
            Height = height;
            Width = width;
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }
    }

    /// <summary>
    /// Represents an individual item within a stash tab.
    /// </summary>
    public class StashItem
    {
        public string BaseName { get; set; }
        public string ClassName { get; set; }
        public int Quantity { get; set; }
        public bool IsFullStack { get; set; }
        public double Price { get; set; } = 0;
        public bool IsPriceValid { get; set; } = false;
        public ItemPosition Position { get; set; }
        public string UniqueHash { get; set; }
        public long PoEItemId { get; set; } = -1;

        public StashItem()
        {
        }

        public StashItem(string baseName, string className, int quantity, bool isFullStack, ItemPosition position,
            double price = 0, bool isPriceValid = false, long poeItemId = -1, string uniqueHash = null)
        {
            BaseName = baseName;
            ClassName = className;
            Quantity = quantity;
            IsFullStack = isFullStack;
            Price = price;
            IsPriceValid = isPriceValid;
            Position = position;
            PoEItemId = poeItemId;
            UniqueHash = uniqueHash;
        }
    }

    /// <summary>
    /// Represents a single stash tab in the game.
    /// </summary>
    public class StashTab
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public List<StashItem> Items { get; set; } = new();
        public DateTime LastUpdatedDateTime { get; set; }
        public int TotalItemQuantity => Items.Sum(item => item.Quantity);
        public long GridSize { get; set; }

        public StashTab()
        {
        }

        public StashTab(int index, string name, string type, long gridSize)
        {
            Index = index;
            Name = name;
            Type = type;
            GridSize = gridSize;
        }
    }

    /// <summary>
    /// Represents the ordered list of stash tabs.
    /// </summary>
    public class Stash
    {
        public List<StashTab> Tabs { get; set; } = new();

        public IList<string> GetAllStashNames()
        {
            return Tabs.Select(tab => tab.Name).ToList();
        }
    }

    /// <summary>
    /// A dictionary-based "recycle bin" for items that vanish from the stash
    /// but might reappear. Items remain for a certain time before expiring.
    /// </summary>
    public static class LostAndFound
    {
        private static readonly Dictionary<string, (StashItem item, DateTime removedAt)> _storage = new();
        private static TimeSpan _expirationTime = TimeSpan.FromMinutes(5);

        public static void SetExpiration(TimeSpan newExpiration)
        {
            _expirationTime = newExpiration;
        }

        public static void StoreItem(StashItem item)
        {
            _storage[item.UniqueHash] = (item, DateTime.Now);
        }

        public static StashItem TryRecover(string uniqueHash)
        {
            if (_storage.TryGetValue(uniqueHash, out var entry))
            {
                if (DateTime.Now - entry.removedAt <= _expirationTime)
                {
                    _storage.Remove(uniqueHash);
                    return entry.item;
                }
                else
                {
                    _storage.Remove(uniqueHash);
                }
            }

            return null;
        }

        public static void CleanupExpired()
        {
            var now = DateTime.Now;
            var expiredKeys = _storage
                .Where(kvp => now - kvp.Value.removedAt > _expirationTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
                _storage.Remove(key);
        }
    }

    /// <summary>
    /// Utility class for generating a unique item hash
    /// from stable item properties (PoEItemId, BaseName, etc.).
    /// </summary>
    public static class ItemHasher
    {
        public static string GenerateHash(StashItem item)
        {
            var rawString = $"{item.PoEItemId}|{item.BaseName}|{item.ClassName}";

            using var sha256 = SHA256.Create();
            var rawBytes = Encoding.UTF8.GetBytes(rawString);
            var hashBytes = sha256.ComputeHash(rawBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
}