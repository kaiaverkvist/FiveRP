using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Features.Weapons;
using FiveRP.Gamemode.Features.Admin;
using FiveRP.Gamemode.Managers;
using Newtonsoft.Json;
using System.Linq;
using System;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Database.Tables;

namespace FiveRP.Gamemode.Features
{
    public class Death : Script
    {
        private const ulong FadeEnabled = 0x4A18E01DF2C87B86;
        private const ulong AutorespawnDisable = 0x2C2B3493FBF51C71;
        private readonly Dictionary<Client, int> _deathTimers;
        private readonly List<DeclinedDeath> _declinedDeaths = new List<DeclinedDeath>();

        public Death()
        {
            _deathTimers = new Dictionary<Client, int>();

            API.onPlayerDeath += OnPlayerDeath;
            API.onPlayerRespawn += OnPlayerRespawn;
            API.onPlayerDisconnected += OnPlayerDisconnected;
        }

        private void OnPlayerDisconnected(Client player, string reason)
        {
            RemovePlayerDeathTimer(player);
        }

        private void OnPlayerRespawn(Client player)
        {
            API.setEntityData(player, "deathmode", false);
            foreach (var death in _declinedDeaths)
            {
                if (death.Client == player)
                {
                    API.setEntityPosition(player, death.Location);
                    API.setEntityRotation(player, death.Rotation);
                    Character senderData = Account.GetPlayerCharacterData(player);
                    API.setEntityDimension(player, senderData.CharacterData.SavedDimension);
                    if (death.Weapons != null)
                    {
                        var weaponHashes = JsonConvert.DeserializeObject<WeaponsJson[]>(death.Weapons);

                        foreach (var weapon in weaponHashes)
                        {
                            API.givePlayerWeapon(player, API.weaponNameToModel(weapon.WeaponHash), weapon.Ammo, false, true);
                        }
                    }
                    _declinedDeaths.Remove(death);
                    break;
                }
            }
        }

        private void OnPlayerDeath(Client player, NetHandle entityKiller, int weapon)
        {
            API.sendNativeToPlayer(player, FadeEnabled, false);
            API.sendNativeToPlayer(player, AutorespawnDisable, true);
            API.setEntityData(player, "deathmode", true);
            API.sendChatMessageToPlayer(player, "You\'ve been killed. You will be respawned after 10 minutes, or you can /acceptdeath to respawn immediately");

            RemovePlayerDeathTimer(player);
            CreatePlayerDeathTimer(player);
        }

        private void RemovePlayerDeathTimer(Client player)
        {
            int t;
            if (_deathTimers.TryGetValue(player, out t))
            {
                if (t != -1)
                {
                    TimingLibrary.CancelQueuedAction(t);
                    _deathTimers.Remove(player);
                }
            }
        }

        private void CreatePlayerDeathTimer(Client player)
        {
            _deathTimers.Add(player, TimingLibrary.scheduleSyncAction(10 * 60 * 1000, () => { EnableRespawn(player); }));
        }

        [Command("acceptdeath", Group = "Player Commands")]
        public void AcceptDeathCommand(Client sender)
        {
            var deathmode = API.getEntityData(sender, "deathmode");
            if (deathmode == null || (bool)deathmode == false)
            {
                API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not dead.");
                return;
            }
            Character senderData = Account.GetPlayerCharacterData(sender);
            senderData.CharacterData.SavedDimension = 0;
            API.setEntityDimension(sender, 0);
            if (senderData != null && senderData.CharacterData != null && senderData.CharacterData.Inventory != null)
                senderData.CharacterData.Inventory.RemoveAllItems();
            EnableRespawn(sender);
        }

        [Command("revive", Group = "Player Commands")]
        public void DeclineDeathCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            if ((AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin, false) && AdminLibrary.OnAdminDuty(sender)) || Account.GetPlayerCharacterData(sender).CharacterData.Organization == 3)
            {
                var deathmode = API.getEntityData(target, "deathmode");
                if (deathmode == null || (bool)deathmode == false)
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ They're not dead.");
                    return;
                }

                if (!((AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins, false) && AdminLibrary.OnAdminDuty(sender))))
                {
                    if (DistanceLibrary.DistanceBetween(sender, target) > 2)
                    {
                        sender.sendChatMessage("~r~Error:~w~ You are not close enough to this player.");
                        return;
                    }
                    if (sender == target)
                    {
                        sender.sendChatMessage("~r~Error:~w~ You can't revive yourself.");
                        return;
                    }
                } else
                {
                    AlertLogging.RaiseAlert($"{sender.name} revived {target.name} as an admin.", "ADMINACTION", 3);
                }

                var weapons = API.getPlayerWeapons(target).ToList();
                _declinedDeaths.Add(new DeclinedDeath(target, API.getEntityPosition(target), API.getEntityRotation(target), JsonConvert.SerializeObject(weapons.Select(weapon => new { hash = Enum.GetName(typeof(WeaponHash), weapon), ammo = API.getPlayerWeaponAmmo(target, weapon) }))));

                EnableRespawn(target);
            }
        }

        private void EnableRespawn(Client player)
        {
            API.sendNativeToPlayer(player, FadeEnabled, true);
            API.sendNativeToPlayer(player, AutorespawnDisable, false);
            RemovePlayerDeathTimer(player);
        }
    }
    public class DeclinedDeath
    {
        public Client Client { get; set; }
        public Vector3 Location { get; set; }
        public Vector3 Rotation { get; set; }
        public string Weapons { get; set; }

        public DeclinedDeath(Client client, Vector3 location, Vector3 rotation, string weapons = null)
        {
            Client = client;
            Location = location;
            Rotation = rotation;
            Weapons = weapons;
        }
    }
}