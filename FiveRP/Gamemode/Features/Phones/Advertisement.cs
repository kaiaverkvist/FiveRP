using System;
using FiveRP.Gamemode.Features.Admin;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Phones
{
    public class Advertisement : Script
    {
        [Command("advert", Alias = "ad", GreedyArg = true, Group = "Player Commands")]
        public void AdvertCommand(Client sender, string text)
        {
            if (API.hasEntityData(sender, "last_ad"))
            {
                DateTime lastReport = API.getEntityData(sender, "last_ad");
                if (DateTime.Now.Subtract(lastReport).TotalMinutes < 6)
                {
                    API.sendChatMessageToPlayer(sender, "You can't send another advertisement yet. Please wait 6 minutes between advertisements.");
                    return;
                }
            }

            var senderData = Account.GetPlayerCharacterData(sender);

            if (senderData != null)
            {
                if (senderData.CharacterData.Money >= 100)
                {
                    senderData.CharacterData.Money -= 100;

                    foreach (var ply in API.getAllPlayers())
                    {
                        var data = Account.GetPlayerCharacterData(ply);
                        if (AdminLibrary.CheckAuthorization(API, ply, AdminLibrary.AnyAdmin, false))
                        {
                            API.sendChatMessageToPlayer(ply, "~#83F52C~", $"[ADMIN][Advertisement] {text} - PH: #{senderData.CharacterData.PhoneNumber} ~r~(({sender.name}))");
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(ply, "~#83F52C~", $"[Advertisement] {text} - PH: #{senderData.CharacterData.PhoneNumber}");   
                        }
                    }

                    API.sendChatMessageToPlayer(sender, "Advertisement posted.");

                    API.setEntityData(sender, "last_ad", DateTime.Now);
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "You cannot afford to post an advertisement for ~g~$100~w~.");
                }
            }
        }
    }
}