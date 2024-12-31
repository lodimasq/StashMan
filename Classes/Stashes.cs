using System;
using System.Collections.Generic;
using System.Linq;

namespace StashMan.Classes
{
    // Represents a single item in a stash tab
    public class StashItem(string name, double price, int quantity)
    {
        public string Name { get; set; } = name;
        public double Price { get; set; } = price;
        public int Quantity { get; set; } = quantity;
    }

    // Represents a stash tab
    public class StashTab(int index, string name, string type)
    {
        public int Index { get; set; } = index;
        public string Name { get; set; } = name;
        public string Type { get; set; } = type;
        public List<StashItem> Items { get; set; } = [];
        public int TotalItemQuantity => Items.Sum(item => item.Quantity);
        public DateTime LastGameSync { get; set; }
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