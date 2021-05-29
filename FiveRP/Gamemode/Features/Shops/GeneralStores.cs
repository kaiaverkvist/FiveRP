using System.Collections.Generic;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Features.Inventories;

namespace FiveRP.Gamemode.Features.Shops
{
    class GeneralStores : Script
    {
        private static List<GeneralStore> _generalStores;
        private int _backpackPrice = 500;
        private int _dufflebagPrice = 850;
        private int _hikingbagPrice = 1250;
        private int _baseballBatPrice = 500;
        private int _wrenchPrice = 350;
        private int _hammerPrice = 350;

        public GeneralStores()
        {
            _generalStores = new List<GeneralStore>();
            API.onResourceStart += OnResourceStart;
            API.onClientEventTrigger += OnClientEventTrigger;
        }

        private void OnResourceStart()
        {
            InitializeGeneralStores();
        }

        private void InitializeGeneralStores()
        {
            _generalStores.Add(new GeneralStore(new Vector3(377.067, 324.765, 103.566)));
            _generalStores.Add(new GeneralStore(new Vector3(-1488.392, -380.691, 40.163)));
            _generalStores.Add(new GeneralStore(new Vector3(-1224.731, -905.193, 12.326)));
            _generalStores.Add(new GeneralStore(new Vector3(-711.750, -913.283, 19.215)));
            _generalStores.Add(new GeneralStore(new Vector3(1159.069, -324.153, 69.205)));
            _generalStores.Add(new GeneralStore(new Vector3(1139.011, -981.324, 46.415)));
            _generalStores.Add(new GeneralStore(new Vector3(29.275, -1347.423, 29.497)));
            _generalStores.Add(new GeneralStore(new Vector3(-51.624, -1755.303, 29.421)));
            _generalStores.Add(new GeneralStore(new Vector3(-3242.286, 1004.686, 12.83072)));
            _generalStores.Add(new GeneralStore(new Vector3(544.2905, 2671.58, 42.15825)));
            _generalStores.Add(new GeneralStore(new Vector3(2558.32, 385.4705, 108.6213)));
            _generalStores.Add(new GeneralStore(new Vector3(2681.11, 3283.12, 55.24067)));
            _generalStores.Add(new GeneralStore(new Vector3(1731.85, 6412.19, 35.03853)));
            _generalStores.Add(new GeneralStore(new Vector3(1964.88, 3741.62, 32.342)));

            foreach (var store in _generalStores)
            {
                API.createTextLabel("~g~\nType /store to see a list of items to buy.", store.Position, 45f, 0.3f);
                var blip = API.createBlip(store.Position);
                API.setBlipSprite(blip, 52);
                API.setBlipColor(blip, 4);
                API.setBlipScale(blip, 1.3f);
                API.setBlipShortRange(blip, true);
            }
        }

