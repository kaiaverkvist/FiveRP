using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;

namespace FiveRP.Gamemode.Library
{
    public abstract class FiveRPScript : Script
    {
        public Client GetClientFromId(int id)
        {
            return PlayerLibrary.GetClientFromId(API, id);
        }

        public Client GetClientFromString(string search, Client sender = null)
        {
            return PlayerLibrary.CommandClientFromString(API, sender, search);
        }
    }
}