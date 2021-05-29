using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Weapons;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using Newtonsoft.Json;

namespace FiveRP.Gamemode.Features.Admin
{
    public class AdminLibrary
    {
        public const string Manager = "Manager";
        public const string Developer = "Developer";
        public const string GameMaster = "Game Master";
        public const string Moderator = "Moderator";
        public const string Property = "Property Staff";

        public static readonly string[] AnyAdmin = { Manager, Developer, GameMaster, Moderator };
        public static readonly string[] GameMasterAdmins = { Manager, Developer, GameMaster };
        public static readonly string[] DevManagers = { Manager, Developer };
        public static readonly string[] PropManagers = { Manager, Developer, Property};

        public static List<Client> OnDutyAdmins = new List<Client>();

        public static bool OnAdminDuty(Client player)
        {
            if (OnDutyAdmins.Contains(player))
            {
                return true;
            }
            return false;
        }

        public static bool HasARole(Client sender, string[] roles)
        {
            var senderCharData = Account.GetPlayerCharacterData(sender);

            if (senderCharData != null)
            {
                return senderCharData.CharacterData.AdminRoles.Any(roles.Contains);
            } else Logging.LogError($"HasARole received no character data for player {sender.name}");
            return false;
        }

        public static bool CheckAuthorization(API api, Client sender, string[] allowedRoles, bool sendError = true)
        {
            if (!HasARole(sender, allowedRoles))
            {
                if (sendError)
                {
                    api.sendChatMessageToPlayer(sender, "~r~Error:~w~ You don't have access to that command.");
                }
                return false;
            }
            return true;
        }

        public static DisciplinaryAction AddDiscipline(Client admin, Client target, string type, string reason)
        {
            using (var context = new Database.Database())
            {

                var adminId = Account.GetPlayerCharacterData(admin).CharacterUser.UserId;
                var userData = Account.GetPlayerCharacterData(target);
                var userId = userData.CharacterUser.UserId;

                var disciplinaryAction = new DisciplinaryAction(userId, adminId, type,  reason);
                context.DisciplinaryActions.Add(disciplinaryAction);
                context.SaveChanges();
                userData.CharacterData.DisciplinaryActions.Add(disciplinaryAction);
                return disciplinaryAction;
            }
        }

        public static void SendAdminMessage(API api, string[] adminRoles, string message)
        {
            foreach (var player in api.getAllPlayers())
            {
                if (HasARole(player, adminRoles))
                {
                    api.sendChatMessageToPlayer(player, "~#FF8080~", message);
                }
            }
        }

        public static void TeleportPlayerTo(API api, Client sender, Vector3 targetPosition)
        {
            api.setEntityPosition(sender, targetPosition);
        }

        public static string GetAdminShortRoleCodes(Client sender)
        {
            var senderData = Account.GetPlayerCharacterData(sender);
            return string.Join("/", senderData.CharacterData.AdminRoleShortcodes.ToArray());
        }

        public static void ToggleAdminDuty(API api, Client sender)
        {
            var charData = Account.GetPlayerCharacterData(sender);
            if (charData != null)
            {
                if (OnAdminDuty(sender)) // Turn GM mode off
                {
                    OnDutyAdmins.Remove(sender);
                    api.setEntityInvincible(sender, false);
                    api.setPlayerName(sender, charData.CharacterFirstname + " " + charData.CharacterLastname);
                    var message =
                        $"Admin {charData.CharacterUser.UserName} (({PlayerLibrary.IdFromClient(sender)}) {NamingFunctions.RoleplayName(sender.name)}) has gone off admin duty.";
                    SendAdminMessage(api, AnyAdmin, message);
                    if (charData.AdminLabel.IsNull != true) { api.deleteEntity(charData.AdminLabel); }

                    // Turn off ghost mode :D
                    api.setEntityTransparency(sender, 255);

                    // Remove the aduty given weapons
                    api.removeAllPlayerWeapons(sender);

                    // Give the player the original weapons back
                    if (api.hasEntityData(sender, "aduty_weapons"))
                    {
                        var adutyWeapons = api.getEntityData(sender, "aduty_weapons");

                        WeaponsJson[] weaponHashes = JsonConvert.DeserializeObject<WeaponsJson[]>(adutyWeapons);

                        if (weaponHashes.Any())
                        {
                            foreach (var weapon in weaponHashes)
                            {
                                api.givePlayerWeapon(sender, api.weaponNameToModel(weapon.WeaponHash), weapon.Ammo,
                                    false, true);
                            }
                        }
                    }
                }
                else // or on
                {
                    var label = api.createTextLabel("~r~Admin", api.getEntityPosition(sender), 50f, 0.4f, true);
                    api.attachEntityToEntity(label, sender, null, new Vector3(0, 0, 1f), new Vector3());
                    api.setEntityInvincible(sender, true);
                    OnDutyAdmins.Add(sender);
                    charData.AdminLabel = label;
                    var message =
                        $"Admin {charData.CharacterUser.UserName} (({PlayerLibrary.IdFromClient(sender)}) {NamingFunctions.RoleplayName(sender.name)}) has gone on duty as {GetAdminShortRoleCodes(sender)}";
                    SendAdminMessage(api, AnyAdmin, message);
                    api.setPlayerName(sender, charData.CharacterUser.UserName);

                    // Ghost mode
                    api.setEntityTransparency(sender, 150);

                    var weapons = sender.weapons;
                    var jsonWeapons = JsonConvert.SerializeObject(weapons.Select(weapon => new { hash = Enum.GetName(typeof(WeaponHash), weapon), ammo = api.getPlayerWeaponAmmo(sender, weapon) }));

                    api.setEntityData(sender, "aduty_weapons", jsonWeapons);

                    // finally remove the weapons
                    api.removeAllPlayerWeapons(sender);
                }
            }
            else Logging.LogError($"ToggleAdminDuty received no character data for player {sender.name}");
        }
    }
}