using System;
using System.Linq;
using System.Timers;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;

namespace FiveRP.Gamemode.Features.Vehicles
{
    public class CarModding : Script
    {
        private readonly List<Vector3> _modLocationList = new List<Vector3>();

        public CarModding()
        {
            API.onResourceStart += OnResourceStart;
            API.onPlayerExitVehicle += OnPlayerExitVehicle;
            _modLocationList.Add(new Vector3(-1388.89, -3257.89, 13.51));
            _modLocationList.Add(new Vector3(-373.64, -121.20, 38.06));
        }

        private void OnPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            if (API.hasEntityData(vehicle, "repair_data"))
            {
                var repairData = (RepairData)API.getEntityData(vehicle, "repair_data");
                repairData.Label.delete();
                foreach (var timer in repairData.Timers)
                {
                    timer.Dispose();   
                }
            }
        }

        private void OnResourceStart()
        {
            foreach (Vector3 _modLocation in _modLocationList)
            {
                API.createTextLabel("~r~Los Santos Customs~w~\nRepair vehicle: /repair ($250)\nRespray vehicle: /spray [col 0] [col 1] ($250)\n/upgrade (Upgrade kits)", _modLocation, 45f, 0.3f);
                var blip = API.createBlip(_modLocation);
                API.setBlipSprite(blip, 402);
                API.setBlipColor(blip, 1);
                API.setBlipScale(blip, 1f);
                API.setBlipShortRange(blip, true);
            }
        }

