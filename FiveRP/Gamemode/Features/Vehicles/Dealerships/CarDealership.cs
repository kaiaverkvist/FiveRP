using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Features.Inventories;

namespace FiveRP.Gamemode.Features.Vehicles.Dealerships
{
    public class CarDealership : Script
    {

        public static Dictionary<Client, NetHandle> previewVehicles = new Dictionary<Client, NetHandle>();
        private Dictionary<Client, Vector3> playerInitialLocation = new Dictionary<Client, Vector3>();

        public CarDealership()
        {
            API.onClientEventTrigger += OnClientEventTrigger;
            API.onPlayerExitVehicle += OnPlayerExitVehicle;
            API.onPlayerDisconnected += OnPlayerDisconnect;
        }

        private void OnPlayerDisconnect(Client player, string reason)
        {
            if (previewVehicles.ContainsKey(player))
            {
                API.deleteEntity(previewVehicles.Get(player));
                previewVehicles.Remove(player);
            }
        }

        private void OnPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            if (previewVehicles.ContainsKey(player))
            {
                API.deleteEntity(previewVehicles.Get(player));
                previewVehicles.Remove(player);
                API.freezePlayer(player, false);
                player.position = playerInitialLocation.Get(player);
                playerInitialLocation.Remove(player);
                API.sendChatMessageToPlayer(player, "You have stopped the vehicle preview because you exited the vehicle.");
            }
        }

