using System;
using FiveRP.Gamemode.Features.Admin;
using FiveRP.Gamemode.Features.Vehicles;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Organizations
{
    public class OrganizationAdmin : Script
    {
        [Command("setorganization", Alias = "setorg", Group = "Organization Commands")]
        public void SetOrganizationCommand(Client sender, string targ, int organization)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins))
            {
                var charData = Account.GetPlayerCharacterData(target);
                var organizationData = OrganizationHandler.GetOrganizationData(organization);

                charData.CharacterData.Organization = organization;
                API.sendChatMessageToPlayer(sender, $"You have set the organization of: {target.name} to ~h~{organizationData.Name}~h~.");
                API.sendChatMessageToPlayer(target, $"Your organization has been set to: {organizationData.Name} by ~h~{sender.name}~h~.");

                AlertLogging.RaiseAlert($"{sender.name} has set {target.name}'s organization to {organization}.", "ADMINACTION", 3);
            }
        }

        [Command("setorganizationrank", Alias = "setorgrank", Group = "Organization Commands")]
        public void SetOrganizationRankCommand(Client sender, string targ, int rank)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins))
            {
                var charData = Account.GetPlayerCharacterData(target);
                var organizationData = OrganizationHandler.GetOrganizationData(charData.CharacterData.Organization);

                charData.CharacterData.OrganizationRank = rank;

                API.sendChatMessageToPlayer(sender,
                    $"You have set {target.name}'s organization rank to: {OrganizationHandler.GetOrganizationRankName(organizationData.Id, charData.CharacterData.OrganizationRank)}.");
                API.sendChatMessageToPlayer(target,
                    $"Your rank has been set to: {OrganizationHandler.GetOrganizationRankName(organizationData.Id, charData.CharacterData.OrganizationRank)} by {sender.name}.");

                AlertLogging.RaiseAlert($"{sender.name} has set {target.name}'s organization rank to {rank}.", "ADMINACTION", 3);
            }
        }

        [Command("orgtow", Group = "Organization Commands")]
        public void OrganizationTowCommand(Client sender, int organization)
        {
            var chData = Account.GetPlayerCharacterData(sender);
            bool orgLeader = false;
            if (chData.CharacterData.Organization == organization && chData.CharacterData.OrganizationRank >= 10)
                orgLeader = true;

            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins) || orgLeader)
            {
                var vehicles = API.getAllVehicles();
                foreach (var vehicle in vehicles)
                {
                    var vehicleData = VehicleHandler.GetVehicleData(vehicle);
                    if (vehicleData.Organization == organization)
                    {
                        if (API.getVehicleOccupants(vehicle).Length > 0)
                            return;

                        API.deleteEntity(vehicle);

                        var newVehicle = API.createVehicle(API.vehicleNameToModel(vehicleData.Model), new Vector3(vehicleData.X, vehicleData.Y, vehicleData.Z), new Vector3(0, 0, vehicleData.H), vehicleData.Colour1, vehicleData.Colour2);

                        API.setVehicleEngineStatus(newVehicle, false);
                        API.setVehicleNumberPlate(newVehicle, vehicleData.Plate);
                        API.setEntityDimension(newVehicle, vehicleData.Dimension);

                        vehicleData.Vehicle = newVehicle;
                    }
                }
            }
        }

        [Command("addorganizationvehicle", Alias = "aorgveh", Group = "Organization Commands")]
        public void AddOrganizationVehicleCommand(Client sender, int organization, int primaryColor, int secondaryColor)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers))
            {
                if (!sender.vehicle.IsNull)
                {
                    var newVehicle = new Database.Tables.FiveRPVehicle
                    {
                        Organization = organization,
                        Model = Enum.GetName(typeof(VehicleHash), API.getEntityModel(API.getPlayerVehicle(sender))),
                        Colour1 = primaryColor,
                        Colour2 = secondaryColor
                    };

                    var pos = API.getEntityPosition(API.getPlayerVehicle(sender));

                    newVehicle.X = pos.X;
                    newVehicle.Y = pos.Y;
                    newVehicle.Z = pos.Z;

                    var rot = API.getEntityRotation(API.getPlayerVehicle(sender));

                    newVehicle.H = rot.Z;

                    newVehicle.Plate = NamingFunctions.RandomString(3) + NamingFunctions.RandomNumberAsString(3);

                    newVehicle.Dimension = API.getEntityDimension(sender);
                    newVehicle.Inventory = "";
                    newVehicle.Health = 1000;
                    newVehicle.EngineHealth = 100;
                    newVehicle.Fuel = 100;


                    using (var dbCtx = new Database.Database())
                    {
                        // add the object to the database, and flag it as newly added
                        dbCtx.Vehicles.Add(newVehicle);
                        dbCtx.Entry(newVehicle).State = System.Data.Entity.EntityState.Added;

                        // save the entry into the database
                        dbCtx.SaveChanges();

                        API.sendChatMessageToPlayer(sender, "Organization vehicle added successfully!");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~You're not in a vehicle!");
                }
            }
        }
    }
}
