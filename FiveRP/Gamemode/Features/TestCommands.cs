using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;

namespace FiveRP.Gamemode.Features
{
    class TestCommands : Script
    {

        public TestCommands()
        {
            if (Config.GetKeyInt("#production")!=1)
            {
                //Initialize anything you need
            }
        }

        [Command("timetest", AddToHelpmanager = false)]
        public void SyncTimeTestCommand(Client sender, int duration = 3)
        {
            var length = duration * 1000;
            TimingLibrary.scheduleSyncAction(length, () => { SendMessage(sender); });
        }

        [Command("atimetest", AddToHelpmanager = false)]
        public void AsyncTimeTestCommand(Client sender, int duration = 5)
        {
            var length = duration * 1000;
            TimingLibrary.scheduleAsyncAction(length, () => { SendMessage(sender); });
        }

        public void SendMessage(Client target)
        {
            target.sendChatMessage("~g~Successful test command.");
        }

    }
}
