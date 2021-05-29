using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Features.Emergency.Police;
using FiveRP.Gamemode.Features.Organizations;
using FiveRP.Gamemode.Features.Vehicles;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Emergency
{
    public class EmergencyCommands : Script
    {
        public EmergencyCommands()
        {
            API.onClientEventTrigger += OnClientEventTrigger;
            API.onPlayerDeath += OnPlayerDeath;
            API.onPlayerDisconnected += OnPlayerDisconnect;
        }

        private void OnPlayerDisconnect(Client player, string reason)
        {
            API.setEntityData(player, "uniform", false);
        }

        private void OnPlayerDeath(Client player, NetHandle entityKiller, int weapon)
        {
            API.setEntityData(player, "uniform", false);
        }

        [Command("m", Alias = "megaphone,mp", GreedyArg = true, Group = "Vehicle Commands")]
        public void MegaphoneCommand(Client player, string message)
        {
            if (player.vehicle == null)
            {
                API.sendChatMessageToPlayer(player, "~r~Error:~w~ You're not in a vehicle.");
            }
            else
            {
                var vehicleData = VehicleHandler.GetVehicleData(API.getPlayerVehicle(player));

                if (vehicleData.Organization >= 1 && OrganizationHandler.GetOrganizationFlag(vehicleData.Organization, "EMERGENCY"))
                {
                    ChatLibrary.SendChatMessageToPlayersInRadiusColored(API, player, ChatLibrary.FarChatRadius*1.5,
                        "~#FFE303~",
                        $"{NamingFunctions.RoleplayName(player.name)} [Megaphone]: {message}");
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "~r~Error:~w~ You're not in an emergency vehicle.");
                }
            }
        }

        [Command("departmental", Alias = "dep,dept", GreedyArg = true, Group = "Organization Commands")]
        public void OrganizationChatCommand(Client sender, string message)
        {
            // TODO: /dep needs to be possible to toggle.
            var senderData = Account.GetPlayerCharacterData(sender);

            if (senderData.CharacterData.Organization > 0 && OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "DEPT"))
            {
                foreach (var ply in API.getAllPlayers())
                {
                    var characterData = Account.GetPlayerCharacterData(ply);
                    var playerOrganizationData = OrganizationHandler.GetOrganizationData(senderData.CharacterData.Organization);

                    if (OrganizationHandler.GetOrganizationFlag(characterData.CharacterData.Organization, "DEPT"))
                    {
                        API.sendChatMessageToPlayer(ply, "~#CECC15~",
                            $"** [{playerOrganizationData.ShortName}] {OrganizationHandler.GetOrganizationRankName(playerOrganizationData.Id, senderData.CharacterData.OrganizationRank)} {NamingFunctions.RoleplayName(sender.name)}: {message} **");
                    }
                }

                // Ignore ply
                ChatLibrary.SendChatMessageToPlayersInRadiusFaded(API, sender, ChatLibrary.DefaultChatRadius, $"{NamingFunctions.RoleplayName(sender.name)} (radio): {message}", sender);
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "You do not have the right access required for this.");
            }
        }

        [Command("duty", Group = "Emergency Commands")]
        public void DutyCommand(Client sender)
        {
            var senderData = Account.GetPlayerCharacterData(sender);

            if (senderData.CharacterData.Organization > 0 &&
                OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "COPDUTY") ||
                OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "MEDDUTY"))
            {
                if (OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "MEDDUTY"))
                {
                    MedicCommands.MedicDuty(API, sender);
                }
                else if (OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "COPDUTY"))
                {
                    PoliceCommands.CopDuty(API, sender);
                }
                else if (senderData.CharacterData.Organization == 0)
                {
                    API.sendChatMessageToPlayer(sender, "You aren't in any position to go on duty.");
                }
            }
        }

        private void OnClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "menu_handler_select_item")
            {
                var menuId = (string) arguments[0];
                var index = (int) arguments[1];
                var charData = Account.GetPlayerCharacterData(sender);
                if (menuId == "uniform_cop")
                {
                    var uniformList = PoliceCommands.CopUniforms.ToList();

                    if (index > 0)
                    {
                        var skin = uniformList.ElementAt(index - 1);
                        API.setPlayerSkin(sender, skin);
                        charData.CharacterData.WardRobe.EquipVariants();
                        API.setEntityData(sender, "uniform", true);
                        API.setPlayerHealth(sender, 100);
                        API.setPlayerArmor(sender, 100);

                        foreach (var weaponType in PoliceCommands.CopLoadout)
                        {
                            API.givePlayerWeapon(sender, weaponType, 500, false, true);
                        }
                    }
                } else if (menuId == "uniform_medic")
                {
                    var uniformList = MedicCommands.MedicUniforms.ToList();

                    if (index > 0)
                    {
                        var skin = uniformList.ElementAt(index - 1);
                        API.setPlayerSkin(sender, skin);
                        charData.CharacterData.WardRobe.EquipVariants();
                        API.setEntityData(sender, "uniform", true);
                        API.setPlayerHealth(sender, 100);
                        API.setPlayerArmor(sender, 100);
                        API.givePlayerWeapon(sender, WeaponHash.Crowbar, 1, false, true);
                        API.givePlayerWeapon(sender, WeaponHash.Flashlight, 1, false, true);
                        API.givePlayerWeapon(sender, WeaponHash.Flare, 10, false, true);
                    }
                }
            }
        }

        [Command("uniform", Group = "Emergency Commands")]
        public void UniformCommand(Client sender)
        {
            var senderData = Account.GetPlayerCharacterData(sender);
            if (senderData.CharacterData.Organization > 0 &&
                OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "COPDUTY") ||
                OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "MEDDUTY"))
            {
                if (API.getEntityData(sender, "uniform") == true)
                {
                    API.sendChatMessageToPlayer(sender, "You're already equipped with your uniform.");
                    return;
                }
                if (OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "MEDDUTY"))
                {
                    if (!MedicCommands.IsOnMedicDuty(sender))
                    {
                        API.sendChatMessageToPlayer(sender, "You must be on duty to do this.");
                        return;
                    }
                    var uniformList = new List<string> {"~r~Cancel"};
                    foreach (var uniform in MedicCommands.MedicUniforms)
                    {
                        uniformList.Add("~g~" + Enum.GetName(typeof(PedHash), uniform));
                    }
                    MenuLibrary.ShowNativeMenu(API, sender, "uniform_medic", "Select your uniform", "Uniform selection", false, uniformList);
                }
                else if (OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "COPDUTY"))
                {
                    if (!PoliceCommands.IsOnPoliceDuty(sender))
                    {
                        API.sendChatMessageToPlayer(sender, "You must be on duty to do this.");
                        return;
                    }
                    var uniformList = new List<string> {"~r~Cancel"};
                    foreach (var uniform in PoliceCommands.CopUniforms)
                    {
                        uniformList.Add("~g~" + Enum.GetName(typeof(PedHash), uniform));
                    }
                    MenuLibrary.ShowNativeMenu(API, sender, "uniform_cop", "Select your uniform", "Uniform selection", false, uniformList);
                }
                else if (senderData.CharacterData.Organization == 0)
                {
                    API.sendChatMessageToPlayer(sender, "You aren't in any position to do this.");
                }
            }
        }
    }
}