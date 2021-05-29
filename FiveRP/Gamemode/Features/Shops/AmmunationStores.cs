using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Managers;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Inventories;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Features.Admin;

namespace FiveRP.Gamemode.Features
{
    public class AmmunationStores: Script
    {
        private static List<AmmunationStore> _ammunationStores = new List<AmmunationStore>();
        private static readonly List<AmmoStoreItem> StoreItems = new List<AmmoStoreItem>()
        {
            new GunStoreItem(WeaponHash.Pistol, 2500, "Pistol"),
            new GunStoreItem(WeaponHash.PumpShotgun, 3500, "Shotgun"),
            new AmmoStoreItem(WeaponHash.Pistol, 245, "Pistol Ammo", 24),
            new AmmoStoreItem(WeaponHash.PumpShotgun, 375, "Shotgun Ammo", 16),
        };

        public AmmunationStores()
        {
            _ammunationStores = new List<AmmunationStore>();

            API.onResourceStart += OnResourceStart;
            API.onClientEventTrigger += OnClientEventTrigger;
        }

        private void OnResourceStart()
        {
            InitializeAmmunationStores();
        }

        private void InitializeAmmunationStores()
        {
            _ammunationStores.Add(new AmmunationStore(new Vector3(16.93, -1115.81, 29.79)));
            _ammunationStores.Add(new AmmunationStore(new Vector3(-663.92, -945.33, 21.79)));
            _ammunationStores.Add(new AmmunationStore(new Vector3(811.96, -2147.35, 29.49)));
            _ammunationStores.Add(new AmmunationStore(new Vector3(2569.75, 304.69, 108.61)));
            _ammunationStores.Add(new AmmunationStore(new Vector3(-324.26, 6075.14, 31.24)));
            _ammunationStores.Add(new AmmunationStore(new Vector3(1699.81, 3751.84, 34.51)));
            _ammunationStores.Add(new AmmunationStore(new Vector3(-1314.77, -390.16, 36.59)));
            _ammunationStores.Add(new AmmunationStore(new Vector3(243.71, -45.04, 69.90)));
            _ammunationStores.Add(new AmmunationStore(new Vector3(843.92, -1023.68, 28.02)));

            foreach (var store in _ammunationStores)
            {
                API.createTextLabel("~g~\nType /buyguns (/bguns) to view available guns.", store.Position, 45f, 0.3f);
                var blip = API.createBlip(store.Position);
                API.setBlipSprite(blip, 313);
                API.setBlipColor(blip, 2);
                API.setBlipScale(blip, 1.3f);
                API.setBlipShortRange(blip, true);
            }
        }

        [Command("buyguns", Alias = "bguns")]
        public void BuyGunsCommand(Client sender)
        {
            foreach (var store in _ammunationStores)
            {
                if (DistanceLibrary.DistanceBetween(API.getEntityPosition(sender), store.Position) <= 40f)
                {
                    Character senderData = Account.GetPlayerCharacterData(sender);
                    if (senderData.CharacterData == null)
                        return;
                    Inventory inventory = senderData.CharacterData.Inventory;
                    Dictionary<Item, int> inventoryItems = inventory.GetItems();
                    Item pfLicense = ItemsLibrary.GetItem("PF License");
                    Item CCWLicense = ItemsLibrary.GetItem("CCW License");
                    if (!inventoryItems.ContainsKey(pfLicense) && !inventoryItems.ContainsKey(CCWLicense))
                    {
                        sender.sendChatMessage($"~r~You need a weapon license to buy from the ammunation.");
                        return;
                    }
                    List<string> temp = new List<string>();
                    StoreItems.ForEach(item => { temp.Add($"{item.Text} x{item.Amount} (${item.Price})"); });
                    MenuLibrary.ShowNativeMenu(API, sender, @"purchase_gun", @"Gun Store", @"Select your gun or ammo", false, temp);
                }
            }
        }

        public void OnClientEventTrigger(Client sender, string eventName, params object[] args)
        {
            if (eventName == "menu_handler_select_item")
            {
                var menu = (string)args[0];
                var index = (int)args[1];
                if (menu == @"purchase_gun")
                {
                    var charData = Account.GetPlayerCharacterData(sender);
                    var storeItem = StoreItems[index];
                    if (charData.CharacterData.Money >= storeItem.Price)
                    {
                        if (storeItem is GunStoreItem)
                        {
                            sender.giveWeapon(storeItem.Weapon, 0, false, false);
                            sender.sendChatMessage($"~g~You have purchased a {storeItem.Text}");
                            AlertLogging.RaiseAlert($"{sender.name} has bought {storeItem.Weapon} at ammunation.", "AMMUNATION", 3);
                            charData.CharacterData.Money -= storeItem.Price;
                        }
                        else
                        {
                            var senderWeapons = sender.weapons;
                            if (senderWeapons.Contains(storeItem.Weapon))
                            {
                                API.setPlayerWeaponAmmo(sender, storeItem.Weapon, storeItem.Amount);
                                sender.sendChatMessage($"~g~You have purchased {storeItem.Text}");
                                AlertLogging.RaiseAlert($"{sender.name} has bought {storeItem.Weapon} ammo ({storeItem.Amount}) at ammunation.", "AMMUNATION", 3);
                                charData.CharacterData.Money -= storeItem.Price;
                            }
                            else
                            {
                                sender.sendChatMessage("You have to own the weapon you are trying to purchase ammo for.");
                            }
                        }

                    } else
                    {
                        sender.sendChatMessage("~r~You can't afford that.");
                    }
                }
            }
        }
    }

    class AmmunationStore
    {
        public Vector3 Position { get; set; }

        public AmmunationStore(Vector3 position)
        {
            Position = position;
        }
    }

    class GunStoreItem : AmmoStoreItem
    {

        public GunStoreItem(WeaponHash weapon, int price, string text) : base(weapon, price, text, 1) {}
    }

    class AmmoStoreItem
    {
        public int Price { get; set; }
        public string Text { get; set; }
        public WeaponHash Weapon { get; set; }
        public int Amount { get; set; }

        public AmmoStoreItem(WeaponHash weapon, int price, string text, int amount)
        {
            Price = price;
            Text = text;
            Weapon = weapon;
            Amount = amount;
        }
    }
}
