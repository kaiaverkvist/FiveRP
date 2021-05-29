using System.Timers;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Library.FunctionLibraries
{
    public static class ChatLibrary
    {
        public const double DefaultChatRadius = 10.0;
        public const double AmeChatRadius = 20.0;
        public const double NearChatRadius = 5.0;
        public const double FarChatRadius = 25.0;
        public const double WhisperChatRadius = 1.0;
        public const double LowChatRadius = 2.0;
        public const int MinFreechatRgb = 100;

        public static void SendEmoteChatMessage(API api, Client sender, string message)
        {
            SendChatMessageToPlayersInRadiusColored(api, sender, DefaultChatRadius, "~#C2A2DA~",
                $"* {NamingFunctions.RoleplayName(sender.name)} {message}");
        }

        public static void SendChatMessageToPlayersInRadius(API api, Client sender, double radius, string message, Client ignoreTarget = null)
        {
            // Find players within the radius of the sender
            var players = DistanceLibrary.CalculatePlayersInRadius(api, sender, radius);
            foreach (var t in players)
            {
                if (t.dimension != sender.dimension)
                    continue;
                if (ignoreTarget == null)
                {
                    api.sendChatMessageToPlayer(t, message);
                }
                else
                {
                    if (t != ignoreTarget)
                    {
                        api.sendChatMessageToPlayer(t, message);
                    }
                }
            }
        }

        public static void SendChatMessageToPlayersInRadiusFaded(API api, Client sender, double radius, string message, Client ignoreTarget = null)
        {
            // Find players within the radius of the sender
            var players = DistanceLibrary.CalculatePlayersInRadius(api, sender, radius);
            foreach (var t in players)
            {
                if (t.dimension != sender.dimension)
                    continue;
                var tDistance = DistanceLibrary.DistanceBetween(t.position, sender.position);

                var normalisedDistance = 1 - (tDistance / ChatLibrary.DefaultChatRadius);
                if (normalisedDistance < 0) // Too far away
                {
                    continue;
                }

                // Make an RGB grey color depending on normalised distance
                var rgbValue = (int)(ChatLibrary.MinFreechatRgb + ((255 - ChatLibrary.MinFreechatRgb) * normalisedDistance));

                // Convert to hex colour string
                string hexColor = $"#{rgbValue:x}{rgbValue:x}{rgbValue:x}";
                
                if (ignoreTarget == null)
                {
                    api.sendChatMessageToPlayer(t, $"~{hexColor}~", message);
                }
                else
                {
                    if (t != ignoreTarget)
                    {
                        api.sendChatMessageToPlayer(t, $"~{hexColor}~", message);
                    }
                }
            }
        }

        public static void SendChatMessageToPlayersInRadiusColored(API api, Client sender, double radius, string color, string message, Client ignoreTarget = null)
        {
            // Find players within the radius of the sender
            var players = DistanceLibrary.CalculatePlayersInRadius(api, sender, radius);
            foreach (var player in players)
            {
                if (player.dimension != sender.dimension)
                    continue;
                if (ignoreTarget == null)
                {
                    api.sendChatMessageToPlayer(player, color, message);
                }
                else
                {
                    if (player != ignoreTarget)
                    {
                        api.sendChatMessageToPlayer(player, color, message);
                    }
                }

            }
        }

        public static void SendLabelEmoteMessage(API api, Client senderClient, string message, bool overrideMessage = false)
        {
            var labelMessage = $"* {NamingFunctions.RoleplayName(senderClient.name)} {message}";

            if (overrideMessage)
            {
                labelMessage = message;
            }

            if (api.hasEntityData(senderClient, "me_label"))
            {
                var label = (TextLabel)api.getEntityData(senderClient, "me_label");

                api.setTextLabelText(label, labelMessage);
            }
            else
            {
                var label = api.createTextLabel(labelMessage, senderClient.position, (float)AmeChatRadius, 0.45f, false, api.getEntityDimension(senderClient));

                api.setTextLabelColor(label, 194, 162, 218, 255);
                api.attachEntityToEntity(label, senderClient, null, new Vector3(0, 0, 1.7f), new Vector3(0, 0, 0));

                api.setEntityData(senderClient, "me_label", label);

                var destroyTimer = new Timer(5000) { AutoReset = false };
                destroyTimer.Start();
                destroyTimer.Elapsed += (sender, e) =>
                {
                    api.resetEntityData(senderClient, "me_label");
                    DestroyTimer_Elapsed(api, sender, e, label);
                };
            }
        }

        private static void DestroyTimer_Elapsed(API api, object sender, ElapsedEventArgs e, NetHandle labelHandle)
        {
            // Destroy the label!
            api.deleteEntity(labelHandle);
        }
    }
}