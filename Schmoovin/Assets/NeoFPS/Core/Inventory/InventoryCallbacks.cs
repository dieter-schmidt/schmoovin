
namespace NeoFPS
{
    public static class InventoryCallbacks
    {
        public delegate bool FilterItem(IInventoryItem item);

        public static bool FilterQuickSlotItems(IInventoryItem item)
        {
            return (item is IQuickSlotItem);
        }

        public static bool FilterCanCarryMultiple(IInventoryItem item)
        {
            return (item.maxQuantity > 1);
        }

        public static bool FilterSingleItemOnly(IInventoryItem item)
        {
            return (item.maxQuantity == 1);
        }
    }
}
