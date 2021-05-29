using System;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Jobs
{
    public class Taxi : JobScript
    {
        public Taxi()
        {
            API.onPlayerEnterVehicle += OnPlayerEnterVehicle;
            API.onPlayerExitVehicle += OnPlayerExitVehicle;        

            Logging.Log("[FIVERP] Taxi job initialized at 'Player Rate'.", ConsoleColor.Yellow);
        }

        private bool IsMeterOn(Client player)
        {
            return (bool)API.getEntityData(player, "is_meter_on");
        }

        [Command("fare", Group = "Job Commands")]
        public void FarePrice(Client player, int price)
        {
            var vehicle = player.vehicle;

            if ((VehicleHash)API.getEntityModel(vehicle) == VehicleHash.Taxi)
            {

                if (player.vehicleSeat == -1)
                {
                    if (price >= 0 && price <= 8)
                    {
                        API.setEntityData(player, "price", price);

                        foreach (var occupant in player.vehicle.occupants)
                        {
                            occupant.sendChatMessage("~#FFFF00~", $"The fare price has been set to ${price}.");
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "The price must be between 1$ and 8$.");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "You must be the driver of the vehicle to control the taximeter.");
                }
            }
        }

        [Command("startmeter", Group = "Job Commands")]
        public void StartMeter(Client player)
        {
            NetHandle vehicle = player.vehicle;
            API.setEntityData(player, "current_to_pay", 0);
            string message = "The taxi meter has been started at a rate of $" + API.getEntityData(player, "price") + " every 10 seconds.";

            if ((VehicleHash)API.getEntityModel(vehicle) == VehicleHash.Taxi)
            {
                if (API.getPlayerVehicleSeat(player) == -1 && API.getEntityData(player, "price") != 0)
                {
                    ChatLibrary.SendChatMessageToPlayersInRadiusColored(API, player, ChatLibrary.DefaultChatRadius, "~#FFFF00~", message);
                    API.setEntityData(player, "is_meter_on", true);
                    FareAdd(player);
                }
                else if (API.getPlayerVehicleSeat(player) == -1 && API.getEntityData(player, "price") == 0)
                {
                    API.sendChatMessageToPlayer(player, "~r~Your fare charge is set to $0. The meter could not be started.");
                }
            }
        }

        [Command("stopmeter", Group = "Job Commands")]
        public void StopMeter(Client player)
        {
            NetHandle vehicle = player.vehicle;
            string message = "The taxi meter was stopped at $" + API.getEntityData(player, "current_to_pay") + ".";


            if ((VehicleHash)API.getEntityModel(vehicle) == VehicleHash.Taxi)
            {

                if (API.getPlayerVehicleSeat(player) == -1 && API.getEntityData(player, "is_meter_on") == true)
                {
                    ChatLibrary.SendChatMessageToPlayersInRadiusColored(API, player, ChatLibrary.DefaultChatRadius, "~#FFFF00~", message);
                    API.setEntityData(player, "current_to_pay", 0);
                    API.setEntityData(player, "is_meter_on", false);
                }
                else if (API.getEntityData(player, "is_meter_on") == false && API.getPlayerVehicleSeat(player) == -1)
                {
                    API.sendChatMessageToPlayer(player, "~r~The meter is already stopped. ~y~Type /startmeter to start it.");
                }
                else
                {

                }
            }
        }


        public void FareAdd(Client player)
        {
            NetHandle vehicle = player.vehicle;        

            if (API.getEntityData(player, "is_meter_on") == true)
            { 
                if (API.getEntityData(player, "is_meter_on") == false)
                {
                    return;
                }
                API.sleep(1000);
                if (API.getEntityData(player, "is_meter_on") == false)
                {
                    return;
                }
                API.sleep(1000);
                if (API.getEntityData(player, "is_meter_on") == false)
                {
                    return;
                }
                API.sleep(1000);
                if (API.getEntityData(player, "is_meter_on") == false)
                {
                    return;
                }
                API.sleep(1000);
                if (API.getEntityData(player, "is_meter_on") == false)
                {
                    return;
                }
                API.sleep(1000);
                if (API.getEntityData(player, "is_meter_on") == false)
                {
                    return;
                }
                API.sleep(1000);
                if (API.getEntityData(player, "is_meter_on") == false)
                {
                    return;
                }
                API.sleep(1000);
                if (API.getEntityData(player, "is_meter_on") == false)
                {
                    return;
                }
                API.sleep(1000);
                if (API.getEntityData(player, "is_meter_on") == false)
                {
                    return;
                }
                API.sleep(1000);
                if (API.getEntityData(player, "is_meter_on") == false)
                {
                    return;
                }
                API.sleep(1000);
                if (API.getEntityData(player, "is_meter_on") == false)
                {
                    return;
                }

                if (player.velocity.X < -1.5f || player.velocity.X > 1.5f || player.velocity.Y < -1.5f || player.velocity.Y > 1.5f || player.velocity.Z < -1.5f || player.velocity.Z > 1.5f)
                {
                    int currentToPay = API.getEntityData(player, "current_to_pay");
                    int farePrice = API.getEntityData(player, "price");
                    var totalPay = currentToPay + farePrice;
                    API.setEntityData(player, "current_to_pay", totalPay);
                    //API.triggerClientEvent(player, "update_taxi_fare", true, currentToPay,);                            
                }
                if (API.getEntityData(player, "is_meter_on") == true && vehicle != null)
                {
                    FareAdd(player);
                }
            }
            else
            {

            }
        }

        public void OnPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            if ((VehicleHash)API.getEntityModel(vehicle) == VehicleHash.Taxi)
            {
                if (API.getPlayerVehicleSeat(player) == -1)
                {              
                    API.sendChatMessageToPlayer(player, "~y~Taxi Information: Type /fare PRICE to set the price of your fares and /startmeter when a customer is in your taxi.");

                    if (API.getEntityData(player, "is_meter_on") == true)
                    {
                        FareAdd(player);
                    }
                }         
            }
        }

        public void OnPlayerExitVehicle(Client player, NetHandle vehicle)
        {

            if ((VehicleHash)API.getEntityModel(vehicle) == VehicleHash.Taxi)
            {

            }
        }

        public override bool Start(Client client)
        {
            //TODO: Start needs to be migrated
            return false;
        }

        public override void Finish(Client client, bool successful)
        {
            //TODO: Finish needs to be migrated
        }
    }
}
