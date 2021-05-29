using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Inventories;

namespace FiveRP.Gamemode.Features.Vehicles
{
    public class VehicleHandler : Script
    {
        public static List<FiveRPVehicle> VehicleList = new List<FiveRPVehicle>();

        public VehicleHandler()
        {
            API.onResourceStart += OnResourceStart;
            API.onVehicleDeath += OnVehicleDeath;
            API.onPlayerExitVehicle += OnPlayerExitVehicle;
            API.onPlayerEnterVehicle += OnPlayerEnterVehicle;

            TimeManager.OnTimeUpdate += OnTimeUpdate;

            AccountHandler.OnAccountLogin += OnAccountLogin;
            AccountHandler.OnAccountLogoutPreRemoveData += OnAccountLogoutPreRemoveData;
        }

        private void OnPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            // unfreeze the entity
            if (API.getVehicleLocked(vehicle))
            {
                player.KickFromVehicle(vehicle);
            }
            API.setEntityPositionFrozen(vehicle, false);
        }

        private void OnPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            var vehicleData = GetVehicleData(vehicle);
            if (vehicleData.Organization == 0)
            {
                SaveVehicle(API, vehicle);
            }
        }

        private void OnVehicleDeath(NetHandle vehicle)
        {
            var vehicleData = GetVehicleData(vehicle);

            if (vehicleData.Organization == 0)
            {
                // TODO: Create way to repair this part of a vehicle. 8% of total value?
                /*var penality = new Random().Next(5, 15);
                if (vehicleData.EngineHealth > penality)
                {
                    vehicleData.EngineHealth -= penality;
                }
                else
                {
                    vehicleData.EngineHealth = 0;
                }*/

                vehicleData.DestroyedCount++;

                var ownerData = Account.CharacterList.FirstOrDefault(p => p.CharacterId == vehicleData.Owner);
                if (ownerData != null)
                {
                    API.sendChatMessageToPlayer(ownerData.CharacterClient,
                        $"Your ~b~{vehicleData.Model}~w~ was destroyed. Use ~b~/vget~w~ and select the vehicle to spawn it again.");
                }

                SaveVehicle(API, vehicleData.Vehicle);
            }

            // Remove the vehicle from the world.
            if (API.doesEntityExist(vehicle))
            {
                API.deleteEntity(vehicle);
            }

            vehicleData.Vehicle = new NetHandle();

            if (vehicleData.Organization >= 1)
            {
                SpawnVehicle(vehicleData);
            }

        }

        private void SpawnVehicle(FiveRPVehicle vehicleData)
        {
            if (vehicleData != null)
            {
                // Spawn the vehicle entity
                var vehicleEntity = API.createVehicle(API.vehicleNameToModel(vehicleData.Model),
                    new Vector3(vehicleData.X, vehicleData.Y, vehicleData.Z), new Vector3(0, 0, vehicleData.H),
                    vehicleData.Colour1, vehicleData.Colour2);

                // Load variables and shit
                API.setVehicleEngineStatus(vehicleEntity, false);
                API.setVehicleNumberPlate(vehicleEntity, vehicleData.Plate);
                API.setEntityDimension(vehicleEntity, vehicleData.Dimension);

                // Set the health of the vehicle
                API.setVehicleHealth(vehicleEntity, vehicleData.Health);

                if (vehicleData.Type == 1)
                {
                    // Spawn the vehicle with some more horsepower, brakes and engine ;)
                    API.setVehicleMod(vehicleEntity, 11, 4); // engine level 4
                    API.setVehicleMod(vehicleEntity, 12, 3); // brake level 3
                    API.setVehicleMod(vehicleEntity, 13, 3); // transmission level 3
                }

                // TODO: Fix engine power multiplier
                //var enginePowerMultiplier = 0.0f - Extensions.Clamp(vehicleData.EngineHealth, 0.0f, 33.5f);
                //API.setVehicleEnginePowerMultiplier(vehicleEntity, enginePowerMultiplier);

                vehicleData.Vehicle = vehicleEntity;

                // Lock the vehicle if it's not a faction vehicle and is also owned by a player.
                if (vehicleData.Owner != 0 && vehicleData.Organization == 0)
                {
                    API.setVehicleLocked(vehicleEntity, true);
                }

                API.setEntityPositionFrozen(vehicleEntity.handle, true);
            }
            else
            {
                throw new Exception("Tried to spawn vehicle with invalid data.");
            }
        }

        private void OnAccountLogoutPreRemoveData(Client player)
        {
            // Flag the player's vehicles so they get removed at the right time:
            var charData = Account.GetPlayerCharacterData(player);
            if (charData != null)
            {
                var vehicles = VehicleList.Where(v => v.Owner == charData.CharacterId && v.Organization != -1).ToList();

                if (vehicles.Any())
                {
                    foreach (var vehicle in vehicles)
                    {
                        if (vehicle.Organization != -1)
                        {
                            API.setEntityData(vehicle.Vehicle, "flagged", true);
                            Logging.Log($"Flagged {vehicle.Model} owned by {vehicle.Owner} for removal on cleanup.");
                        }
                    }
                }
            }
        }

        private void OnTimeUpdate(DateTime time)
        {
            if (time.Minute == 59)
            {
                var vehicles = API.getAllVehicles();

                foreach (var vehicleToRemove in vehicles)
                {
                    var vehicleData = GetVehicleData(vehicleToRemove);

                    // For every flagged vehicle, run the cleanup and remove vehicles with the "flagged" entity data set to true.
                    if (API.getEntityData(vehicleToRemove, "flagged") == true && vehicleData.Organization == 0)
                    {
                        API.deleteEntity(vehicleToRemove);
                        var entry = VehicleList.First(v => v.Vehicle == vehicleToRemove);
                        VehicleList.Remove(entry);
                    }
                }
            }
        }

        private void OnAccountLogin(Client player)
        {
            //SpawnPlayerVehicles(player);
        }


        private void OnResourceStart()
        {
            LoadVehicles();
        }

        private void LoadVehicles()
        {
            Logging.Log("[FIVERP] Loading FiveRP vehicles.", ConsoleColor.Yellow);
            try
            {
                using (var context = new Database.Database())
                {
                    // From "fiverp_vehicles" select *
                    var query = (from p in context.Vehicles
                        select p).ToList();
                    var count = 0;
                    foreach (var data in query)
                    {
                        var vehicleData = new FiveRPVehicle
                        {
                            Vehicle = new NetHandle(),
                            Id = data.Id,
                            Model = data.Model,
                            X = data.X,
                            Y = data.Y,
                            Z = data.Z,
                            H = data.H,
                            Colour1 = data.Colour1,
                            Colour2 = data.Colour2,
                            Organization = data.Organization,
                            Owner = data.Owner,
                            Plate = data.Plate,
                            Dimension = data.Dimension,
                            Type = data.Type,
                            InsuranceType = data.InsuranceType,
                            Health = data.Health,
                            EngineHealth = data.EngineHealth,
                            Fuel = data.Fuel,
                            DistanceDriven = data.DistanceDriven,
                            DestroyedCount = data.DestroyedCount,
                            Inventory = data.Inventory,
                            VehicleInventory = new VehicleInventory()
                        };
                        vehicleData.VehicleInventory = new VehicleInventory(vehicleData);
                        VehicleList.Add(vehicleData);

                        if (data.Organization > 0)
                        {
                            SpawnVehicle(vehicleData);
                        }

                        count++;
                    }

                    Logging.Log($"[FIVERP] Loaded {count} vehicles.", ConsoleColor.DarkGreen);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError("Exception: " + ex);
            }
        }

        public static void SaveVehicle(API api, NetHandle vehicle)
        {
            try
            {
                using (var context = new Database.Database())
                {
                    var vehicleTest = VehicleList.Select(p => p.Vehicle == vehicle);
                    if (vehicleTest.Any())
                    {

                        var vehicleData = GetVehicleData(vehicle);

                        if (vehicleData != null)
                        {
                            if (vehicleData.Organization == -1)
                            {

                            }
                            else
                            {
                                if (vehicleData.VehicleInventory != null)
                                    vehicleData.VehicleInventory.SaveInventory();
                                var query = (from ch in context.Vehicles
                                             where ch.Id == vehicleData.Id
                                             select ch).ToList().FirstOrDefault();

                                if (query != null)
                                {
                                    // all the fields are set
                                    query.Id = vehicleData.Id;
                                    query.Model = vehicleData.Model;
                                    query.X = vehicleData.X;
                                    query.Y = vehicleData.Y;
                                    query.Z = vehicleData.Z;
                                    query.H = vehicleData.H;

                                    query.Colour1 = vehicleData.Colour1;
                                    query.Colour2 = vehicleData.Colour2;

                                    query.Organization = vehicleData.Organization;
                                    query.Owner = vehicleData.Owner;
                                    query.Plate = vehicleData.Plate;
                                    query.Dimension = vehicleData.Dimension;
                                    query.Type = vehicleData.Type;
                                    query.InsuranceType = vehicleData.InsuranceType;
                                    query.Health = vehicleData.Health;
                                    query.EngineHealth = vehicleData.EngineHealth;
                                    query.Fuel = vehicleData.Fuel;
                                    query.DistanceDriven = vehicleData.DistanceDriven;
                                    query.DestroyedCount = vehicleData.DestroyedCount;
                                    query.Inventory = vehicleData.Inventory;
                                    query.VehicleInventory = vehicleData.VehicleInventory;
                                    context.Vehicles.Attach(query);

                                    //save modified entity using new Context
                                    using (var dbCtx = new Database.Database())
                                    {
                                        //3. Mark entity as modified
                                        dbCtx.Entry(query).State = System.Data.Entity.EntityState.Modified;

                                        dbCtx.SaveChanges();
                                    }
                                }
                            }
                        }
                        else
                        {
                            Logging.LogError("SaveVehicle failed (vehicleData was null)!");
                            AlertLogging.RaiseAlert("SaveVehicle failed (vehicleData was null)!", "DEBUG", 5);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogError("Exception: " + ex);
            }
        }

        public static FiveRPVehicle GetVehicleData(NetHandle vehicle)
        {
            var vehicleData = new FiveRPVehicle();
            foreach (var veh in VehicleList)
            {
                if (veh.Vehicle == vehicle)
                {
                    vehicleData = veh;
                }
            }
            return vehicleData;
        }

        public static FiveRPVehicle GetVehicleDataById(int id)
        {
            var vehicleData = new FiveRPVehicle();
            foreach (var veh in VehicleList)
            {
                if (veh.Id == id)
                {
                    vehicleData = veh;
                }
            }
            return vehicleData;
        }
    }
}
