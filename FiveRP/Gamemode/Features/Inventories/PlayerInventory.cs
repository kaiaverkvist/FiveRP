using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using GTANetworkServer;
using Newtonsoft.Json;
using FiveRP.Gamemode.Managers;
using GTANetworkShared;
using System;
using FiveRP.Gamemode.Library;

namespace FiveRP.Gamemode.Features.Inventories
{
    public class PlayerInventory : Inventory
    {
        private Client _player;

        public PlayerInventory(Client player, FiveRpCharacter charData)
        {
            _player = player;
            _inventoryItems = new Dictionary<Item, int>();
            if (charData.Items.Length > 0)
            {
                ItemsJson[] itemHashes = JsonConvert.DeserializeObject<ItemsJson[]>(charData.Items);
                foreach (ItemsJson itemJson in itemHashes)
                {
                    Item item = ItemsLibrary.GetItem(itemJson.ItemHash);
                    if (item != null)
                        AddItem(item, itemJson.Amount);
                }
            }
        }

        public PlayerInventory()
        {

        }

        public override void SaveInventory()
        {
            var characterData = Account.GetPlayerCharacterData(_player);
            if (characterData != null)
            {
                if (!_player.IsNull)
                {
                    var jsonInventory = JsonConvert.SerializeObject(_inventoryItems.Select(item => new { hash = item.Key.Name, amount = item.Value } ));
                    characterData.CharacterData.Items = jsonInventory;
                }
            }
        }

        public void SaveWeapons()
        {
            var characterData = Account.GetPlayerCharacterData(_player);
            var weapons = _player.weapons;
            var jsonWeapons =
                JsonConvert.SerializeObject(
                    weapons.Select(
                        weapon =>
                            new
                            {
                                hash = Enum.GetName(typeof(WeaponHash), weapon),
                                ammo = API.getPlayerWeaponAmmo(_player, weapon)
                            }));
            characterData.CharacterData.Weapons = jsonWeapons;
        }
    }
}