using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Library;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Teleports
{
    public class TeleportHandler : Script
    {
        public static List<Teleport> TeleportList = new List<Teleport>();

        public TeleportHandler()
        {
            API.onResourceStart += OnResourceStart;
        }

        private void OnResourceStart()
        {
            LoadTeleports();
        }

        private void LoadTeleports()
        {
            Logging.Log("[FIVERP] Loading FiveRP teleports.", ConsoleColor.Yellow);
            try
            {
                using (var context = new Database.Database())
                {
                    var query = (from t in context.Teleports
                        select t).ToList();
                    var count = 0;
                    foreach (var data in query)
                    {
                        var teleportData = new Teleport
                        {
                            Id = data.Id,
                            Name = data.Name,
                            EnterX = data.EnterX,
                            EnterY = data.EnterY,
                            EnterZ = data.EnterZ,
                            EnterH = data.EnterH,
                            ExitX = data.ExitX,
                            ExitY = data.ExitY,
                            ExitZ = data.ExitZ,
                            ExitH = data.ExitH,
                            InteriorDim = data.InteriorDim,
                            ExteriorDim = data.ExteriorDim,
                            Organization = data.Organization
                        };

                        TeleportList.Add(teleportData);

                        API.createTextLabel("~r~" + teleportData.Name + "~w~\n/enter", new Vector3(data.EnterX, data.EnterY, data.EnterZ), 64, 0.3f, false, data.ExteriorDim);
                        API.createTextLabel("~r~" + teleportData.Name + "~w~\n/exit", new Vector3(data.ExitX, data.ExitY, data.ExitZ), 64, 0.3f, false, data.InteriorDim);

                        count++;
                    }

                    Logging.Log($"[FIVERP] Loaded {count} teleports.", ConsoleColor.DarkGreen);
                }

            }
            catch (Exception ex)
            {
                Logging.LogError("Exception: " + ex);
            }
        }

        public static Teleport GetTeleportData(int id)
        {
            var teleportData = new Teleport();
            foreach (var tp in TeleportList)
            {
                if (tp.Id == id)
                {
                    teleportData = tp;
                }
            }
            return teleportData;
        }
    }
}
