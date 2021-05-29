using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Vehicles;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Admin.AdminCommands
{
    public class AdminVehicleCommands : Script
    {
        public Dictionary<Client, NetHandle> ActiveAdminVehicle = new Dictionary<Client, NetHandle>();


        public AdminVehicleCommands()
        {
            API.onPlayerExitVehicle += API_onPlayerExitVehicle;

            AccountHandler.OnAccountLogoutPreRemoveData += AccountHandler_OnAccountLogoutPreRemoveData;

            MenuLibrary.OnMenuSelected += MenuLibrary_OnMenuSelected;
        }

        private void MenuLibrary_OnMenuSelected(Client sender, string menuName, int selectedIndex)
        {
            if (menuName == "admin_vehicle_list")
            {
                if (API.hasEntityData(sender, "admin_vehicle_list_player"))
                {
                    var player = (Client)API.getEntityData(sender, "admin_vehicle_list_player");

                    var charData = Account.GetPlayerCharacterData(player);

                    if (charData != null)
                    {
                        var vehicles = VehicleHandler.VehicleList.Where(v => v.Owner == charData.CharacterId && v.Organization != -1);
                        var vehicle = vehicles.ElementAt(selectedIndex);

                        API.setEntityData(sender, "admin_vehicle_list_vehicle", vehicle);

                        var optionList = new List<string> { "Cancel", "~g~Goto vehicle", "~r~Despawn vehicle", "~b~Bring vehicle to here", "~b~Force spawn vehicle" };

                        MenuLibrary.ShowNativeMenu(API, sender, "admin_vehicle_list_select", "Vehicle actions", $"Select action for {vehicle.Model}", false, optionList);
                    }
                }
            }

            if (menuName == "admin_vehicle_list_select")
            {
                if (API.hasEntityData(sender, "admin_vehicle_list_vehicle"))
                {
                    var vehicle = (FiveRPVehicle)API.getEntityData(sender, "admin_vehicle_list_vehicle");

                    var index = selectedIndex;

                    switch (index)
                    {
                        case 0:
                            API.sendChatMessageToPlayer(sender, "You have canceled this action.");
                            break;
                        case 1:
                            if (!API.doesEntityExist(vehicle.Vehicle))
                            {
                                AdminLibrary.TeleportPlayerTo(API, sender,
                                    new Vector3(vehicle.X, vehicle.Y, vehicle.Z));
                                API.sendChatMessageToPlayer(sender, "You have teleported to the vehicle's parking spot because the vehicle wasn't spawned.");
                            }
                            else
                            {
                                AdminLibrary.TeleportPlayerTo(API, sender, API.getEntityPosition(vehicle.Vehicle));
                                API.sendChatMessageToPlayer(sender, "You have teleported to the vehicle.");
                            }
                            break;
                        case 2:
                            if (API.doesEntityExist(vehicle.Vehicle))
                            {
                                API.deleteEntity(vehicle.Vehicle);
                                vehicle.Vehicle = new NetHandle();

                                API.sendChatMessageToPlayer(sender, "You have despawned this vehicle.");
                            }
                            else sender.sendChatMessage("This vehicle does not exist.");
                            break;
                        case 3:
                            if (API.doesEntityExist(vehicle.Vehicle))
                            {
                                API.setEntityPosition(vehicle.Vehicle, sender.position);

                                API.sendChatMessageToPlayer(sender, "You have teleported this vehicle to you.");
                            }
                            else sender.sendChatMessage("This vehicle does not exist.");
                            break;
                        case 4:
                            if (!API.doesEntityExist(vehicle.Vehicle))
                            {
                                var vehicleEntity = API.createVehicle(API.vehicleNameToModel(vehicle.Model), new Vector3(vehicle.X, vehicle.Y, vehicle.Z), new Vector3(0, 0, vehicle.H), vehicle.Colour1, vehicle.Colour2);

                                // Load variables and shit
                                API.setVehicleEngineStatus(vehicleEntity, false);
                                API.setVehicleNumberPlate(vehicleEntity, vehicle.Plate);
                                API.setEntityDimension(vehicleEntity, vehicle.Dimension);

                                if (vehicle.Type == 1)
                                {
                                    // Spawn the vehicle with some more horsepower, brakes and engine ;)
                                    API.setVehicleMod(vehicleEntity, 11, 4); // engine level 4
                                    API.setVehicleMod(vehicleEntity, 12, 3); // brake level 3
                                    API.setVehicleMod(vehicleEntity, 13, 3); // transmission level 3
                                }

                                vehicle.Vehicle = vehicleEntity;

                                // Lock the vehicle if it's not a faction vehicle and is also owned by a player.
                                if (vehicle.Owner != 0 && vehicle.Organization == 0)
                                {
                                    API.setVehicleLocked(vehicleEntity, true);
                                }

                                sender.sendChatMessage($"You have spawned this ~b~{vehicle.Model}~w~. It'll be parked at it's parking spot.");

                                // Freeze the entity so it doesn't fall through the world
                                API.setEntityPositionFrozen(vehicleEntity.handle, true);
                            } else sender.sendChatMessage("The vehicle is already spawned.");
                            break;
                    }
                }
            }
        }

        private void AccountHandler_OnAccountLogoutPreRemoveData(Client player)
        {
            // This block of code removes the administrator vehicles once the admin who spawned them disconnects.
            if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin))
            {
                foreach (var vehicle in API.getAllVehicles())
                {
                    if (ActiveAdminVehicle.ContainsKey(player))
                    {
                        if (ActiveAdminVehicle[player] == vehicle && ActiveAdminVehicle[player] != default(NetHandle))
                        {
                            API.deleteEntity(vehicle);
                            ActiveAdminVehicle[player] = default(NetHandle);
                        }
                    }
                }
            }
        }

        private void API_onPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            var vehicleData = VehicleHandler.GetVehicleData(vehicle);
            if (vehicleData.Organization == -1 && vehicleData.Plate == "ADMIN")
            {
                if (API.doesEntityExist(vehicle))
                {
                    API.deleteEntity(vehicle);
                    if (VehicleHandler.VehicleList.Contains(vehicleData))
                    {
                        VehicleHandler.VehicleList.Remove(vehicleData);
                    }
                }
            }
        }

        [Command("admincar", Alias = "veh,acar", AddToHelpmanager = true, Group = "Admin Commands")]
        public void SpawnVehicleCommand(Client sender, VehicleHash model)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins))
            {
                if (AdminLibrary.OnAdminDuty(sender))
                {
                    if (API?.getPlayerVehicle(sender) != null)
                    {
                        API_onPlayerExitVehicle(sender, API.getPlayerVehicle(sender));
                    }
                    var rot = API?.getEntityRotation(sender);
                    Debug.Assert(rot != null, "rot != null");
                    var veh = API?.createVehicle(model, API.getEntityPosition(sender), new Vector3(0, 0, rot.Z), 0, 0);

                    var vehicleData = new FiveRPVehicle
                    {
                        Model = Enum.GetName(typeof(VehicleHash), API.getEntityModel(API.getPlayerVehicle(sender))),
                        Id = -1,
                        Organization = -1,
                        Owner = 0,
                        Plate = "ADMIN"
                    };

                    API.setVehicleNumberPlate(veh, "ADMIN");
                    //API.setEntityTransparency(veh, 150);

                    vehicleData.Vehicle = veh;

                    VehicleHandler.VehicleList.Add(vehicleData);

                    ActiveAdminVehicle[sender] = veh;

                    // Ensures the admin vehicle or whatever is removed!
                    if (API.isPlayerInAnyVehicle(sender))
                    {
                        API.warpPlayerOutOfVehicle(sender, sender.vehicle);
                    }

                    API.setPlayerIntoVehicle(sender, veh, -1);

                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                }
            }
        }

        [Command("vehcol", AddToHelpmanager = true, Group = "Admin Commands")]
        public void VehicleColorsCommand(Client sender, int primaryColor, int secondaryColor)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                if (AdminLibrary.OnAdminDuty(sender))
                {

                    if (!sender.vehicle.IsNull)
                    {
                        API.setVehiclePrimaryColor(sender.vehicle, primaryColor);
                        API.setVehicleSecondaryColor(sender.vehicle, secondaryColor);
                        API.sendChatMessageToPlayer(sender, "Colors applied successfully!");
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~You're not in a vehicle!");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                }
            }
        }

        [Command("fixvehicle", Alias = "fixveh,vfix,afix", AddToHelpmanager = true, Group = "Admin Commands")]
        public void FixVehicle(Client sender)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                if (AdminLibrary.OnAdminDuty(sender))
                {
                    var vehicle = sender.vehicle;
                    if (API.doesEntityExist(sender.vehicle))
                    {
                        API.setVehicleHealth(vehicle, 1000f);
                        API.repairVehicle(vehicle);
                        API.sendChatMessageToPlayer(sender, "Vehicle has been fixed!");
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~You're not in a vehicle!");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                }
            }
        }

        [Command("playervehicles", Alias = "pveh", Group = "Vehicle Commands")]
        public void GetVehicleCommand(Client sender, string targ)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
                if (target == null) return;

                var charData = Account.GetPlayerCharacterData(target);
                if (charData != null)
                {
                    var vehicles =
                        VehicleHandler.VehicleList.Where(v => v.Owner == charData.CharacterId && v.Organization != -1);
                    var vehicleList = new List<string>();

                    foreach (var vehicle in vehicles)
                    {
                        vehicleList.Add($"[{vehicle.Id}] {vehicle.Model}");
                    }

                    MenuLibrary.ShowNativeMenu(API, sender, "admin_vehicle_list", "Vehicles",
                        $"{target.name}'s vehicles.", false, vehicleList);

                    API.setEntityData(sender, "admin_vehicle_list_player", target);
                }
            }
        }
    }
}