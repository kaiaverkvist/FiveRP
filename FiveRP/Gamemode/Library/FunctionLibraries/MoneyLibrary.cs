using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;

namespace FiveRP.Gamemode.Library.FunctionLibraries
{
    public static class MoneyLibrary
    {
        public static MoneyTransfer AddMoneyTransfer(Client fromCharacter, Client toCharacter, MoneyTransfer.TransferType type, int amount)
        {
            var fromData = Account.GetPlayerCharacterData(fromCharacter);
            var toData = Account.GetPlayerCharacterData(toCharacter);

            return AddMoneyTransfer(fromData.CharacterId, toData.CharacterId, type, amount);
        }

        public static MoneyTransfer AddMoneyTransfer(int fromCharacterId, int toCharacterId, MoneyTransfer.TransferType type, int amount)
        {
            using (var context = new Database.Database())
            {
                var moneyTransfer = new MoneyTransfer(fromCharacterId, toCharacterId, type, amount);

                context.MoneyTransfers.Add(moneyTransfer);
                context.SaveChanges();

                return moneyTransfer;
            }
        }
    }
}