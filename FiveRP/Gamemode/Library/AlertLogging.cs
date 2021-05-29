using System;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Admin;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;

namespace FiveRP.Gamemode.Library
{
    public class AlertLogging : Script
    {
        public delegate void AlertEvent(string text, string category, int level);
        public static event AlertEvent OnRaiseAlert;

        public static void RaiseAlert(string alertText, string category = "default", int level = 1)
        {
            var time = DateTime.Now.ToUnixTimestamp();

            var alert = new Alert
            {
                AlertLevel = level,
                AlertCategory = category,
                AlertString = alertText,
                Timestamp = time
            };

            using (var dbCtx = new Database.Database())
            {
                // add the object to the database, and flag it as newly added
                dbCtx.Alerts.Add(alert);
                dbCtx.Entry(alert).State = System.Data.Entity.EntityState.Added;

                // save the entry into the database
                dbCtx.SaveChanges();
            }

            Logging.Log($"[FiveRP-ALERT]: '{alertText}'.", ConsoleColor.DarkRed);

            // Invoke the event
            OnRaiseAlert?.Invoke(alertText, category, level);

            if (level >= 3)
            {
                AdminLibrary.SendAdminMessage(API.shared, AdminLibrary.AnyAdmin, $"[Alert]: {alertText}");
            }
        }
    }
}