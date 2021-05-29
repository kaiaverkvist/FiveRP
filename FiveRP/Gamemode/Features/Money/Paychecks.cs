using System;
using System.Linq;
using FiveRP.Gamemode.Features.Admin;
using FiveRP.Gamemode.Features.Vehicles;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Properties;

namespace FiveRP.Gamemode.Features.Money
{
    public class Paychecks : Script
    {
        public Paychecks()
        {
            TimeManager.OnTimeUpdate += OnTimeUpdate;
        }

        private void OnTimeUpdate(DateTime time)
        {
            foreach (var player in API.getAllPlayers())
            {
                if (API.getEntitySyncedData(player, "logged") == true)
                {
                    if (DateTime.Now.Minute == 59)
                    {
                        GivePaycheck(API, player);
                    }
                }
            }
        }

        private static void GivePaycheck(API api, Client player)
        {
            var charData = Account.GetPlayerCharacterData(player);
            if (charData != null)
            {
                int organizationBonus = 1250;
                int basepay = 250;
                int poorBonus = 150;
                int paycheckAmount = 0;
                int cashboxMoney = 0;
                foreach (Property property in charData.OwnedPropertyList)
                {
                    cashboxMoney += property.PropertyCashbox;
                }
                int expenses = 0; // (int)Math.Round((charData.CharacterData.Money + charData.CharacterData.Bank + cashboxMoney) * 0.2f / 100);

                int activityBonus = charData.CharacterData.ActivityStreak * 25;


                int vehicleCount =
                    VehicleHandler.VehicleList.Count(v => v.Owner == charData.CharacterData.CharacterUcpId);
                int vehicleUpkeep = 15 * vehicleCount;

                paycheckAmount = basepay;

                if (charData.CharacterData.Organization != 0)
                    paycheckAmount += organizationBonus;
                else
                    paycheckAmount += poorBonus;

                int rentPrice = 0;
                foreach (Property property in charData.RentedPropertyList)
                {
                    rentPrice += property.PropertyRentPrice;
                    property.PropertyCashbox += property.PropertyRentPrice;
                    PropertyHandler.SaveProperty(property);
                }

                charData.CharacterData.Bank += paycheckAmount + activityBonus - expenses - vehicleUpkeep;

                api.sendChatMessageToPlayer(player, "~g~=== Paycheck ===");
                //api.sendChatMessageToPlayer(player, $"Expenses: ~r~${NamingFunctions.FormatMoney(expenses)} ~w~(Wealth tax: 0.2%)");
                api.sendChatMessageToPlayer(player, $"Vehicle upkeep: ~r~${NamingFunctions.FormatMoney(vehicleUpkeep)} ~w~(15$/vehicle)");
                api.sendChatMessageToPlayer(player, $"Properties rent price: ~r~${NamingFunctions.FormatMoney(rentPrice)}");

                api.sendChatMessageToPlayer(player, $"Activity bonus: ~g~${NamingFunctions.FormatMoney(activityBonus)} ~w~(25$/consecutive days played)");
                api.sendChatMessageToPlayer(player, $"Base pay: ~g~${NamingFunctions.FormatMoney(basepay)}");
                if (charData.CharacterData.Organization != 0)
                {
                    api.sendChatMessageToPlayer(player,
                        $"Organization bonus: ~g~${NamingFunctions.FormatMoney(organizationBonus)}");
                }
                else
                    api.sendChatMessageToPlayer(player,
                        $"Unemployment insurance: ~g~${NamingFunctions.FormatMoney(poorBonus)}");
                api.sendChatMessageToPlayer(player, $"Total Earnings: ${NamingFunctions.FormatMoney(paycheckAmount - expenses - rentPrice - vehicleUpkeep)}");
            }
        }

        [Command("forcepaycheck", AddToHelpmanager = false, Group = "Admin Commands")]
        public void ForcePaycheck(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers))
            {
                if (AdminLibrary.OnAdminDuty(sender))
                {
                    GivePaycheck(API, target);
                    API.sendChatMessageToPlayer(sender, $"You have forced a paycheck on {target.name}.");
                    API.sendChatMessageToPlayer(target, $"You have been given a paycheck by {sender.name}.");

                    AlertLogging.RaiseAlert($"{sender.name} has forced a paycheck on {target.name}.", "ADMINACTION", 3);
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "You need to be on admin duty to do this.");
                }
            }
        }
    }
}