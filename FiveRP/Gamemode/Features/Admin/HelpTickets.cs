using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Library;
using GTANetworkServer;
using FiveRP.Gamemode.Library.FunctionLibraries;

namespace FiveRP.Gamemode.Features.Admin
{
    public class HelpTickets : Script
    {
        public static List<Ticket> TicketList = new List<Ticket>();

        public HelpTickets()
        {
            API.onPlayerDisconnected += OnPlayerDisconnected;
        }

        private void OnPlayerDisconnected(Client player, string reason)
        {
            // Remove all tickets made by the given player.
            TicketList.RemoveAll(rep => rep.Submitter == player);
        }

        [Command("helpme", Alias = "hme", Group = "Player Commands", GreedyArg = true)]
        public void HelpmeCommand(Client sender, string ticketText)
        {
            if (API.hasEntityData(sender, "last_ticket"))
            {
                DateTime lastReport = API.getEntityData(sender, "last_ticket");
                if (DateTime.Now.Subtract(lastReport).TotalMinutes < 5)
                {
                    API.sendChatMessageToPlayer(sender, "~b~You can't create another help ticket yet. Please wait 5 minutes between each ticket.");
                    return;
                }
            }

            // figure out the ID to use:
            int id;
            if (TicketList.Count < 1)
            {
                id = TicketList.Count;
            }
            else
            {
                id = TicketList.Last().Id + 1;
            }

            var ticketItem = new Ticket
            {
                Id = id,
                TicketText = ticketText,
                Submitter = sender
            };

            TicketList.Add(ticketItem);

            API.sendChatMessageToPlayer(sender, "~b~Your ticket has been submitted to the administration team.");

            API.setEntityData(sender, "last_ticket", DateTime.Now);

            var players = API.getAllPlayers();

            foreach (var player in players)
            {
                if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin, false))
                {
                    API.sendChatMessageToPlayer(player, $"[Ticket]: ~b~(ticket id: {ticketItem.Id}) | ({PlayerLibrary.IdFromClient(sender)}) {sender.name}: {ticketText}.");
                }
            }

            // Raise alert
            AlertLogging.RaiseAlert($"{sender.name} submitted ticket: {ticketText}.", "TICKET");
        }

        [Command("tickets", Group = "Admin Commands", GreedyArg = true)]
        public void TicketsCommand(Client player)
        {
            if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin))
            {
                API.sendChatMessageToPlayer(player, $"~b~Type /answerticket (/at) to answer a ticket.");
                foreach (var ticket in TicketList)
                {
                    API.sendChatMessageToPlayer(player, $"~b~[id: {ticket.Id}, by: ({PlayerLibrary.IdFromClient(ticket.Submitter)}) {ticket.Submitter.name}]: {ticket.TicketText}");
                }
            }
        }

        [Command("answerticket", Alias = "at", GreedyArg = true, Group = "Admin Commands")]
        public void AnswerTicketCommand(Client player, int ticketId, string answer = "")
        {
            if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin))
            {
                // if an item with the correct report id is found:
                var reportExists = TicketList.Any(rep => rep.Id == ticketId);

                if (reportExists)
                {
                    var ticket = TicketList.Find(rep => rep.Id == ticketId);
                    API.sendChatMessageToPlayer(ticket.Submitter, $"~b~Your ticket is being answered. Please wait to receive your answer.");
                    API.sendChatMessageToPlayer(ticket.Submitter, $"~b~Ticket: ~w~[{ticket.TicketText}] ~b~was answered by ({player.handle.Value}) {player.name}.");
                    //API.sendChatMessageToPlayer(player, $"You have accepted the ticket (id: {ticket.Id}). Submitter: {ticket.Submitter.name}");

                    if (answer != "" && ticket.Submitter != null)
                    {
                        API.sendChatMessageToPlayer(ticket.Submitter, $"~b~[Ticket]: {answer}");
                    }

                    AdminLibrary.SendAdminMessage(API, AdminLibrary.AnyAdmin, $"~b~{player.name} has answered ticket {ticket.Id}.");

                    TicketList.Remove(ticket);
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "~#FF8080~", $"That ticket does not exist.");
                }
            }
        }

        [Command("cleartickets", Group = "Admin Commands")]
        public void ClearTicketsCommand(Client player, string confirm = "")
        {
            if (AdminLibrary.CheckAuthorization(API, player, AdminLibrary.AnyAdmin))
            {
                if (confirm != "confirm")
                {
                    API.sendChatMessageToPlayer(player,
                        $"Please type ~g~/cleartickets confirm~w~ to confirm that you wish to clear the tickets.");
                    API.sendChatMessageToPlayer(player,
                        $"~r~This will delete all help tickets currently unanswered!");
                }
                else
                {
                    TicketList.Clear();

                    AdminLibrary.SendAdminMessage(API, AdminLibrary.AnyAdmin, $"~b~{player.name} has cleared all tickets.");
                }
            }
        }
    }

    public class Ticket
    {
        public int Id { get; set; }
        public string TicketText { get; set; }
        public Client Submitter { get; set; }
    }
}