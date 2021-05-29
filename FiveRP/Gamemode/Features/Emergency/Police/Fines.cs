using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Organizations;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;

namespace FiveRP.Gamemode.Features.Emergency.Police
{
    public class Fines : Script
    {
        [Command("fine", Alias = "ticket", Group = "Police Commands", GreedyArg = true)]
        public void FineCommand(Client sender, string targ, int amount, string reason)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            var senderData = Account.GetPlayerCharacterData(sender);
            var targetData = Account.GetPlayerCharacterData(target);

            if (OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "LAW"))
            {
                if (amount > 0 && amount < 9000)
                {
                    if (DistanceLibrary.DistanceBetween(sender.position, target.position) < 10f)
                    {
                        // Add the fine if everything checks out
                        AddFine(targetData.CharacterId, senderData.CharacterId, amount, reason);
                        ChatLibrary.SendEmoteChatMessage(API, sender, $"has fined {target.name} for ${NamingFunctions.FormatMoney(amount)} for '{reason}'.");
                        
                        API.sendChatMessageToPlayer(target, "You have received a fine. Use /fines to see your fines.");
                        RefreshFinesFromDb(target);
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(sender, "You have to be closer to the person you are trying to fine.");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "The fine cannot be 0 and must be below $9,000.");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "You don't have the required permission to do this.");
            }
        }

        [Command("checkfines", Alias = "cfines", Group = "Police Commands", GreedyArg = true)]
        public void CheckFinesCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            var senderData = Account.GetPlayerCharacterData(sender);
            var targetData = Account.GetPlayerCharacterData(target);

            if (OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "LAW"))
            {
                if (DistanceLibrary.DistanceBetween(sender.position, target.position) < 10f)
                {
                    List<FiveRPFine> fines = targetData.CharacterData.Fines;
                    if (fines == null || fines.Count == 0)
                        API.sendChatMessageToPlayer(sender, $"{target.name} has no fines.");
                    else
                    {
                        API.sendChatMessageToPlayer(sender, $"|-----| {target.name}'s fines |-----|");
                        foreach (var fine in fines.Where(p => p.Paid == false))
                        {
                            bool overdue = DateTime.Now.Subtract(fine.Added).TotalHours > 48;

                            if (overdue)
                            {
                                API.sendChatMessageToPlayer(sender, $"~r~OVERDUE FINE | (id: {fine.Id}) ${NamingFunctions.FormatMoney(fine.Amount)} | Reason: {fine.Reason}");
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(sender, "~#FBEC5D~",
                                    $"FINE | (id: {fine.Id}) ${NamingFunctions.FormatMoney(fine.Amount)} | Reason: {fine.Reason}");
                            }
                        }
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "You have to be closer to the person you are trying to check fines.");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "You don't have the required permission to do this.");
            }
        }

        [Command("fines", Group = "Player Commands")]
        public void FinesCommand(Client sender)
        {
            var senderData = Account.GetPlayerCharacterData(sender);
            var fines = senderData.CharacterData.Fines;

            if (fines.Any())
            {
                API.sendChatMessageToPlayer(sender, "Your fines:");
                foreach (var fine in fines.Where(p => p.Paid == false))
                {
                    bool overdue = DateTime.Now.Subtract(fine.Added).TotalHours > 48;

                    if (overdue)
                    {
                        API.sendChatMessageToPlayer(sender, $"~r~OVERDUE FINE | (id: {fine.Id}) ${NamingFunctions.FormatMoney(fine.Amount)} | Reason: {fine.Reason}");
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(sender, "~#FBEC5D~",
                            $"FINE | (id: {fine.Id}) ${NamingFunctions.FormatMoney(fine.Amount)} | Reason: {fine.Reason}");
                    }
                }
                API.sendChatMessageToPlayer(sender, "~b~Use /payfine to pay your fines.");
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "~#FBEC5D~", "No unpaid fines to display.");
            }
        }

        [Command("payfine", Group = "Player Commands")]
        public void PayFineCommand(Client sender, int id)
        {
            var senderData = Account.GetPlayerCharacterData(sender);
            var allFines = senderData.CharacterData.Fines;
            if (allFines.Any())
            {
                var unpaidFineById = allFines.FirstOrDefault(p => p.Id == id);

                if (unpaidFineById != null)
                {
                    if (senderData.CharacterData.Bank >= unpaidFineById.Amount)
                    {
                        if (unpaidFineById.Paid == false)
                        {
                            API.sendChatMessageToPlayer(sender,
                                $"You have paid the fine. Deducted ~g~{NamingFunctions.FormatMoney(unpaidFineById.Amount)} ~w~from your bank account.");
                            senderData.CharacterData.Bank -= unpaidFineById.Amount;

                            senderData.CharacterData.Fines.First(p => p.Id == id).Paid = true;

                            // Set the fine as paid through the DB

                            using (var context = new Database.Database())
                            {

                                var userFines = (from fines in context.Fines
                                    where fines.Id == id
                                    select fines).FirstOrDefault();

                                if (userFines != default(FiveRPFine))
                                {
                                    userFines.Paid = true;

                                    context.Fines.Attach(userFines);
                                    context.Entry(userFines).State = System.Data.Entity.EntityState.Modified;

                                    context.SaveChanges();

                                    RefreshFinesFromDb(sender);
                                }
                                else
                                {
                                    Logging.Log("Unable to pay fine.");
                                }
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(sender, "This fine was already paid.");
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(sender,
                            "You cannot afford to pay your fine using your bank balance.");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "The given fine is not valid.");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "~#FBEC5D~", "You have no fines to pay.");
            }
        }

        private void RefreshFinesFromDb(Client player)
        {
            var character = Account.GetPlayerCharacterData(player);

            using (var context = new Database.Database())
            {
                var userFines = (from fines in context.Fines
                                 where fines.CharacterId == character.CharacterId
                                 select fines).ToList();

                SetFines(player, userFines);
            }
        }

        public void SetFines(Client player, List<FiveRPFine> fines)
        {
            var characterData = Account.GetPlayerCharacterData(player);
            
            characterData.CharacterData.Fines = fines;
        }

        public void AddFine(int characterId, int officerId, int amount, string reason)
        {
            try
            {
                var newFine = new FiveRPFine
                {
                    CharacterId = characterId,
                    OfficerId = officerId,
                    Amount = amount,
                    Reason = reason,
                    Added = DateTime.Now,
                    Paid = false
                };

                using (var dbCtx = new Database.Database())
                {
                    // add the object to the database, and flag it as newly added
                    dbCtx.Fines.Add(newFine);
                    dbCtx.Entry(newFine).State = System.Data.Entity.EntityState.Added;

                    // save the entry into the database
                    dbCtx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logging.LogError("Exception: " + ex);
            }
        }
    }
}