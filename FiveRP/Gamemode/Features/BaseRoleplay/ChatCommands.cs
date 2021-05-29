using System;
using FiveRP.Gamemode.Features.Admin;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;

namespace FiveRP.Gamemode.Features.BaseRoleplay
{
    public class ChatCommands : Script
    {
        public ChatCommands()
        {
            API.onChatMessage += OnPlayerChatMessage;
            API.onChatCommand += OnChatCommand;
        }

        private void OnChatCommand(Client sender, string command, CancelEventArgs cancel)
        {
            if (!sender.hasSyncedData("logged") ||
                (sender.hasSyncedData("logged") && sender.getSyncedData("logged") == false))
            {
                if (!command.StartsWith("/login"))
                {
                    sender.sendChatMessage("You can't send a chat command before logging in.");
                    cancel.Cancel = true;
                }
            }

            if (API.hasEntityData(sender, "last_command"))
            {
                var lastCommand = (DateTime) API.getEntityData(sender, "last_command");

                if (DateTime.Now.Subtract(lastCommand).TotalSeconds < 0.3)
                {
                    sender.sendChatMessage("You cannot send a command this quickly.");
                    cancel.Cancel = true;
                }

                API.setEntityData(sender, "last_command", DateTime.Now);
            }
            else API.setEntityData(sender, "last_command", DateTime.Now);
        }

        #region Commands

        // Local chat area White color (does not fade) 
        // Format: (( {Text} ))
        [Command("b", GreedyArg = true, Group = "Chat Commands")]
        public void LocalOocCommand(Client sender, string message)
        {
            ChatLibrary.SendChatMessageToPlayersInRadiusColored(API, sender, ChatLibrary.DefaultChatRadius, "~#808080~",
                $"~h~(( ({sender.handle.Value}) {NamingFunctions.RoleplayName(sender.name)}: {message} ))~h~");
        }

        [Command("playerid", Alias = "id", AddToHelpmanager = false, Group = "Chat commands")]
        public void PlayerIdCommand(Client sender, string target)
        {
            int pid;
            if (int.TryParse(target, out pid))
            {
                var client = PlayerLibrary.GetClientFromId(API, pid);
                sender.sendChatMessage($"The name of {pid} is {client.name}");
            }
            else
            {
                foreach (var player in API.getAllPlayers())
                {
                    if (player.name.ToLower().Equals(target.ToLower()) || player.name.ToLower().Contains(target.ToLower()))
                        sender.sendChatMessage($"The ID of {player.name} is {PlayerLibrary.IdFromClient(player)}");
                }
            }
        }

        // Local chat area coloured (No ID yet) 
        // Format: * {Name} {Text} *
        [Command("me", GreedyArg = true, Group = "Chat Commands")]
        public void MeCommand(Client sender, string message)
        {
            ChatLibrary.SendChatMessageToPlayersInRadiusColored(API, sender, ChatLibrary.DefaultChatRadius, "~#C2A2DA~",
                $"* {NamingFunctions.RoleplayName(sender.name)} {message}");
        }

        [Command("ame", GreedyArg = true, Group = "Chat Commands")]
        public void AmeCommand(Client senderClient, string message)
        {
            ChatLibrary.SendLabelEmoteMessage(API, senderClient, message);
        }

        // Local chat area coloured (No ID yet) 
        // Format: * {Name} {Text} *
        [Command("melow", GreedyArg = true, Group = "Chat Commands")]
        public void MeLowCommand(Client sender, string message)
        {
            ChatLibrary.SendChatMessageToPlayersInRadiusColored(API, sender, ChatLibrary.LowChatRadius, "~#C2A2DA~",
                $"* {NamingFunctions.RoleplayName(sender.name)} {message}");
        }

        [Command("my", GreedyArg = true, Group = "Chat Commands")]
        public void MyCommand(Client sender, string message)
        {
            ChatLibrary.SendChatMessageToPlayersInRadiusColored(API, sender, ChatLibrary.DefaultChatRadius, "~#C2A2DA~",
                $"* {NamingFunctions.RoleplayName(sender.name)}'s {message}");
        }

        // Local chat area coloured (No ID yet) 
        // Format: * {Text} ({Name}) * 
        // Note the brackets around character name
        [Command("do", GreedyArg = true, Group = "Chat Commands")]
        public void DoCommand(Client sender, string emote)
        {
            ChatLibrary.SendChatMessageToPlayersInRadiusColored(API, sender, ChatLibrary.DefaultChatRadius, "~#C2A2DA~",
                $"* {emote} (({NamingFunctions.RoleplayName(sender.name)})) *");
        }

