using System.Collections.Generic;

namespace FiveRP.Gamemode.Features.Inventories
{
    public static class ItemsLibrary
    {
        private static List<Item> _itemList;

        public static void SetItemList(List <Item> itemList)
        {
            _itemList = itemList;
        }

        public static Item GetItem(string itemName)
        {
            foreach (Item item in _itemList)
            {
                if (itemName.ToLower() == item.Name.ToLower())
                    return item;
            }
            return null;
        }
    }
}
