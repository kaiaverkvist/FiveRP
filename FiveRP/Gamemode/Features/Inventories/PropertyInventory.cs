using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using GTANetworkServer;
using Newtonsoft.Json;
using FiveRP.Gamemode.Managers;
using FiveRP.Gamemode.Features.Vehicles;
using System;
using FiveRP.Gamemode.Features.Properties;

namespace FiveRP.Gamemode.Features.Inventories
{
    public class PropertyInventory : Inventory
    {
        private Property _property;

        public PropertyInventory(Property property)
        {
            _property = property;
            _inventoryItems = new Dictionary<Item, int>();
            _currentWeight = 0;
            _maxWeight = 50000;
            switch (_property.PropertyInventorySize.ToLower())
            {
                case "s":
                    _maxWeight = 50000;
                    break;
                case "m":
                    _maxWeight = 100000;
                    break;
                case "l":
                    _maxWeight = 250000;
                    break;
                case "xl":
                    _maxWeight = 500000;
                    break;
                case "xxl":
                    _maxWeight = 1000000;
                    break;
            }

            if (_property.PropertyInventory.Length > 0)
            {
                ItemsJson[] itemHashes = JsonConvert.DeserializeObject<ItemsJson[]>(_property.PropertyInventory);
                foreach (ItemsJson itemJson in itemHashes)
                {
                    Item item = ItemsLibrary.GetItem(itemJson.ItemHash);
                    if (item != null)
                        AddItem(item, itemJson.Amount);
                }
            }
        }

        public PropertyInventory()
        {

        }

        public override void SaveInventory()
        {
            if (_property != null)
            {
                var jsonInventory = JsonConvert.SerializeObject(_inventoryItems.Select(item => new { hash = item.Key.Name, amount = item.Value }));
                _property.PropertyInventory = jsonInventory;
                PropertyHandler.SaveProperty(_property);
            }
        }
    }
}
