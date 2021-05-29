using System.IO;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Vehicles
{
    public class VehicleSpecialActions : Script
    {
        public VehicleSpecialActions()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
            API.onPlayerExitVehicle += API_onPlayerExitVehicle;
        }

        private void API_onPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            if ((VehicleHash)API.getEntityModel(vehicle) == VehicleHash.Polmav)
            {
                API.sendNativeToPlayer(player, Hash.SET_SEETHROUGH, false);
            }
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "vehicle_special_action")
            {
                if (API.isPlayerInAnyVehicle(sender))
                {
                    var vehicle = sender.vehicle;

                    switch ((VehicleHash) vehicle.model)
                    {
                        case VehicleHash.Buzzard2:
                            HeliThermal(sender);
                            break;
                        case VehicleHash.Polmav:
                            HeliThermal(sender);
                            break;
                        case VehicleHash.Police:
                            SirenLightToggle(sender);
                            break;
                        case VehicleHash.Police2:
                            SirenLightToggle(sender);
                            break;
                        case VehicleHash.Police3:
                            SirenLightToggle(sender);
                            break;
                        case VehicleHash.Police4:
                            SirenLightToggle(sender);
                            break;
                        case VehicleHash.PoliceT:
                            SirenLightToggle(sender);
                            break;
                        case VehicleHash.Policeb:
                            SirenLightToggle(sender);
                            break;
                        case VehicleHash.Sheriff:
                            SirenLightToggle(sender);
                            break;
                        case VehicleHash.Sheriff2:
                            SirenLightToggle(sender);
                            break;
                        case VehicleHash.Riot:
                            SirenLightToggle(sender);
                            break;
                        case VehicleHash.Ambulance:
                            SirenLightToggle(sender);
                            break;
                        case VehicleHash.FireTruck:
                            SirenLightToggle(sender);
                            break;
                        case VehicleHash.FBI:
                            SirenLightToggle(sender);
                            break;
                        case VehicleHash.FBI2:
                            SirenLightToggle(sender);
                            break;
                    }
                } else sender.sendChatMessage("You are not in a vehicle.");
            }
        }

        private void HeliThermal(Client sender)
        {
            if (API.hasEntityData(sender, "polmav_thermal"))
            {
                if (API.getEntityData(sender, "polmav_thermal") == true)
                {
                    API.setEntityData(sender, "polmav_thermal", false);
                    API.sendNativeToPlayer(sender, Hash.SET_SEETHROUGH, false);
                }
                else
                {
                    API.setEntityData(sender, "polmav_thermal", true);
                    API.sendNativeToPlayer(sender, Hash.SET_SEETHROUGH, true);
                }
            }
            else
            {
                API.setEntityData(sender, "polmav_thermal", true);
                API.sendNativeToPlayer(sender, Hash.SET_SEETHROUGH, true);
            }
        }

        private void SirenLightToggle(Client sender)
        {
            if (sender.vehicle.siren)
            {
                if (API.hasEntityData(sender.vehicle.handle, "police_siren_on"))
                {
                    if ((bool)API.getEntityData(sender.vehicle.handle, "police_siren_on"))
                    {
                        API.sendNativeToPlayersInRange(sender.position, 150f, 0xD8050E0EB60CF274, sender.vehicle.handle, false);
                        API.setEntityData(sender.vehicle.handle, "police_siren_on", false);
                    }
                    else
                    {
                        API.sendNativeToPlayersInRange(sender.position, 150f, 0xD8050E0EB60CF274, sender.vehicle.handle, true);
                        API.setEntityData(sender.vehicle.handle, "police_siren_on", true);
                    }
                }
                else API.setEntityData(sender.vehicle.handle, "police_siren_on", true);
            } else sender.sendChatMessage("Your siren needs to be on before you can turn the sound off or on.");
        }
    }
}