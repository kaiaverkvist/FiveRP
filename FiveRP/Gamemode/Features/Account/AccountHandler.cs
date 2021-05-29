using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Phones;
using FiveRP.Gamemode.Features.Weapons;
using FiveRP.Gamemode.Library;
using GTANetworkServer;
using GTANetworkShared;
using Newtonsoft.Json;
using FiveRP.Gamemode.Features.Inventories;
using FiveRP.Gamemode.Features.Organizations;
using FiveRP.Gamemode.Features.Vehicles;
using FiveRP.Gamemode.Features.Properties;
using FiveRP.Gamemode.Features.Customization;

// TODO: Refactor entire class to be shorter if possible. Currently: too much fitted into one class. Should be seperated.
// ReSharper disable once CheckNamespace
namespace FiveRP.Gamemode.Managers
{
    public class AccountHandler : Script
    {
        #region Events
        public delegate void AccountEvent(Client player);
        public static event AccountEvent OnAccountLogin;
        public static event AccountEvent OnAccountLogoutPreRemoveData;
        #endregion

        const string AccountPromptLogin = "account_prompt_login";

        private static readonly List<int> ActiveUsers = new List<int>();

        public AccountHandler()
        {
            API.onPlayerConnected += OnPlayerConnected;
            API.onPlayerDisconnected += OnPlayerDisconnected;
            OnAccountLogoutPreRemoveData += AccountHandler_OnAccountLogoutPreRemoveData;
            API.onPlayerFinishedDownload += OnPlayerFinishedDownload;
            API.onClientEventTrigger += OnClientEvent;

            TimeManager.OnTimeUpdate += OnTimeUpdate;
        }

        private void AccountHandler_OnAccountLogoutPreRemoveData(Client player)
        {
            // Active user tracking
            var data = Account.GetPlayerCharacterData(player);
            if (data != null)
            {
                if (ActiveUsers.Contains(data.CharacterUser.UserId))
                {
                    ActiveUsers.Remove(data.CharacterUser.UserId);
                }

                SavePlayerData(player, true);
                Account.RemoveCharacterData(API, player);
            }
        }

        #region Player hooks
        private void OnTimeUpdate(DateTime time)
        {
            // When the function is ran, for each player, get the character data and add a minute to the stats, and save the player's data.
            // Notice, this is important in order to ensure that player data is actually saved.
            foreach (var ply in API.getAllPlayers())
            {
                var character = Account.GetPlayerCharacterData(ply);

                if (character != null)
                {
                    character.CharacterData.Minutes += 1;
                }

                // Save the player's data.
                SavePlayerData(ply, false);
            }
        }

        private void OnPlayerFinishedDownload(Client player)
        {
            API.triggerClientEvent(player, AccountPromptLogin);
        }

        private void OnPlayerDisconnected(Client player, string reason)
        {
            if (API.hasEntitySyncedData(player, "logged"))
            {
                if (API.getEntitySyncedData(player, "logged") == true)
                {
                    OnAccountLogoutPreRemoveData?.Invoke(player);
                }
            }
        }

        private void OnPlayerConnected(Client player)
        {
            API.setEntityTransparency(player, 0);
            API.setEntityPositionFrozen(player, player, true);
            API.freezePlayer(player, true);
            API.setEntityDimension(player, player.name.Length + player.ping);
            API.setEntityInvincible(player, true);
            API.setEntitySyncedData(player, "logged", false);
            API.setEntitySyncedData(player, "nethandle_id", player.handle.Value);
            API.setEntityData(player, "login_attempts", 0);

            player.sendChatMessage("Use /login [username/email] [password] to login to the server.");
        }
        #endregion

