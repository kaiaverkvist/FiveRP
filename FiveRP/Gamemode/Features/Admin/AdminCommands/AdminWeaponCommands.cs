using System;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Admin.AdminCommands
{
    public class AdminWeaponCommands : Script
    {
        [Command("weaponlist", AddToHelpmanager = false, Group = "Admin Commands")]
        public void WeaponListCommand(Client sender)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins))
            {
                var values = Enum.GetNames(typeof(WeaponHash));
                foreach (var batch in values.Batch(8))
                {
                    API.sendChatMessageToPlayer(sender, string.Join(", ", batch));
                }
            }
        }

        [Command("giveweapon", AddToHelpmanager = false, GreedyArg = true, Group = "Admin Commands")]
        public void WeaponCommand(Client sender, string targ, WeaponHash weapon)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins))
            {
                if (AdminLibrary.OnAdminDuty(sender))
                {
                    if (Enum.IsDefined(typeof(WeaponHash), weapon))
                    {
                        API.givePlayerWeapon(target, weapon, 1, true, true);
                    }
                    else sender.sendChatMessage("Invalid weapon. ~b~Try using correct capitalization.");
                    API.sendChatMessageToPlayer(target, $"Admin gave you {Enum.GetName(typeof(WeaponHash), weapon)}");
                    AlertLogging.RaiseAlert($"{sender.name} spawned a weapon ({weapon}) for ID: {targ}", "ADMINACTION");
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                }
            }
        }

        [Command("giveammo", AddToHelpmanager = false, GreedyArg = false, Group = "Admin Commands")]
        public void AmmoCommand(Client sender, string targ, WeaponHash weapon, int count)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins))
            {
                if (AdminLibrary.OnAdminDuty(sender))
                {
                    if (Enum.IsDefined(typeof(WeaponHash), weapon))
                    {
                        API.givePlayerWeapon(target, weapon, count, true, true);
                    }
                    else sender.sendChatMessage("Invalid weapon. ~b~Try using correct capitalization and stuff.");
                    API.sendChatMessageToPlayer(target,
                        $"Admin added {count} ammo to your {Enum.GetName(typeof(WeaponHash), weapon)}");
                    AlertLogging.RaiseAlert($"{sender.name} spawned an {count} ({weapon}) for ID: {targ}", "ADMINACTION");
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                }
            }
        }
    }
}