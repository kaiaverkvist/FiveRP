using System;
using FiveRP.Gamemode.Features.Admin;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using FiveRP.Gamemode.Features.Inventories;

namespace FiveRP.Gamemode.Features.Vehicles
{
    public class VehicleAdmin : Script
    {
        [Command("setparking", AddToHelpmanager = true, Group = "Vehicle Administration")]
        public void ForceParkCommand(Client sender)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, new[] { AdminLibrary.Developer, AdminLibrary.Manager, AdminLibrary.GameMaster, AdminLibrary.Moderator }))
            {
                if (sender.vehicle != null)
                {
                    var vehicleData = VehicleHandler.GetVehicleData(sender.vehicle);

                    if (vehicleData != null)
                    {
                        var parking = sender.vehicle.position;
                        vehicleData.X = parking.X;
                        vehicleData.Y = parking.Y;
                        vehicleData.Z = parking.Z;
                        vehicleData.H = sender.vehicle.rotation.Z;

                        VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);
                        sender.sendChatMessage("You have set the vehicle's parking spot forcefully.");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "You are not in a vehicle.");
                }
            }
        }

        [Command("setvehicle", AddToHelpmanager = true, Group = "Vehicle Administration")]
        public void SetVehicleXCommand(Client sender, string argument = "", string extra = "")
        {
            if (AdminLibrary.CheckAuthorization(API, sender, new[] { AdminLibrary.Developer, AdminLibrary.Manager, AdminLibrary.GameMaster }))
            {
                if (sender.vehicle != null)
                {
                    var vehicleData = VehicleHandler.GetVehicleData(sender.vehicle);

                    if (argument == "model")
                    {
                        if (extra != "")
                        {
                            if (Enum.IsDefined(typeof(VehicleHash), extra))
                            {
                                // Set the model.
                                vehicleData.Model = Enum.GetName(typeof(VehicleHash), API.vehicleNameToModel(extra));
                                VehicleInventory inventory = vehicleData.VehicleInventory;
                                VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);

                                API.deleteEntity(vehicleData.Vehicle);

                                // Spawn the vehicle again:
                                var vehicleEntity = API.createVehicle(API.vehicleNameToModel(vehicleData.Model),
                                    new Vector3(vehicleData.X, vehicleData.Y, vehicleData.Z), new Vector3(0, 0, vehicleData.H), vehicleData.Colour1,
                                    vehicleData.Colour2);

                                // Load variables and shit
                                API.setVehicleEngineStatus(vehicleEntity, false);
                                API.setVehicleNumberPlate(vehicleEntity, vehicleData.Plate);
                                API.setEntityDimension(vehicleEntity, vehicleData.Dimension);
                                vehicleData.VehicleInventory = inventory;
                                API.setPlayerIntoVehicle(sender, vehicleEntity, -1);

                                if (vehicleData.Type == 1)
                                {
                                    // Spawn the vehicle with some more horsepower, brakes and engine ;)
                                    API.setVehicleMod(vehicleEntity, 11, 4); // engine level 4
                                    API.setVehicleMod(vehicleEntity, 12, 3); // brake level 3
                                    API.setVehicleMod(vehicleEntity, 13, 3); // transmission level 3
                                }

                                vehicleData.Vehicle = vehicleEntity;

                                API.sendChatMessageToPlayer(sender, "You have set this vehicle's model.");
                            } else sender.sendChatMessage("Invalid vehicle model.");
                        }
                    }
                    else if (argument == "plate")
                    {
                        if (extra != "")
                        {
                            vehicleData.Plate = extra;
                            API.setVehicleNumberPlate(vehicleData.Vehicle, extra);
                        }
                    }
                    else if (argument == "owner")
                    {
                        if (extra != "")
                        {
                            int owner;

                            // Set the model.
                            var parseSuccess = int.TryParse(extra, out owner);

                            if (parseSuccess)
                            {
                                vehicleData.Owner = owner;
                                VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);

                                API.sendChatMessageToPlayer(sender, "This vehicle is now owned by the player id specified.");
                            }
                        }
                    }
                    else if (argument == "type")
                    {
                        if (extra != "")
                        {
                            int type;

                            // Set the model.
                            var parseSuccess = int.TryParse(extra, out type);

                            if (parseSuccess)
                            {
                                vehicleData.Type = type;
                                VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);

                                API.sendChatMessageToPlayer(sender, "You have set the vehicle type.");
                            }
                        }
                    }
                    else if (argument == "organization")
                    {
                        if (extra != "")
                        {
                            int faction;

                            // Set the model.
                            var parseSuccess = int.TryParse(extra, out faction);

                            if (parseSuccess)
                            {
                                vehicleData.Organization = faction;
                                VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);

                                API.sendChatMessageToPlayer(sender, "This vehicle is now owned by the organization as specified.");
                            }
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(sender, "~g~Usage: /setvehicle [model/plate/owner/type/organization]");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "You are not in a vehicle.");
                }
            }
        }

        [Command("adminlock", Alias = "alock", Group = "Vehicle Commands")]
        public void AdminLockCommand(Client player)
        {
            var foundVehicle = false;

            if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin))
            {
                if (player.vehicle == null)
                {
                    var allVehicles = API.getAllVehicles();

                    foreach (var vehicle in allVehicles)
                    {
                        // If the vehicle is close enough, do the lock check.
                        if (DistanceLibrary.DistanceBetween(player.position, API.getEntityPosition(vehicle)) < 5.0)
                        {
                            if (API.getVehicleLocked(vehicle))
                            {
                                API.sendChatMessageToPlayer(player, "You have ~g~unlocked~w~ the vehicle.");
                                API.setVehicleLocked(vehicle, false);
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player, "You have ~r~locked~w~ the vehicle.");
                                API.setVehicleLocked(vehicle, true);
                            }

                            foundVehicle = true;

                            // break out of the foreach loop since we've found a vehicle.
                            break;
                        }
                    }

                    if (!foundVehicle)
                    {
                        API.sendChatMessageToPlayer(player, "You are not close enough to any vehicles you own.");
                    }
                }
                else // if the player is in a vehicle
                {
                    if (API.getVehicleLocked(player.vehicle))
                    {
                        API.sendChatMessageToPlayer(player, "You have ~g~unlocked~w~ your vehicle.");
                        API.setVehicleLocked(player.vehicle, false);
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "You have ~r~locked~w~ your vehicle.");
                        API.setVehicleLocked(player.vehicle, true);
                    }
                }
            }
        }

        [Command("adminengine", Alias = "aengine", Group = "Vehicle Commands")]
        public void AdminEngineCommand(Client player)
        {
            if (player.vehicle == null)
            {
                API.sendChatMessageToPlayer(player, "~r~Error:~w~ You're not in a vehicle.");
            }
            else
            {
                var seat = API.getPlayerVehicleSeat(player);
                // Check that the player is actually the driver.
                if (seat == -1)
                {
                    if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin))
                    {
                        var engine = API.getVehicleEngineStatus(API.getPlayerVehicle(player));
                        if (engine)
                        {
                            API.setVehicleEngineStatus(API.getPlayerVehicle(player), false);
                            ChatLibrary.SendEmoteChatMessage(API, player, "has turned off the engine of the vehicle.");
                        }
                        else
                        {
                            API.setVehicleEngineStatus(API.getPlayerVehicle(player), true);
                            ChatLibrary.SendEmoteChatMessage(API, player, "has turned on the engine of the vehicle.");
                        }
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "~r~Error:~w~ You're not the driver of the vehicle.");
                }
            }
        }
    }
}
