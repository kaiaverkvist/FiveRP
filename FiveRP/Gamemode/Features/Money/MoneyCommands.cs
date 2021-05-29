using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using System;

namespace FiveRP.Gamemode.Features.Money
{
    public class MoneyCommands : Script
    {
        [Command("charity", Group = "Financial Commands")]
        public void CharityCommand(Client sender, int amount)
        {
            var senderData = Account.GetPlayerCharacterData(sender);

            if (senderData.CharacterData.Money >= amount && amount > 0)
            {
                var senderPos = sender.position;
                senderData.CharacterData.Money -= amount;
                API.sendChatMessageToPlayer(sender,
                    $"~g~{DateTime.Now} {NamingFunctions.RoleplayName(sender.name)} gave ${NamingFunctions.FormatMoney(amount)} to charity.");
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "You cannot afford to pay this amount.");
            }
        }

        [Command("pay", Group = "Financial Commands")]
        public void PayCommand(Client sender, string targ, int amount)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            var senderData = Account.GetPlayerCharacterData(sender);
            var targetData = Account.GetPlayerCharacterData(target);

            if (senderData.CharacterData.Money >= amount && amount > 0)
            {
                if (sender != target)
                {
                    var senderPos = sender.position;
                    var targetPos = target.position;

                    if (DistanceLibrary.DistanceBetween(senderPos, targetPos) <= 8f)
                    {
                        senderData.CharacterData.Money -= amount;
                        targetData.CharacterData.Money += amount;

                        API.sendChatMessageToPlayer(target,
                            $"~g~{NamingFunctions.RoleplayName(sender.name)} paid you ${NamingFunctions.FormatMoney(amount)}.");
                        API.sendChatMessageToPlayer(sender,
                            $"~g~You paid ${NamingFunctions.FormatMoney(amount)} to {NamingFunctions.RoleplayName(target.name)}");
                        MoneyLibrary.AddMoneyTransfer(sender, target, MoneyTransfer.TransferType.Cash, amount);

                        // if the amount is more than 35,000$, generate an alert.
                        if (amount > 35000)
                        {
                            AlertLogging.RaiseAlert(
                                $"{sender.name} paid {NamingFunctions.FormatMoney(amount)} to {target.name}.", "PLAYERACTION", 1);
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(sender, "You are too far away from the player you are giving money to.");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "You can't pay yourself.");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "You cannot afford to pay this amount.");
            }
        }

        [Command("transfer", Group = "Financial Commands")]
        public void TransferBankCommand(Client sender, string targ, int amount)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            var inBank = API.getEntityData(sender, Bank.InBank);
            if (inBank != true)
            {
                API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You\'re not near a bank");
                return;
            }

            var senderData = Account.GetPlayerCharacterData(sender);
            var targetData = Account.GetPlayerCharacterData(target);

            if (senderData.CharacterData.Bank >= amount && amount > 0)
            {
                if (sender != target)
                {
                    senderData.CharacterData.Bank -= amount;
                    targetData.CharacterData.Bank += amount;

                    API.sendChatMessageToPlayer(target, $"~g~{NamingFunctions.RoleplayName(sender.name)} has transferred ${NamingFunctions.FormatMoney(amount)} to your bank account.");
                    API.sendChatMessageToPlayer(sender, $"~g~You have transferred ${NamingFunctions.FormatMoney(amount)} to {NamingFunctions.RoleplayName(target.name)}'s bank account.");
                    MoneyLibrary.AddMoneyTransfer(sender, target, MoneyTransfer.TransferType.Bank, amount);

                    // if the amount is more than 35,000$, generate an alert.
                    if (amount > 35000)
                    {
                        AlertLogging.RaiseAlert($"{sender.name} paid {NamingFunctions.FormatMoney(amount)} to {target.name}.", level: 2);
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "You can't transfer money to yourself.");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "You cannot afford to transfer this amount.");
            }
        }

        [Command("withdraw", Group = "Financial Commands")]
        public void WithdrawCommand(Client sender, int amount)
        {
            var inBank = API.getEntityData(sender, Bank.InBank);
            if (inBank != true)
            {
                API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You\'re not near a bank");
                return;
            }

            var senderData = Account.GetPlayerCharacterData(sender);

            if (senderData.CharacterData.Bank >= amount && amount > 0)
            {
                senderData.CharacterData.Bank -= amount;
                senderData.CharacterData.Money += amount;

                API.sendChatMessageToPlayer(sender, $"~g~You have withdrawn ${NamingFunctions.FormatMoney(amount)}.");
                MoneyLibrary.AddMoneyTransfer(sender, sender, MoneyTransfer.TransferType.Withdraw, amount);
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "You cannot afford to withdraw this amount.");
            }
        }

        [Command("deposit", Group = "Financial Commands")]
        public void DepositCommand(Client sender, int amount)
        {
            var inBank = API.getEntityData(sender, Bank.InBank);
            if (inBank != true)
            {
                API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You\'re not near a bank");
                return;
            }

            var senderData = Account.GetPlayerCharacterData(sender);

            if (senderData.CharacterData.Money >= amount && amount > 0)
            {
                senderData.CharacterData.Money -= amount;
                senderData.CharacterData.Bank += amount;

                API.sendChatMessageToPlayer(sender, $"~g~You have deposited ${NamingFunctions.FormatMoney(amount)}.");
                MoneyLibrary.AddMoneyTransfer(sender, sender, MoneyTransfer.TransferType.Deposit, amount);
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "You cannot afford to deposit this amount.");
            }
        }
    }
}