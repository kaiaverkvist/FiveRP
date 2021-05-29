    using GTANetworkServer;

namespace FiveRP.Gamemode.Library.FunctionLibraries
{
    class PlayerLibrary
    {

        public static Client GetClientFromId(API api, int id)
        {
            foreach (var player in api.getAllPlayers())
            {
                if (player.handle.Value == id) return player;
            }
            return null;
        }

        public static Client CommandClientFromString(API api, Client sender, string target, bool sendReturnMessage = true)
        {
            int pid;
            if (int.TryParse(target, out pid))
            {
                var t = GetClientFromId(api, pid);
                if (t==null && sendReturnMessage == true && sender != null) sender.sendChatMessage("~r~A player with that id can not be found.");
                return t;
            }
            else
            {
                foreach (var player in api.getAllPlayers())
                {
                    if (player.name.Equals(target, System.StringComparison.InvariantCultureIgnoreCase)) return player;
                }
                if (sender != null)
                    sender.sendChatMessage("~r~A player with that id can not be found.");
            }
            return null;
        }

        public static int IdFromClient(Client target)
        {
            return target.handle.Value;
        }

    }
}
