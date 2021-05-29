using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Features.Admin;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Features.Inventories;
using FiveRP.Gamemode.Database.Tables;
using System;
using FiveRP.Gamemode.Features.Vehicles.Dealerships;

namespace FiveRP.Gamemode.Features.Vehicles
{
    public class VehicleCommands : Script
    {
        private readonly List<ColShape> vgetColShapes = new List<ColShape>();

        public VehicleCommands()
        {
            API.onPlayerExitVehicle += OnPlayerExitVehicle;
            API.onPlayerEnterVehicle += OnPlayerEnterVehicle;
            MenuLibrary.OnMenuSelected += MenuLibrary_OnMenuSelected;

            API.onEntityEnterColShape += API_onEntityEnterColShape;
            API.onPlayerDisconnected += OnPlayerDisconnected;
        }

        private void ChangeDoorState(Client player, NetHandle vehicle, int doorId)
        {
            string door = "";
            switch (doorId)
            {
                case 0:
                    door = "left door of the car.";
                    break;
                case 1:
                    door = "right door of the car.";
                    break;
                case 2:
                    door = "left rear door of the car.";
                    break;
                case 3:
                    door = "right rear door of the car.";
                    break;
                case 4:
                    door = "hood of the car.";
                    break;
                case 5:
                    door = "trunk of the car.";
                    break;
            }

            if (API.getVehicleDoorState(vehicle, doorId))
            {
                ChatLibrary.SendLabelEmoteMessage(API, player, "closes the " + door);
                API.setVehicleDoorState(vehicle, doorId, false);
            }
            else
            {
                ChatLibrary.SendLabelEmoteMessage(API, player, "opens the " + door);
                API.setVehicleDoorState(vehicle, doorId, true);
            }
        }

        private void MenuLibrary_OnMenuSelected(Client sender, string menuName, int selectedIndex)
        {
            if (menuName == "vehicle_management")
            {
                var allVehicles = API.getAllVehicles();
                foreach (var vehicle in allVehicles)
                {
                    if (DistanceLibrary.DistanceBetween(sender.position, API.getEntityPosition(vehicle)) < 5.0 || (sender.vehicle != null && sender.vehicle == vehicle))
                    {
                        bool canOpen = false;
                        if (sender.vehicle != null && sender.vehicle == vehicle)
                            canOpen = true;
                        else if (API.getVehicleLocked(vehicle))
                        {
                            API.sendChatMessageToPlayer(sender, "~r~The vehicle is locked.");
                            return;
                        }
                        else
                            canOpen = true;

                        if (canOpen)
                        {
                            if (selectedIndex == 0)
                                ChangeDoorState(sender, vehicle, 4);
                            else if (selectedIndex == 1)
                                ChangeDoorState(sender, vehicle, 5);
                            else if (selectedIndex == 6)
                                VehicleItems(sender);
                            else if (selectedIndex == 7)
                                TakeAllItemsFromVehicle(sender);
                            else if (selectedIndex == 8)
                                PlaceAllItemsFromVehicle(sender);
                            else
                                ChangeDoorState(sender, vehicle, selectedIndex - 2);
                        }
                        return;
                    }
                }
            }
            else if (menuName == "vehicle_get_list")
            {
                var charData = Account.GetPlayerCharacterData(sender);
                var vehicles = VehicleHandler.VehicleList.Where(v => v.Owner == charData.CharacterId && v.Organization != -1);

                var vehicle = vehicles.ElementAt(selectedIndex);
                if (!API.doesEntityExist(vehicle.Vehicle))
                {
                    var vehicleEntity = API.createVehicle(API.vehicleNameToModel(vehicle.Model), new Vector3(vehicle.X, vehicle.Y, vehicle.Z), new Vector3(0, 0, vehicle.H), vehicle.Colour1, vehicle.Colour2);

                    // Load variables and shit
                    API.setVehicleEngineStatus(vehicleEntity, false);
                    API.setVehicleNumberPlate(vehicleEntity, vehicle.Plate);
                    API.setEntityDimension(vehicleEntity, 0);

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

                    sender.sendChatMessage("Your vehicle has been spawned! Go to the ~y~yellow blip~w~ on the map to find your vehicle.");

                    // Freeze the entity so it doesn't fall through the world
                    API.setEntityPositionFrozen(vehicleEntity.handle, true);

                    // Create the spawn blip
                    BlipLibrary.CreateBlipForPlayer(API, sender, $"vehicle_spawn_blip_{vehicle.Id}", new Vector3(vehicle.X, vehicle.Y, vehicle.Z), 46);
                    var colShape = API.createCylinderColShape(new Vector3(vehicle.X, vehicle.Y, vehicle.Z), 10f, 30f);
                    colShape.setData("colshape_player_blip", sender);
                    colShape.setData("colshape_vehicle_id", vehicle.Id);

                    // Add the collision shape
                    vgetColShapes.Add(colShape);
                }

                if (API.getVehicleOccupants(vehicle.Vehicle).Any())
                    sender.sendChatMessage("Your vehicle is occupied, so we could not help you locate it.");
                else
                {
                    sender.sendChatMessage("We've placed a ~y~blip~w~ on your map to help you locate your vehicle.");
                    BlipLibrary.CreateBlipForPlayer(API, sender, $"vehicle_spawn_blip_{vehicle.Id}", new Vector3(vehicle.X, vehicle.Y, vehicle.Z), 46);
                    var colShape = API.createCylinderColShape(new Vector3(vehicle.X, vehicle.Y, vehicle.Z), 10f, 30f);
                    colShape.setData("colshape_player_blip", sender);
                    colShape.setData("colshape_vehicle_id", vehicle.Id);
                }
            }
        }

