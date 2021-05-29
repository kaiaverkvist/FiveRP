using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Money;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;

namespace FiveRP.Gamemode.Features.Admin.AdminCommands
{
    public class AdminMoneyCommands : Script
    {

        [Command("givecash", AddToHelpmanager = true, Group = "Admin Commands")]
        public void GiveCashCommand(Client sender, string targ, int amount)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            if (AdminLibrary.CheckAuthorization(API, sender, new[] { AdminLibrary.Manager }))
            {
                var senderData = Account.GetPlayerCharacterData(sender);
                var targetData = Account.GetPlayerCharacterData(target);

                if (!AdminLibrary.OnAdminDuty(sender))
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                    return;
                }

                targetData.CharacterData.Money += amount;
                API.sendChatMessageToPlayer(sender,
                    $"~y~Added ${NamingFunctions.FormatMoney(amount)} to {NamingFunctions.RoleplayName(target.name)}'s cash balance");
                MoneyLibrary.AddMoneyTransfer(senderData.CharacterId, targetData.CharacterId,
                    MoneyTransfer.TransferType.AdminCash, amount);

                AlertLogging.RaiseAlert(
                        $"{sender.name} has added ${NamingFunctions.FormatMoney(amount)} to {target.name}'s wallet.",
                        "ADMINACTION", 3);
            }
        }

        [Command("givebank", AddToHelpmanager = true, Group = "Admin Commands")]
        public void GiveBankCommand(Client sender, string targ, int amount)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            if (AdminLibrary.CheckAuthorization(API, sender, new []{ AdminLibrary.Manager }))
            {
                var senderData = Account.GetPlayerCharacterData(sender);
                var targetData = Account.GetPlayerCharacterData(target);

                if (!AdminLibrary.OnAdminDuty(sender))
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                    return;
                }

                targetData.CharacterData.Bank += amount;
                API.sendChatMessageToPlayer(sender,
                    $"~y~Added ${NamingFunctions.FormatMoney(amount)} to {NamingFunctions.RoleplayName(target.name)}'s bank account");
                MoneyLibrary.AddMoneyTransfer(senderData.CharacterId, targetData.CharacterId,
                    MoneyTransfer.TransferType.AdminBank, amount);

                AlertLogging.RaiseAlert(
                        $"{sender.name} has added ${NamingFunctions.FormatMoney(amount)} to {target.name}'s bank account.",
                        "ADMINACTION", 3);
            }
        }
    }
}