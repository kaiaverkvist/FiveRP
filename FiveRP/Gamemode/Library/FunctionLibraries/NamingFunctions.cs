using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FiveRP.Gamemode.Library.FunctionLibraries
{
    public static class NamingFunctions
    {
        public static Dictionary<string, string> ZoneNameDictionary = new Dictionary<string, string>()
        {
            {"AIRP", "Los Santos International Airport"},
            {"ALAMO", "Alamo Sea"},
            {"ALTA", "Alta"},
            {"ARMYB", "Fort Zancudo"},
            {"BANHAMC", "Banham Canyon Dr"},
            {"BANNING", "Banning"},
            {"BEACH", "Vespucci Beach"},
            {"BHAMCA", "Banham Canyon"},
            {"BRADP", "Braddock Pass"},
            {"BRADT", "Braddock Tunnel"},
            {"BURTON", "Burton"},
            {"CALAFB", "Calafia Bridge"},
            {"CANNY", "Raton Canyon"},
            {"CCREAK", "Cassidy Creek"},
            {"CHAMH", "Chamberlain Hills"},
            {"CHIL", "Vinewood Hills"},
            {"CHU", "Chumash"},
            {"CMSW", "Chiliad Mountain State Wilderness"},
            {"CYPRE", "Cypress Flats"},
            {"DAVIS", "Davis"},
            {"DELBE", "Del Perro Beach"},
            {"DELPE", "Del Perro"},
            {"DELSOL", "La Puerta"},
            {"DESRT", "Grand Senora Desert"},
            {"DOWNT", "Downtown"},
            {"DTVINE", "Downtown Vinewood"},
            {"EAST_V", "East Vinewood"},
            {"EBURO", "El Burro Heights"},
            {"ELGORL", "El Gordo Lighthouse"},
            {"ELYSIAN", "Elysian Island"},
            {"GALFISH", "Galilee"},
            {"GOLF", "GWC and Golfing Society"},
            {"GRAPES", "Grapeseed"},
            {"GREATC", "Great Chaparral"},
            {"HARMO", "Harmony"},
            {"HAWICK", "Hawick"},
            {"HORS", "Vinewood Racetrack"},
            {"HUMLAB", "Humane Labs and Research"},
            {"JAIL", "Bolingbroke Penitentiary"},
            {"KOREAT", "Little Seoul"},
            {"LACT", "Land Act Reservoir"},
            {"LAGO", "Lago Zancudo"},
            {"LDAM", "Land Act Dam"},
            {"LEGSQU", "Legion Square"},
            {"LMESA", "La Mesa"},
            {"LOSPUER", "La Puerta"},
            {"MIRR", "Mirror Park"},
            {"MORN", "Morningwood"},
            {"MOVIE", "Richards Majestic"},
            {"MTCHIL", "Mount Chiliad"},
            {"MTGORDO", "Mount Gordo"},
            {"MTJOSE", "Mount Josiah"},
            {"MURRI", "Murrieta Heights"},
            {"NCHU", "North Chumash"},
            {"NOOSE", "N.O.O.S.E"},
            {"OCEANA", "Pacific Ocean"},
            {"PALCOV", "Paleto Cove"},
            {"PALETO", "Paleto Bay"},
            {"PALFOR", "Paleto Forest"},
            {"PALHIGH", "Palomino Highlands"},
            {"PALMPOW", "Palmer-Taylor Power Station"},
            {"PBLUFF", "Pacific Bluffs"},
            {"PBOX", "Pillbox Hill"},
            {"PROCOB", "Procopio Beach"},
            {"RANCHO", "Rancho"},
            {"RGLEN", "Richman Glen"},
            {"RICHM", "Richman"},
            {"ROCKF", "Rockford Hills"},
            {"RTRAK", "Redwood Lights Track"},
            {"SANAND", "San Andreas"},
            {"SANCHIA", "San Chianski Mountain Range"},
            {"SANDY", "Sandy Shores"},
            {"SKID", "Mission Row"},
            {"SLAB", "Stab City"},
            {"STAD", "Maze Bank Arena"},
            {"STRAW", "Strawberry"},
            {"TATAMO", "Tataviam Mountains"},
            {"TERMINA", "Terminal"},
            {"TEXTI", "Textile City"},
            {"TONGVAH", "Tongva Hills"},
            {"TONGVAV", "Tongva Valley"},
            {"VCANA", "Vespucci Canals"},
            {"VESP", "Vespucci"},
            {"VINE", "Vinewood"},
            {"WINDF", "Ron Alternates Wind Farm"},
            {"WVINE", "West Vinewood"},
            {"ZANCUDO", "Zancudo River"},
            {"ZP_ORT", "Port of South Los Santos"},
            {"ZQ_UAR", "Davis Quartz"}
        };

        /// <summary>
        /// Converts an underscored name to a valid "Firstname Lastname" name.
        /// </summary>
        /// <param name="name">player name</param>
        /// <returns>name without underscore</returns>
        public static string RoleplayName(string name)
        {
            var chname = name.ToCharArray();

            for (var i = 0; i < name.Length; i++)
            {
                if (chname[i] == '_') chname[i] = ' ';
            }

            var rpname = string.Join("", chname);

            return rpname;
        }

        private static readonly Random Random = new Random();

        /// <summary>
        /// Generate a random string of the specified length
        /// </summary>
        /// <param name="length">the length of the random string</param>
        /// <returns>random letters as a string</returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generates a random number string of the specified length
        /// </summary>
        /// <param name="length">length of the number string</param>
        /// <returns>random numbers as a string</returns>
        public static string RandomNumberAsString(int length)
        {
            const string chars = "1234567890";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Formats a string of digits, adding commas between thousands.
        /// </summary>
        /// <param name="money">amount</param>
        /// <returns>formatted amount</returns>
        public static string FormatMoney(int money)
        {
            var nFormatInfo = new NumberFormatInfo {NumberGroupSeparator = ","};

            var formatted = money.ToString("N0", nFormatInfo);
            return formatted;
        }
    }
}
