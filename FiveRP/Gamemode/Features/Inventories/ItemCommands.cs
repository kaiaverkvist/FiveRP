using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using GTANetworkServer;
using FiveRP.Gamemode.Managers;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Inventories
{
    public class ItemCommands : Script
    {
        [Command("deleteitem", GreedyArg = true, Group = "Inventory Commands")]
        public void DropItemCommand(Client player, string itemid, string amount)
        {
            int itemAmount = 0;
            int itemId = 0;
            bool parsedAmount = Int32.TryParse(amount.ToString(), out itemAmount);
            bool parsedItem = Int32.TryParse(itemid.ToString(), out itemId);
            Character senderData = Account.GetPlayerCharacterData(player);
            if (senderData == null || senderData.CharacterData == null)
                return;
            Inventory senderInventory = senderData.CharacterData.Inventory;
            List<Item> itemList = senderInventory.GetItems().Keys.ToList();
            if (itemId <= 0 || itemId > itemList.Count)
            {
                API.sendChatMessageToPlayer(player, "~r~Invalid item id.");
                return;
            }
            Item item = itemList[itemId - 1];
            if (item == null || !parsedAmount || !parsedItem || itemAmount <= 0)
            {
                API.sendChatMessageToPlayer(player, "~r~Invalid item id or amount.");
                return;
            }
            if (senderInventory.CanRemoveItem(item, itemAmount))
            {
                senderInventory.RemoveItem(item, itemAmount);
                senderInventory.SaveInventory();
                API.sendChatMessageToPlayer(player, "~r~You deleted " + item.Name + " (" + itemAmount + ")");
                ChatLibrary.SendLabelEmoteMessage(API, player, "drops an item on the ground.");
            }
            else
                API.sendChatMessageToPlayer(player, "~r~You do not have " + amount + " " + item.Name);
        }

        [Command("myitems", Alias = "myinv,inv,inventory,items", GreedyArg = true, Group = "Inventory Commands")]
        public void MyItemsCommand(Client player)
        {
            Character senderData = Account.GetPlayerCharacterData(player);
            if (senderData.CharacterData == null)
                return;
            Inventory inventory = senderData.CharacterData.Inventory;
            Dictionary<Item, int> inventoryItems = inventory.GetItems();
            API.sendChatMessageToPlayer(player, $"~g~Money on hand: ${NamingFunctions.FormatMoney(senderData.CharacterData.Money)}");
            API.sendChatMessageToPlayer(player, "~g~|------ " + player.name + "'s Items ------|");
            int id = 0;
            foreach (KeyValuePair<Item, int> item in inventoryItems)
            {
                if (item.Key.Weight == 0)
                    API.sendChatMessageToPlayer(player, "~y~" + ++id + ": " + item.Key.Name + " x" + item.Value);
                else
                    API.sendChatMessageToPlayer(player, "~y~" + ++id + ": " + item.Key.Name + " x" + item.Value + " (" + item.Value * item.Key.Weight + "g)");
            }
            API.sendChatMessageToPlayer(player, "~y~Total weight: " + inventory.GetCurrentWeight() + "/" + inventory.GetMaxWeight() + " grams");
            WeaponHash[] weapons = API.getPlayerWeapons(player);
            id = 0;
            if (weapons.Length > 0)
                API.sendChatMessageToPlayer(player, "~g~|------ " + player.name + "'s Equipped Weapons ------|");
            foreach (WeaponHash weapon in player.weapons)
                API.sendChatMessageToPlayer(player, "~y~Index" + id++ + ": " + weapon);
        }

        [Command("showitems", GreedyArg = true, Group = "Inventory Commands")]
        public void ShowItemsCommand(Client player, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, player, targ);
            if (target == null) return;

            if (DistanceLibrary.DistanceBetween(player.position, target.position) > 5f)
            {
                API.sendChatMessageToPlayer(player, "~r~You are too far away.");
                return;
            }
            Character senderData = Account.GetPlayerCharacterData(player);
            if (senderData.CharacterData == null)
                return;
            Inventory inventory = senderData.CharacterData.Inventory;
            Dictionary<Item, int> inventoryItems = inventory.GetItems();
            API.sendChatMessageToPlayer(target, $"~g~Money on hand: ${NamingFunctions.FormatMoney(senderData.CharacterData.Money)}");
            API.sendChatMessageToPlayer(target, "~g~|------ " + player.name + "'s Items ------|");
            int id = 0;
            foreach (KeyValuePair<Item, int> item in inventoryItems)
                API.sendChatMessageToPlayer(target, "~y~" + ++id + ": " + item.Key.Name + " x" + item.Value + " (" + item.Value * item.Key.Weight + "g)");
            API.sendChatMessageToPlayer(target, "~y~Total weight: " + inventory.GetCurrentWeight() + "/" + inventory.GetMaxWeight() + " grams");
            WeaponHash[] weapons = API.getPlayerWeapons(player);
            id = 0;
            if (weapons.Length > 0)
                API.sendChatMessageToPlayer(target, "~g~|------ " + player.name + "'s Equipped Weapons ------|");
            foreach (WeaponHash weapon in player.weapons)
                API.sendChatMessageToPlayer(target, "~y~Index " + id++ + ": " + weapon);
            API.sendChatMessageToPlayer(player, "~g~You have shown your inventory to " + target.name);
        }

        [Command("showlicenses", GreedyArg = true, Group = "Inventory Commands")]
        public void ShowLicensesCommand(Client player, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, player, targ);
            if (target == null) return;

            if (DistanceLibrary.DistanceBetween(player.position, target.position) > 5f)
            {
                API.sendChatMessageToPlayer(player, "~r~You are too far away.");
                return;
            }
            Character senderData = Account.GetPlayerCharacterData(player);
            if (senderData.CharacterData == null)
                return;
            Inventory inventory = senderData.CharacterData.Inventory;
            Dictionary<Item, int> inventoryItems = inventory.GetItems();
            API.sendChatMessageToPlayer(target, "~g~|------ " + player.name + "'s Licenses ------|");
            foreach (KeyValuePair<Item, int> item in inventoryItems)
            {
                if (item.Key.ItemType == ItemTypes.License)
                    API.sendChatMessageToPlayer(target, "~y~" + item.Key.Name);
            }
            API.sendChatMessageToPlayer(player, "~g~You have shown your licenses to " + target.name);
        }

        [Command("giveitem", GreedyArg = true, Group = "Inventory Commands")]
        public void GiveItemCommand(Client player, string targ, string itemid, string amount)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, player, targ);
            if (target == null) return;

            if (DistanceLibrary.DistanceBetween(player.position, target.position) > 5f)
            {
                API.sendChatMessageToPlayer(player, "~r~You are too far away.");
                return;
            }
            int itemAmount = 0;
            int itemId = 0;
            bool parsedAmount = Int32.TryParse(amount.ToString(), out itemAmount);
            bool parsedItem = Int32.TryParse(itemid.ToString(), out itemId);
            Character senderData = Account.GetPlayerCharacterData(player);
            if (senderData == null || senderData.CharacterData == null)
                return;
            Inventory senderInventory = senderData.CharacterData.Inventory;
            List<Item> itemList = senderInventory.GetItems().Keys.ToList();
            if (itemId <= 0 || itemId > itemList.Count)
            {
                API.sendChatMessageToPlayer(player, "~r~Invalid item id.");
                return;
            }
            Item item = itemList[itemId - 1];
            if (item == null || !parsedAmount || !parsedItem || itemAmount <= 0 || !item.Giveable)
            {
                API.sendChatMessageToPlayer(player, "~r~Item cannot be given, or has an invalid name or amount.");
                return;
            }
            Character receiverData = Account.GetPlayerCharacterData(target);
            Inventory receiverInventory = receiverData.CharacterData.Inventory;
            if (senderInventory.CanRemoveItem(item, itemAmount) && receiverInventory.CanAddItem(item, itemAmount))
            {
                senderInventory.RemoveItem(item, itemAmount);
                receiverInventory.AddItem(item, itemAmount);
                senderInventory.SaveInventory();
                receiverInventory.SaveInventory();
                API.sendChatMessageToPlayer(player, "~g~You gave " + item.Name + " (" + itemAmount + ") to " + target.name + ".");
                ChatLibrary.SendLabelEmoteMessage(API, player, "gives something to " + target.nametag);
                API.sendChatMessageToPlayer(target, "~g~You received " + item.Name + " (" + itemAmount + ") from " + player.name + ".");
            }
            else
                API.sendChatMessageToPlayer(player, "~r~You do not have " + amount + " " + item.Name);
        }

        [Command("useitem", GreedyArg = true, Group = "Inventory Commands")]
        public void UseItemCommand(Client player, string itemid)
        {
            Character senderData = Account.GetPlayerCharacterData(player);
            if (senderData == null || senderData.CharacterData == null)
                return;
            Inventory senderInventory = senderData.CharacterData.Inventory;
            int itemId = 0;
            bool parsedItem = Int32.TryParse(itemid.ToString(), out itemId);
            List<Item> itemList = senderInventory.GetItems().Keys.ToList();
            if (itemId <= 0 || itemId > itemList.Count)
            {
                API.sendChatMessageToPlayer(player, "~r~Invalid item id.");
                return;
            }
            Item item = itemList[itemId - 1];
            if (item == null)
            {
                API.sendChatMessageToPlayer(player, "~r~Invalid item id.");
                return;
            }
            if (senderInventory.CanRemoveItem(item, 1))
            {
                if (item.ItemType == ItemTypes.Consumable || item.ItemType == ItemTypes.Drug)
                {
                    senderInventory.RemoveItem(item, 1);
                    senderInventory.SaveInventory();
                    API.sendChatMessageToPlayer(player, "~g~You have consumed the " + item.Name);
                }
                item.RunUsage(player);
            }
            else
                API.sendChatMessageToPlayer(player, "~r~You do not have any " + item.Name);
        }
    }
}