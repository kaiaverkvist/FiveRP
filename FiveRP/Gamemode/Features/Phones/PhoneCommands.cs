using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Organizations;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;

namespace FiveRP.Gamemode.Features.Phones
{
    public class PhoneData
    {
        public int Number { get; set; }
        public Client Player { get; set; }
        public PhoneCall Call { get; set; }
    }

    public class PhoneCall
    {
        public Client Caller { get; set; }
        public Client Receiver { get; set; }
        public bool Answered { get; set; }
    }

    public class PhoneCommands : Script
    {
        public static readonly List<PhoneData> PhoneDataList = new List<PhoneData>();

        public PhoneCommands()
        {
            AccountHandler.OnAccountLogoutPreRemoveData += AccountHandler_OnAccountLogoutPreRemoveData;
        }

        private void AccountHandler_OnAccountLogoutPreRemoveData(Client player)
        {
            var phoneData = PhoneDataList.FirstOrDefault(p => p.Player == player);

            if (phoneData != default(PhoneData))
            {
                if (PhoneDataList.Contains(phoneData))
                {
                    PhoneDataList.Remove(phoneData);
                }
            }
        }

        [Command("sms", GreedyArg = true, Group = "Phone Commands")]
        public void SmsCommand(Client player, int number, string text)
        {
            //Fetching data
            var senderData = Account.GetPlayerCharacterData(player);

            var receiverPhoneData = PhoneDataList.FirstOrDefault(p => p.Number == number);
            var senderPhoneData = PhoneDataList.FirstOrDefault(p => p.Number == senderData.CharacterData.PhoneNumber);

            if (receiverPhoneData != default(PhoneData) && senderPhoneData != default(PhoneData))
            {
                var receiverData = Account.GetPlayerCharacterData(receiverPhoneData.Player);

                if (text.Length > 250)
                {
                    API.sendChatMessageToPlayer(player, "Messages have a limit of 250 characters");
                }
                else
                {
                    //Store message on the database
                    using (var context = new Database.Database())
                    {
                        var newDbMessage = new PhoneMessage
                        {
                            Message = text,
                            ReceiverId = receiverData.CharacterId,
                            SenderId = senderData.CharacterId
                        };

                        context.PhoneMessages.Add(newDbMessage);
                        context.Entry(newDbMessage).State = System.Data.Entity.EntityState.Added;
                        context.SaveChanges();

                        player.sendChatMessage($"You have sent a message: [{text}] to the number ~g~{number}.~w~");
                        receiverData.CharacterClient.sendChatMessage($"SMS [#~g~{senderPhoneData.Number}~w~]: {text}");
                        receiverData.CharacterData.PhoneMessages.Add(newDbMessage);
                    }
                }
            }
            else player.sendChatMessage("The message could not be sent. Are you sure this is a valid number?");
        }

        [Command("contacts", Group = "Phone Commands")]
        public void ContactsCommand(Client player)
        {
            var playerModel = Account.GetPlayerCharacterData(player);
            var contacts = playerModel.CharacterData.PhoneContacts;

            if (contacts != null)
            {
                if (contacts.Any())
                {
                    API.sendChatMessageToPlayer(player, "Your contacts");
                    foreach (var contact in contacts)
                    {
                        API.sendChatMessageToPlayer(player,
                            $"~b~CONTACT~w~ | {contact.ContactName} (#{contact.ContactNumber})");
                    }
                    API.sendChatMessageToPlayer(player, "Use ~b~/addcontact~w~ to add a contact");
                    API.sendChatMessageToPlayer(player, "Use ~b~/removecontact~w~ to remove a contact");
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "You have no contacts to display.");
                    API.sendChatMessageToPlayer(player, "Use ~b~/addcontact~w~ to add a contact");
                }
            }
            else player.sendChatMessage("~r~No contacts found.");
        }