        private void OnClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "menu_handler_select_item")
            {
                if ((string)arguments[0] == "general_store")
                {
                    var senderData = Account.GetPlayerCharacterData(sender);
                    if (senderData == null || senderData.CharacterData == null)
                        return;
                    string menuChoice = (string)arguments[2];
                    if (menuChoice.Contains("Backpack") && senderData.CharacterData.Money >= _backpackPrice)
                    {
                        if (AddItem(sender, "Backpack"))
                        {
                            senderData.CharacterData.Money -= _backpackPrice;
                            API.sendChatMessageToPlayer(sender, "You have bought a backpack for ~g~$" + _backpackPrice);
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "~r~You already have a bag, drop it first.");
                    }
                    else if (menuChoice.Contains("Dufflebag") && senderData.CharacterData.Money >= _dufflebagPrice)
                    {
                        if (AddItem(sender, "Dufflebag"))
                        {
                            senderData.CharacterData.Money -= _dufflebagPrice;
                            API.sendChatMessageToPlayer(sender, "You have bought a dufflebag for ~g~$" + _dufflebagPrice);
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "~r~You already have a bag, drop it first.");
                    }
                    else if (menuChoice.Contains("Hiking Bag") && senderData.CharacterData.Money >= _hikingbagPrice)
                    {
                        if (AddItem(sender, "Hiking Bag"))
                        {
                            senderData.CharacterData.Money -= _hikingbagPrice;
                            API.sendChatMessageToPlayer(sender, "You have bought an hiking bag for ~g~$" + _hikingbagPrice);
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "~r~You already have a bag, drop it first.");
                    }
                    else if (menuChoice.Contains("Donut") && senderData.CharacterData.Money >= 30)
                    {
                        if (AddItem(sender, "Donut"))
                        {
                            senderData.CharacterData.Money -= 30;
                            API.sendChatMessageToPlayer(sender, "You have bought a donut for ~g~$30.");
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "~r~You cannot hold more than one donut.");
                    }
                    else if (menuChoice.Contains("Hotdog") && senderData.CharacterData.Money >= 75)
                    {
                        if (AddItem(sender, "Hotdog"))
                        {
                            senderData.CharacterData.Money -= 75;
                            API.sendChatMessageToPlayer(sender, "You have bought a hotdog for ~g~$75.");
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "~r~You cannot hold more than one hotdog.");
                    }
                    else if (menuChoice.Contains("Hammer") && senderData.CharacterData.Money >= _hammerPrice)
                    {
                        if (AddItem(sender, "Hammer"))
                        {
                            senderData.CharacterData.Money -= _hammerPrice;
                            API.sendChatMessageToPlayer(sender, "You have bought a hammer for ~g~$" + _hammerPrice + "~w~. Use /equip to equip it!");
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "~r~The item cannot be added to your inventory.");
                    }
                    else if (menuChoice.Contains("Baseball Bat") && senderData.CharacterData.Money >= _baseballBatPrice)
                    {
                        if (AddItem(sender, "Bat"))
                        {
                            senderData.CharacterData.Money -= _baseballBatPrice;
                            API.sendChatMessageToPlayer(sender, "You have bought a bat for ~g~$" + _baseballBatPrice + "~w~. Use /equip to equip it!");
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "~r~The item cannot be added to your inventory.");
                    }
                    else if (menuChoice.Contains("Wrench") && senderData.CharacterData.Money >= _wrenchPrice)
                    {
                        if (AddItem(sender, "Wrench"))
                        {
                            senderData.CharacterData.Money -= _wrenchPrice;
                            API.sendChatMessageToPlayer(sender, "You have bought a wrench for ~g~$" + _wrenchPrice + "~w~. Use /equip to equip it!");
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "~r~The item cannot be added to your inventory.");
                    }
                    else
                        API.sendChatMessageToPlayer(sender, "~r~You do not have enough money.");
                }
            }
        }

        [Command("store")]
        public void BuyGeneralCommand(Client sender)
        {
            foreach (var store in _generalStores)
            {
                if (DistanceLibrary.DistanceBetween(API.getEntityPosition(sender), store.Position) <= 100f)
                {
                    MenuLibrary.ShowNativeMenu(API, sender, "general_store", "24/7",
                        "Choose an item to buy", false, new List<string>(new[]
                        {"Backpack (+2.5kg) - ~g~$" + _backpackPrice, "Dufflebag (+5kg) - ~g~$" + _dufflebagPrice, "Hiking Bag (+7.5kg) - ~g~$" + _hikingbagPrice, "Donut - ~g~$30", "Hotdog - ~g~75",
                        "Baseball Bat - ~g~$" + _baseballBatPrice, "Wrench - ~g~$" + _wrenchPrice, "Hammer - ~g~$" + _hammerPrice}));
                }
            }
        }

        public bool AddItem(Client sender, string itemName)
        {
            var senderData = Account.GetPlayerCharacterData(sender);
            if (senderData == null || senderData.CharacterData == null)
                return false;
            var inventory = senderData.CharacterData.Inventory;
            Item item = ItemsLibrary.GetItem(itemName);
            if (item == null || !inventory.CanAddItem(item, 1))
                return false;
            return inventory.AddItem(ItemsLibrary.GetItem(itemName), 1);
        }
    }

    class GeneralStore
    {
        public Vector3 Position { get; set; }

        public GeneralStore(Vector3 position)
        {
            Position = position;
        }
    }
}