        private void OnClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "menu_handler_select_item")
            {
                if ((string)arguments[0] == "dealership_menu")
                {
                    var vehicles = DealershipHandler.DealershipVehicleList;
                    // Remove everything after the first ~ tag indicating a colour change.
                    var input = (string)arguments[2];
                    input = input.Substring(0, input.IndexOf("~", StringComparison.Ordinal) - 1);

                    var vehicle = vehicles.FirstOrDefault(x => x.ListingFriendlyName == input);
                    if (vehicle != null)
                    {
                        if (sender.vehicle == null)
                        {
                            // Launch a vehicle preview for the vehicle selected.
                            PreviewVehicle(sender, vehicle);
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(sender, "Please exit your vehicle.");
                        }
                    }
                } else if ((string) arguments[0] == "dealership_menu_preview")
                {
                    if ((int) arguments[1] == 1)
                    {
                        BuyVehicle(sender);
                    }
                    else
                    {
                        // If cancel is pressed:
                        if (API.isPlayerInAnyVehicle(sender))
                        {
                            OnPlayerExitVehicle(sender, sender.vehicle);
                        }
                    }
                }
            }
        }

        [Command("buyvehicle", Alias = "bveh,buyveh", Group = "Vehicle Commands")]
        public void DealershipCommand(Client sender)
        {
            foreach (var dealership in DealershipHandler.DealershipList)
            {
                if (DistanceLibrary.DistanceBetween(API.getEntityPosition(sender), dealership.GetPosition()) <= 60f)
                {
                    OpenDealershipMenu(sender, dealership);
                }
            }
        }

        private void OpenDealershipMenu(Client sender, Dealership dealership)
        {
            var vehiclesList = new List<string>();

            var vehicles = dealership.GetDealershipVehicleList().OrderBy(p => p.ListingPrice);
            foreach (var data in vehicles)
                vehiclesList.Add($"{data.ListingFriendlyName} ~g~${NamingFunctions.FormatMoney(data.ListingPrice)}~w~");

            MenuLibrary.ShowNativeMenu(API, sender, "dealership_menu", "Vehicle Dealership", "Buy a vehicle", false,
                vehiclesList);
        }

        private void PreviewVehicle(Client sender, DealershipVehicle vehicleData)
        {

            var playerPosition = sender.position;
            var playerRotation = sender.rotation;

            playerInitialLocation.Set(sender, playerPosition);

            Vehicle previewVehicle;
            if (vehicleData.ListingCategory == DealershipCategories.Plane)
                previewVehicle = API.createVehicle(API.vehicleNameToModel(vehicleData.ListingModel), new Vector3(-1223.304, -2585.691, 13.51464), new Vector3(0, 0, 60.28771), 0, 0);
            else if (vehicleData.ListingCategory == DealershipCategories.CountyPlane)
                previewVehicle = API.createVehicle(API.vehicleNameToModel(vehicleData.ListingModel), new Vector3(1708.6, 3256.2, 41), new Vector3(0,0,100.5), 0, 0);
            else if (vehicleData.ListingCategory == DealershipCategories.Boats)
                previewVehicle = API.createVehicle(API.vehicleNameToModel(vehicleData.ListingModel), new Vector3(-785.0436, -1437.265, 0.389993), new Vector3(-1.80968, -0.05723587, 136.7106), 0, 0);
            else
                previewVehicle = API.createVehicle(API.vehicleNameToModel(vehicleData.ListingModel), playerPosition, playerRotation, 0, 0);

            previewVehicles.Set(sender, previewVehicle);

            // Make the vehicle transparent.
            API.setEntityTransparency(previewVehicle, 220);

            // Make the vehicle collisionless to prevent many forms of abuse.
            API.setEntityCollisionless(previewVehicle, true);

            // Turn the engine off so the person can't drive away.
            API.setVehicleEngineStatus(previewVehicle, false);

            API.setPlayerIntoVehicle(sender, previewVehicle, -1);

            API.freezePlayer(sender, true);
            API.setEntityPositionFrozen(previewVehicle, true);

            PreviewMenu(sender, vehicleData);
        }

        public void PreviewMenu(Client sender, DealershipVehicle vehicleData)
        {

                var previewList = new List<string>
                {
                    "~r~Cancel",
                    $"Buy Vehicle for ~g~${NamingFunctions.FormatMoney(vehicleData.ListingPrice)}"
                };

                MenuLibrary.ShowNativeMenu(API, sender, "dealership_menu_preview", "Vehicle Preview",
                    "Purchase options", false, previewList);
        }

        public void BuyVehicle(Client sender)
        {
            var vehicle = sender.vehicle;

            if (!vehicle.IsNull)
            {
                var charData = Account.GetPlayerCharacterData(sender);

                // find a vehicle in the dealership list that has a model matching the one the player is in
                var dealershipListing = DealershipHandler.DealershipVehicleList.Find(p => p.ListingModel == Enum.GetName(typeof(VehicleHash), API.getEntityModel(vehicle)));

                if (dealershipListing != null)
                {
                    if (charData.CharacterData.Bank >= dealershipListing.ListingPrice)
                    {
                        // Pull the money, and create the vehicle.
                        charData.CharacterData.Bank -= dealershipListing.ListingPrice;

                        var newVehicle = new FiveRPVehicle
                        {
                            Organization = 0,
                            Model = Enum.GetName(typeof(VehicleHash), API.getEntityModel(vehicle)),
                            Colour1 = 0,
                            Colour2 = 0,
                            Owner = charData.CharacterId,
                            Type = 0,
                            Fuel = 100.0f,
                            Health = 1000.0f,
                            EngineHealth = 100.0f,
                            Inventory = "",
                            VehicleInventory = new VehicleInventory()
                        };

                        var pos = API.getEntityPosition(API.getPlayerVehicle(sender));

                        newVehicle.X = pos.X;
                        newVehicle.Y = pos.Y;
                        newVehicle.Z = pos.Z;

                        var rot = API.getEntityRotation(API.getPlayerVehicle(sender));
                        newVehicle.H = rot.Z;

                        // Generate a plate
                        newVehicle.Plate = NamingFunctions.RandomString(3) + NamingFunctions.RandomNumberAsString(3);

                        newVehicle.Dimension = API.getEntityDimension(sender);

                        using (var dbCtx = new Database.Database())
                        {
                            // add the object to the database, and flag it as newly added
                            dbCtx.Vehicles.Add(newVehicle);
                            dbCtx.Entry(newVehicle).State = System.Data.Entity.EntityState.Added;

                            // save the entry into the database
                            dbCtx.SaveChanges();

                            API.sendChatMessageToPlayer(sender, "You have bought the vehicle!");
                            API.sendChatMessageToPlayer(sender, "Please use ~g~/vbuypark~w~ ($250) to set the spawning position of the vehicle.");
                            API.sendChatMessageToPlayer(sender,
                                "When the vehicle is despawned, you can use /vget to retrieve it again.");
                            API.sendChatMessageToPlayer(sender, "To despawn your vehicle, you may use /vpark at the park location of the vehicle.");
                        }

                        // 
                        API.setEntityTransparency(vehicle, 255);
                        API.setEntityCollisionless(vehicle, false);

                        API.setVehicleEngineStatus(vehicle, false);

                        API.setPlayerIntoVehicle(sender, vehicle, -1);

                        API.freezePlayer(sender, false);

                        playerInitialLocation.Remove(sender);
                        previewVehicles.Remove(sender);

                        API.setVehicleNumberPlate(vehicle, newVehicle.Plate);

                        var vehicleData = new FiveRPVehicle
                        {
                            Vehicle = vehicle,
                            Id = newVehicle.Id,
                            Model = newVehicle.Model,
                            X = newVehicle.X,
                            Y = newVehicle.Y,
                            Z = newVehicle.Z,
                            H = newVehicle.H,
                            Colour1 = newVehicle.Colour1,
                            Colour2 = newVehicle.Colour2,
                            Organization = newVehicle.Organization,
                            Owner = newVehicle.Owner,
                            Plate = newVehicle.Plate,
                            Dimension = newVehicle.Dimension,
                            Health = newVehicle.Health,
                            Fuel = newVehicle.Fuel,
                            EngineHealth = newVehicle.EngineHealth,
                            Inventory = "",
                            VehicleInventory = new VehicleInventory()
                        };
                        vehicleData.VehicleInventory = new VehicleInventory(vehicleData);
                        // Add the vehicle to the main vehicle list (ie. the static list the server uses to keep track of vehicles)
                        VehicleHandler.VehicleList.Add(vehicleData);
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(sender, "You cannot afford this vehicle with the money you currently have in your bank account.");
                        API.sendChatMessageToPlayer(sender, "~g~Notice: Your money must be in your bank account.~w~");
                    }
                }
            }
            else
            {
                // Send an error alert.
                AlertLogging.RaiseAlert($"{sender.name} almost bought a vehicle without being assigned a vehicle. Something went wrong.", "DEBUG", 3);
            }
        }
    }
}