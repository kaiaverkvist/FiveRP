using System;
using System.Data.Entity;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.BaseRoleplay;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Features.Customization;

namespace FiveRP.Gamemode.Features.Admin.AdminCommands
{
    public class AdminPlayerManagementCommands : Script
    {
        [Command("notify", Alias = "n", GreedyArg = true, AddToHelpmanager = false, Group = "Admin Commands")]
        public void NotifyCommand(Client sender, string targ, string reason)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var adminUser = Account.GetPlayerCharacterData(sender).CharacterUser.UserName;

                API.sendChatMessageToPlayer(target, "~r~=========================");
                API.sendChatMessageToPlayer(target,
                    $"~r~You have received a warning notification from ~h~{adminUser}~h~ for:~n~");
                API.sendChatMessageToPlayer(target, $"~s~{reason}~n~");
                API.sendChatMessageToPlayer(target, "If you ignore this warning, you may be disciplined.");
                API.sendChatMessageToPlayer(target, "~r~=========================");

                API.sendChatMessageToPlayer(sender,
                    $"~r~You notified {NamingFunctions.RoleplayName(target.name)} for: {reason}");

                AdminLibrary.AddDiscipline(sender, target, "notification", reason);
            }
        }

        [Command("freeze", GreedyArg = true, AddToHelpmanager = false, Group = "Admin Commands")]
        public void FreezeCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var adminUser = Account.GetPlayerCharacterData(sender).CharacterUser.UserName;

                API.sendChatMessageToPlayer(target,
                    $"~r~You have been freezed by ~h~{adminUser}~h~");
                API.sendChatMessageToPlayer(sender,
                    $"~r~You freezed {NamingFunctions.RoleplayName(target.name)}");

                API.freezePlayer(target, true);

                AdminLibrary.AddDiscipline(sender, target, "freeze", "freeze");
            }
        }

        [Command("unfreeze", GreedyArg = true, AddToHelpmanager = false, Group = "Admin Commands")]
        public void UnfreezeCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var adminUser = Account.GetPlayerCharacterData(sender).CharacterUser.UserName;

                API.sendChatMessageToPlayer(target,
                    $"~r~You have been unfreezed by ~h~{adminUser}~h~");
                API.sendChatMessageToPlayer(sender,
                    $"~r~You unfreezed {NamingFunctions.RoleplayName(target.name)}");

                API.freezePlayer(target, false);
            }
        }

        [Command("ban", GreedyArg = true, AddToHelpmanager = false, Group = "Admin Commands")]
        public void BanCommand(Client sender, string targ, string reason)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins))
            {
                var userData = Account.GetPlayerCharacterData(target);

                AdminLibrary.AddDiscipline(sender, target, "ban", reason);
                userData.CharacterData.BannedUntil = DateTime.Now.AddYears(10);
                API.kickPlayer(target, reason);
                API.sendChatMessageToAll($"~h~~r~Admin {sender.name} permanently banned {target.name} for reason: [{reason}]");

                AlertLogging.RaiseAlert($"Admin {sender.name} permanently banned {target.name} for reason: {reason}", "ADMINACTION", 3);
            }
        }

        [Command("tempban", GreedyArg = true, AddToHelpmanager = false, Group = "Admin Commands")]
        public void TempBanCommand(Client sender, string targ, int days, string reason)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var userData = Account.GetPlayerCharacterData(target);

                AdminLibrary.AddDiscipline(sender, target, "ban", reason);
                userData.CharacterData.BannedUntil = DateTime.Now.AddDays(days);
                API.kickPlayer(target, reason);
                API.sendChatMessageToAll($"~h~~r~Admin {sender.name} banned {target.name} {days} days for reason: [{reason}]");

                AlertLogging.RaiseAlert($"Admin {sender.name} banned {target.name} {days} days for reason: {reason}", "ADMINACTION", 3);
            }
        }

        [Command("unban", GreedyArg = true, AddToHelpmanager = false, Group = "Admin Commands")]
        public void UnbanCommand(Client sender, string targetCharacterName, string reason)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins))
            {
                using (var context = new Database.Database())
                {
                    var nameSplit = targetCharacterName.Split('_');
                    if (nameSplit.Length != 2)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~Error:~w~ Invalid name format (Firstname_Lastname required)");
                        return;
                    }

                    var firstName = nameSplit[0];
                    var lastName = nameSplit[1];
                    var userQuery = (from p in context.Character
                        join us in context.User on p.CharacterMember equals us.UserId
                        join ch in context.FiveRpCharacters on p.CharacterId equals ch.CharacterUcpId
                        where p.CharacterFirstname == firstName && p.CharacterLastname == lastName
                        select new { Character = p, FiveRPCharacter = ch, User = us });

                    var row = userQuery.FirstOrDefault();

                    if (row == null)
                    {
                        API.sendChatMessageToPlayer(sender,
                            $"~r~Error:~w~ Couldn't find character matching name ~h~{targetCharacterName}");
                        return;
                    }


                    var fiveRpCharacter = row.FiveRPCharacter;


                    if (fiveRpCharacter.BannedUntil == null || fiveRpCharacter.BannedUntil < DateTime.Now)

                    {
                        API.sendChatMessageToPlayer(sender, $"~r~Error:~w~ ~h~{targetCharacterName}~h~ is not banned.");
                        return;
                    }

                    fiveRpCharacter.BannedUntil = null;
                    context.FiveRpCharacters.Attach(fiveRpCharacter);

                    var entry = context.Entry(fiveRpCharacter);
                    entry.State = EntityState.Modified;
                    entry.Property(e => e.BannedUntil).IsModified = true;


                    var adminId = Account.GetPlayerCharacterData(sender).CharacterUser.UserId;
                    var unbanAction = new DisciplinaryAction(row.User.UserId, adminId, "unban", reason);

                    context.DisciplinaryActions.Add(unbanAction);
                    context.SaveChanges();

                    API.sendChatMessageToPlayer(sender, $"~g~Successfully unbanned ~h~{targetCharacterName}");
                    AlertLogging.RaiseAlert($"Admin {sender.name} has unbanned {targetCharacterName} for reason: {reason}", "ADMINACTION", 3);
                }
            }
        }

        [Command("viewdisciplinaryactions", Alias = "vda", AddToHelpmanager = false, Group = "Admin Commands")]
        public void ViewWarnsCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var charData = Account.GetPlayerCharacterData(target);

                API.sendChatMessageToPlayer(sender,
                    $"~r~Admin action history for ~h~{NamingFunctions.RoleplayName(target.name)}~h~:");
                if (charData.CharacterData.DisciplinaryActions.Count == 0)
                {
                    API.sendChatMessageToPlayer(sender, "None");
                }
                else
                {
                    using (var context = new Database.Database())
                    {
                        foreach (var discipline in charData.CharacterData.DisciplinaryActions)
                        {
                            var adminUser = (from u in context.User where u.UserId == discipline.AdminUserId select u).AsNoTracking().ToList().FirstOrDefault();
                            var adminName = (adminUser == null ? "~r~Unknown~w~" : adminUser.UserName);
                            API.sendChatMessageToPlayer(sender,
                                $"{discipline.Type}: ({discipline.Reason}) by {adminName} on {discipline.CreatedAt:yyyy/MM/dd}");
                        }
                    }
                }
            }
        }

        [Command("kick", GreedyArg = true, AddToHelpmanager = false, Group = "Admin Commands")]
        public void KickCommand(Client sender, string targ, string reason)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                if (API.hasEntityData(target, "logged"))
                {
                    if (API.getEntityData(target, "logged") == true)
                    {
                        AdminLibrary.AddDiscipline(sender, target, "kick", reason);
                        API.sendChatMessageToAll($"~r~~h~{NamingFunctions.RoleplayName(target.name)}~h~ was kicked for ~h~{reason}");
                    }
                }
                API.kickPlayer(target, reason);
                API.sendChatMessageToAll($"~r~~h~{NamingFunctions.RoleplayName(target.name)}~h~ was kicked for ~h~{reason}");
                AlertLogging.RaiseAlert($"Admin {sender.name} kicked {target.name} for reason: {reason}", "ADMINACTION", 3);
            }
        }

        [Command("kicktemp", Alias = "tempkick", GreedyArg = true, AddToHelpmanager = false, Group = "Admin Commands")]
        public void KickTempCommand(Client sender, string targ, int minutes, string reason)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                if (API.hasEntityData(target, "logged"))
                {
                    if (API.getEntityData(target, "logged") == true)
                    {
                        AdminLibrary.AddDiscipline(sender, target, "kick", reason);
                        API.sendChatMessageToAll($"~r~~h~{NamingFunctions.RoleplayName(target.name)}~h~ was kicked for ~h~{reason}");
                    }
                }
                var userData = Account.GetPlayerCharacterData(target);
                AdminLibrary.AddDiscipline(sender, target, "kickban", reason);
                userData.CharacterData.BannedUntil = DateTime.Now.AddMinutes(minutes);
                API.kickPlayer(target, reason);
                API.sendChatMessageToAll($"~r~~h~{NamingFunctions.RoleplayName(target.name)}~h~ was kicked {minutes} minutes for ~h~{reason}");
                AlertLogging.RaiseAlert($"Admin {sender.name} kicked {target.name} {minutes} minutes for reason: {reason}", "ADMINACTION", 3);
            }
        }

        [Command("skin", AddToHelpmanager = false, Group = "Admin Commands")]
        public void ChangeSkinCommand(Client sender, string targ, string model)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                var charData = Account.GetPlayerCharacterData(target);
                if (AdminLibrary.OnAdminDuty(sender))
                {
                    if (target.vehicle == null)
                    {
                        if (!Enum.IsDefined(typeof(PedHash), model))
                        {
                            API.sendChatMessageToPlayer(sender, "~r~Invalid model name " + model);
                            return;
                        }
                        PedHash pedModel = API.pedNameToModel(model);
                        API.setPlayerSkin(target, pedModel);
                        charData.CharacterData.WardRobe.EquipVariants();
                        API.sendNativeToPlayer(target, 0x45EEE61580806D63, sender);

                        // Set the skin stored in the database.
                        charData.CharacterData.Skin = model;
                        API.sendChatMessageToPlayer(sender, $"You have set the skin of {target.name} to {model}.");
                    }
                    else sender.sendChatMessage("The player is in a vehicle. Tell them to exit their vehicle.");
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                }
            }
        }

        [Command("setdimension", Alias = "setvw,setdim", AddToHelpmanager = false, Group = "Admin Commands")]
        public void SetDimensionCommand(Client sender, string targ, int dimension)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                if (AdminLibrary.OnAdminDuty(sender))
                {
                    API.setEntityDimension(target, dimension);
                    API.sendChatMessageToPlayer(sender, $"You have set the dimension of {target.name} to {dimension}.");
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty.");
                }
            }
        }

        [Command("check", Alias = "checkstats,getstats", AddToHelpmanager = false, Group = "Admin Commands")]
        public void CheckStats(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                if (AdminLibrary.OnAdminDuty(sender))
                {
                    // Stats printout:
                    PlayerCommands.GetStats(API, target, sender);
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty.");
                }
            }
        }

        [Command("sethealth", Alias = "sethp", AddToHelpmanager = false, Group = "Admin Commands")]
        public void SetHealthCommand(Client sender, string targ, int value, bool ignoreSafety = false)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                if (AdminLibrary.OnAdminDuty(sender))
                {
                    if (API.hasEntityData(target, "deathmode"))
                    {
                        if (API.getEntityData(target, "deathmode"))
                        {
                            sender.sendChatMessage("~r~Error:~w~ That player is dead. Use /revive");
                            return;
                        }
                    }

                    if (value < 0 && !ignoreSafety)
                    {
                        sender.sendChatMessage("~r~Error:~w~ Can't set negative health (valid range 0-100). Use /slay");
                        return;
                    }
                    target.health = value;
                    if (value < 0)
                    {
                        target.sendChatMessage($"{sender.name} slayed you.");
                        sender.sendChatMessage($"You slayed {target.name}.");
                        AlertLogging.RaiseAlert($"{sender.name} has slain {target.name}.", "ADMINACTION");
                    }
                    else
                    {
                        target.sendChatMessage($"{sender.name} set your health to {value}.");
                        sender.sendChatMessage($"You set {target.name}'s health to {value}.");
                        AlertLogging.RaiseAlert($"{sender.name} has set {target.name}'s health to {value}.", "ADMINACTION");
                    }
                }
                else
                {
                    sender.sendChatMessage("~r~Error:~w~ You're not an admin duty.");
                }
            }
        }

        [Command("setarmour", Alias = "setarmor", AddToHelpmanager = false, Group = "Admin Commands")]
        public void SetArmourCommand(Client sender, string targ, int value)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                if (AdminLibrary.OnAdminDuty(sender))
                {
                    if (API.hasEntityData(target, "deathmode"))
                    {
                        if (API.getEntityData(target, "deathmode"))
                        {
                            sender.sendChatMessage("~r~Error:~w~ That player is dead. Use /revive");
                            return;
                        }
                    }

                    if (value < 0)
                    {
                        sender.sendChatMessage("~r~Error:~w~ Can't set negative armour (valid range 0-100).");
                        return;
                    }
                    target.armor = value;
                    target.sendChatMessage($"{sender.name} set your armour to {value}.");
                    sender.sendChatMessage($"You set {target.name}'s armour to {value}.");
                    AlertLogging.RaiseAlert($"{sender.name} has set {target.name}'s armour to {value}.", "ADMINACTION");
                }
                else
                {
                    sender.sendChatMessage("~r~Error:~w~ You're not an admin duty.");
                }
            }
        }

        [Command("slay", Alias = "sleigh", AddToHelpmanager = false, Group = "Admin Commands")]
        public void SlayCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                SetHealthCommand(sender, targ, -1, true);
            }
            else
            {
                sender.sendChatMessage("~r~Error:~w~ You're not an admin duty.");
            }
        }
    }
}