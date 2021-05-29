using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;

namespace FiveRP.Gamemode.Features.Organizations
{
    public class OrganizationCommands : Script
    {
        [Command("orgchat", Alias = "f,oc", GreedyArg = true, Group = "Organization Commands")]
        public void OrganizationChatCommand(Client sender, string message)
        {
            // TODO: /orgchat needs to be possible to toggle.
            var chData = Account.GetPlayerCharacterData(sender);
            if (chData != null)
            {
                foreach (var ply in API.getAllPlayers())
                {
                    var characterData = Account.GetPlayerCharacterData(ply);
                    if (characterData != null)
                    {
                        var orgData = OrganizationHandler.GetOrganizationData(characterData.CharacterData.Organization);

                        if (orgData != null)
                        {
                            if (chData.CharacterData.Organization == characterData.CharacterData.Organization && chData.CharacterData.Organization != 0)
                            {

                                API.sendChatMessageToPlayer(ply, "~" + orgData.Colour + "~",
                                    $"(( ({sender.handle.Value}) {OrganizationHandler.GetOrganizationRankName(orgData.Id, chData.CharacterData.OrganizationRank)} {NamingFunctions.RoleplayName(sender.name)}: {message} ))");
                            }
                        }
                    }
                }
            }
        }

        [Command("orgonline", Alias = "online", GreedyArg = true, Group = "Organization Commands")]
        public void OrganizationOnlineCommand(Client sender)
        {
            var senderData = Account.GetPlayerCharacterData(sender);

            if (senderData != null)
            {
                if (senderData.CharacterData.Organization != 0)
                {
                    API.sendChatMessageToPlayer(sender, "Organization members:");
                    foreach (var ply in API.getAllPlayers())
                    {
                        var characterData = Account.GetPlayerCharacterData(ply);
                        if (characterData != null)
                        {
                            if (senderData.CharacterData.Organization == characterData.CharacterData.Organization)
                            {
                                API.sendChatMessageToPlayer(sender, $"~b~{ply.name}");
                            }
                        }
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "You aren't in a org.");
                }
            }
        }
    }
}