        private void OnPlayerDisconnected(Client player, string reason)
        {
            foreach (var colshape in vgetColShapes)
            {
                if (colshape.hasData("colshape_player_blip"))
                {
                    var data = colshape.getData("colshape_player_blip");
                    if (data == player)
                    {
                        if (vgetColShapes.Contains(colshape))
                        {
                            vgetColShapes.Remove(colshape);
                        }

                        API.deleteColShape(colshape);
                    }
                }
            }
        }

        private void API_onEntityEnterColShape(ColShape colshape, NetHandle entity)
        {
            if (colshape.hasData("colshape_player_blip"))
            {
                var data = colshape.getData("colshape_player_blip");
                if (data == entity)
                {
                    if (colshape.hasData("colshape_vehicle_id"))
                    {
                        var player = API.getPlayerFromHandle(entity);
                        var vehicleId = colshape.getData("colshape_vehicle_id");

                        if (vgetColShapes.Contains(colshape))
                        {
                            vgetColShapes.Remove(colshape);
                        }
                        API.deleteColShape(colshape);

                        player.sendChatMessage("You have found your vehicle.");
                        BlipLibrary.RemoveBlipForPlayer(API, player, $"vehicle_spawn_blip_{vehicleId}");
                    }
                }
            }
        }

        private void OnPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            var vehicleData = VehicleHandler.GetVehicleData(vehicle);
            if (vehicleData?.Organization > 0 && vehicleData.Organization != Account.GetPlayerCharacterData(player).CharacterData.Organization)
            {
                if (player.vehicleSeat == -1)
                {
                    // If the player is the driver, and the vehicle's faction is not the same as the players, kick them out of the vehicle.
                    API.warpPlayerOutOfVehicle(player, vehicle);
                }
            }
        }