        // Global out of character chat
        // Format: (( OOC: {Name}: {Text} ))
        [Command("o", GreedyArg = true, Alias = "ooc,outofchar", Group = "Chat Commands")]
        public void GlobalOocCommand(Client sender, string message)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                API.sendChatMessageToAll($"~h~(( OOC: ({sender.handle.Value}) {NamingFunctions.RoleplayName(sender.name)}: {message} ))~h~");
            }
        }

        // Shouts (wider radius than Free text) 
        // Format: {Name} (Shout): {Text}
        [Command("s", GreedyArg = true, Alias = "shout,sh", Group = "Chat Commands")]
        public void ShoutCommand(Client sender, string message)
        {
            ChatLibrary.SendChatMessageToPlayersInRadius(API, sender, ChatLibrary.FarChatRadius,
                $"~w~~h~{NamingFunctions.RoleplayName(sender.name)} shouts: {message}~h~");
        }

        // Low(smaller radius than Free text) 
        // Format: {Name} (Low): {Text}
        [Command("l", GreedyArg = true, Alias = "low", Group = "Chat Commands")]
        public void LowCommand(Client sender, string message)
        {
            ChatLibrary.SendChatMessageToPlayersInRadius(API, sender, ChatLibrary.LowChatRadius, $"~w~{NamingFunctions.RoleplayName(sender.name)}~w~ says [low]: {message}");
        }

        // Chats the target player 
        // Format: {Name} (Whisper): {Text} 
        // A /me is shown to everyone
        // Format: {Name} whispers to {TargetName}
        [Command("w", GreedyArg = true, Alias = "whisper,whisp", Group = "Chat Commands")]
        public void WhisperCommand(Client sender, string targ, string message)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            if (DistanceLibrary.DistanceBetween(target.position, API.getEntityPosition(sender)) < 2f)
            {
                var formattedMessage = $"~y~{NamingFunctions.RoleplayName(sender.name)}~y~ (Whisper): {message}";
                API.sendChatMessageToPlayer(sender, formattedMessage);
                API.sendChatMessageToPlayer(target, formattedMessage);
                ChatLibrary.SendLabelEmoteMessage(API, sender, $"* {NamingFunctions.RoleplayName(NamingFunctions.RoleplayName(sender.name))} whispers to {NamingFunctions.RoleplayName(target.name)}.", true);
            }
            else
            {
                API.sendChatMessageToPlayer(sender, $"You're too far away from {NamingFunctions.RoleplayName(target.name)}!");
            }
        }

        // PM the target player 
        // Format: (( PM to/from Player: Message ))
        [Command("pm", GreedyArg = true, Group = "Chat Commands")]
        public void PrivateMessageCommand(Client sender, string targ, string message)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            API.sendChatMessageToPlayer(sender, "~#FFD700~",
                $"(( PM to ({target.handle.Value}) {NamingFunctions.RoleplayName(target.name)}: {message} ))");
            API.sendChatMessageToPlayer(target, "~#FFD700~",
                $"(( PM from ({sender.handle.Value}) {NamingFunctions.RoleplayName(sender.name)}: {message} ))");

            API.setEntityData(target, "lastPMid", PlayerLibrary.IdFromClient(sender).ToString());
        }

        [Command("re", GreedyArg = true, Group = "Chat Commands")]
        public void PrivateMessageReCommand(Client sender, string message)
        {           
            if (!API.hasEntityData(sender, "lastPMid")) return;
            Client target = PlayerLibrary.CommandClientFromString(API, sender, API.getEntityData(sender, "lastPMid"));
            if (target == null) return;

            API.sendChatMessageToPlayer(sender, "~#FFD700~",
                $"(( PM to ({target.handle.Value}) {NamingFunctions.RoleplayName(target.name)}: {message} ))");
            API.sendChatMessageToPlayer(target, "~#FFD700~",
                $"(( PM from ({sender.handle.Value}) {NamingFunctions.RoleplayName(sender.name)}: {message} ))");

            API.setEntityData(target, "lastPMid", PlayerLibrary.IdFromClient(sender).ToString());
        }

        [Command("dice", GreedyArg = false, Group = "Chat Commands")]
        public void DiceCommand(Client sender)
        {
            Random random = new Random();

            int value = random.Next(1, 6);

            ChatLibrary.SendChatMessageToPlayersInRadius(API, sender, ChatLibrary.DefaultChatRadius,
                $"** {NamingFunctions.RoleplayName(sender.name)} has thrown a dice, landing on {value}. ((1 to 6))");
        }

        [Command("roll", GreedyArg = false, Group = "Chat Commands")]
        public void RollCommand(Client sender, int lowest, int highest)
        {
            Random random = new Random();

            int value = random.Next(lowest, highest);

            ChatLibrary.SendChatMessageToPlayersInRadius(API, sender, ChatLibrary.DefaultChatRadius,
                $"** {NamingFunctions.RoleplayName(sender.name)} has rolled {value}. (({lowest} to {highest}))");
        }

        #endregion

        #region Event Handlers
        private void OnPlayerChatMessage(Client sender, string message, CancelEventArgs cancel)
        {
            // Prevent the default behavior
            cancel.Cancel = true;

            if (sender.hasSyncedData("logged"))
            {
                if (sender.getSyncedData("logged") == true)
                {
                    if (AdminLibrary.OnAdminDuty(sender))
                    {
                        LocalOocCommand(sender, message);
                        return;
                    }

                    // Send the message locally
                    Logging.Log($"[FiveRP] (Local IC) {NamingFunctions.RoleplayName(sender.name)}: {message}");

                    foreach (var playerDistance in DistanceLibrary.PlayersDistanceFrom(API, API.getEntityPosition(sender)))
                    {
                        var distance = playerDistance.Item2;
                        var player = playerDistance.Item1;
                        if (player.dimension != sender.dimension)
                            continue;
                        var normalisedDistance = 1 - (distance / ChatLibrary.DefaultChatRadius);
                        if (normalisedDistance < 0) // Too far away
                        {
                            continue;
                        }

                        // Make an RGB grey color depending on normalised distance
                        var rgbValue = (int)(ChatLibrary.MinFreechatRgb + ((255 - ChatLibrary.MinFreechatRgb) * normalisedDistance));

                        // Convert to hex colour string
                        string hexColor = $"#{rgbValue:x}{rgbValue:x}{rgbValue:x}";
                        API.sendChatMessageToPlayer(player, $"~{hexColor}~",
                            $"{NamingFunctions.RoleplayName(sender.name)} says: {message}");
                    }
                } else sender.sendChatMessage("You can't send a chat message before logging in.");
            } else sender.sendChatMessage("You can't send a chat message before logging in.");
        }
        #endregion
    }
}