using FiveRP.Gamemode.Features.Admin;
using GTANetworkServer;

namespace FiveRP.Gamemode.Features.Money
{
    public class MoneyAdmin : Script
    {
        [Command("tptobank", Group = "Admin Commands")]
        public void TpToBankCommand(Client sender, int id)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers)) { return; }

            if (id >= Bank.Banks.Count)
            {
                API.sendChatMessageToPlayer(sender, $"~r~Error:~w~ No bank with ID {id}");
            }
            else
            {
                var bank = Bank.Banks[id];
                API.sendChatMessageToPlayer(sender, $"TPing to bank {id} : {bank.Name}");
                var pos = bank.Position;
                pos.Z += 1f;
                API.setEntityPosition(sender, pos);
            }
        }

        [Command("bankdoors", Group = "Admin Commands")]
        public void UnlockBankCommand(Client sender, bool locked, float heading)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers)) { return; }

            Bank.SetBankDoorsLockedForClient(API, sender, locked, heading);
            var playerPos = sender.position;
            API.sendChatMessageToPlayer(sender, $"Your position is {playerPos}");
            foreach (var bankDoor in Bank.BankDoors)
            {
                if (bankDoor.Position.DistanceToSquared(playerPos) < 25)
                {
                    API.sendChatMessageToPlayer(sender, $"You're near bank door {bankDoor.Hash} / {bankDoor.Hash} / {bankDoor.Position}");
                }
            }
        }

        [Command("tptodoor", Group = "Admin Commands")]
        public void TpToDoorCommand(Client sender, int id)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers)) { return; }

            if (id >= Bank.BankDoors.Count)
            {
                API.sendChatMessageToPlayer(sender, $"~r~Error:~w~ No bankDoor with ID {id}");
            }
            else
            {
                var bankDoor = Bank.BankDoors[id];
                API.sendChatMessageToPlayer(sender, $"TPing to bankDoor {id} : {bankDoor.Hash}");
                API.setEntityPosition(sender, bankDoor.Position);
            }
        }
    }
}