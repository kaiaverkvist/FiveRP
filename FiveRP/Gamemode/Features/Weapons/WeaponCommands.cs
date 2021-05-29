using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Inventories;
using FiveRP.Gamemode.Managers;
using System;
using System.Collections.Generic;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Features.Emergency.Police;
using Newtonsoft.Json;

namespace FiveRP.Gamemode.Features.Weapons
{
    public class WeaponCommands : Script
    {
        [Command("dropweapon", Group = "Weapon Commands")]
        public void DropWeaponCommand(Client player, int index, string confirm = "")
        {
            var weapons = API.getPlayerWeapons(player);

            if (weapons.Any())
            {
                if (!weapons.ElementAtOrDefault(index).Equals(default(WeaponHash)))
                {
                    if (confirm != "confirm")
                    {
                        API.sendChatMessageToPlayer(player,
                            $"Please type ~g~/dropweapon {index} confirm~w~ to confirm that you wish to drop this weapon.");
                        API.sendChatMessageToPlayer(player,
                            "~r~This will delete the weapon permanently!");
                    }
                    else
                    {
                        API.removePlayerWeapon(player, weapons.ElementAt(index));
                        ChatLibrary.SendLabelEmoteMessage(API, player, "drops a weapon on the ground.");
                        Character senderData = Account.GetPlayerCharacterData(player);
                        if (senderData.CharacterData == null)
                            return;
                        PlayerInventory inventory = senderData.CharacterData.Inventory;
                        inventory.SaveWeapons();
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "You can't drop this weapon.");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You don't have any weapons to drop.");
            }
        }

        [Command("weapons", Group = "Weapon Commands")]
        public void WeaponsCommand(Client player)
        {
            var weapons = API.getPlayerWeapons(player);

            if (weapons.Any())
            {
                for (int i = 0; i < weapons.Length; i++)
                {
                    API.sendChatMessageToPlayer(player, $"Weapon: {weapons[i]}, index: {i}");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You don't have any weapons to drop.");
            }
        }

        private bool CanEquipWeapon(Client player, Item weaponItem)
        {
            WeaponHash[] weaponList = player.weapons;
            ItemTypes weaponType = weaponItem.ItemType;
            if (weaponType == ItemTypes.MeleeWeapon || weaponType == ItemTypes.PistolWeapon || weaponType == ItemTypes.ExplosiveWeapon)
                return true;

            foreach (WeaponHash weaponHash in weaponList)
            {
                Item weaponItemEquipped = ItemsLibrary.GetItem(Enum.GetName(typeof(WeaponHash), weaponHash));
                if (weaponItemEquipped.ItemType == ItemTypes.RifleWeapon || weaponItemEquipped.ItemType == ItemTypes.ShotgunWeapon || weaponItemEquipped.ItemType == ItemTypes.RocketWeapon || weaponItemEquipped.ItemType == ItemTypes.SniperWeapon)
                {
                    API.sendChatMessageToPlayer(player, "You cannot equip two large weapons.");
                    return false;
                }
            }
            return true;
        }

        [Command("unequip", Group = "Weapon Commands")]
        public void UnEquipWeapon(Client player, int index)
        {
            if (PoliceCommands.IsOnPoliceDuty(player) || MedicCommands.IsOnMedicDuty(player))
            {
                API.sendChatMessageToPlayer(player, "You cannot unequip weapons from your inventory while on duty.");
                return;
            }

            var weapons = API.getPlayerWeapons(player);
            if (index >= weapons.Length)
            {
                API.sendChatMessageToPlayer(player, "Invalid index, use /weapons to get the index.");
                return;
            }

            if (weapons.Any())
            {
                if (!weapons.ElementAtOrDefault(index).Equals(default(WeaponHash)))
                {
                    Character senderData = Account.GetPlayerCharacterData(player);
                    if (senderData.CharacterData == null)
                        return;
                    PlayerInventory inventory = senderData.CharacterData.Inventory;
                    Item weaponItem = ItemsLibrary.GetItem(Enum.GetName(typeof(WeaponHash), weapons.ElementAt(index)));
                    if (inventory == null || weaponItem == null)
                        return;
                    int ammoCount = API.getPlayerWeaponAmmo(player, weapons.ElementAt(index));
                    Item ammoItem = null;
                    if (weaponItem.ItemType == ItemTypes.RifleWeapon)
                        ammoItem = ItemsLibrary.GetItem("Rifle ammo");
                    else if (weaponItem.ItemType == ItemTypes.PistolWeapon)
                        ammoItem = ItemsLibrary.GetItem("Pistol ammo");
                    else if (weaponItem.ItemType == ItemTypes.ShotgunWeapon)
                        ammoItem = ItemsLibrary.GetItem("Shotgun shell");
                    else if (weaponItem.ItemType == ItemTypes.SniperWeapon)
                        ammoItem = ItemsLibrary.GetItem("Long range ammo");
                    else if (weaponItem.ItemType == ItemTypes.RocketWeapon)
                        ammoItem = ItemsLibrary.GetItem("Rocket");
                    else
                        ammoItem = null;

                    if (inventory.CanAddItem(weaponItem, 1))
                    {
                        if (ammoItem == null) //explosive or melee weapon
                        {
                            if (ammoCount == 0) // melee
                                ammoCount = 1;
                            inventory.AddItem(weaponItem, ammoCount);
                            API.removePlayerWeapon(player, weapons.ElementAt(index));
                            inventory.SaveWeapons();
                        }
                        else
                        {
                            inventory.AddItem(weaponItem, 1);
                            if (inventory.CanAddItem(ammoItem, ammoCount))
                            {
                                if (ammoCount > 0)
                                    inventory.AddItem(ammoItem, ammoCount);
                                API.removePlayerWeapon(player, weapons.ElementAt(index));
                                inventory.SaveWeapons();
                            }
                            else
                                inventory.RemoveItem(weaponItem, 1);
                        }
                        ChatLibrary.SendLabelEmoteMessage(API, player, "hides a weapon.");
                    }
                    else
                        API.sendChatMessageToPlayer(player, "You cannot store this item in your inventory, drop some items first.");
                }
                else
                    API.sendChatMessageToPlayer(player, "Invalid weapon index, type /weapons to list your weapons.");
            }
        }

        [Command("equip", Group = "Weapon Commands")]
        public void EquipWeapon(Client player, int itemid, int ammo_count = 0)
        {
            if (PoliceCommands.IsOnPoliceDuty(player) || MedicCommands.IsOnMedicDuty(player))
            {
                API.sendChatMessageToPlayer(player, "You cannot equip weapons from your inventory while on duty.");
                return;
            }

            if (ammo_count < 0)
            {
                API.sendChatMessageToPlayer(player, "Invalid ammo amount.");
                return;
            }

            itemid -= 1;
            Character senderData = Account.GetPlayerCharacterData(player);
            if (senderData.CharacterData == null)
                return;
            PlayerInventory inventory = senderData.CharacterData.Inventory;
            var itemList = inventory.GetItems();
            if (itemList.Count <= itemid)
            {
                API.sendChatMessageToPlayer(player, "Invalid inventory index, use /myitems to get the index.");
                return;
            }
            Item weaponItem = itemList.ElementAt(itemid).Key;
            if (weaponItem == null || !CanEquipWeapon(player, weaponItem))
                return;
            Item ammo = null;
            if (weaponItem.ItemType == ItemTypes.RifleWeapon)
                ammo = ItemsLibrary.GetItem("Rifle ammo");
            else if (weaponItem.ItemType == ItemTypes.PistolWeapon)
                ammo = ItemsLibrary.GetItem("Pistol ammo");
            else if (weaponItem.ItemType == ItemTypes.ShotgunWeapon)
                ammo = ItemsLibrary.GetItem("Shotgun shell");
            else if (weaponItem.ItemType == ItemTypes.SniperWeapon)
                ammo = ItemsLibrary.GetItem("Long range ammo");
            else if (weaponItem.ItemType == ItemTypes.RocketWeapon)
                ammo = ItemsLibrary.GetItem("Rocket");
            else if (weaponItem.ItemType == ItemTypes.ExplosiveWeapon || weaponItem.ItemType == ItemTypes.MeleeWeapon)
                ammo = null;
            else
            {
                API.sendChatMessageToPlayer(player, "Please choose a weapon.");
                return;
            }
            int ammoCount = 0;
            if (ammo == null)
                ammoCount = itemList[weaponItem];
            else if (itemList.ContainsKey(ammo))
                ammoCount = itemList[ammo];
            if (ammoCount < ammo_count)
            {
                API.sendChatMessageToPlayer(player, "Not enough ammo.");
                return;
            }
            if (ammo == null) // melee or explosive
            {
                if (inventory.CanRemoveItem(weaponItem, 1))
                {
                    inventory.RemoveItem(weaponItem, 1);
                    API.givePlayerWeapon(player, API.weaponNameToModel(weaponItem.Name), 1, false, true);
                    inventory.SaveWeapons();
                }
            }
            else
            {
                if (inventory.CanRemoveItem(weaponItem, 1) && inventory.CanRemoveItem(ammo, ammo_count))
                {
                    inventory.RemoveItem(weaponItem, 1);
                    inventory.RemoveItem(ammo, ammo_count);
                    API.givePlayerWeapon(player, API.weaponNameToModel(weaponItem.Name), ammo_count, false, true);
                    ChatLibrary.SendLabelEmoteMessage(API, player, "reaches for a weapon.");
                    inventory.SaveWeapons();
                }
            }
        }

        [Command("unload", Group = "Weapon Commands")]
        public void UnloadWeapon(Client player, int index, int ammo_count)
        {
            if (PoliceCommands.IsOnPoliceDuty(player) || MedicCommands.IsOnMedicDuty(player))
            {
                API.sendChatMessageToPlayer(player, "You cannot unload any ammo from your weapons while on duty.");
                return;
            }

            if (ammo_count <= 0)
            {
                API.sendChatMessageToPlayer(player, "Invalid ammo amount.");
                return;
            }
            var weapons = API.getPlayerWeapons(player);
            if (index >= weapons.Length || index < 0)
            {
                API.sendChatMessageToPlayer(player, "Invalid weapon index, use /weapons to get the index.");
                return;
            }

            if (weapons.Any())
            {
                if (!weapons.ElementAtOrDefault(index).Equals(default(WeaponHash)))
                {
                    Character senderData = Account.GetPlayerCharacterData(player);
                    if (senderData.CharacterData == null)
                        return;
                    PlayerInventory inventory = senderData.CharacterData.Inventory;
                    Item weaponItem = ItemsLibrary.GetItem(Enum.GetName(typeof(WeaponHash), weapons.ElementAt(index)));
                    if (inventory == null || weaponItem == null)
                        return;
                    int ammoCount = API.getPlayerWeaponAmmo(player, weapons.ElementAt(index));
                    if (ammoCount < ammo_count)
                    {
                        API.sendChatMessageToPlayer(player, "Not enough ammo.");
                        return;
                    }
                    Item ammoItem = null;
                    if (weaponItem.ItemType == ItemTypes.RifleWeapon)
                        ammoItem = ItemsLibrary.GetItem("Rifle ammo");
                    else if (weaponItem.ItemType == ItemTypes.PistolWeapon)
                        ammoItem = ItemsLibrary.GetItem("Pistol ammo");
                    else if (weaponItem.ItemType == ItemTypes.ShotgunWeapon)
                        ammoItem = ItemsLibrary.GetItem("Shotgun shell");
                    else if (weaponItem.ItemType == ItemTypes.SniperWeapon)
                        ammoItem = ItemsLibrary.GetItem("Long range ammo");
                    else if (weaponItem.ItemType == ItemTypes.RocketWeapon)
                        ammoItem = ItemsLibrary.GetItem("Rocket");
                    else
                    {
                        API.sendChatMessageToPlayer(player, "You cannot unload ammo from an explosive or melee weapon.");
                        return;
                    }

                    if (inventory.CanAddItem(ammoItem, ammo_count))
                    {
                        //removePlayerWeapon does not remove ammo, only removeAllPlayerWeapons does, so we have to use this workaround for now. Confirmed bug on GTAN side.
                        inventory.AddItem(ammoItem, ammo_count);
                        API.givePlayerWeapon(player, API.weaponNameToModel(weaponItem.Name), ammoCount - ammo_count, false, true);
                        ChatLibrary.SendLabelEmoteMessage(API, player, "unloads ammo from the weapon.");
                        inventory.SaveWeapons();
                    }
                }
            }
        }

        [Command("reload", Group = "Weapon Commands")]
        public void ReloadWeapon(Client player, int index, int ammo_count)
        {
            if (PoliceCommands.IsOnPoliceDuty(player) || MedicCommands.IsOnMedicDuty(player))
            {
                API.sendChatMessageToPlayer(player, "You cannot load any ammo in your weapons while on duty.");
                return;
            }

            if (ammo_count <= 0)
            {
                API.sendChatMessageToPlayer(player, "Invalid ammo amount.");
                return;
            }

            if (player.weapons.Any())
            {
                if (!player.weapons.ElementAtOrDefault(index).Equals(default(WeaponHash)))
                {
                    Character senderData = Account.GetPlayerCharacterData(player);
                    if (senderData.CharacterData == null)
                        return;
                    PlayerInventory inventory = senderData.CharacterData.Inventory;
                    var itemList = inventory.GetItems();
                    Item weaponItem = ItemsLibrary.GetItem(Enum.GetName(typeof(WeaponHash), player.weapons.ElementAt(index)));
                    if (weaponItem == null)
                        return;
                    Item ammo = null;
                    if (weaponItem.ItemType == ItemTypes.RifleWeapon)
                    {
                        ammo = ItemsLibrary.GetItem("Rifle ammo");
                        if (!itemList.ContainsKey(ammo))
                        {
                            API.sendChatMessageToPlayer(player, "You need rifle ammo in your inventory.");
                            return;
                        }
                    }
                    else if (weaponItem.ItemType == ItemTypes.PistolWeapon)
                    {
                        ammo = ItemsLibrary.GetItem("Pistol ammo");
                        if (!itemList.ContainsKey(ammo))
                        {
                            API.sendChatMessageToPlayer(player, "You need pistol ammo in your inventory.");
                            return;
                        }
                    }
                    else if (weaponItem.ItemType == ItemTypes.ShotgunWeapon)
                    {
                        ammo = ItemsLibrary.GetItem("Shotgun shell");
                        if (!itemList.ContainsKey(ammo))
                        {
                            API.sendChatMessageToPlayer(player, "You need shotgun shells in your inventory.");
                            return;
                        }
                    }
                    else if (weaponItem.ItemType == ItemTypes.SniperWeapon)
                    {
                        ammo = ItemsLibrary.GetItem("Long range ammo");
                        if (!itemList.ContainsKey(ammo))
                        {
                            API.sendChatMessageToPlayer(player, "You need long range ammo in your inventory.");
                            return;
                        }
                    }
                    else if (weaponItem.ItemType == ItemTypes.RocketWeapon)
                    {
                        ammo = ItemsLibrary.GetItem("Rocket");
                        if (!itemList.ContainsKey(ammo))
                        {
                            API.sendChatMessageToPlayer(player, "You need a rocket in your inventory.");
                            return;
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "Please choose a valid weapon from /weapons, you cannot add ammunation in melee or explosive weapons.");
                        return;
                    }
                    if (inventory.CanRemoveItem(ammo, ammo_count))
                    {
                        inventory.RemoveItem(ammo, ammo_count);
                        int ammoCount = API.getPlayerWeaponAmmo(player, player.weapons.ElementAt(index));
                        API.setPlayerWeaponAmmo(player, API.weaponNameToModel(weaponItem.Name), ammoCount + ammo_count);
                        ChatLibrary.SendLabelEmoteMessage(API, player, "is loading ammo in the weapon.");
                        inventory.SaveWeapons();
                    }
                    else
                        API.sendChatMessageToPlayer(player, "Not enough ammo.");
                }
            }
        }
    }
}