using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using GTANetworkServer;
using Newtonsoft.Json;
using FiveRP.Gamemode.Managers;

namespace FiveRP.Gamemode.Features.Inventories
{
    public abstract class Inventory : Script
    {
        protected Dictionary<Item,int> _inventoryItems;
        protected int _maxWeight = 5000;
        protected int _initialMaxWeight = 5000;
        protected int _currentWeight;

        public Inventory()
        {

        }

        public virtual bool RemoveAllItems()
        {
            Dictionary<Item, int> newInventory = new Dictionary<Item, int>();
            foreach (Item item in _inventoryItems.Keys)
            {
                if (item.ItemType == ItemTypes.License)
                    newInventory.Add(item, _inventoryItems[item]);
            }
            _inventoryItems = newInventory;
            _maxWeight = _initialMaxWeight;
            _currentWeight = 0;
            return true;
        }

        public bool CanAddItem(Item item, int amount)
        {
            if (amount <= 0)
                return true;

            if (_inventoryItems.ContainsKey(item) && !item.Stackable)
                return false;
            else
                return _currentWeight + (item.Weight * amount) <= _maxWeight;
        }

        public bool AddBag(Item bag, int amount)
        {
            foreach (KeyValuePair<Item, int> item in _inventoryItems)
            {
                if (item.Key.ItemType == ItemTypes.Storage)
                    return false;
            }
            _maxWeight = _initialMaxWeight + amount;
            return true;
        }

        public bool RemoveBag(Item bag, int amount)
        {
            _maxWeight -= amount;
            return true;
        }

        public bool AddItem(Item item, int amount)
        {
            if (amount <= 0)
                return true;

            bool noBag = true;
            if (item.Name == "Backpack")
                noBag = AddBag(item, 2500);
            else if (item.Name == "Dufflebag")
                noBag = AddBag(item, 5000);
            else if (item.Name == "Hiking Bag")
                noBag = AddBag(item, 7500);
            if (!noBag)
                return false;

            if (_currentWeight + (item.Weight * amount) <= _maxWeight)
            {
                if (_inventoryItems.ContainsKey(item))
                {
                    if (item.Stackable)
                        _inventoryItems[item] += amount;
                    else
                        return false;
                }
                else
                    _inventoryItems.Add(item, amount);
                _currentWeight += item.Weight * amount;
                return true;
            }  
            return false;
        }

        public bool CanRemoveItem(Item item, int amount)
        {
            if (amount <= 0)
                return true;

            if (_inventoryItems.ContainsKey(item))
            {
                if (_inventoryItems[item] >= amount)
                    return true;
            }
            return false;
        }

        public bool RemoveItem(Item item, int amount)
        {
            if (amount <= 0)
                return true;

            if (item.Name == "Backpack")
                RemoveBag(item, 2500);
            else if (item.Name == "Dufflebag")
                RemoveBag(item, 5000);
            else if (item.Name == "Hiking Bag")
                RemoveBag(item, 7500);

            if (_inventoryItems.ContainsKey(item))
            {
                if (_inventoryItems[item] < amount)
                    return false;
                else if (_inventoryItems[item] == amount)
                    _inventoryItems.Remove(item);
                else if (_inventoryItems[item] > amount)
                    _inventoryItems[item] -= amount;
                _currentWeight -= item.Weight * amount;
                return true;
            }
            return false;
        }

        public Dictionary<Item, int> GetItems()
        {
            return _inventoryItems;
        }

        public double GetMaxWeight()
        {
            return _maxWeight;
        }

        public virtual double GetCurrentWeight()
        {
            return _currentWeight;
        }

        public abstract void SaveInventory();
    }
}
