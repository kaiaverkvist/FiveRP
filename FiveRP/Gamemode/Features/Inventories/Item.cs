using GTANetworkServer;
using System;

namespace FiveRP.Gamemode.Features.Inventories
{
    public enum ItemTypes
    {
        Consumable,
        RifleWeapon,
        PistolWeapon,
        ShotgunWeapon,
        SniperWeapon,
        RocketWeapon,
        ExplosiveWeapon,
        MeleeWeapon,
        Ammo,
        Drug,
        Miscellaneous,
        License,
        Storage,
        Fish
    }

    public class Item
    {
        public ItemTypes ItemType { get; }
        public string Name { get; }
        public int Weight { get; }
        public Func<Client, bool> Usage { get; }
        public bool Stackable { get;  }
        public bool Giveable { get; }

        public Item(string name, int weight, ItemTypes itemType, Func<Client, bool> usage, bool stackable, bool giveable)
        {
            Name = name;
            Weight = weight;
            ItemType = itemType;
            Usage = usage;
            Stackable = stackable;
            Giveable = giveable;

            ItemsHandler._itemList.Add(this);
        }

        public bool RunUsage(Client player)
        {
            return Usage != null && Usage(player);
        }
    }
}
