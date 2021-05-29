using System.Collections.Generic;
using FiveRP.Gamemode.Features.Emergency.Police.Jail;
using FiveRP.Gamemode.Features.Organizations;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Features.Inventories;
using FiveRP.Gamemode.Database.Tables;
using System.Linq;
using FiveRP.Gamemode.Features.Customization;

namespace FiveRP.Gamemode.Features.Emergency.Police
{
    public class PoliceCommands : Script
    {
        public static readonly Vector3 PoliceDutyPoint = new Vector3(452.3714, -980.3326, 30.68958);

        private static readonly List<Client> OnDutyPoliceList = new List<Client>();

        public static readonly PedHash[] CopUniforms = {PedHash.Cop01SFY, PedHash.Cop01SMY, PedHash.TrafficWarden, PedHash.KorLieut01GMY, PedHash.Sheriff01SMY, PedHash.Sheriff01SFY, PedHash.Ranger01SMY, PedHash.Ranger01SFY, PedHash.CiaSec01SMM, PedHash.Pilot01SMY, PedHash.Hwaycop01SMY, PedHash.Swat01SMY, PedHash.Paper, PedHash.Michelle};
        public static readonly WeaponHash[] CopLoadout = { WeaponHash.CombatPistol, WeaponHash.Nightstick, WeaponHash.StunGun, WeaponHash.PumpShotgun, WeaponHash.Flashlight, WeaponHash.Flare };

        public PoliceCommands()
        {
            API.onResourceStart += OnResourceStart;
            API.onPlayerDeath += OnPlayerDeath;
            API.onPlayerDisconnected += OnPlayerDisconnect;
        }


        private void OnResourceStart()
        {
            API.createTextLabel("~b~DUTY POINT~w~\nUse /duty and /uniform to go on police duty.", PoliceDutyPoint, 10f, 0.5f);

            var blip = API.createBlip(PoliceDutyPoint);
            API.setBlipSprite(blip, 60);
            API.setBlipColor(blip, 38);
            API.setBlipShortRange(blip, true);
        }

        private void OnPlayerDisconnect(Client player, string reason)
        {
            if (IsOnPoliceDuty(player))
            {
                var senderData = Account.GetPlayerCharacterData(player);
                if (senderData != null && senderData.CharacterData != null)
                    API.setPlayerSkin(player, API.pedNameToModel(senderData.CharacterData.Skin));
                player.removeAllWeapons();
                OnDutyPoliceList.Remove(player);
                API.setEntityData(player, "uniform", false);
            }
        }

        private void OnPlayerDeath(Client player, NetHandle entityKiller, int weapon)
        {
            if (IsOnPoliceDuty(player))
            {
                var senderData = Account.GetPlayerCharacterData(player);
                API.setPlayerSkin(player, API.pedNameToModel(senderData.CharacterData.Skin));
                senderData.CharacterData.WardRobe.EquipVariants();
                API.setEntityData(player, "uniform", false);
                // Remove from the duty pool.
                OnDutyPoliceList.Remove(player);
            }
        }

        public static bool IsOnPoliceDuty(Client player)
        {
            return OnDutyPoliceList.Contains(player);
        }

        public static void CopDuty(API api, Client sender)
        {
            var senderData = Account.GetPlayerCharacterData(sender);

            if (senderData.CharacterData.Organization > 0 && OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "COPDUTY"))
            {
                if (IsOnPoliceDuty(sender))
                {
                    api.sendChatMessageToPlayer(sender, "~#388E8E~", "You are no longer on duty.");
                    api.setPlayerArmor(sender, 0);
                    api.setEntityData(sender, "uniform", false);
                    api.setPlayerSkin(sender, api.pedNameToModel(senderData.CharacterData.Skin));
                    senderData.CharacterData.WardRobe.EquipVariants();

                    foreach (var weaponType in CopLoadout)
                    {
                        api.removePlayerWeapon(sender, weaponType);
                    }

                    OnDutyPoliceList.Remove(sender);
                }
                else
                {
                    if (api.getPlayerWeapons(sender).Length > 0)
                    {
                        api.sendChatMessageToPlayer(sender, "~#388E8E~",
                            "You cannot go on police duty while you have weapons equipped.");
                    }
                    else
                    {
                        api.sendChatMessageToPlayer(sender, "~#388E8E~", "You are now on duty.");

                        OnDutyPoliceList.Add(sender);
                    }
                }
            }
            else
            {
                api.sendChatMessageToPlayer(sender, "You do not have the right access required for this.");
            }
        }

