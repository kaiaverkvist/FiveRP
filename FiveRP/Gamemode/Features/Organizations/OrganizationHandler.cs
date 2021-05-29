using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Library;
using GTANetworkServer;

namespace FiveRP.Gamemode.Features.Organizations
{
    public class OrganizationHandler : Script
    {
        public static List<Organization> OrganizationList = new List<Organization>();

        public OrganizationHandler()
        {
            API.onResourceStart += OnResourceStart;
        }

        private void OnResourceStart()
        {
            LoadOrganizations();
        }

        private void LoadOrganizations()
        {
            Logging.Log("[FIVERP] Loading FiveRP organizations.", ConsoleColor.Yellow);
            try
            {
                using (var context = new Database.Database())
                {
                    var query = (from t in context.Organizations
                        select t).ToList();
                    var count = 0;
                    foreach (var data in query)
                    {
                        var orgData = new Organization
                        {
                            Id = data.Id,
                            Name = data.Name,
                            ShortName = data.ShortName,
                            Colour = data.Colour,
                            Type = data.Type,
                            Flags = data.Flags,
                            SpawnX = data.SpawnX,
                            SpawnY = data.SpawnY,
                            SpawnZ = data.SpawnZ,
                            Rank1 = data.Rank1,
                            Rank2 = data.Rank2,
                            Rank3 = data.Rank3,
                            Rank4 = data.Rank4,
                            Rank5 = data.Rank5,
                            Rank6 = data.Rank6,
                            Rank7 = data.Rank7,
                            Rank8 = data.Rank8,
                            Rank9 = data.Rank9,
                            Rank10 = data.Rank10,
                            Rank11 = data.Rank11,
                            Rank12 = data.Rank12,
                            Rank13 = data.Rank13,
                            Rank14 = data.Rank14,
                            Rank15 = data.Rank15
                        };

                        OrganizationList.Add(orgData);
                        count++;
                    }
                    Logging.Log($"[FIVERP] Loaded {count} organizations.", ConsoleColor.DarkGreen);
                }

                foreach (var organization in OrganizationList)
                {
                    Logging.Log($"Organization {organization.Name} ({organization.ShortName}) loaded.");
                }

            }
            catch (Exception ex)
            {
                Logging.LogError("Exception: " + ex);
            }
        }

        public static Organization GetOrganizationData(int id)
        {
            return OrganizationList.FirstOrDefault(f => f.Id == id);
        }

        /// <summary>
        /// Return a boolean for the existance of a given string flag for the specified organization.
        /// </summary>
        /// <param name="organization">id</param>
        /// <param name="flag">flag</param>
        /// <returns>flag exists</returns>
        public static bool GetOrganizationFlag(int organization, string flag)
        {
            var hasFlag = false;
            var orgData = GetOrganizationData(organization);

            if (orgData != null)
            {
                if (orgData.Flags != null)
                {
                    var flags = orgData.Flags;

                    var splitFlag = flags.Split(',');

                    var flagList = new List<string>();

                    flagList.AddRange(splitFlag);

                    if (flagList.Contains(flag))
                    {
                        hasFlag = true;
                    }
                }
                else return false;
            }

            return hasFlag;
        }

        /// <summary>
        /// Returns the name of a organization rank.
        /// </summary>
        /// <param name="organization">id</param>
        /// <param name="rank">rank</param>
        /// <returns>organization rank name</returns>
        public static string GetOrganizationRankName(int organization, int rank)
        {
            string orgRank;

            var orgData = GetOrganizationData(organization);

            switch (rank)
            {
                case 0:
                    orgRank = "";
                    break;
                case 1:
                    orgRank = orgData.Rank1;
                    break;
                case 2:
                    orgRank = orgData.Rank2;
                    break;
                case 3:
                    orgRank = orgData.Rank3;
                    break;
                case 4:
                    orgRank = orgData.Rank4;
                    break;
                case 5:
                    orgRank = orgData.Rank5;
                    break;
                case 6:
                    orgRank = orgData.Rank6;
                    break;
                case 7:
                    orgRank = orgData.Rank7;
                    break;
                case 8:
                    orgRank = orgData.Rank8;
                    break;
                case 9:
                    orgRank = orgData.Rank9;
                    break;
                case 10:
                    orgRank = orgData.Rank10;
                    break;
                case 11:
                    orgRank = orgData.Rank11;
                    break;
                case 12:
                    orgRank = orgData.Rank12;
                    break;
                case 13:
                    orgRank = orgData.Rank13;
                    break;
                case 14:
                    orgRank = orgData.Rank14;
                    break;
                case 15:
                    orgRank = orgData.Rank15;
                    break;
                default:
                    orgRank = null;
                    break;
            }

            return orgRank;
        }
    }
}
