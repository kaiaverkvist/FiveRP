using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Library;
using GTANetworkServer;

// ReSharper disable once CheckNamespace
namespace FiveRP.Gamemode.Managers
{
    public static class Account
    {
        public static readonly List<Character> CharacterList = new List<Character>();

        /// <summary>
        /// Used to return the list item in the Character list relevant for the specified client.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Character GetPlayerCharacterData(Client player)
        {
            try
            {
                var characterData = CharacterList.FirstOrDefault(p => p.CharacterClient == player);
                return characterData;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[CHARACTER DATA RETURNED NULL]: Exception: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Adds character data to the list
        /// </summary>
        /// <param name="character"></param>
        public static void AddCharacterData(Character character)
        {
            CharacterList.Add(character);
        }

        /// <summary>
        /// Removes a Client's data from the list.
        /// !! Should only be used when disconnecting and only once.
        /// </summary>
        /// <param name="api"></param>
        /// <param name="player"></param>
        public static void RemoveCharacterData(API api, Client player)
        {
            var charData = GetPlayerCharacterData(player);

            if (charData != null)
            {
                // Removing the admin label
                if (charData.AdminLabel != null)
                {
                    if (api.doesEntityExist(charData.AdminLabel))
                    {
                        api.deleteEntity(charData.AdminLabel);
                    }
                }

                CharacterList.Remove(charData);
                Logging.Log("Cleaned up character data for " + player.name);
            }
        }

        /// <summary>
        /// Determines whether the specified character data exists in the list.
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public static bool CharacterDataExists(Character character)
        {
            if (CharacterList.Contains(character))
            {
                return true;
            }
            return false;
        }
    }
}