using ItemFilterLibrary;

namespace StashMan.Filter;

public interface IIFilter
{
    bool CompareItem(ItemData itemData, ItemQuery filterData);
}