        [Command("addcontact", GreedyArg = true, Group = "Phone Commands")]
        public void AddContactCommand(Client player, int number, string name)
        {
            var playerModel = Account.GetPlayerCharacterData(player);
            var contactExists = playerModel.CharacterData.PhoneContacts.FirstOrDefault(p => p.HostNumber == number);

            if (contactExists != default(PhoneContact))
            {
                API.sendChatMessageToPlayer(player, "You already have a contact with that number!");
                return;
            }
            using (var context = new Database.Database())
            {
                var contact = new PhoneContact
                {
                    ContactName = name,
                    ContactNumber = number,
                    HostNumber = playerModel.CharacterData.PhoneNumber
                };

                context.PhoneContacts.Add(contact);

                context.Entry(contact).State = System.Data.Entity.EntityState.Added;
                context.SaveChanges();

                // Add the contact
                playerModel.CharacterData.PhoneContacts.Add(contact);
            }
            API.sendChatMessageToPlayer(player, "Contact added");
        }

        [Command("removecontact", Group = "Phone Commands")]
        public void RemoveContactCommand(Client player, int number)
        {
            var playerModel = Account.GetPlayerCharacterData(player);
            var contactExists = playerModel.CharacterData.PhoneContacts.FirstOrDefault(p => p.ContactNumber == number);

            if (!playerModel.CharacterData.PhoneContacts.Contains(contactExists) &&
                contactExists == default(PhoneContact))
            {
                API.sendChatMessageToPlayer(player, "You don't have a contact with that number!");
            }
            else
            {
                using (var context = new Database.Database())
                {
                    var contactToRemove = (from contact in context.PhoneContacts
                        where contact.HostNumber == playerModel.CharacterData.PhoneNumber
                              && contact.ContactNumber == number
                        select contact).First();
                    context.Entry(contactToRemove).State = System.Data.Entity.EntityState.Deleted;
                    context.PhoneContacts.Remove(contactToRemove);
                    context.SaveChanges();
                }
                playerModel.CharacterData.PhoneContacts.Remove(contactExists);
                API.sendChatMessageToPlayer(player, "Contact removed");
            }
        }

        [Command("messages", Group = "Phone Commands")]
        public void MessagesCommand(Client player)
        {
            var receiverModel = Account.GetPlayerCharacterData(player);
            API.sendChatMessageToPlayer(player, "~b~Your conversations:");
            var contacts = receiverModel.CharacterData.PhoneContacts;
            foreach (var message in receiverModel.CharacterData.PhoneMessages)
            {
                // Ensure the message hasn't been deleted
                if (!message.Deleted)
                {
                    var senderData = Account.CharacterList.FirstOrDefault(p => p.CharacterId == message.SenderId);
                    if (senderData != null && Account.CharacterDataExists(senderData))
                    {
                        // Used to store the contact name if the number is recognized

                        var matchingContact =
                            contacts.FirstOrDefault(p => p.ContactNumber == senderData.CharacterData.PhoneNumber);
                        var messageIdentifier = matchingContact != null
                            ? $"({matchingContact.ContactName}/#{matchingContact.ContactNumber})"
                            : $"(#{senderData.CharacterData.PhoneNumber})";
                        API.sendChatMessageToPlayer(player,
                            $"~b~MESSAGE~w~ | (id: {message.Id}) {messageIdentifier} - {message.Message}");
                    }
                }
            }
            API.sendChatMessageToPlayer(player, "Use ~b~/deletemessage~w~ to delete a message.");
        }

