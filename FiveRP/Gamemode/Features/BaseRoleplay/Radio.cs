using FiveRP.Gamemode.Features.Organizations;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;

namespace FiveRP.Gamemode.Features.BaseRoleplay
{
    public class Radio : Script
    {
        [Command("radio", Alias = "r", GreedyArg = true, Group = "Radio Commands")]
        public void RadioChatCommand(Client sender, string message)
        {
            var chData = Account.GetPlayerCharacterData(sender);

            if (chData != null)
            {
                if (chData.CharacterData.RadioChannel == 0)
                {
                    API.sendChatMessageToPlayer(sender, "You can't use this channel. Use /setfrequency to set your channel.");
                    return;
                }
                foreach (var ply in API.getAllPlayers())
                {
                    var characterData = Account.GetPlayerCharacterData(ply);
                    if (characterData != null)
                    {
                        if (chData.CharacterData.RadioChannel == characterData.CharacterData.RadioChannel)
                        {
                            if (characterData.CharacterData.RadioChannel >= 1)
                            {
                                API.sendChatMessageToPlayer(ply, "~#D8D8BF~",
                                    $"**[CH: {characterData.CharacterData.RadioChannel}] {NamingFunctions.RoleplayName(sender.name)}: {message}");
                            }
                        }
                    }
                }

                ChatLibrary.SendChatMessageToPlayersInRadiusFaded(API, sender, ChatLibrary.DefaultChatRadius, $"{NamingFunctions.RoleplayName(sender.name)} (radio): {message}", sender); // last argument: ignore sender so they don't get double messages
            }
        }

        [Command("setfrequency", Alias = "setchannel", Group = "Radio Commands")]
        public void SetFrequencyCommand(Client sender, int channel)
        {

            var characterData = Account.GetPlayerCharacterData(sender);
            if (characterData != null)
            {
                if (channel >= 0 && channel <= 999999)
                {
                    if (channel >= 900 && channel <= 999)
                    {
                        if (OrganizationHandler.GetOrganizationFlag(characterData.CharacterData.Organization, "MED") || OrganizationHandler.GetOrganizationFlag(characterData.CharacterData.Organization, "LAW"))
                        {
                            characterData.CharacterData.RadioChannel = channel;
                            API.sendChatMessageToPlayer(sender, "~#D8D8BF~", $"You have set your radio channel to CH: {channel}.");
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(sender, "You do not have access to this encrypted channel range.");
                        }
                    }
                    else
                    {
                        characterData.CharacterData.RadioChannel = channel;
                        API.sendChatMessageToPlayer(sender, "~#D8D8BF~", $"You have set your radio channel to CH: {channel}.");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~#D8D8BF~", "This channel cannot be used.");
                }
            }
        }
    }
}