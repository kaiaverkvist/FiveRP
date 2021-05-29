using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Library;
using GTANetworkServer;
using FiveRP.Gamemode.Library.FunctionLibraries;

namespace FiveRP.Gamemode.Features.Admin
{
    public class Reports : Script
    {
        public static List<Report> ReportList = new List<Report>();

        public Reports()
        {
            API.onPlayerDisconnected += OnPlayerDisconnected;
        }

        private void OnPlayerDisconnected(Client player, string reason)
        {
            // Remove all reports made by the given player.
            ReportList.RemoveAll(rep => rep.Submitter == player);
        }

        [Command("report", Group = "Player Commands", GreedyArg = true)]
        public void ReportCommand(Client sender, string reportText)
        {
            if (API.hasEntityData(sender, "last_report"))
            {
                DateTime lastReport = API.getEntityData(sender, "last_report");
                if (DateTime.Now.Subtract(lastReport).TotalMinutes < 5)
                {
                    API.sendChatMessageToPlayer(sender, "You can't send another report yet. Please wait 5 minutes between reports.");
                    return;
                }
            }

            // figure out the ID to use:
            int id;
            if (ReportList.Count < 1)
            {
                id = ReportList.Count;
            }
            else
            {
                id = ReportList.Last().Id + 1;
            }

            var reportItem = new Report
            {
                Id = id,
                ReportText = reportText,
                Submitter = sender
            };

            ReportList.Add(reportItem);

            API.sendChatMessageToPlayer(sender, "~r~Your report has been submitted to the administration team.");

            API.setEntityData(sender, "last_report", DateTime.Now);

            var players = API.getAllPlayers();

            foreach (var player in players)
            {
                if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin, false))
                {
                    API.sendChatMessageToPlayer(player, "~#FF8080~", $"[AdmREPORT]: (report id: {reportItem.Id}) | ({PlayerLibrary.IdFromClient(sender)}) {sender.name} has submitted a report: {reportText}.");
                }
            }

            // Raise alert
            AlertLogging.RaiseAlert($"{sender.name} submitted report: {reportText}.", "REPORT");
        }

        [Command("reports", Alias = "viewreports,vr", Group = "Admin Commands", GreedyArg = true)]
        public void ReportsCommand(Client player)
        {
            if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin))
            {
                API.sendChatMessageToPlayer(player, "~#FF8080~", $"Type /ar [id] to handle a report or /tr [id] to trash a report.");
                foreach (var report in ReportList)
                {
                    API.sendChatMessageToPlayer(player, $"~w~[id: ~g~{report.Id}~w~, by: ~r~({PlayerLibrary.IdFromClient(report.Submitter)}) {report.Submitter.name}~w~]: {report.ReportText}");
                }
            }
        }

        [Command("acceptreport", Alias = "ar,areport", Group = "Admin Commands")]
        public void AcceptReportCommand(Client player, int reportId)
        {
            if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin))
            {
                // if an item with the correct report id is found:
                var reportExists = ReportList.Any(rep => rep.Id == reportId);

                if (reportExists)
                {
                    var report = ReportList.Find(rep => rep.Id == reportId);
                    API.sendChatMessageToPlayer(report.Submitter, $"~r~Your report is being investigated.");
                    API.sendChatMessageToPlayer(report.Submitter, $"~r~Report: ~w~[{report.ReportText}]~r~ was answered by ({player.handle.Value}) {player.name}.");
                    //API.sendChatMessageToPlayer(player, $"You have accepted the report (id: {report.Id}). Submitter: {report.Submitter.name}");
                    AdminLibrary.SendAdminMessage(API, AdminLibrary.AnyAdmin, $"{player.name} has accepted report {report.Id}.");

                    ReportList.Remove(report);
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "~#FF8080~", $"That report does not exist.");
                }
            }
        }

        [Command("trashreport", Alias = "tr,treport", Group = "Admin Commands", GreedyArg = true)]
        public void TrashReportCommand(Client player, int reportId, string trashReason)
        {
            if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin))
            {
                var reportExists = ReportList.Any(rep => rep.Id == reportId);

                if (reportExists)
                {
                    var report = ReportList.Find(rep => rep.Id == reportId);
                    API.sendChatMessageToPlayer(report.Submitter, $"~r~Your report has been trashed for the reason: {trashReason}.");
                    //API.sendChatMessageToPlayer(player, $"You have trashed report (id: {report.Id}). Submitter: {report.Submitter.name}");
                    AdminLibrary.SendAdminMessage(API, AdminLibrary.AnyAdmin, $"{player.name} has trashed report {report.Id}.");
                    ReportList.Remove(report);
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "~#FF8080~", $"That report does not exist.");
                }
            }
        }

        [Command("clearreports", Group = "Admin Commands")]
        public void ClearReportsCommand(Client player, string confirm = "")
        {
            if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin))
            {
                if (confirm != "confirm")
                {
                    API.sendChatMessageToPlayer(player,
                        $"Please type ~g~/clearreports confirm~w~ to confirm that you wish to clear the reports.");
                    API.sendChatMessageToPlayer(player,
                        $"~r~This will delete all reports currently unanswered!");
                }
                else
                {
                    ReportList.Clear();

                    AdminLibrary.SendAdminMessage(API, AdminLibrary.AnyAdmin, $"{player.name} has cleared all reports.");
                }
            }
        }
    }

    public class Report
    {
        public int Id { get; set; }
        public string ReportText { get; set; }
        public Client Submitter { get; set; }
    }
}