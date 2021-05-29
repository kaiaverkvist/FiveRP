using System.Collections.Generic;
using GTANetworkServer;

namespace FiveRP.Gamemode.Features.Inventories
{
    public class ItemsHandler : Script
    {
        public static List<Item> _itemList;
        private string[] _fishNames = { "Hoki", "Red Cod", "Lemon Fish", "Gurnard", "Monk Fish", "Sole", "Tarakihi", "Salmon", "Snapper", "Blue Cod" };
        private string[] _smallWeaponsNames = { "Pistol", "CombatPistol", "FlareGun", "StunGun", "APPistol", "VintagePistol", "CombatPDW", "MachinePistol", "MarksmanPistol", "HeavyPistol", "Pistol50", "SNSPistol", "Revolver" };
        private string[] _largeWeaponsNames = { "BullpupRifle", "CombatMG", "Gusenberg", "CompactRifle", "Minigun", "SMG",  "MicroSMG", "CompactLauncher", "AssaultSMG", "MG",  "Musket", "CarbineRifle", "AdvancedRifle", "MiniSMG", "AssaultRifle", "SpecialCarbine" };
        private string[] _shotgunWeaponsNames = { "PumpShotgun", "SawnoffShotgun", "HeavyShotgun", "BullpupShotgun", "Autoshotgun", "DoubleBarrelShotgun", "AssaultShotgun"};
        private string[] _explosiveWeaponsNames = { "Flare", "PetrolCan", "StickyBomb", "Molotov", "SmokeGrenade", "Grenade", "BZGas", "ProximityMine", "Pipebomb" };
        private string[] _meleeWeaponsNames = { "Nightstick", "Hammer", "Golfclub", "Ball", "Wrench", "Snowball", "FireExtinguisher", "Hatchet", "Bottle", "Parachute", "Machete", "SwitchBlade", "KnuckleDuster", "Battleaxe", "Crowbar", "Flashlight", "Dagger", "Poolcue", "Bat", "Knife" };
        private string[] _rocketWeaponsName = { "Railgun", "RPG", "Firework", "GrenadeLauncherSmoke", "HomingLauncher", "GrenadeLauncher"};
        private string[] _sniperWeaponsName = { "HeavySniper", "SniperRifle", "MarksmanRifle" };

        public ItemsHandler()
        {
            _itemList = new List<Item>();
            new Item("Driving License", 0, ItemTypes.License, null, false, false);
            new Item("PF License", 0, ItemTypes.License, null, false, false);
            new Item("CCW License", 0, ItemTypes.License, null, false, false);
            new Item("Mechanical Components", 5, ItemTypes.Consumable, null, true, true);
            new Item("Donut", 50, ItemTypes.Consumable, DonutUsage, false, true);
            new Item("Hotdog", 150, ItemTypes.Consumable, HotdogUsage, false, true);
            new Item("Backpack", 0, ItemTypes.Storage, null, false, false);
            new Item("Dufflebag", 0, ItemTypes.Storage, null, false, false);
            new Item("Hiking Bag", 0, ItemTypes.Storage, null, false, false);
            new Item("Rifle ammo", 2, ItemTypes.Ammo, null, true, true);
            new Item("Shotgun shell", 2, ItemTypes.Ammo, null, true, true);
            new Item("Pistol ammo", 2, ItemTypes.Ammo, null, true, true);
            new Item("Long range ammo", 2, ItemTypes.Ammo, null, true, true);
            new Item("Rocket", 2000, ItemTypes.Ammo, null, true, true);

            foreach (string fishName in _fishNames)
                new Item(fishName, 500, ItemTypes.Fish, null, true, true);
            foreach (string smallWeaponName in _smallWeaponsNames)
                new Item(smallWeaponName, 1000, ItemTypes.PistolWeapon, null, true, true);
            foreach (string largeWeaponName in _largeWeaponsNames)
                new Item(largeWeaponName, 3000, ItemTypes.RifleWeapon, null, true, true);
            foreach (string shotgunWeaponName in _shotgunWeaponsNames)
                new Item(shotgunWeaponName, 3000, ItemTypes.ShotgunWeapon, null, true, true);
            foreach (string explosiveWeaponName in _explosiveWeaponsNames)
                new Item(explosiveWeaponName, 300, ItemTypes.ExplosiveWeapon, null, true, true);
            foreach (string meleeWeaponName in _meleeWeaponsNames)
                new Item(meleeWeaponName, 500, ItemTypes.MeleeWeapon, null, true, true);
            foreach (string sniperWeaponName in _sniperWeaponsName)
                new Item(sniperWeaponName, 3000, ItemTypes.SniperWeapon, null, true, true);
            foreach (string rocketWeaponName in _rocketWeaponsName)
                new Item(rocketWeaponName, 3000, ItemTypes.RocketWeapon, null, true, true);

            ItemsLibrary.SetItemList(_itemList);
        }

        private bool DonutUsage(Client sender)
        {
            sender.health += 10;
            return true;
        }

        private bool HotdogUsage(Client sender)
        {
            sender.health += 30;
            return true;
        }
    }
}