        [Command("deletemessage", Group = "Phone Commands")]
        public void DeleteMessageCommand(Client player, int messageId)
        {
            var playerModel = Account.GetPlayerCharacterData(player);
            var messageExists = playerModel.CharacterData.PhoneMessages.FirstOrDefault(p => p.Id == messageId);

            if (!playerModel.CharacterData.PhoneMessages.Contains(messageExists) &&
                messageExists == default(PhoneMessage))
            {
                API.sendChatMessageToPlayer(player, "You don't have a message with that id.");
            }
            else
            {
                using (var context = new Database.Database())
                {
                    var messageToRemove = (from message in context.PhoneMessages
                        where message.ReceiverId == playerModel.CharacterId
                              && message.Id == messageId
                        select message).First();

                    messageToRemove.Deleted = true;

                    context.PhoneMessages.Attach(messageToRemove);

                    //save modified entity using new Context
                    using (var dbCtx = new Database.Database())
                    {
                        //3. Mark entity as modified
                        dbCtx.Entry(messageToRemove).State = System.Data.Entity.EntityState.Modified;

                        dbCtx.SaveChanges();
                    }
                }
                playerModel.CharacterData.PhoneMessages.Remove(messageExists);
                API.sendChatMessageToPlayer(player, "Message deleted.");
            }
        }

        [Command("911", GreedyArg = true, Group = "Phone Commands")]
        public void NineOneOneCommand(Client sender, string message)
        {

            var senderData = Account.GetPlayerCharacterData(sender);

            if (senderData != null)
            {
                foreach (var ply in API.getAllPlayers())
                {
                    var characterData = Account.GetPlayerCharacterData(ply);
                    if (characterData != null)
                    {
                        if (OrganizationHandler.GetOrganizationFlag(characterData.CharacterData.Organization, "EMERGENCY"))
                        {
                            API.sendChatMessageToPlayer(ply, "~#01FCFF~",
                                $"[911 MESSAGE]: (From number #{senderData.CharacterData.PhoneNumber}) {NamingFunctions.RoleplayName(sender.name)}: {message} **");
                        }
                    }
                }
                API.sendChatMessageToPlayer(sender, $"~b~You have sent the 911 message.");
            }
        }

        [Command("call", Alias = "c", GreedyArg = false, Group = "Phone Commands")]
        public void CallCommand(Client player, int number)
        {
            // Fetch data
            var senderData = Account.GetPlayerCharacterData(player);

            if (senderData != null)
            {
                var receiverPhoneData = PhoneDataList.FirstOrDefault(p => p.Number == number);
                var senderPhoneData = PhoneDataList.FirstOrDefault(p => p.Number == senderData.CharacterData.PhoneNumber);

                if (receiverPhoneData != default(PhoneData) && senderPhoneData != default(PhoneData))
                {
                    var receiverData = Account.GetPlayerCharacterData(receiverPhoneData.Player);

                    if (receiverData != null)
                    {
                        if (receiverPhoneData.Call != null || senderPhoneData.Call != null)
                        {
                            player.sendChatMessage("You or the person you are trying to call is already in a call.");
                            return;
                        }

                        // Create a new call object
                        var call = new PhoneCall
                        {
                            Caller = player,
                            Receiver = receiverPhoneData.Player,
                            Answered = false
                        };

                        receiverPhoneData.Call = call;
                        senderPhoneData.Call = call;
                        // Inform the reciever that he has a call to answer
                        API.sendChatMessageToPlayer(senderPhoneData.Call.Receiver,
                            "[PHONE] Your phone is calling. ~g~/pickup~w~ or ~o~/hangup~w~");
                        API.sendChatMessageToPlayer(senderPhoneData.Call.Caller, "~c~Calling...");
                    }

                }
                else player.sendChatMessage("The call did not go through. Is this a valid number?");
            }
            else player.sendChatMessage("~r~Critical error occured when trying to call. (Chardata set to null)");
        }

