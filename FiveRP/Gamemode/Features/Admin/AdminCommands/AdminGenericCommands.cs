using System;
using System.IO;
using System.Linq;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using Newtonsoft.Json;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Inventories;

namespace FiveRP.Gamemode.Features.Admin.AdminCommands
{
    public class AdminGenericCommands : Script
    {
        public AdminGenericCommands()
        {
            API.onPlayerDisconnected += OnPlayerDisconnected;
        }

        private void OnPlayerDisconnected(Client player, string reason)
        {
            if (AdminLibrary.OnDutyAdmins.Contains(player))
            {
                AdminLibrary.OnDutyAdmins.Remove(player);
            }
        }

        [Command("ahelp", AddToHelpmanager = false, Group = "Admin Commands")]
        public void AdminHelpCommand(Client sender)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                API.sendChatMessageToPlayer(sender,
                    "Generic   | /a /admins /aduty /tempname /getweapons /god /spec(tate) /specoff /save /gps");
                API.sendChatMessageToPlayer(sender,
                    "Player Mgmt.   | /unban /aduty /vda /id /kick /kicktemp /notify /tempban /ban /freeze /unfreeze /skin /setvw /check /sethealth /setarmour /slay /revive /givecash /givebank");
                API.sendChatMessageToPlayer(sender,
                    "Reports   | /admins /report /reports /ar /tr /clearreports /tickets /at /cleartickets");
                API.sendChatMessageToPlayer(sender,
                    "Teleport   | /tp /tpcoord /gethere /goback /gps /gotogarbage /tptr /tptobank /gotointerior /tptrucking");
                API.sendChatMessageToPlayer(sender,
                    "Organization   | /setorganization (/setorg) /setorganizationrank (/setorgrank) /orgtow /addorganizationvehicle");
                API.sendChatMessageToPlayer(sender,
                    "Vehicle   | /veh /acar /vehcol /vfix /setvehicle /setparking /playervehicles (/pveh) /alock /aengine");
                API.sendChatMessageToPlayer(sender,
                    "Weapons   | /weaponlist /giveweapon /giveammo /allweapons");
                API.sendChatMessageToPlayer(sender,
                    "Inventory   | /seeitems /giveplayeritem /deleteplayeritem");
                API.sendChatMessageToPlayer(sender,
                    "Properties   | /createproperty /aplock /pinfo /gotointerior /pmove /pinterior /prename /pprice /powner");
            }
        }

        [Command("a", GreedyArg = true, AddToHelpmanager = false, Group = "Admin Commands")]
        public void AdminChatCommand(Client sender, string chat)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var charData = Account.GetPlayerCharacterData(sender);
                if (charData != null)
                {
                    var adminUserName = charData.CharacterUser.UserName;
                    var adminRoleCodes = AdminLibrary.GetAdminShortRoleCodes(sender);
                    var characterName = charData.CharacterFirstname + "_" + charData.CharacterLastname; //explicit for when they're in GM mode.
                    var chatMessage = $"[{adminRoleCodes} {adminUserName} - ({PlayerLibrary.IdFromClient(sender)}) {NamingFunctions.RoleplayName(characterName)}]:~w~ {chat}";
                    AdminLibrary.SendAdminMessage(API, AdminLibrary.AnyAdmin, chatMessage);
                }
            }
        }

        [Command("admins", Group = "Player Commands")]
        public void AdminsList(Client sender)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                API.sendChatMessageToPlayer(sender, "Admins online:");
                foreach (var player in API.getAllPlayers())
                {
                    var playerData = Account.GetPlayerCharacterData(player);
                    if (playerData != null)
                    {
                        if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin, false))
                        {
                            if (AdminLibrary.OnDutyAdmins.Contains(player))
                            {
                                API.sendChatMessageToPlayer(sender,
                                    $"~g~{playerData.CharacterFirstname} {playerData.CharacterLastname} ({playerData.CharacterUser.UserName})~w~ - {AdminLibrary.GetAdminShortRoleCodes(player)}");
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(sender,
                                    $"~c~{player.name} ({playerData.CharacterUser.UserName})~w~ - {AdminLibrary.GetAdminShortRoleCodes(player)}");
                            }
                        }
                    }
                }
            }
        }

        [Command("aduty", AddToHelpmanager = false, Group = "Admin Commands")]
        public void AdminModeCommand(Client sender)
        {
            if(AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                AdminLibrary.ToggleAdminDuty(API, sender);
            }
        }

        [Command("tempname", AddToHelpmanager = false, Group = "Admin Commands")]
        public void TempNameCommand(Client sender, string name)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins))
            {
                API.sendChatMessageToPlayer(sender, $"You have set your display name to {name}.");
                AlertLogging.RaiseAlert($"{sender.name} has set their display name to {name}.");

                // Set the display name.
                sender.name = name;
            }
        }

        [Command("getweapons", AddToHelpmanager = false, Group = "Admin Commands")]
        public void GetAllWeaponsCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var weapons = API.getPlayerWeapons(target).ToList();
                var json = JsonConvert.SerializeObject(weapons.Select(weapon => new { hash = Enum.GetName(typeof(WeaponHash), weapon) }));

                API.sendChatMessageToPlayer(sender, json);
            }
        }

        [Command("spectate", Alias = "spec", AddToHelpmanager = false, Group = "Admin Commands")]
        public void SpectateCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                API.setPlayerToSpectatePlayer(sender, target);
                sender.sendChatMessage("Use /stopspectate to stop spectating.");
            }
        }

        [Command("stopspectate", Alias = "specoff", AddToHelpmanager = false, Group = "Admin Commands")]
        public void SpectateOffCommand(Client sender)
        {
            if(AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                API.unspectatePlayer(sender);
            }
        }

        [Command("save", GreedyArg = true, AddToHelpmanager = false, Group = "Admin Commands")]
        public void SaveCommand(Client sender, string description)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var position = sender.position;
                var rotation = sender.rotation;

                var path = "savedpositions.txt";
                using (var sw = File.AppendText(path))
                {
                    System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                    customCulture.NumberFormat.NumberDecimalSeparator = ".";

                    System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

                    //sw.WriteLineAsync($"position: new Vector3(" + position.X + ", " + position.Y + ", " + position.Z + ") rotation: new Vector3(" + API.getEntityRotation(sender) + ") // " + description);
                    sw.WriteLineAsync($"------ {description} ---- \n(Saved by {sender.name} at {DateTime.Now})\nposition\n\t\tnew Vector3({position.X}, {position.Y}, {position.Z})\nrotation\n\t\tnew Vector3({rotation.X}, {rotation.Y}, {rotation.Z})\n-------------\n");
                }
                API.sendChatMessageToPlayer(sender, "You have saved your current position as " + description + ". It will be found in the savedpositions.txt file.");
            }
        }

        [Command("givedrivinglicense", GreedyArg = true, Group = "Inventory Commands")]
        public void AddDrivingLicense(Client player)
        {
            if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin))
            {
                Character receiverData = Account.GetPlayerCharacterData(player);
                Inventory receiverInventory = receiverData.CharacterData.Inventory;
                Item item = ItemsLibrary.GetItem("Driving License");
                int amount = 1;
                if (receiverInventory.CanAddItem(item, amount))
                {
                    receiverInventory.AddItem(item, amount);
                    receiverInventory.SaveInventory();
                    API.sendChatMessageToPlayer(player, "~g~You received " + item.Name + " (" + amount + ").");
                }
            }
        }
    }
}