        [Command("repair", Group = "Vehicle Modification Commands")]
        public void RepairCommand(Client player)
        {
            var playerPos = player.position;
            bool tooFar = true;
            foreach (Vector3 _modLocation in _modLocationList)
            {
                if (playerPos.DistanceToSquared(_modLocation) < 45f)
                {
                    tooFar = false;
                    var vehicle = player.vehicle;
                    if (!player.vehicle.IsNull)
                    {
                        var playerData = Account.GetPlayerCharacterData(player);

                        if (playerData.CharacterData.Money >= 250)
                        {
                            if (API.getVehicleHealth(vehicle) > 990f)
                            {
                                API.sendChatMessageToPlayer(player, "This vehicle is not damaged.");
                                return;
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player, "Your vehicle is being repaired ..");

                                var timeLeft = DateTime.Now.AddMinutes(1);

                                Timer repairTimer = new Timer
                                {
                                    Interval = 60000,
                                    Enabled = true,
                                    AutoReset = false
                                };

                                Timer updateTimer = new Timer
                                {
                                    Interval = 1000,
                                    Enabled = true,
                                    AutoReset = true
                                };

                                var repairLabel = API.createTextLabel($"~g~Repairing: {Math.Round(timeLeft.Subtract(DateTime.Now).TotalSeconds)} seconds left.", API.getEntityPosition(vehicle), 50f, 0.4f, true);
                                API.attachEntityToEntity(repairLabel, vehicle, null, new Vector3(0, 0, 1f), new Vector3());

                                var repairData = new RepairData
                                {
                                    Label = repairLabel,
                                    Timers = new[] { updateTimer, repairTimer }
                                };

                                API.setEntityData(vehicle, "repair_data", repairData);

                                updateTimer.Elapsed += (sender, args) =>
                                {
                                    playerPos = player.position;
                                    if (playerPos.DistanceToSquared(_modLocation) > 45f)
                                    {
                                        API.sendChatMessageToPlayer(player,
                                            "~r~Error:~w~ You\'ve moved from Los Santos Customs location. Repairing process stopped.");
                                        repairTimer.Dispose();
                                        updateTimer.Dispose();

                                        repairLabel.delete();
                                    }

                                    repairLabel.text =
                                        $"~g~Repairing: {Math.Round(timeLeft.Subtract(DateTime.Now).TotalSeconds)} seconds left.";
                                };

                                repairTimer.Elapsed += (sender, args) =>
                                {
                                    API.setVehicleHealth(vehicle, 1000f);
                                    API.repairVehicle(vehicle);
                                    API.sendChatMessageToPlayer(player, "Your vehicle has been fixed for $250!");
                                    playerData.CharacterData.Money -= 250;
                                    repairTimer.Dispose();
                                    updateTimer.Dispose();
                                    repairLabel.delete();
                                };
                                return;
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "~r~ERROR: ~w~You can't afford this.");
                            return;
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "~r~ERROR: ~w~You're not in a vehicle!");
                        return;
                    }
                }
            }
            if (tooFar)
            {
                API.sendChatMessageToPlayer(player,
                    "~r~Error:~w~ You\'re not at the right location for repairs. Please go to Los Santos Customs.");
            }
        }

        [Command("spray", Group = "Vehicle Modification Commands")]
        public void SprayCommand(Client player, int primaryColor, int secondaryColor)
        {
            var playerPos = player.position;
            bool tooFar = true;
            foreach (Vector3 _modLocation in _modLocationList)
            {
                if (playerPos.DistanceToSquared(_modLocation) < 45f)
                {
                    var vehicle = player.vehicle;
                    tooFar = false;
                    if (!player.vehicle.IsNull)
                    {
                        var playerData = Account.GetPlayerCharacterData(player);
                        var vehicleData = VehicleHandler.GetVehicleData(player.vehicle);

                        // If the vehicle is owned by the player attempting to respray.
                        if (playerData.CharacterId == vehicleData.Owner)
                        {
                            if (playerData.CharacterData.Money >= 250)
                            {
                                var timeLeft = DateTime.Now.AddMinutes(1);

                                Timer resprayTimer = new Timer
                                {
                                    Interval = 60000,
                                    Enabled = true,
                                    AutoReset = false
                                };

                                Timer updateTimer = new Timer
                                {
                                    Interval = 1000,
                                    Enabled = true,
                                    AutoReset = true
                                };

                                var resprayLabel = API.createTextLabel($"~b~Respraying: {Math.Round(timeLeft.Subtract(DateTime.Now).TotalSeconds)} seconds left.", API.getEntityPosition(vehicle), 50f, 0.4f, true);
                                API.attachEntityToEntity(resprayLabel, vehicle, null, new Vector3(0, 0, 1f), new Vector3());

                                var resprayData = new RepairData
                                {
                                    Label = resprayLabel,
                                    Timers = new[] { updateTimer, resprayTimer }
                                };

                                API.setEntityData(vehicle, "repair_data", resprayData);

                                updateTimer.Elapsed += (sender, args) =>
                                {
                                    playerPos = player.position;
                                    if (playerPos.DistanceToSquared(_modLocation) > 45f)
                                    {
                                        API.sendChatMessageToPlayer(player,
                                            "~r~Error:~w~ You\'ve moved from Los Santos Customs location. Respraying process stopped.");
                                        resprayTimer.Dispose();
                                        updateTimer.Dispose();

                                        resprayLabel.delete();
                                    }

                                    resprayLabel.text =
                                        $"~b~Respraying: {Math.Round(timeLeft.Subtract(DateTime.Now).TotalSeconds)} seconds left.";
                                };

                                resprayTimer.Elapsed += (sender, args) =>
                                {
                                    API.setVehiclePrimaryColor(vehicle, primaryColor);
                                    API.setVehicleSecondaryColor(vehicle, secondaryColor);
                                    API.sendChatMessageToPlayer(player, "Your vehicle has been resprayed for $250!");

                                    vehicleData.Colour1 = primaryColor;
                                    vehicleData.Colour2 = secondaryColor;

                                    VehicleHandler.SaveVehicle(API, vehicle);

                                    playerData.CharacterData.Money -= 250;
                                    resprayTimer.Dispose();
                                    updateTimer.Dispose();
                                    resprayLabel.delete();
                                };
                                return;
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player, "~r~ERROR: ~w~You can't afford this.");
                                return;
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "This vehicle isn't owned by you.");
                            return;
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "~r~ERROR: ~w~You're not in a vehicle!");
                        return;
                    }
                }
            }
            if (tooFar)
            {
                API.sendChatMessageToPlayer(player,
                    "~r~Error:~w~ You\'re not at the right location for resprays. Please go to Los Santos Customs.");
            }
        }

        [Command("upgrade", Group = "Vehicle Modification Commands")]
        public void UpgradeCommand(Client player, string confirmation = "")
        {
            var playerPos = player.position;
            bool tooFar = true;
            foreach (Vector3 _modLocation in _modLocationList)
            {
                if (playerPos.DistanceToSquared(_modLocation) < 45f)
                {
                    tooFar = false;
                    var vehicle = player.vehicle;
                    if (vehicle != null)
                    {
                        var playerData = Account.GetPlayerCharacterData(player);
                        var vehicleData = VehicleHandler.GetVehicleData(vehicle);

                        var vehicleModel = (VehicleHash)vehicle.model;

                        var upgradeKit = VehicleUpgradeKits.UpgradeKits.FirstOrDefault(p => p.InitialVehicle == vehicleModel);

                        // If the vehicle is owned by the player attempting to respray.
                        if (playerData.CharacterId == vehicleData.Owner)
                        {
                            if (VehicleUpgradeKits.UpgradeKits.Any(p => p.InitialVehicle == vehicleModel))
                            {
                                if (confirmation == "confirm")
                                {
                                    if (playerData.CharacterData.Money >= upgradeKit.UpgradePrice)
                                    {
                                        playerData.CharacterData.Money -= upgradeKit.UpgradePrice;

                                        API.sendChatMessageToPlayer(player,
                                            $"You have spent ${NamingFunctions.FormatMoney(upgradeKit.UpgradePrice)} upgrading your vehicle.");

                                        // Set the model.
                                        vehicleData.Model = Enum.GetName(typeof(VehicleHash), upgradeKit.UpgradedVehicle);

                                        VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);

                                        API.deleteEntity(vehicleData.Vehicle);

                                        // Spawn the vehicle again:
                                        var vehicleEntity = API.createVehicle(API.vehicleNameToModel(vehicleData.Model),
                                            player.vehicle.position, new Vector3(0, 0, player.vehicle.rotation.Z), vehicleData.Colour1,
                                            vehicleData.Colour2);

                                        // Load variables and shit
                                        API.setVehicleEngineStatus(vehicleEntity, false);
                                        API.setVehicleNumberPlate(vehicleEntity, vehicleData.Plate);
                                        API.setEntityDimension(vehicleEntity, vehicleData.Dimension);

                                        API.setPlayerIntoVehicle(player, vehicleEntity, -1);

                                        vehicleData.Vehicle = vehicleEntity;
                                        return;
                                    }
                                    else
                                    {
                                        API.sendChatMessageToPlayer(player, "~r~ERROR: ~w~You can't afford this.");
                                        return;
                                    }
                                }
                                else
                                {
                                    API.sendChatMessageToPlayer(player, "This vehicle has no available upgrade kits.");
                                    return;
                                }
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player, "This vehicle cannot be upgraded using an upgrade kit.");
                                return;
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "This vehicle isn't owned by you.");
                            return;
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "~r~ERROR: ~w~You're not in a vehicle!");
                        return;
                    }
                }
            }
            if (tooFar)
            {
                API.sendChatMessageToPlayer(player,
                    "~r~Error:~w~ You\'re not at the right location for upgrades. Please go to Los Santos Customs.");
            }
        }
    }

    public class RepairData
    {
        public TextLabel Label { get; set; }
        public Timer[] Timers { get; set; }
    }
}