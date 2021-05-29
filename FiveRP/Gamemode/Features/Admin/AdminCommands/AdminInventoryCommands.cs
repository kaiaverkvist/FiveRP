using System;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Managers;
using FiveRP.Gamemode.Features.Inventories;
using System.Collections.Generic;
using System.Linq;

namespace FiveRP.Gamemode.Features.Admin.AdminCommands
{
    public class AdminInventoryCommands : Script
    {
        [Command("seeitems", AddToHelpmanager = false, Group = "Admin Commands")]
        public void SeePlayerItems(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                Character targetData = Account.GetPlayerCharacterData(target);
                if (targetData.CharacterData == null)
                    return;
                Inventory inventory = targetData.CharacterData.Inventory;
                Dictionary<Item, int> inventoryItems = inventory.GetItems();
                API.sendChatMessageToPlayer(sender, "~g~|------ " + target.name + "'s Items ------|");
                API.sendChatMessageToPlayer(sender, $"~g~Money on hand: ${NamingFunctions.FormatMoney(targetData.CharacterData.Money)}");
                int id = 0;
                foreach (KeyValuePair<Item, int> item in inventoryItems)
                    API.sendChatMessageToPlayer(sender, "~y~" + ++id + ": " + item.Key.Name + " x" + item.Value + " (" + item.Value * item.Key.Weight + "g)");
                API.sendChatMessageToPlayer(sender, "~y~Total weight: " + inventory.GetCurrentWeight() + "/" + inventory.GetMaxWeight() + " grams");
            }
        }

        [Command("deleteplayeritem", Alias = "adpitem, adeleteitem, adelitem", GreedyArg = true, Group = "Inventory Commands")]
        public void DropPlayerItemCommand(Client player, string targ, string itemid, string amount)
        {
            if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.GameMasterAdmins))
            {
                Client target = PlayerLibrary.CommandClientFromString(API, player, targ);
                if (target == null) return;
                int itemAmount = 0;
                int itemId = 0;
                bool parsedAmount = Int32.TryParse(amount.ToString(), out itemAmount);
                bool parsedItem = Int32.TryParse(itemid.ToString(), out itemId);
                Character targetData = Account.GetPlayerCharacterData(target);
                if (targetData == null || targetData.CharacterData == null)
                    return;
                Inventory targetInventory = targetData.CharacterData.Inventory;
                List<Item> itemList = targetInventory.GetItems().Keys.ToList();
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
                if (targetInventory.CanRemoveItem(item, itemAmount))
                {
                    targetInventory.RemoveItem(item, itemAmount);
                    targetInventory.SaveInventory();
                    API.sendChatMessageToPlayer(player, "~r~You deleted " + item.Name + " (" + itemAmount + ")");
                    AlertLogging.RaiseAlert($"{player.name} removed an item ({item.Name}) for ID: {targ}", "ADMINACTION");
                }
                else
                    API.sendChatMessageToPlayer(player, "~r~The player does not have " + amount + " " + item.Name);
            }
        }

        [Command("giveplayeritem", Alias = "agiveitem,agitem", GreedyArg = true, Group = "Inventory Commands")]
        public void GivePlayerItemCommand(Client player, string targ, string amount, string itemname)
        {
            if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.GameMasterAdmins))
            {
                Client target = PlayerLibrary.CommandClientFromString(API, player, targ);
                if (target == null) return;

                int itemAmount = 0;
                bool parsedAmount = Int32.TryParse(amount.ToString(), out itemAmount);
                Character targetData = Account.GetPlayerCharacterData(target);
                if (targetData == null || targetData.CharacterData == null)
                    return;

                Item item = ItemsLibrary.GetItem(itemname);
                if (item == null || !parsedAmount || itemAmount <= 0)
                {
                    API.sendChatMessageToPlayer(player, "~r~Invalid name or amount.");
                    return;
                }
                Character receiverData = Account.GetPlayerCharacterData(target);
                Inventory receiverInventory = receiverData.CharacterData.Inventory;
                if (receiverInventory.CanAddItem(item, itemAmount))
                {
                    receiverInventory.AddItem(item, itemAmount);
                    receiverInventory.SaveInventory();
                    API.sendChatMessageToPlayer(player, "~g~You gave " + item.Name + " (" + itemAmount + ") to " + target.name + ".");
                    API.sendChatMessageToPlayer(target, "~g~You received " + item.Name + " (" + itemAmount + ") from " + player.name + ".");
                    AlertLogging.RaiseAlert($"{player.name} spawned an item ({item.Name}) for ID: {targ}", "ADMINACTION");
                }
                else
                    API.sendChatMessageToPlayer(player, "~r~Not enough storage to add the item.");
            }
        }

        [Command("giveweapon", AddToHelpmanager = false, GreedyArg = true, Group = "Admin Commands")]
        public void WeaponCommand(Client sender, string targ, WeaponHash weapon)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins))
            {
                if (AdminLibrary.OnAdminDuty(sender))
                {
                    if (Enum.IsDefined(typeof(WeaponHash), weapon))
                    {
                        API.givePlayerWeapon(target, weapon, 1, true, true);
                    }
                    else sender.sendChatMessage("Invalid weapon. ~b~Try using correct capitalization.");
                    API.sendChatMessageToPlayer(target, $"Admin gave you {Enum.GetName(typeof(WeaponHash), weapon)}");
                    AlertLogging.RaiseAlert($"{sender.name} spawned a weapon ({weapon}) for ID: {targ}", "ADMINACTION");
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                }
            }
        }

        [Command("giveammo", AddToHelpmanager = false, GreedyArg = false, Group = "Admin Commands")]
        public void AmmoCommand(Client sender, string targ, WeaponHash weapon, int count)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins))
            {
                if (AdminLibrary.OnAdminDuty(sender))
                {
                    if (Enum.IsDefined(typeof(WeaponHash), weapon))
                    {
                        API.setPlayerWeaponAmmo(target, weapon, count);
                    }
                    else sender.sendChatMessage("Invalid weapon. ~b~Try using correct capitalization and stuff.");
                    API.sendChatMessageToPlayer(target,
                        $"Admin added {count} ammo to your {Enum.GetName(typeof(WeaponHash), weapon)}");
                    AlertLogging.RaiseAlert($"{sender.name} spawned an {count} ({weapon}) for ID: {targ}", "ADMINACTION");
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                }
            }
        }
    }
}