        private void OnPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            // Reset the seatbelt, assuming the player "unbuckles" while exiting.
            API.setPlayerSeatbelt(player, false);
        }

        [Command("engine", Group = "Vehicle Commands")]
        public void EngineCommand(Client player)
        {
            if (CarDealership.previewVehicles.ContainsKey(player))
            {
                API.sendChatMessageToPlayer(player, "~r~Error:~w~ You cannot engine this vehicle.");
                return;
            }
            if (player.vehicle == null)
            {
                API.sendChatMessageToPlayer(player, "~r~Error:~w~ You're not in a vehicle.");
            }
            else
            {
                var seat = API.getPlayerVehicleSeat(player);
                var isAdmin = AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin, false);

                // Check that the player is actually the driver.
                if (seat == -1)
                {
                    var vehicleData = VehicleHandler.GetVehicleData(API.getPlayerVehicle(player));
                    var characterData = Account.GetPlayerCharacterData(player);

                    if (vehicleData.Organization >= 1 && vehicleData.Organization != Account.GetPlayerCharacterData(player).CharacterData.Organization)
                    {
                        API.sendChatMessageToPlayer(player, "~r~Error:~w~ You do not have access to this vehicle.");
                    }
                    else if (vehicleData.Organization < 0 && !isAdmin)
                    {
                        API.sendChatMessageToPlayer(player, "~r~Error:~w~ You do not have access to this vehicle.");
                    }
                    else if(vehicleData.Owner != 0 && vehicleData.Owner != characterData.CharacterId)
                    {
                        API.sendChatMessageToPlayer(player, "~r~Error:~w~ You do not have access to this vehicle.");
                    }
                    else
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
                } else
                {
                    API.sendChatMessageToPlayer(player, "~r~Error:~w~ You're not the driver of the vehicle.");
                }
            }
        }

        [Command("seatbelt", Alias = "sb", Group = "Vehicle Commands")]
        public void SeatbeltCommand(Client player)
        {
            if (player.vehicle == null)
            {
                API.sendChatMessageToPlayer(player, "~r~Error:~w~ You're not in a vehicle.");
            }
            else
            {
                var seatbelt = API.getPlayerSeatbelt(player);
                if (seatbelt)
                {
                    API.setPlayerSeatbelt(player, false);
                    ChatLibrary.SendLabelEmoteMessage(API, player, "has unfastened their seatbelt.");
                }
                else
                {
                    API.setPlayerSeatbelt(player, true);
                    ChatLibrary.SendLabelEmoteMessage(API, player, "has fastened their seatbelt.");
                }
            
            }
        }

        [Command("vehicleget", Alias = "vget", Group = "Vehicle Commands")]
        public void GetVehicleCommand(Client sender)
        {
            var charData = Account.GetPlayerCharacterData(sender);
            var vehicles = VehicleHandler.VehicleList.Where(v => v.Owner == charData.CharacterId && v.Organization != -1);
            var vehicleList = new List<string>();

            foreach (var vehicle in vehicles)
            {
                vehicleList.Add($"[{vehicle.Id}] {vehicle.Model}");
            }

            MenuLibrary.ShowNativeMenu(API, sender, "vehicle_get_list", "Your Vehicles", "Select a vehicle to spawn.", false, vehicleList);
        }

        [Command("vehiclepark", Alias = "vpark", Group = "Vehicle Commands")]
        public void ParkVehicleCommand(Client player)
        {
            if (player.vehicle == null)
            {
                API.sendChatMessageToPlayer(player, "~r~Error:~w~ You're not in a vehicle.");
            }
            else
            {
                var vehicleData = VehicleHandler.GetVehicleData(player.vehicle);
                var characterData = Account.GetPlayerCharacterData(player);

                if (vehicleData != null)
                {
                    if (vehicleData.Owner == characterData.CharacterId && vehicleData.Organization == 0)
                    {
                        var currentPosition = player.vehicle.position;
                        var parkingPosition = new Vector3(vehicleData.X, vehicleData.Y, vehicleData.Z);

                        // Save the vehicle!
                        VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);

                        if (DistanceLibrary.DistanceBetween(currentPosition, parkingPosition) < 10.0)
                        {
                            API.setEntityPosition(vehicleData.Vehicle, parkingPosition);
                            API.setEntityRotation(vehicleData.Vehicle, new Vector3(0, 0, vehicleData.H));
                            API.setVehicleEngineStatus(vehicleData.Vehicle, false);
                            /*API.sendChatMessageToPlayer(player, "You have parked your vehicle (~g~Notice: you do not need to do this. Simply park where your vehicle is parked.~w~).");*/
                            if (API.doesEntityExist(vehicleData.Vehicle))
                            {
                                API.deleteEntity(vehicleData.Vehicle);

                                player.sendChatMessage("You have despawned your vehicle.");
                                vehicleData.Vehicle = default(NetHandle);
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "You are too far away from the parking spot of this vehicle.");
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "This vehicle is not owned by you.");
                    }
                }
            }
        }

        [Command("vbuyparking", Alias = "vbuypark", Group = "Vehicle Commands")]
        public void BuyParkingCommand(Client player)
        {
            if (player.vehicle == null)
            {
                API.sendChatMessageToPlayer(player, "~r~Error:~w~ You're not in a vehicle.");
            }
            else
            {
                var currentPos = player.vehicle.position;
                var currentRot = player.vehicle.rotation;
                var vehicleData = VehicleHandler.GetVehicleData(player.vehicle);
                var characterData = Account.GetPlayerCharacterData(player);

                if (vehicleData != null)
                {
                    // TODO: Add config option (through db) for parking spot money
                    if (characterData.CharacterData.Money >= 250)
                    {
                        if (characterData.CharacterId == vehicleData.Owner && vehicleData.Organization == 0)
                        {
                            characterData.CharacterData.Money -= 250;

                            vehicleData.X = currentPos.X;
                            vehicleData.Y = currentPos.Y;
                            vehicleData.Z = currentPos.Z;
                            // Rotation
                            vehicleData.H = currentRot.Z;

                            VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);

                            API.sendChatMessageToPlayer(player, "~g~You have bought this parking spot for $250. Your vehicle will now spawn here.");
                        } else player.sendChatMessage("You don't own this vehicle.");
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "~r~Error:~w~ You cannot afford to buy this parking spot.");
                    }
                }
            }
        }

        [Command("lockvehicle", Alias = "vlock,lock", Group = "Vehicle Commands")]
        public void LockCommand(Client player)
        {
            var foundVehicle = false;

            var characterData = Account.GetPlayerCharacterData(player);
            if (player.vehicle == null)
            {
                var allVehicles = API.getAllVehicles();

                foreach (var vehicle in allVehicles)
                {
                    // If the vehicle is close enough, do the lock check.
                    if (DistanceLibrary.DistanceBetween(player.position, API.getEntityPosition(vehicle)) < 5.0)
                    {
                        var vehicleData = VehicleHandler.GetVehicleData(vehicle);

                        if (vehicleData.Owner == characterData.CharacterData.CharacterUcpId)
                        {
                            if (API.getVehicleLocked(vehicle))
                            {
                                API.sendChatMessageToPlayer(player, "You have ~g~unlocked~w~ your vehicle.");
                                API.setVehicleLocked(vehicle, false);
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player, "You have ~r~locked~w~ your vehicle.");
                                API.setVehicleLocked(vehicle, true);
                            }
                            foundVehicle = true;
                            break;
                        }
                        else
                        {
                            //API.sendChatMessageToPlayer(player, "You do not have access to this vehicle.");
                        }

                        // break out of the foreach loop since we've found a vehicle.
                    }
                }

                if (!foundVehicle)
                {
                    API.sendChatMessageToPlayer(player, "You are not close enough to any vehicles you own.");
                }
            }
            else // if the player is in a vehicle
            {
                var vehicleData = VehicleHandler.GetVehicleData(player.vehicle);

                if (vehicleData.Owner == characterData.CharacterData.CharacterUcpId)
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
                else
                {
                    API.sendChatMessageToPlayer(player, "You do not have access to this vehicle.");
                }
            }
        }

        [Command("vehmenu", Alias = "vmenu", Group = "Vehicle Commands")]
        public void VehicleMenu(Client player)
        {
            var allVehicles = API.getAllVehicles();
            foreach (var vehicle in allVehicles)
            {
                if (DistanceLibrary.DistanceBetween(player.position, API.getEntityPosition(vehicle)) < 5.0 || (player.vehicle != null && player.vehicle == vehicle))
                {
                    bool canOpen = false;
                    if (player.vehicle != null && player.vehicle == vehicle)
                        canOpen = true;
                    else if (API.getVehicleLocked(vehicle))
                    {
                        API.sendChatMessageToPlayer(player, "~r~The vehicle is locked.");
                        return;
                    }
                    else
                        canOpen = true;

                    if (canOpen)
                    {
                        var vehicleData = VehicleHandler.GetVehicleData(vehicle);
                        List<string> actionList = new List<string>();
                        actionList.Add("Open/Close Hood");
                        actionList.Add("Open/Close Trunk");
                        actionList.Add("Open/Close front left door");
                        actionList.Add("Open/Close front right door");
                        actionList.Add("Open/Close rear left door");
                        actionList.Add("Open/Close rear right door");
                        actionList.Add("See all items");
                        actionList.Add("Take all items");
                        actionList.Add("Put all items");
                        MenuLibrary.ShowNativeMenu(API, player, "vehicle_management", "Vehicle manager", "Select an action", false, actionList);
                    }
                    return;
                }
            }
        }

        [Command("vdoor", Alias = "vehdoor", Group = "Vehicle Commands")]
        public void OpenVehicleDoor(Client player, string door)
        {
            var allVehicles = API.getAllVehicles();
            foreach (var vehicle in allVehicles)
            {
                if (DistanceLibrary.DistanceBetween(player.position, API.getEntityPosition(vehicle)) < 5.0 || (player.vehicle != null && player.vehicle == vehicle))
                {
                    bool canOpen = false;
                    if (player.vehicle != null && player.vehicle == vehicle)
                        canOpen = true;
                    else if (API.getVehicleLocked(vehicle))
                    {
                        API.sendChatMessageToPlayer(player, "~r~The vehicle is locked.");
                        return;
                    }
                    else
                        canOpen = true;

                    if (canOpen)
                    {
                        if (door.ToLower() == "left")
                            ChangeDoorState(player, vehicle, 0);
                        else if (door.ToLower() == "right")
                            ChangeDoorState(player, vehicle, 1);
                        else if (door.ToLower() == "rleft" || door.ToLower() == "rearleft")
                            ChangeDoorState(player, vehicle, 2);
                        else if (door.ToLower() == "rright" || door.ToLower() == "rearright")
                            ChangeDoorState(player, vehicle, 3);
                        else
                            API.sendChatMessageToPlayer(player, "~r~Wrong door, use: left, right, rleft, rright.");
                    }
                    return;
                }
            }
        }

        [Command("hood", Alias = "vehdoor", Group = "Vehicle Commands")]
        public void OpenVehicleHood(Client player)
        {
            var allVehicles = API.getAllVehicles();
            foreach (var vehicle in allVehicles)
            {
                if (DistanceLibrary.DistanceBetween(player.position, API.getEntityPosition(vehicle)) < 5.0 || (player.vehicle != null && player.vehicle == vehicle))
                {
                    bool canOpen = false;
                    if (player.vehicle != null && player.vehicle == vehicle)
                        canOpen = true;
                    else if (API.getVehicleLocked(vehicle))
                    {
                        API.sendChatMessageToPlayer(player, "~r~The vehicle is locked.");
                        return;
                    }
                    else
                        canOpen = true;

                    if (canOpen)
                        ChangeDoorState(player, vehicle, 4);
                    return;
                }
            }
        }

        [Command("trunk", Group = "Vehicle Commands")]
        public void OpenVehicleTrunk(Client player)
        {
            var allVehicles = API.getAllVehicles();
            foreach (var vehicle in allVehicles)
            {
                if (DistanceLibrary.DistanceBetween(player.position, API.getEntityPosition(vehicle)) < 5.0 || (player.vehicle != null && player.vehicle == vehicle))
                {
                    bool canOpen = false;
                    if (player.vehicle != null && player.vehicle == vehicle)
                        canOpen = true;
                    else if (API.getVehicleLocked(vehicle))
                    {
                        API.sendChatMessageToPlayer(player, "~r~The vehicle is locked.");
                        return;
                    }
                    else
                        canOpen = true;

                    if (canOpen)
                        ChangeDoorState(player, vehicle, 5);
                    return;
                }
            }
        }

        [Command("vehplaceitem", Alias = "vpitem", Group = "Vehicle Commands")]
        public void PlaceItemInVehicle(Client player, string itemid, string amount = "1")
        {
            var allVehicles = API.getAllVehicles();
            foreach (var vehicle in allVehicles)
            {
                if (DistanceLibrary.DistanceBetween(player.position, API.getEntityPosition(vehicle)) < 5.0 || (player.vehicle != null && player.vehicle == vehicle))
                {
                    bool isInVehicle = false;
                    if (player.vehicle != null && player.vehicle == vehicle)
                        isInVehicle = true;
                    if (!API.getVehicleDoorState(vehicle, 5) && !isInVehicle)
                    {
                        API.sendChatMessageToPlayer(player, "~r~You need to open the trunk of the vehicle first.");
                        return;
                    }
                    var vehicleData = VehicleHandler.GetVehicleData(vehicle);
                    var characterData = Account.GetPlayerCharacterData(player);
                    int itemAmount = 0;
                    int itemId = 0;
                    bool parsedAmount = Int32.TryParse(amount.ToString(), out itemAmount);
                    bool parsedItem = Int32.TryParse(itemid.ToString(), out itemId);

                    if (vehicleData != null && characterData != null)
                    {
                        VehicleInventory vehicleInventory = vehicleData.VehicleInventory;
                        Inventory playerInventory = characterData.CharacterData.Inventory;
                        List<Item> itemList = playerInventory.GetItems().Keys.ToList();
                        if (itemId <= 0 || itemId > itemList.Count)
                        {
                            API.sendChatMessageToPlayer(player, "~r~Invalid item id.");
                            return;
                        }
                        Item item = itemList[itemId - 1];
                        if (item == null || !parsedAmount || !parsedItem || itemAmount <= 0 || !item.Giveable || item.ItemType == ItemTypes.Fish)
                        {
                            API.sendChatMessageToPlayer(player, "~r~Invalid item id, amount or item cannot be placed in a car.");
                            return;
                        }
                        if (playerInventory.CanRemoveItem(item, itemAmount) && vehicleInventory.CanAddItem(item, itemAmount))
                        {
                            playerInventory.RemoveItem(item, itemAmount);
                            vehicleInventory.AddItem(item, itemAmount);
                            API.sendChatMessageToPlayer(player, "~r~You placed " + item.Name + " (" + itemAmount + ") in the vehicle.");
                            ChatLibrary.SendLabelEmoteMessage(API, player, "places something in the vehicle.");
                            VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);
                        }
                        else
                            API.sendChatMessageToPlayer(player, "~r~You do not have " + amount + " " + item.Name);
                    }
                    return;
                }
            }
        }

        [Command("vehtakeitem", Alias = "vtitem", Group = "Vehicle Commands")]
        public void TakeItemFromVehicle(Client player, string itemid, string amount = "1")
        {
            var allVehicles = API.getAllVehicles();
            foreach (var vehicle in allVehicles)
            {
                if (DistanceLibrary.DistanceBetween(player.position, API.getEntityPosition(vehicle)) < 5.0 || (player.vehicle != null && player.vehicle == vehicle))
                {
                    bool isInVehicle = false;
                    if (player.vehicle != null && player.vehicle == vehicle)
                        isInVehicle = true;
                    if (!API.getVehicleDoorState(vehicle, 5) && !isInVehicle)
                    {
                        API.sendChatMessageToPlayer(player, "~r~You need to open the trunk of the vehicle first.");
                        return;
                    }
                    var vehicleData = VehicleHandler.GetVehicleData(vehicle);
                    var characterData = Account.GetPlayerCharacterData(player);
                    int itemAmount = 0;
                    int itemId = 0;
                    bool parsedAmount = Int32.TryParse(amount.ToString(), out itemAmount);
                    bool parsedItem = Int32.TryParse(itemid.ToString(), out itemId);

                    if (vehicleData != null && characterData != null)
                    {
                        VehicleInventory vehicleInventory = vehicleData.VehicleInventory;
                        Inventory playerInventory = characterData.CharacterData.Inventory;
                        List<Item> itemList = vehicleInventory.GetItems().Keys.ToList();
                        if (itemId <= 0 || itemId > itemList.Count)
                        {
                            API.sendChatMessageToPlayer(player, "~r~Invalid item id.");
                            return;
                        }
                        Item item = itemList[itemId - 1];
                        if (item == null || !parsedAmount || !parsedItem || itemAmount <= 0 || !item.Giveable)
                        {
                            API.sendChatMessageToPlayer(player, "~r~Invalid item id, amount or item cannot be given.");
                            return;
                        }
                        if (playerInventory.CanAddItem(item, itemAmount) && vehicleInventory.CanRemoveItem(item, itemAmount))
                        {
                            playerInventory.AddItem(item, itemAmount);
                            vehicleInventory.RemoveItem(item, itemAmount);
                            API.sendChatMessageToPlayer(player, "~r~You took " + item.Name + " (" + itemAmount + ") from the vehicle.");
                            ChatLibrary.SendLabelEmoteMessage(API, player, "takes something from the vehicle.");
                            VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);
                        }
                        else
                            API.sendChatMessageToPlayer(player, "~r~The vehicle does not have " + amount + " " + item.Name);
                    }
                    return;
                }
            }
        }

        [Command("vehtakeitems", Alias = "vtitems", Group = "Vehicle Commands")]
        public void TakeAllItemsFromVehicle(Client player)
        {
            var allVehicles = API.getAllVehicles();
            foreach (var vehicle in allVehicles)
            {
                if (DistanceLibrary.DistanceBetween(player.position, API.getEntityPosition(vehicle)) < 5.0 || (player.vehicle != null && player.vehicle == vehicle))
                {
                    bool isInVehicle = false;
                    if (player.vehicle != null && player.vehicle == vehicle)
                        isInVehicle = true;
                    if (!API.getVehicleDoorState(vehicle, 5) && !isInVehicle)
                    {
                        API.sendChatMessageToPlayer(player, "~r~You need to open the trunk of the vehicle first.");
                        return;
                    }
                    var vehicleData = VehicleHandler.GetVehicleData(vehicle);
                    var characterData = Account.GetPlayerCharacterData(player);

                    if (vehicleData != null && characterData != null)
                    {
                        VehicleInventory vehicleInventory = vehicleData.VehicleInventory;
                        Inventory playerInventory = characterData.CharacterData.Inventory;
                        List<Item> itemList = vehicleInventory.GetItems().Keys.ToList();
                        bool tookItems = false;
                        foreach (Item item in itemList)
                        {
                            int itemAmount = vehicleInventory.GetItems()[item];
                            if (playerInventory.CanAddItem(item, itemAmount) && vehicleInventory.CanRemoveItem(item, itemAmount))
                            {
                                tookItems = true;
                                playerInventory.AddItem(item, itemAmount);
                                vehicleInventory.RemoveItem(item, itemAmount);
                                API.sendChatMessageToPlayer(player, "~r~You took " + item.Name + " (" + itemAmount + ") from the vehicle.");
                            }
                        }
                        if (tookItems)
                        {
                            ChatLibrary.SendLabelEmoteMessage(API, player, "takes something from the vehicle.");
                            VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);
                        }
                    }
                    return;
                }
            }
        }

        [Command("vehplaceitems", Alias = "vpitems", Group = "Vehicle Commands")]
        public void PlaceAllItemsFromVehicle(Client player)
        {
            var allVehicles = API.getAllVehicles();
            foreach (var vehicle in allVehicles)
            {
                if (DistanceLibrary.DistanceBetween(player.position, API.getEntityPosition(vehicle)) < 5.0 || (player.vehicle != null && player.vehicle == vehicle))
                {
                    bool isInVehicle = false;
                    if (player.vehicle != null && player.vehicle == vehicle)
                        isInVehicle = true;
                    if (!API.getVehicleDoorState(vehicle, 5) && !isInVehicle)
                    {
                        API.sendChatMessageToPlayer(player, "~r~You need to open the trunk of the vehicle first.");
                        return;
                    }
                    var vehicleData = VehicleHandler.GetVehicleData(vehicle);
                    var characterData = Account.GetPlayerCharacterData(player);

                    if (vehicleData != null && characterData != null)
                    {
                        VehicleInventory vehicleInventory = vehicleData.VehicleInventory;
                        Inventory playerInventory = characterData.CharacterData.Inventory;
                        List<Item> itemList = playerInventory.GetItems().Keys.ToList();
                        bool tookItems = false;
                        foreach (Item item in itemList)
                        {
                            int itemAmount = playerInventory.GetItems()[item];
                            if (playerInventory.CanRemoveItem(item, itemAmount) && vehicleInventory.CanAddItem(item, itemAmount) && item.Giveable && item.ItemType != ItemTypes.Fish)
                            {
                                tookItems = true;
                                playerInventory.RemoveItem(item, itemAmount);
                                vehicleInventory.AddItem(item, itemAmount);
                                API.sendChatMessageToPlayer(player, "~r~You placed " + item.Name + " (" + itemAmount + ") in the vehicle.");
                            }
                        }
                        if (tookItems)
                        {
                            ChatLibrary.SendLabelEmoteMessage(API, player, "places something in the vehicle.");
                            VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);
                        }
                    }
                    return;
                }
            }
        }

        [Command("vehitems", Alias = "vehinv,vinv,vehinventory,vitems", GreedyArg = true, Group = "Inventory Commands")]
        public void VehicleItems(Client player)
        {
            var allVehicles = API.getAllVehicles();
            foreach (var vehicle in allVehicles)
            {
                if (DistanceLibrary.DistanceBetween(player.position, API.getEntityPosition(vehicle)) < 5.0 || (player.vehicle != null && player.vehicle == vehicle))
                {
                    bool isInVehicle = false;
                    if (player.vehicle != null && player.vehicle == vehicle)
                        isInVehicle = true;
                    if (!API.getVehicleDoorState(vehicle, 5) && !isInVehicle)
                    {
                        API.sendChatMessageToPlayer(player, "~r~You need to open the trunk of the vehicle first.");
                        return;
                    }
                    var vehicleData = VehicleHandler.GetVehicleData(vehicle);
                    if (vehicleData == null)
                        return;
                    VehicleInventory inventory = vehicleData.VehicleInventory;
                    if (inventory == null)
                    {
                        API.sendChatMessageToPlayer(player, "~r~You cannot access this vehicle's inventory.");
                        return;
                    }
                    Dictionary<Item, int> inventoryItems = inventory.GetItems();
                    API.sendChatMessageToPlayer(player, "~g~|-------- Vehicle Items --------|");
                    int id = 0;
                    foreach (KeyValuePair<Item, int> item in inventoryItems)
                    {
                        if (item.Key.Weight == 0)
                            API.sendChatMessageToPlayer(player, "~y~" + ++id + ": " + item.Key.Name + " x" + item.Value);
                        else
                            API.sendChatMessageToPlayer(player, "~y~" + ++id + ": " + item.Key.Name + " x" + item.Value + " (" + item.Value * item.Key.Weight + "g)");
                    }
                    API.sendChatMessageToPlayer(player, "~y~Total weight: " + inventory.GetCurrentWeight() + "/" + inventory.GetMaxWeight() + " grams");
                    return;
                }
            }
        }

        [Command("vstats")]
        public void VehicleStatsCommand(Client sender)
        {
        
        }

        [Command("vfind")]
        public void VehicleFindCommand(Client sender)
        {
            sender.sendChatMessage("Please type /vget and select your vehicle. A ~y~yellow blip~w~ should appear on your minimap showing the location of the vehicle.");
        }
    }
}