        [Command("policeduty", Alias = "pduty", Group = "Police Commands")]
        public void CopDutyCommand(Client sender)
        {
            CopDuty(API, sender);
        }

        [Command("db", Group = "Emergency Commands")]
        public void DetectiveBureauCommand(Client sender, string armor = "")
        {
            var senderData = Account.GetPlayerCharacterData(sender);

            if (senderData.CharacterData.Organization > 0 &&
                OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "COPDUTY"))
            {
                if (API.getEntityData(sender, "uniform") == true)
                {
                    API.sendChatMessageToPlayer(sender, "You're already equipped with your detective equipment.");
                    return;
                }
                if (!IsOnPoliceDuty(sender))
                {
                    API.sendChatMessageToPlayer(sender, "You must be on duty to use this command.");
                    return;
                }
                API.setPlayerHealth(sender, 100);
                API.setEntityData(sender, "uniform", true);
                if (armor == "armor")
                    API.setPlayerArmor(sender, 100);
                else
                    API.sendChatMessageToPlayer(sender, "Use /db armor to equip an armor too.");

                foreach (var weaponType in PoliceCommands.CopLoadout)
                {
                    API.givePlayerWeapon(sender, weaponType, 500, false, true);
                }
            }
            else if (senderData.CharacterData.Organization == 0)
            {
                API.sendChatMessageToPlayer(sender, "You aren't in any position to go on duty.");
            }
        }

        [Command("frisk", Group = "Police Commands")]
        public void FriskPlayer(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            var playerData = Account.GetPlayerCharacterData(sender);
            var targetData = Account.GetPlayerCharacterData(target);
            if (playerData != null)
            {
                var faction = OrganizationHandler.GetOrganizationData(playerData.CharacterData.Organization);
                if (faction != null)
                {
                    if (OrganizationHandler.GetOrganizationFlag(faction.Id, "LAW"))
                    {
                        if (IsOnPoliceDuty(sender))
                        {
                            if (DistanceLibrary.DistanceBetween(sender.position, target.position) > 5f)
                            {
                                API.sendChatMessageToPlayer(sender, "~r~You are too far away.");
                                return;
                            }
                            Inventory inventory = targetData.CharacterData.Inventory;
                            Dictionary<Item, int> inventoryItems = inventory.GetItems();
                            API.sendChatMessageToPlayer(sender, "~g~|------ " + target.name + "'s Items ------|");
                            API.sendChatMessageToPlayer(sender, $"~g~Money on hand: ${NamingFunctions.FormatMoney(targetData.CharacterData.Money)}");
                            int id = 0;
                            foreach (KeyValuePair<Item, int> item in inventoryItems)
                                API.sendChatMessageToPlayer(sender, "~y~" + ++id + ": " + item.Key.Name + " x" + item.Value + " (" + item.Value * item.Key.Weight + "g)");
                            API.sendChatMessageToPlayer(sender, "~y~Total weight: " + inventory.GetCurrentWeight() + "/" + inventory.GetMaxWeight() + " grams");
                            API.sendChatMessageToPlayer(target, "~g~You were frisked by " + sender.name);
                            API.sendChatMessageToPlayer(sender, "~g~|------ " + target.name + "'s Weapons ------|");
                            id = 0;
                            foreach (var weapon in target.weapons)
                                API.sendChatMessageToPlayer(sender, "~y~" + ++id + ": " + weapon.ToString());
                        }
                        else
                            API.sendChatMessageToPlayer(sender, $"~r~You must be on duty to use this command.");
                    }
                    else
                        API.sendChatMessageToPlayer(sender, $"~r~You must be a law officer to use this command.");
                }
            }
        }

        [Command("seizeweapons", Group = "Police Commands")]
        public void SeizePlayerWeapons(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            var playerData = Account.GetPlayerCharacterData(sender);

            if (playerData != null)
            {
                var faction = OrganizationHandler.GetOrganizationData(playerData.CharacterData.Organization);
                if (faction != null)
                {
                    if (OrganizationHandler.GetOrganizationFlag(faction.Id, "LAW"))
                    {
                        if (IsOnPoliceDuty(sender))
                        {
                            if (DistanceLibrary.DistanceBetween(sender.position, target.position) > 5f)
                            {
                                API.sendChatMessageToPlayer(sender, "~r~You are too far away.");
                                return;
                            }
                            target.removeAllWeapons();
                            API.sendChatMessageToPlayer(target, "~r~Your weapons were seized by " + sender.name);
                            API.sendChatMessageToPlayer(target, "~g~You seized " + target.name + "'s weapons.");
                        }
                        else
                            API.sendChatMessageToPlayer(sender, $"~r~You must be on duty to use this command.");
                    }
                    else
                        API.sendChatMessageToPlayer(sender, $"~r~You must be a law officer to use this command.");
                }
            }
        }

        [Command("seizeitem", Group = "Police Commands")]
        public void SeizePlayerItem(Client sender, string targ, int itemid, int amount = 1)
        {
            Client player = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (player == null) return;
            Character playerData = Account.GetPlayerCharacterData(player);
            if (playerData == null || playerData.CharacterData == null)
                return;
            Inventory targetInventory = playerData.CharacterData.Inventory;
            List<Item> itemList = targetInventory.GetItems().Keys.ToList();
            if (itemid <= 0 || itemid > itemList.Count)
            {
                API.sendChatMessageToPlayer(player, "~r~Invalid item id.");
                return;
            }
            Item item = itemList[itemid - 1];
            if (item == null || amount <= 0)
            {
                API.sendChatMessageToPlayer(player, "~r~Invalid item id or amount.");
                return;
            }
            if (targetInventory.CanRemoveItem(item, amount))
            {
                targetInventory.RemoveItem(item, amount);
                targetInventory.SaveInventory();
                API.sendChatMessageToPlayer(player, "~r~Your " + item.Name + " (" + amount + ")" + " was seized by " + sender.name);
                API.sendChatMessageToPlayer(player, "~g~You seized " + item.Name + " (" + amount + ")" + " from " + player.name);
            }
            else
                API.sendChatMessageToPlayer(player, "~r~" + player.name + " does not have " + amount + " " + item.Name);
        }

        [Command("givelicense", "~y~USAGE:~w~ /givelicense target licenseid (1=driving license, 2=PF (SD only), 3=CCW (SD only))")]
        public void GiveLicense(Client sender, string targ, int license)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            var playerData = Account.GetPlayerCharacterData(sender);

            if (playerData != null)
            {
                var faction = OrganizationHandler.GetOrganizationData(playerData.CharacterData.Organization);
                if (faction != null)
                {
                    if (OrganizationHandler.GetOrganizationFlag(faction.Id, "LAW"))
                    {
                        if (IsOnPoliceDuty(sender))
                        {
                            var targetData = Account.GetPlayerCharacterData(target);
                            if (targetData != null)
                            {
                                PlayerInventory targetInventory = targetData.CharacterData.Inventory;
                                Item drivingLicense = ItemsLibrary.GetItem("Driving License");
                                Item pfLicense = ItemsLibrary.GetItem("PF License");
                                Item ccwLicense = ItemsLibrary.GetItem("CCW License");
                                Item givenItem = null;
                                if (license == 1 && targetInventory.CanAddItem(drivingLicense, 1))
                                {
                                    targetInventory.AddItem(drivingLicense, 1);
                                    givenItem = drivingLicense;
                                }
                                else if (license == 2 && targetInventory.CanAddItem(pfLicense, 1))
                                {
                                    targetInventory.AddItem(pfLicense, 1);
                                    givenItem = pfLicense;
                                }
                                else if (license == 3 && targetInventory.CanAddItem(ccwLicense, 1))
                                {
                                    targetInventory.AddItem(ccwLicense, 1);
                                    givenItem = ccwLicense;
                                }

                                if (givenItem != null)
                                {
                                    API.sendChatMessageToPlayer(sender, $"~g~You received a {givenItem.Name} from {sender.name}.");
                                    API.sendChatMessageToPlayer(sender, $"~g~You gave a {givenItem.Name} to {target.name}.");
                                }
                                else
                                    API.sendChatMessageToPlayer(sender, $"~r~Invalid license ID (1-driving, 2-PF (SD only), 3-CCW (SD only))");
                            }
                        }
                        else
                            API.sendChatMessageToPlayer(sender, $"~r~You must be on duty to use this command.");
                    }
                    else
                        API.sendChatMessageToPlayer(sender, $"~r~You must be a law officer to use this command.");
                }
            }
        }

        [Command("takelicense", "~y~USAGE:~w~ /takelicense target licenseid (1=driving license, 2=PF (SD only), 3=CCW (SD only))")]
        public void TakeLicense(Client sender, string targ, int license)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            var playerData = Account.GetPlayerCharacterData(sender);

            if (playerData != null)
            {
                var faction = OrganizationHandler.GetOrganizationData(playerData.CharacterData.Organization);
                if (faction != null)
                {
                    if (OrganizationHandler.GetOrganizationFlag(faction.Id, "LAW"))
                    {
                        if (IsOnPoliceDuty(sender))
                        {
                            var targetData = Account.GetPlayerCharacterData(target);
                            if (targetData != null)
                            {
                                PlayerInventory targetInventory = targetData.CharacterData.Inventory;
                                Item drivingLicense = ItemsLibrary.GetItem("Driving License");
                                Item pfLicense = ItemsLibrary.GetItem("PF License");
                                Item ccwLicense = ItemsLibrary.GetItem("CCW License");
                                Item givenItem = null;
                                if (license == 1 && targetInventory.CanRemoveItem(drivingLicense, 1))
                                {
                                    targetInventory.RemoveItem(drivingLicense, 1);
                                    givenItem = drivingLicense;
                                }
                                else if (license == 2 && targetInventory.CanRemoveItem(pfLicense, 1))
                                {
                                    targetInventory.RemoveItem(pfLicense, 1);
                                    givenItem = pfLicense;
                                }
                                else if (license == 3 && targetInventory.CanRemoveItem(ccwLicense, 1))
                                {
                                    targetInventory.RemoveItem(ccwLicense, 1);
                                    givenItem = ccwLicense;
                                }

                                if (givenItem != null)
                                {
                                    API.sendChatMessageToPlayer(sender, $"~r~{sender.name} took your {givenItem.Name}.");
                                    API.sendChatMessageToPlayer(sender, $"~g~You took {givenItem.Name} from {target.name}.");
                                }
                                else
                                    API.sendChatMessageToPlayer(sender, $"~r~Invalid license ID (2/3 SD only) or the player doesn't have the license.");
                            }
                        }
                        else
                            API.sendChatMessageToPlayer(sender, $"~r~You must be on duty to use this command.");
                    }
                    else
                        API.sendChatMessageToPlayer(sender, $"~r~You must be a law officer to use this command.");
                }
            }
        }

        [Command("arrest", Group = "Police Commands")]
        public void ArrestCommand(Client sender, string targ, int time)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            var playerData = Account.GetPlayerCharacterData(sender);
            
            if (playerData != null)
            {
                var faction = OrganizationHandler.GetOrganizationData(playerData.CharacterData.Organization);
                if (faction != null)
                {
                    if (OrganizationHandler.GetOrganizationFlag(faction.Id, "LAW"))
                    {
                        if (IsOnPoliceDuty(sender))
                        {
                            if (DistanceLibrary.DistanceBetween(target.position, PoliceJail.ArrestPoint) < 5f)
                            {
                                if (time > 3 && time <= 900)
                                {
                                    var targetData = Account.GetPlayerCharacterData(target);
                                    if (targetData != null)
                                    {
                                        targetData.CharacterData.JailTime = time;
                                        API.setEntityPosition(target, PoliceJail.GetRandomJailPosition());
                                        API.setEntityDimension(target, 0);
                                        API.sendChatMessageToPlayer(target, $"~b~You have been arrested for {time} minutes.");
                                        API.sendChatMessageToPlayer(sender, $"~b~You have arrested {target.name} for {time} minutes.");

                                        API.removeAllPlayerWeapons(target);
                                        UncuffPlayer(API, target);
                                    }
                                }
                                else
                                {
                                    API.sendChatMessageToPlayer(sender, "The time of the arrest is not within the acceptable range. [4, 900]");
                                }
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(sender, "The person you are trying to arrest must be close to the arrest point.");
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(sender, "You must be on police duty to do this.");
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not in a faction that can do this.");
                    }
                }
            }
        }

        [Command("cuff", Group = "Police Commands")]
        public void CuffCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            if (DistanceLibrary.DistanceBetween(sender.position, target.position) < 3f)
            {
                var playerData = Account.GetPlayerCharacterData(sender);
                var faction = OrganizationHandler.GetOrganizationData(playerData.CharacterData.Organization);

                if (API.hasEntityData(sender, "cuffed"))
                {
                    if (API.getEntityData(sender, "cuffed") == true)
                    {
                        API.sendChatMessageToPlayer(sender, "You can't cuff someone while you're cuffed.");
                    }
                }

                if (OrganizationHandler.GetOrganizationFlag(faction.Id, "LAW"))
                {
                    // Ensure the player is relatively slow moving
                    if (API.getPlayerVelocity(sender).Length() < 1f)
                    {
                        API.sendChatMessageToPlayer(sender, $"You have cuffed {target.name}.");
                        CuffPlayer(API, target);
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not in a faction that can do this.");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "You're too far away to do this.");
            }
        }

        [Command("uncuff", Group = "Police Commands")]
        public void UncuffCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            if (DistanceLibrary.DistanceBetween(sender.position, target.position) < 3f)
            {
                if (API.hasEntityData(sender, "cuffed"))
                {
                    if (API.getEntityData(sender, "cuffed") == true)
                    {
                        API.sendChatMessageToPlayer(sender, "You can't uncuff someone while you're cuffed.");
                    }
                }

                UncuffPlayer(API, target);
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "You're too far away to do this.");
            }
        }

        public static void UncuffPlayer(API api, Client player)
        {
            api.setEntityData(player, "cuffed", false);
            api.sendNativeToPlayer(player, Hash.UNCUFF_PED, player.handle);
            api.stopPlayerAnimation(player);
        }

        public static void CuffPlayer(API api, Client player)
        {
            // Cuff the player by forcing an anim on the player and try to restrict them from using their guns.
            api.playPlayerAnimation(player, 49, "mp_arresting", "idle");
            api.sendNativeToPlayer(player, Hash.SET_ENABLE_HANDCUFFS, player.handle, true);
            api.sendNativeToPlayer(player, Hash.SET_CURRENT_PED_WEAPON, WeaponHash.Unarmed);
            api.setEntityData(player, "cuffed", true);
        }
    }
}