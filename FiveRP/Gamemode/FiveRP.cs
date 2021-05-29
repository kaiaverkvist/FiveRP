using System;
using System.Reflection;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;

/*
    FiveRP script
    Courtesy of the FiveRP Developer Team (www.fiverp.net)
    -
    “Dream as if you’ll live forever, live as if you’ll die today.” ~ James Dean
*/

namespace FiveRP.Gamemode
{
    public class FiveRp : Script
    {
        public FiveRp()
        {
            Logging.Log("[FIVERP] Starting FiveRP..", ConsoleColor.Yellow);

            var linkTimeLocal = Extensions.GetLinkerTime(Assembly.GetExecutingAssembly());

            Logging.Log("[FIVERP] This version of the server was built: " + linkTimeLocal, ConsoleColor.DarkCyan);
            Console.Title = "[FiveRP] Built: " + linkTimeLocal;

            // Load the server.cfg file and add all the keys to the list.
            Config.LoadConfig("server.cfg");
        }
    }
}
