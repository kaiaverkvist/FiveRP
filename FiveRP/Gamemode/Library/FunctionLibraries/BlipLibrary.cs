using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Library.FunctionLibraries
{
    public static class BlipLibrary
    {
        public static void CreateBlipForPlayer(API api, Client player, string name, Vector3 position, int color)
        {
            api.triggerClientEvent(player, "blip_create", name, position, color);
        }

        public static void RemoveBlipForPlayer(API api, Client player, string name)
        {
            api.triggerClientEvent(player, "blip_remove", name);
        }
    }
}