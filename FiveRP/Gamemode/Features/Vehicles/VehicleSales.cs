using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Vehicles
{
    public class VehicleSales : Script
    {
        public VehicleSales()
        {
            API.onPlayerDisconnected += OnPlayerDisconnected;
            API.onVehicleDeath += OnVehicleDeath;
        }

        private void OnVehicleDeath(NetHandle entity)
        {
            // Remove sale data if the vehicle is destroyed
            if (API.hasEntityData(entity, "vehicle_sale_data"))
            {
                var saleData = (VehicleSaleData)API.getEntityData(entity, "vehicle_sale_data");

                saleData.SaleLabel.delete();

                // Set the vehicle back to normal after removing the text label
                API.setEntityData(entity, "vehicle_sale_data", null);
            }
        }

        private void OnPlayerDisconnected(Client player, string reason)
        {
            // Remove the vehicle from sale if the player is the seller.
            foreach (var vehicle in API.getAllVehicles())
            {
                if (API.hasEntityData(vehicle, "vehicle_sale_data"))
                {
                    var saleData = (VehicleSaleData)API.getEntityData(vehicle, "vehicle_sale_data");
                    if (saleData.SellerClient == player)
                    {
                        saleData.SaleLabel.delete();

                        // Set the vehicle back to normal after removing the text label
                        API.resetEntityData(vehicle, "vehicle_sale_data");
                    }
                }
            }
        }

        [Command("abortsale", Group = "Vehicle Commands")]
        public void AbortSaleCommand(Client sender)
        {
            if (sender.vehicle != null && sender.vehicleSeat == -1)
            {
                var vehicle = sender.vehicle;
                var senderData = Account.GetPlayerCharacterData(sender);

                if (senderData != null)
                {
                    var vehicleData = VehicleHandler.GetVehicleData(vehicle);

                    if (senderData.CharacterId == vehicleData.Owner)
                    {
                        if (API.hasEntityData(vehicle, "vehicle_sale_data"))
                        {
                            var saleData = (VehicleSaleData)API.getEntityData(vehicle, "vehicle_sale_data");

                            saleData.SaleLabel.delete();

                            // Set the vehicle back to normal after removing the text label
                            API.resetEntityData(vehicle, "vehicle_sale_data");

                            sender.sendChatMessage("You have aborted the sale of the vehicle.");
                        }
                        else sender.sendChatMessage("This vehicle is not for sale.");
                    }
                    else sender.sendChatMessage("You don't own this vehicle.");
                }
            }
            else
            {
                sender.sendChatMessage("You have to be in the driver's seat of a vehicle for sale that you own to abort the sale.");
            }
        }

        [Command("sellvehicle", Alias = "sellv", Group = "Vehicle Commands")]
        public void SellVehicleCommand(Client sender, int price, string player = null)
        {
            Client buyer = PlayerLibrary.CommandClientFromString(API, sender, player, false);

            if (sender.vehicle != null && sender.vehicleSeat == -1)
            {
                var vehicle = sender.vehicle;
                var senderData = Account.GetPlayerCharacterData(sender);
                var vehicleData = VehicleHandler.GetVehicleData(vehicle);

                if (!API.hasEntityData(vehicle, "vehicle_sale_data"))
                {
                    if (senderData.CharacterId == vehicleData.Owner)
                    {
                        if (price > 5)
                        {
                            // So if the vehicle is owned by the player, and (the price is right), let 
                            var saleData = new VehicleSaleData
                            {
                                Price = price,
                                Buyable = true,
                                SellerClient = sender
                            };

                            var message =
                                $"This vehicle is for sale\n~g~Price: ${NamingFunctions.FormatMoney(price)}\n~b~/buyplayervehicle {vehicleData.Id}~b~ to purchase.";

                            if (buyer != null)
                            {
                                saleData.PurchaserClient = buyer;
                                message =
                                    $"This vehicle is for sale\n~g~Price: ${NamingFunctions.FormatMoney(price)}\n~r~For sale to: {NamingFunctions.RoleplayName(buyer.name)}\n~b~/buyplayervehicle {vehicleData.Id}~b~ to purchase.";
                            }

                            API.setEntityData(vehicle, "vehicle_sale_data", saleData);

                            var saleLabel = API.createTextLabel(message, API.getEntityPosition(vehicle), 20f, 0.6f, true);
                            API.attachEntityToEntity(saleLabel, vehicle, null, new Vector3(0, 0, 0f), new Vector3());

                            // Keep track of the text label so we can delete it later
                            saleData.SaleLabel = saleLabel;

                            sender.sendChatMessage(
                                "Your vehicle is now up for sale. Type ~b~/abortsale~w~ while in the vehicle to withdraw the vehicle from the sale.");

                        }
                        else sender.sendChatMessage("The price must be higher than $5.");
                    }
                    else sender.sendChatMessage("You don't own this vehicle.");
                } else sender.sendChatMessage("This vehicle is already for sale.");
            }
            else
            {
                sender.sendChatMessage("You have to be in the driver's seat of a vehicle you own to sell it.");
            }
        }

        [Command("buyplayervehicle", Alias = "buypvehicle", Group = "Vehicle Commands")]
        public void BuyPlayerVehicleCommand(Client sender, int id, string confirm = "")
        {
            var senderData = Account.GetPlayerCharacterData(sender);
            var vehicleData = VehicleHandler.GetVehicleDataById(id);

            if (vehicleData == null)
            {
                sender.sendChatMessage("This vehicle doesn't exist.");
                return;
            }
            // TODO: LOG THIS ROUTINE!!!
            if (API.hasEntityData(vehicleData.Vehicle, "vehicle_sale_data"))
            {
                var saleData = (VehicleSaleData) API.getEntityData(vehicleData.Vehicle, "vehicle_sale_data");
                var ownerData = Account.GetPlayerCharacterData(saleData.SellerClient);

                var vehicle = vehicleData.Vehicle;
                var vehiclePos = API.getEntityPosition(vehicle);

                if (DistanceLibrary.DistanceBetween(vehiclePos, sender.position) < 8f)
                {
                    if (DistanceLibrary.DistanceBetween(vehiclePos, saleData.SellerClient.position) > 8f)
                    {
                        sender.sendChatMessage("The owner of the vehicle must be closer to the vehicle before you can buy it.");
                        return;
                    }

                    if (confirm == "confirm")
                    {
                        if (saleData.Buyable)
                        {
                            if (saleData.PurchaserClient != null)
                            {
                                if (saleData.PurchaserClient == sender)
                                {
                                    // The sender can buy the vehicle.
                                    if (senderData.CharacterData.Money >= saleData.Price)
                                    {
                                        // transfer some money
                                        senderData.CharacterData.Money -= saleData.Price;
                                        ownerData.CharacterData.Money += saleData.Price;
                                        vehicleData.Owner = senderData.CharacterId;

                                        // save the vehicle
                                        VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);

                                        // remove the text label
                                        saleData.SaleLabel.delete();

                                        saleData.SellerClient.sendChatMessage($"{NamingFunctions.RoleplayName(sender.name)} bought your vehicle for ~g~${NamingFunctions.FormatMoney(saleData.Price)}~w~.");

                                        sender.sendChatMessage($"You have bought the vehicle for ~g~${NamingFunctions.FormatMoney(saleData.Price)}~w~.");
                                    }
                                    else sender.sendChatMessage("You cannot afford this vehicle.");
                                }
                                else sender.sendChatMessage("You aren't the right buyer for this vehicle.");
                            }
                            else
                            {
                                // Anyone can buy the vehicle.
                                if (senderData.CharacterData.Money >= saleData.Price)
                                {
                                    // transfer some money
                                    senderData.CharacterData.Money -= saleData.Price;
                                    ownerData.CharacterData.Money += saleData.Price;
                                    vehicleData.Owner = senderData.CharacterId;

                                    // save the vehicle
                                    VehicleHandler.SaveVehicle(API, vehicleData.Vehicle);

                                    // remove the text label
                                    saleData.SaleLabel.delete();
                                }
                                else sender.sendChatMessage("You cannot afford this vehicle.");
                            }
                        }
                        else sender.sendChatMessage("This vehicle is not buyable right now.");
                    }
                    else sender.sendChatMessage($"Please type ~b~/buyplayervehicle {id} confirm~w~ to confirm that you really wish to buy this vehicle for ~g~${NamingFunctions.FormatMoney(saleData.Price)}~w~.");
                } else sender.sendChatMessage("You must be closer to the vehicle you are trying to buy.");
            }
        }
    }

    public class VehicleSaleData
    {
        public int Price { get; set; }
        public bool Buyable { get; set; }
        public Client PurchaserClient { get; set; }
        public Client SellerClient { get; set; }

        public TextLabel SaleLabel { get; set; }
    }
}