        #region Player authentication & pre-login data retrieval
        private void GetPlayerData(Client player, string email, string password)
        {
            try
            {
                var auth = Authentication.Auth(email, password, player.socialClubName);

                if (auth.Success)
                {
                    if (auth.Application == false || auth.Enabled == false)
                    {
                        API.kickPlayer(player, "Your application has not been approved, or your account is not enabled.");
                    }

                    using (var context = new Database.Database())
                    {
                        var charCount = (from p in context.Character
                                         join us in context.User on p.CharacterMember equals us.UserId
                                         where us.UserEmail == email || us.UserName == email
                                         select p).ToList();

                        // If the player doesn't have a character yet, kick them!
                        if (!charCount.Any())
                        {
                            API.kickPlayer(player, "You need to make a character via the UCP.");
                        }
                        else if (charCount.Any()) // or if they do, let them see the character selection menu
                        {
                            var characters = new Dictionary<int, string>();

                            var query = (from p in context.Character
                                         join ch in context.FiveRpCharacters on p.CharacterId equals ch.CharacterUcpId
                                         join us in context.User on p.CharacterMember equals us.UserId
                                         where p.CharacterMember == us.UserId
                                         select new
                                         {
                                             // Select a minimal amount of data since we aren't selecting a character just yet
                                             characterID = p.CharacterId,
                                             characterFirstname = p.CharacterFirstname,
                                             characterLastname = p.CharacterLastname,
                                             characterMember = p.CharacterMember,
                                             characterEnabled = p.CharacterEnabled,

                                             character_ucp_id = ch.CharacterUcpId,
                                         }).Take(charCount.Count).ToList();


                            var argumentList = new object[32];

                            foreach (var data in charCount)
                            {
                                characters.Add(data.CharacterId, data.CharacterFirstname + "_" + data.CharacterLastname);
                            }

                            // Set the first item in the list to the amount of items required in the menu.
                            argumentList[0] = characters.Count;

                            // Do this (for each character) times
                            // Loop through the data in the query and find character names, and add them to the list that gets sent to the client.
                            // This controls the character selection list
                            for (var i = 0; i < characters.Count; i++)
                            {
                                foreach (var _ in query)
                                {
                                    argumentList[i + 1] = characters.ElementAt(i).Value;
                                }
                            }
                            if (characters.Count >= 1)
                            {
                                API.triggerClientEvent(player, "account_charlist", argumentList);
                                Logging.Log("Triggering character selection screen for " + player.name);
                            }
                            else
                            {
                                API.kickPlayer(player, "~r~You need to have a character approved to play on the server.");
                                Logging.Log("Found no characters for " + player.name);
                            }
                        }
                    }
                }
                else
                {
                    // Something went wrong, do the password prompt again.
                    Logging.Log("Authentication for " + player.name + " failed with message: " + auth.Message);
                    API.triggerClientEvent(player, AccountPromptLogin);
                    API.setEntityData(player, "login_attempts", API.getEntityData(player, "login_attempts") + 1);

                    API.sendChatMessageToPlayer(player, "~r~You typed the incorrect password or username.");

                    if (API.getEntityData(player, "login_attempts") > 3)
                    {
                        API.sendChatMessageToPlayer(player, "~r~You have used all your login attempts and have been kicked as a result.~w~");
                        AlertLogging.RaiseAlert("Player " + player.name + " was kicked for exceeding 3 login attempts.", "AUTHENTICATION");
                        API.kickPlayer(player);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogError("Exception: " + ex);
            }
        }
        #endregion

        #region Saving player data
        private void SavePlayerData(Client player, bool disconnectFlag)
        {
            try
            {
                var characterData = Account.GetPlayerCharacterData(player);
                if (characterData != null)
                {
                    if (!player.IsNull && disconnectFlag == false)
                    {
                        // Weapon saving routine
                        var weapons = API.getPlayerWeapons(player);
                        var jsonWeapons =
                            JsonConvert.SerializeObject(
                                weapons.Select(
                                    weapon =>
                                        new
                                        {
                                            hash = Enum.GetName(typeof(WeaponHash), weapon),
                                            ammo =
                                                API.getPlayerWeaponAmmo(player, weapon)
                                        }));
                        characterData.CharacterData.Weapons = jsonWeapons;
                    }

                    // Save items
                    characterData.CharacterData.Inventory.SaveInventory();
                    characterData.CharacterData.WardRobe.SaveVariants();

                    using (var context = new Database.Database())
                    {
                        // Queries
                        var query = (from ch in context.FiveRpCharacters
                                     where ch.CharacterUcpId == characterData.CharacterData.CharacterUcpId
                                     select ch).ToList().First();

                        // all the character fields are set
                        query.CharacterUcpId = characterData.CharacterData.CharacterUcpId;
                        query.BannedUntil = characterData.CharacterData.BannedUntil;
                        query.Money = characterData.CharacterData.Money;
                        query.Bank = characterData.CharacterData.Bank;
                        query.Skin = characterData.CharacterData.Skin;
                        query.Variants = characterData.CharacterData.Variants;
                        query.Level = characterData.CharacterData.Level;
                        query.Minutes = characterData.CharacterData.Minutes;
                        query.Organization = characterData.CharacterData.Organization;
                        query.OrganizationRank = characterData.CharacterData.OrganizationRank;
                        query.Spawn = characterData.CharacterData.Spawn;
                        query.JailTime = characterData.CharacterData.JailTime;
                        query.RadioChannel = characterData.CharacterData.RadioChannel;
                        query.PhoneNumber = characterData.CharacterData.PhoneNumber;
                        query.SavedDimension = API.getEntityDimension(player);
                        query.Items = characterData.CharacterData.Items;
                        query.Weapons = characterData.CharacterData.Weapons;
                        query.ActivityStreak = characterData.CharacterData.ActivityStreak;
                        query.LastLogin = characterData.CharacterData.LastLogin;
                        query.UpdatedAt = DateTime.Now;

                        // if the spawn type is 1 (last saved position):
                        // save the position of the player to the coordinates
                        if (query.Spawn == 1)
                        {
                            var pos = player.position;

                            query.SavedX = pos.X;
                            query.SavedY = pos.Y;
                            query.SavedZ = pos.Z;
                        }

                        context.FiveRpCharacters.Attach(query);

                        context.Entry(query).State = EntityState.Modified;

                        context.SaveChanges();
                    }
                }
                else
                {
                    if (disconnectFlag == false)
                    {
                        Logging.LogError("Player " + player.name + " had characterdata set to null.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogError("Exception: " + ex);
                API.kickPlayer(player, "Something went wrong. Try again.");
                AlertLogging.RaiseAlert($"Something went wrong when {player.name} attempted to save their player data.");
            }
        }
        #endregion

        #region Connecting / Assigning player data
        private void OnClientEvent(Client player, string eventName, params object[] arguments)
        {
            if (eventName == "account_prompt_send")
            {
                var email = Convert.ToString(arguments[0]).Replace('|', '@');
                var password = Convert.ToString(arguments[1]);

                GetPlayerData(player, email, password);
            }

            if (eventName == "account_selected")
            {
                var index = Convert.ToInt32(arguments[0]);
                var argEmail = Convert.ToString(arguments[1]).Replace('|', '@');
                try
                {
                    using (var context = new Database.Database())
                    {
                        var userQuery = (from p in context.Character
                                         join us in context.User on p.CharacterMember equals us.UserId
                                         join ch in context.FiveRpCharacters on p.CharacterId equals ch.CharacterUcpId
                                         where us.UserEmail == argEmail || us.UserName == argEmail
                                         select new
                                         {
                                             userID = us.UserId,
                                             userName = us.UserName,
                                             userEmail = us.UserEmail,

                                             characterID = p.CharacterId,
                                             characterFirstname = p.CharacterFirstname,
                                             characterLastname = p.CharacterLastname,
                                             characterMember = p.CharacterMember,
                                             characterEnabled = p.CharacterEnabled,

                                             character_ucp_id = ch.CharacterUcpId,
                                             bannedUntil = ch.BannedUntil,
                                             money = ch.Money,
                                             bank = ch.Bank,
                                             skin = ch.Skin,
                                             variant = ch.Variants,
                                             level = ch.Level,
                                             minutes = ch.Minutes,
                                             organization = ch.Organization,
                                             organizationrank = ch.OrganizationRank,
                                             spawn = ch.Spawn,
                                             jailtime = ch.JailTime,
                                             radiochannel = ch.RadioChannel,
                                             phone_number = ch.PhoneNumber,
                                             saved_x = ch.SavedX,
                                             saved_y = ch.SavedY,
                                             saved_z = ch.SavedZ,
                                             saved_dimension = ch.SavedDimension,
                                             items = ch.Items,
                                             weapons = ch.Weapons,
                                             activity_streak = ch.ActivityStreak,
                                             last_login = ch.LastLogin,

                                             updated_at = ch.UpdatedAt
                                         }).AsNoTracking().ToList();

                        var firstOrDefault = userQuery.FirstOrDefault();

                        if (firstOrDefault != null)
                        {
                            var userId = firstOrDefault.userID;

                            if (userId != 0)
                            {
                                var userRoles = (from roles in context.Roles
                                                 join userhasrole in context.UserHasRoles on userId equals userhasrole.UserId into userhasrole
                                                 from ushro in userhasrole.DefaultIfEmpty()
                                                 join userrole in context.Roles on ushro.RoleId equals userrole.Id into userrole
                                                 from role in userrole.DefaultIfEmpty()
                                                 select new
                                                 {
                                                     role.Id,
                                                     role.Name,
                                                     role.Shortcode
                                                 });

                                var query = userQuery.ElementAt(index - 1);

                                var disciplinaryQuery = (from ds in context.DisciplinaryActions
                                                         where ds.UserId == query.userID
                                                         select ds).ToList();

                                // initialize new character data instances
                                var character = new Character
                                {
                                    CharacterId = query.characterID,
                                    CharacterUser = new User
                                    {
                                        UserId = query.userID,
                                        UserName = query.userName,
                                        UserEmail = query.userEmail
                                    },
                                    CharacterFirstname = query.characterFirstname,
                                    CharacterLastname = query.characterLastname
                                };

                                var userFines = (from fines in context.Fines
                                                 where fines.CharacterId == character.CharacterId
                                                 select fines).ToList();

                                var characterData = new FiveRpCharacter
                                {
                                    Client = player,
                                    DisciplinaryActions = disciplinaryQuery,
                                    CharacterUcpId = query.character_ucp_id,
                                    BannedUntil = query.bannedUntil,
                                    Money = query.money,
                                    Bank = query.bank,
                                    Skin = query.skin,
                                    Variants = query.variant,
                                    Level = query.level,
                                    Minutes = query.minutes,
                                    Organization = query.organization,
                                    OrganizationRank = query.organizationrank,
                                    Spawn = query.spawn,
                                    JailTime = query.jailtime,
                                    RadioChannel = query.radiochannel,
                                    PhoneNumber = query.phone_number,
                                    SavedX = query.saved_x,
                                    SavedY = query.saved_y,
                                    SavedZ = query.saved_z,
                                    SavedDimension = query.saved_dimension,
                                    Items = query.items,
                                    Weapons = query.weapons,
                                    ActivityStreak = query.activity_streak,
                                    LastLogin = query.last_login,
                                    AdminRoles = userRoles.Select(u => u.Name).Distinct().ToList(),
                                    AdminRoleShortcodes = userRoles.Select(u => u.Shortcode).Distinct().ToList(),
                                    Fines = userFines,
                                    Inventory = new PlayerInventory(),
                                    WardRobe = new Wardrobe()
                                };

                                #region Phone system
                                if (query.phone_number == 0)
                                {
                                    var random = new Random();
                                    var newPhoneNumber = 0;

                                    while ((from ch in context.FiveRpCharacters
                                            where ch.PhoneNumber == newPhoneNumber
                                            select ch.PhoneNumber).Count() != 0 || newPhoneNumber == 0)
                                    {
                                        newPhoneNumber = random.Next(10000, 9999999);
                                    }
                                    characterData.PhoneNumber = newPhoneNumber;
                                    var phoneData = new PhoneData
                                    {
                                        Number = newPhoneNumber,
                                        Player = player,
                                        Call = null
                                    };

                                    PhoneCommands.PhoneDataList.Add(phoneData);

                                    var messagesList = (from sms in context.PhoneMessages
                                                        where sms.SenderId == phoneData.Number || sms.ReceiverId == phoneData.Number
                                                        select sms).ToList();

                                    characterData.PhoneMessages = messagesList;

                                    var contactsList = (from contact in context.PhoneContacts
                                                        where contact.HostNumber == phoneData.Number
                                                        select contact).ToList();

                                    characterData.PhoneContacts = contactsList;
                                }
                                else
                                {
                                    var phoneData = new PhoneData
                                    {
                                        Number = characterData.PhoneNumber,
                                        Player = player,
                                        Call = null
                                    };

                                    PhoneCommands.PhoneDataList.Add(phoneData);

                                    var messagesList = (from sms in context.PhoneMessages
                                                        where sms.ReceiverId == character.CharacterId && sms.Deleted == false
                                                        select sms).ToList();

                                    characterData.PhoneMessages = messagesList;

                                    var contactsList = (from contact in context.PhoneContacts
                                                        where contact.HostNumber == phoneData.Number
                                                        select contact).ToList();

                                    characterData.PhoneContacts = contactsList;
                                }
                                #endregion

                                #region Inventory system
                                PlayerInventory inventory = new PlayerInventory(player, characterData);
                                characterData.Inventory = inventory;
                                #endregion

                                #region Properties init
                                List<Property> playerRentedProperties = new List<Property>();
                                List<Property> playerOwnedProperties = new List<Property>();
                                foreach (Property property in PropertyHandler.PropertyList)
                                {
                                    if (property.PropertyOwner == characterData.CharacterUcpId)
                                    {
                                        playerOwnedProperties.Add(property);
                                    }
                                    else
                                    {
                                        List<Character> tenants = new List<Character>();
                                        if (property.PropertyTenants.Length != 0)
                                        {
                                            List<int> tenantList = property.PropertyTenants.Split(',').Select(Int32.Parse).ToList();
                                            if (tenantList.Contains(characterData.CharacterUcpId))
                                                playerRentedProperties.Add(property);
                                        }
                                    }
                                }
                                character.OwnedPropertyList = playerOwnedProperties;
                                character.RentedPropertyList = playerRentedProperties;
                                #endregion
                                // character fields is then put into the instance of character data
                                character.CharacterData = characterData;

                                // set the client
                                character.CharacterClient = player;

                                if (ActiveUsers.Contains(userId))
                                {
                                    API.kickPlayer(player, "~r~This user is already connected to the server.");
                                }
                                else
                                {
                                    // Add the user to the active user pool
                                    ActiveUsers.Add(userId);
                                }

                                InitializePlayerData(player, character);
                                ActivityStreakCalculation(player);

                                Account.AddCharacterData(character);
                                Wardrobe wardRobe = new Wardrobe(player);
                                characterData.WardRobe = wardRobe;
                                // Access control
                                if (query.bannedUntil != null && (query.bannedUntil > DateTime.Now))
                                {
                                    API.kickPlayer(player, "You are banned.");
                                    return;
                                }
                                else
                                {
                                    characterData.BannedUntil = null;
                                }

                                if (query.characterEnabled == 0)
                                {
                                    API.kickPlayer(player, "Your character has not been enabled yet.");
                                }

                                character.CharacterData.LastLogin = DateTime.Now;

                                API.setEntityInvincible(player, false);

                                OnAccountLogin?.Invoke(player);

                            }
                            else API.kickPlayer(player, "An error occured while assigning your User ID.");
                        }
                        else API.kickPlayer(player, "An error occured while logging you in. Try again!");
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogError("Exception: " + ex);
                }

            }
        }
        #endregion

        #region Helper methods
        private void ActivityStreakCalculation(Client player)
        {
            var charData = Account.GetPlayerCharacterData(player);
            if (charData != null)
            {
                // Activity streaks:
                // if the player has a streak, and it's been more than 24 hours since they last logged in, give them a streak point
                // if it's been more than 32 hours, remove a streak point.
                if (DateTime.Now.Day != charData.CharacterData.LastLogin.Day)
                {
                    if (charData.CharacterData.ActivityStreak <= 10)
                    {
                        charData.CharacterData.ActivityStreak++;
                    }
                }
                else if (DateTime.Now.Subtract(charData.CharacterData.LastLogin).TotalDays > 2)
                {
                    if (charData.CharacterData.ActivityStreak >
                        DateTime.Now.Subtract(charData.CharacterData.LastLogin).TotalDays)
                    {
                        charData.CharacterData.ActivityStreak -=
                            (int)Math.Round(DateTime.Now.Subtract(charData.CharacterData.LastLogin).TotalDays);
                        Logging.Log(
                            $"Removed {Math.Round(DateTime.Now.Subtract(charData.CharacterData.LastLogin).TotalDays)} activity streak points from {charData.CharacterClient.name}");
                    }
                    else
                    {
                        charData.CharacterData.ActivityStreak = 0;
                        Logging.Log(
                            $"Removed activity streaks from {charData.CharacterClient.name}");
                    }
                }
            }
        }

        private void InitializePlayerData(Client player, Character charData)
        {
            if (charData != null)
            {
                // set the player skin
                if (charData.CharacterData.Skin != "")
                {
                    API.setPlayerSkin(player, API.pedNameToModel(charData.CharacterData.Skin));
                }
                else
                {
                    API.setPlayerSkin(player, PedHash.Michael);
                }

                // Set the initial query
                API.setEntityTransparency(player, 255);
                API.setEntityPositionFrozen(player, player, false);
                API.freezePlayer(player, false);
                API.setEntityDimension(player, charData.CharacterData.SavedDimension);


                API.setEntitySyncedData(player, "logged", true);

                var name = charData.CharacterFirstname + "_" + charData.CharacterLastname;

                API.setPlayerName(player, name.Replace(' ', '_'));

                // If the player has not set their spawn, spawn them at the airport.
                // Notice: This state will only be triggered the first time someone spawns on their character.
                if (charData.CharacterData.Spawn == 0)
                {
                    API.setEntityPosition(player, new Vector3(-1032.40, -2730.52, 13.76));
                    // Give them some starting cash.
                    charData.CharacterData.Money += 100;
                    charData.CharacterData.Bank += 300;

                    charData.CharacterData.Spawn = 1;
                }
                // If the player's spawn variable is 1, set them to spawn at their last coordinates.
                else if (charData.CharacterData.Spawn == 1)
                {
                    var pos = new Vector3(charData.CharacterData.SavedX, charData.CharacterData.SavedY,
                        charData.CharacterData.SavedZ);
                    API.setEntityPosition(player, pos);
                }
                else if (charData.CharacterData.Spawn == 2)
                {
                    if (charData.CharacterData.Organization != 0)
                    {
                        var organizationData = OrganizationHandler.GetOrganizationData(charData.CharacterData.Organization);
                        var pos = new Vector3(organizationData.SpawnX, organizationData.SpawnY, organizationData.SpawnZ);
                        API.setEntityPosition(player, pos);
                    }
                    else
                    {
                        API.setEntityPosition(player, new Vector3(-1032.40, -2730.52, 13.76)); // Spawn at airport if otherwise!
                        charData.CharacterData.Spawn = 1;
                    }
                }

                // Get the weapons and give them to the player upon loading
                if (charData.CharacterData.Weapons.Length > 0)
                {
                    var weaponHashes = JsonConvert.DeserializeObject<WeaponsJson[]>(charData.CharacterData.Weapons);
                    int LEOCount = 0;
                    foreach (var weapon in weaponHashes)
                    {
                        API.givePlayerWeapon(player, API.weaponNameToModel(weapon.WeaponHash), weapon.Ammo, false, true);
                        if (weapon.WeaponHash == "Flare" || weapon.WeaponHash == "CombatPistol" || weapon.WeaponHash == "Flashlight" || weapon.WeaponHash == "Crowbar" || weapon.WeaponHash == "StunGun")
                            LEOCount++;
                    }
                    if (LEOCount >= 2 && OrganizationHandler.GetOrganizationFlag(charData.CharacterData.Organization, "EMERGENCY"))
                        API.removeAllPlayerWeapons(player);
                }

                API.triggerClientEvent(player, "moneyUpdate", charData.CharacterData.Money);
            }
            else
            {
                API.kickPlayer(player, "~r~An error ocurred with your login, please try again.");
                Logging.Log("Something went wrong with initializing player data. (charData was null!)");
            }
        }

        #endregion

        #region Commands
        [Command("login")]
        public void LoginCommand(Client player, string user, string password)
        {
            if (API.getEntitySyncedData(player, "logged") == true)
            {
                API.sendChatMessageToPlayer(player, "~r~Error:~w~ You\'re already logged in");
                return;
            }

            API.triggerClientEvent(player, "debug_set_email", user);
            OnClientEvent(player, "account_prompt_send", user, password);
        }
        #endregion
    }
}
