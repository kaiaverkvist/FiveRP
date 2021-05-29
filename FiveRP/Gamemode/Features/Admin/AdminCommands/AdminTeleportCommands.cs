using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Admin.AdminCommands
{
    public class AdminTeleportCommands : Script
    {
        [Command("goto", Alias = "tp", AddToHelpmanager = false, Group = "Admin Commands")]
        public void GotoCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var charData = Account.GetPlayerCharacterData(sender);

                // If the player isn't on admin duty, toggle it for them
                if (!AdminLibrary.OnAdminDuty(sender))
                {
                    AdminLibrary.ToggleAdminDuty(API, sender);
                }

                // Save their current position so we can return them once they're done.
                charData.AdminTeleportPosition = sender.position;
                sender.dimension = target.dimension;
                charData.CharacterData.SavedDimension = target.dimension;
                AdminLibrary.TeleportPlayerTo(API, sender, API.getEntityPosition(target));
                API.sendChatMessageToPlayer(sender, $"~y~You teleported to {NamingFunctions.RoleplayName(target.name)}");
            }
        }

        [Command("gotocoord", Alias = "tpcoord", AddToHelpmanager = false, Group = "Admin Commands")]
        public void GotoCoordCommand(Client sender, float x, float y, float z)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                API.setEntityPosition(sender, new Vector3(x, y, z));
                API.sendChatMessageToPlayer(sender, "You have been teleported to the given position.");
            }
        }

        [Command("gethere", Alias = "tphere", AddToHelpmanager = false, Group = "Admin Commands")]
        public void GetHereCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                AdminLibrary.TeleportPlayerTo(API, target, API.getEntityPosition(sender));
                target.dimension = sender.dimension;
                var charData = Account.GetPlayerCharacterData(sender);
                if (charData != null)
                    charData.CharacterData.SavedDimension = target.dimension;
                API.sendChatMessageToPlayer(sender, $"~y~You teleported {NamingFunctions.RoleplayName(target.name)} to you");
            }
        }

        [Command("goback", AddToHelpmanager = false, Group = "Admin Commands")]
        public void GoBackCommand(Client sender)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var charData = Account.GetPlayerCharacterData(sender);
                if (charData.AdminTeleportPosition == null)
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You have no position to go back to");
                    return;
                }

                AdminLibrary.TeleportPlayerTo(API, sender, charData.AdminTeleportPosition);
                charData.AdminTeleportPosition = null;
            }
        }

        //[Command("tpall", Alias = "getallhere,getall", AddToHelpmanager = false, Group = "Admin Commands")]
        //public void TpAllCommand(Client sender)
        //{
        //    if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers))
        //    {
        //        var players = API.getAllPlayers();
        //        var senderPos = sender.position;
        //        foreach (var player in players)
        //        {
        //            API.setEntityPosition(player, senderPos);
        //        }
        //        API.sendChatMessageToAll("All players have been teleported to " + NamingFunctions.RoleplayName(sender.name));
        //    }
        //}
    }
}