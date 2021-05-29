using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Properties;
using FiveRP.Gamemode.Features.Teleports;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.BaseRoleplay
{
    public class SpecialCommands : Script
    {
        [Command("enter", Group = "Player Commands")]
        public void EnterCommand(Client sender)
        {
            Enter(sender);
        }

        [Command("exit", Group = "Player Commands")]
        public void ExitCommand(Client sender)
        {
            Exit(sender);
        }

        private void Enter(Client sender)
        {
            var charData = Account.GetPlayerCharacterData(sender);
            foreach (var property in PropertyHandler.PropertyList)
            {
                var pos = sender.position;
                if (sender.dimension == property.PropertyExtDimension)
                {
                    if (pos.DistanceToSquared(new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ)) < 5)
                    {
                        if (property.PropertyLocked)
                        {
                            API.sendChatMessageToPlayer(sender, "~r~The door is locked.");
                            return;
                        }

                        if ((property.PropertyType == PropertyType.ResidentialGarage || property.PropertyType == PropertyType.CommercialGarage) && (API.isPlayerInAnyVehicle(sender) && API.getPlayerVehicleSeat(sender) == -1))
                        {
                            API.setEntityDimension(sender, property.PropertyId);
                            API.setEntityPosition(sender.vehicle, Interiors.GetInteriorPosition(property.PropertyInterior));
                            charData.CharacterData.SavedDimension = property.PropertyId;
                            sender.stopAnimation();
                        }
                        else if (!API.isPlayerInAnyVehicle(sender))
                        {
                            API.setEntityDimension(sender, property.PropertyId);
                            charData.CharacterData.SavedDimension = property.PropertyId;
                            sender.stopAnimation();
                            API.setEntityPosition(sender, Interiors.GetInteriorPosition(property.PropertyInterior));
                        }

                        if (property.PropertyOwner == charData.CharacterId)
                            API.sendChatMessageToPlayer(sender, "~g~Welcome to your house.");
                        return;
                    }
                }
            }

            foreach (var tp in TeleportHandler.TeleportList)
            {
                var pos = sender.position;
                if (API.getEntityDimension(sender) == tp.ExteriorDim)
                {
                    if (pos.DistanceToSquared(new Vector3(tp.EnterX, tp.EnterY, tp.EnterZ)) < 5)
                    {
                        if (tp.Organization == charData.CharacterData.Organization)
                        {
                            // If the tp has a set faction, and that faction is the same as the player, tp them
                            if (sender.vehicle != null)
                            {
                                var allPlayers = API.getAllPlayers();
                                var vehicle = API.getPlayerVehicle(sender);

                                if (sender.vehicleSeat == -1)
                                {
                                    foreach (var player in allPlayers)
                                    {
                                        if (player.vehicle == vehicle)
                                        {
                                            if (player != sender)
                                            {
                                                API.warpPlayerOutOfVehicle(player, vehicle);
                                                API.setEntityDimension(player, tp.InteriorDim);
                                                charData.CharacterData.SavedDimension = tp.InteriorDim;
                                                return;
                                            }
                                        }
                                    }

                                    API.setEntityRotation(sender.vehicle, new Vector3(0, 0, tp.ExitH));
                                    API.setEntityPosition(sender.vehicle, new Vector3(tp.ExitX, tp.ExitY, tp.ExitZ));
                                    charData.CharacterData.SavedDimension = tp.InteriorDim;
                                    API.setEntityDimension(sender, tp.InteriorDim);
                                    return;
                                }
                            }
                            else
                            {
                                API.setEntityPosition(sender, new Vector3(tp.ExitX, tp.ExitY, tp.ExitZ));
                                API.setEntityDimension(sender, tp.InteriorDim);
                                charData.CharacterData.SavedDimension = tp.InteriorDim;
                                return;
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(sender, "You do not have access to this.");
                        }
                    }
                }
            }
        }

        private void Exit(Client sender)
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            foreach (var property in PropertyHandler.PropertyList)
            {
                var pos = sender.position;

                if (sender.dimension == property.PropertyId)
                {
                    if (pos.DistanceToSquared(Interiors.GetInteriorPosition(property.PropertyInterior)) < 5)
                    {
                        if ((property.PropertyType == PropertyType.ResidentialGarage || property.PropertyType == PropertyType.CommercialGarage) && (API.isPlayerInAnyVehicle(sender) && API.getPlayerVehicleSeat(sender) == -1))
                        {
                            API.setEntityPosition(sender.vehicle, Interiors.GetInteriorPosition(property.PropertyInterior));
                            API.setEntityDimension(sender, property.PropertyExtDimension);
                            senderData.CharacterData.SavedDimension = property.PropertyExtDimension;
                            API.setEntityPosition(sender.vehicle,
    new Vector3(property.PropertyExteriorX, property.PropertyExteriorY,
        property.PropertyExteriorZ));
                        }
                        else if (!API.isPlayerInAnyVehicle(sender))
                        {
                            API.setEntityPosition(sender, Interiors.GetInteriorPosition(property.PropertyInterior));
                            API.setEntityDimension(sender, property.PropertyExtDimension);
                            senderData.CharacterData.SavedDimension = property.PropertyExtDimension;
                            API.setEntityPosition(sender,
    new Vector3(property.PropertyExteriorX, property.PropertyExteriorY,
        property.PropertyExteriorZ));
                        }
                    }
                }
            }

            foreach (var tp in TeleportHandler.TeleportList)
            {
                var pos = sender.position;

                if (API.getEntityDimension(sender) == tp.InteriorDim)
                {
                    if (pos.DistanceToSquared(new Vector3(tp.ExitX, tp.ExitY, tp.ExitZ)) < 5)
                    {
                        if (sender.vehicle != null)
                        {
                            var allPlayers = API.getAllPlayers();
                            var vehicle = API.getPlayerVehicle(sender);

                            if (sender.vehicleSeat == -1)
                            {
                                foreach (var player in allPlayers)
                                {
                                    if (player.vehicle == vehicle)
                                    {
                                        if (player != sender)
                                        {
                                            API.warpPlayerOutOfVehicle(player, vehicle);
                                            API.setEntityDimension(player, tp.ExteriorDim);
                                        }
                                    }
                                }

                                API.setEntityRotation(vehicle, new Vector3(0, 0, tp.EnterH));
                                API.setEntityPosition(vehicle, new Vector3(tp.EnterX, tp.EnterY, tp.EnterZ));
                                API.setEntityDimension(vehicle, tp.ExteriorDim);
                                senderData.CharacterData.SavedDimension = tp.ExteriorDim;
                                API.setEntityDimension(sender, tp.ExteriorDim);
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(sender, "You're not the driver of the vehicle");
                            }
                        }
                        else
                        {
                            API.setEntityPosition(sender, new Vector3(tp.EnterX, tp.EnterY, tp.EnterZ));
                            API.setEntityDimension(sender, tp.ExteriorDim);
                            senderData.CharacterData.SavedDimension = tp.ExteriorDim;
                        }
                    }
                }
            }
        }
    }
}