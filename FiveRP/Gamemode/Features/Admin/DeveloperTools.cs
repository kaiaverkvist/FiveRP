using GTANetworkServer;

namespace FiveRP.Gamemode.Features.Admin
{
    public class DeveloperTools : Script
    {
        /*[Command("devtune", AddToHelpmanager = true, Group = "Developer Tools")]
        public void VehicleTune(Client sender, float power = 100f, float torque = 2.0f)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers))
            {
                if(AdminLibrary.OnAdminDuty(sender))
                {
                    var vehicle = sender.vehicle;
                    if (API.doesEntityExist(sender.vehicle))
                    {
                        API.setVehicleEnginePowerMultiplier(vehicle, power);
                        API.setVehicleEngineTorqueMultiplier(vehicle, torque);
                        API.setVehicleMod(vehicle, 11, 4); // engine level 4
                        API.setVehicleMod(vehicle, 12, 3); // brake level 3
                        API.setVehicleMod(vehicle, 13, 3); // transmission level 3

                        API.sendChatMessageToPlayer(sender, "Tune applied successfully!");
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~You're not in a vehicle!");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                }
            }
        }*/

        /*[Command("restart", AddToHelpmanager = false, Group = "Admin Commands")]
        public void RestartCommand(Client sender, string resource)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers))
            {
                if (API.doesResourceExist(resource))
                {
                    API.stopResource(resource);
                    API.startResource(resource);

                    API.sendChatMessageToPlayer(sender, "~g~Restarted resource \"" + resource + "\"");
                }
            }
        }
        [Command("start", AddToHelpmanager = false, Group = "Admin Commands")]
        public void StartCommand(Client sender, string resource)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers))
            {
                if (API.doesResourceExist(resource))
                {
                    API.startResource(resource);

                    API.sendChatMessageToPlayer(sender, "~g~Started resource \"" + resource + "\"");
                }
            }
        }

        [Command("stop", AddToHelpmanager = false, Group = "Admin Commands")]
        public void StopCommand(Client sender, string resource)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers))
            {
                if (API.doesResourceExist(resource))
                {
                    API.stopResource(resource);

                    API.sendChatMessageToPlayer(sender, "~g~Stopped resource \"" + resource + "\"");
                }
            }
        }*/

        [Command("whereami", Alias = "coords,jackiechan,whatthefuckisgoingon,gps", Group = "Admin Commands", AddToHelpmanager = false)]
        public void WhereAmICommand(Client sender)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var loc = sender.position;
                var rotation = API.getEntityRotation(sender);
                sender.sendChatMessage($"~g~X:{loc.X}, Y:{loc.Y}, Z:{loc.Z}" + ", R.x:" + rotation.X + ", R.y:" + rotation.Y + ", R.z:" + rotation.Z);
            }
        }

        [Command("entdata", Group = "Admin Commands", AddToHelpmanager = false)]
        public void EntDataCommand(Client sender, Client target)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var allEntData = API.getAllEntityData(target);
                foreach (var data in allEntData)
                {
                    var key = API.getEntityData(target, data);
                    sender.sendChatMessage($"Ent data ~g~{data}~w~: {key}");
                }
            }
        }
    }
}