        [Command("hangup", Alias = "h", Group = "Phone Commands")]
        public void HangupCommand(Client player)
        {
            //Fetching data
            var senderData = Account.GetPlayerCharacterData(player);

            if (senderData != null)
            {
                var senderPhoneData = PhoneDataList.FirstOrDefault(p => p.Number == senderData.CharacterData.PhoneNumber);

                if (senderPhoneData != null)
                {
                    // Check conditions
                    if (senderPhoneData.Call == null)
                    {
                        API.sendChatMessageToPlayer(player, "You are not in a call.");
                        return;
                    }

                    var receiverPhoneData = PhoneDataList.FirstOrDefault(p => senderPhoneData.Call.Receiver == p.Player);

                    // If everything is alright, hang up the call.
                    if (receiverPhoneData != null)
                    {
                        API.sendChatMessageToPlayer(senderPhoneData.Call.Receiver, "Your call has been hung up.");
                        API.sendChatMessageToPlayer(senderPhoneData.Call.Caller, "Your call has been hung up.");

                        senderPhoneData.Call = null;
                        receiverPhoneData.Call = null;
                    }
                    else player.sendChatMessage("Whoops. Something went wrong.");
                }
            }
        }

        [Command("pickup", Group = "Phone Commands")]
        public void PickupCommand(Client player)
        {
            // Fetching data
            var senderData = Account.GetPlayerCharacterData(player);

            if (senderData != null)
            {
                var senderPhoneData = PhoneDataList.FirstOrDefault(p => p.Number == senderData.CharacterData.PhoneNumber);

                if (senderPhoneData != null)
                {
                    // Check conditions
                    if (senderPhoneData.Call != null && senderPhoneData.Call.Answered)
                    {
                        API.sendChatMessageToPlayer(player, "You are already in a call.");
                        return;
                    }

                    if (senderPhoneData.Call == null)
                    {
                        API.sendChatMessageToPlayer(player, "There's no call to answer.");
                        return;
                    }

                    var receiverPhoneData = PhoneDataList.FirstOrDefault(p => p.Player == senderPhoneData.Call.Receiver);

                    if (receiverPhoneData != null)
                    {
                        // Assign it to the players
                        senderPhoneData.Call.Answered = true;
                        receiverPhoneData.Call.Answered = true;

                        // Send them a message for UX purposes
                        API.sendChatMessageToPlayer(senderPhoneData.Call.Caller,
                            "Your call has been connected. Use ~o~/p~w~ to talk on the phone.");
                        API.sendChatMessageToPlayer(senderPhoneData.Call.Receiver,
                            "Your call has been connected. Use ~o~/p~w~ to talk on the phone.");
                    }
                }
            }
        }

        [Command("p", GreedyArg = true, Group = "Phone Commands")]
        public void PhoneTalkCommand(Client player, string text)
        {
            // Fetching data
            var senderData = Account.GetPlayerCharacterData(player);

            if (senderData != null)
            {
                var senderPhoneData = PhoneDataList.FirstOrDefault(p => p.Number == senderData.CharacterData.PhoneNumber);

                if (senderPhoneData != null)
                {
                    if (senderPhoneData.Call == null ^ (senderPhoneData.Call != null && senderPhoneData.Call.Answered == false))
                    {
                        API.sendChatMessageToPlayer(player, "You are not in a call.");
                        return;
                    }

                    var receiverPhoneData = PhoneDataList.FirstOrDefault(p => p.Player == senderPhoneData.Call.Receiver);

                    if (receiverPhoneData != null)
                    {
                        // Check conditions
                        if ((senderPhoneData.Call != null && receiverPhoneData.Call != null) || (senderPhoneData.Call != null && receiverPhoneData.Call != null && senderPhoneData.Call.Answered && receiverPhoneData.Call.Answered))
                        {
                            // TODO: Fix null ref ex when using /p without a call
                            senderPhoneData.Call.Receiver.sendChatMessage($"~y~{NamingFunctions.RoleplayName(player.name)} (cellphone): {text}");
                            senderPhoneData.Call.Caller.sendChatMessage($"~y~{NamingFunctions.RoleplayName(player.name)} (cellphone): {text}");

                            ChatLibrary.SendChatMessageToPlayersInRadiusFaded(API, player, ChatLibrary.DefaultChatRadius, $"{NamingFunctions.RoleplayName(player.name)} (cellphone): {text}", player);
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "You are not in a call.");
                        }
                    }
                }
            }
        }
    }
}
