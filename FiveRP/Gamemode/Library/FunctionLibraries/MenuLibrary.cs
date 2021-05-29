using System.Collections.Generic;
using GTANetworkServer;

namespace FiveRP.Gamemode.Library.FunctionLibraries
{
    public class MenuLibrary : Script
    {
        public delegate void MenuEvent(Client sender, string menuName, int selectedIndex);
        public static event MenuEvent OnMenuSelected;

        public MenuLibrary()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "menu_handler_select_item")
            {
                var menuName = (string)arguments[0];
                var index = (int)arguments[1];

                OnMenuSelected?.Invoke(sender, menuName, index);
            }
        }

        public static void ShowNativeMenu(API api, Client player, string menuId, string banner, string subtitle, bool noExit, List<string> items)
        {
            api.triggerClientEvent(player, "menu_handler_create_menu", menuId, banner, subtitle, noExit, items);
        }
    }
}