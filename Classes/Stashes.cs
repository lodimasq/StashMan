using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ExileCore2.PoEMemory.Components;

namespace StashMan.Classes
{
    // Represents a single item in a stash tab
    public class StashItem(
        string baseName,
        string className,
        double price,
        int quantity,
        bool isFullStack,
        ItemPosition itemPosition)
    {
        public string BaseName { get; set; } = baseName;
        public string ClassName { get; set; } = className;
        public double Price { get; set; } = 0;
        public int Quantity { get; set; } = quantity;
        public bool IsFullStack { get; set; } = isFullStack;
        public ItemPosition ItemPosition { get; set; }
    }

    // Holds position/size data for a stash item
    public class ItemPosition(
        int gridHeight,
        int gridWidth,
        float height,
        float width,
        Vector2 topLeft,
        Vector2 bottomRight)
    {
        public int GridHeight { get; set; } = gridHeight;
        public int GridWidth { get; set; } = gridWidth;
        public float Height { get; set; } = height;
        public float Width { get; set; } = width;
        public Vector2 TopLeft { get; set; } = topLeft;
        public Vector2 BottomRight { get; set; } = bottomRight;
    }

    // Represents a stash tab
    public class StashTab(
        int index,
        string name,
        string type,
        long gridSize)
    {
        public int Index { get; set; } = index;
        public string Name { get; set; } = name;
        public string Type { get; set; } = type;
        public long GridSize { get; set; } = gridSize;
        public List<StashItem> Items { get; set; } = [];
        public int TotalItemQuantity => Items.Sum(item => item.Quantity);
        public DateTime LastUpdatedDateTime { get; set; }
    }

    // Represents the ordered list of stash tabs
    public class Stash
    {
        public List<StashTab> Tabs { get; set; } = [];

        public IList<string> GetAllStashNames()
        {
            return Tabs.Select(tab => tab.Name).ToList();
        }
    }
}