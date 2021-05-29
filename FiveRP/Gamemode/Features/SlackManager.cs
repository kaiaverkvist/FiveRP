using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;

namespace FiveRP.Gamemode.Features
{
    public class SlackManager : Script
    {
        public SlackClient SlackClient;
        public SlackManager()
        {
            API.onResourceStart += OnResourceStart;
            AlertLogging.OnRaiseAlert += onAlertRaised;
        }

        private void onAlertRaised(string text, string category, int level)
        {
            var message = $"[Alert] *{text}* | _{category}_";

            if (Config.GetKeyInt("#production") == 0)
            {
                message = "*[TEST SERVER]*" + message;
            }

            if (level >= 3)
            {
                SendSlackMessage("FiveRP Relay", message, "alerts");
            }
        }

        private void OnResourceStart()
        {
            string slack_url = "https://hooks.slack.com/services/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";

            SlackClient = new SlackClient(slack_url);
        }

        public void SendSlackMessage(string username, string text, string channel)
        {
            Payload payload = new Payload
            {
                username = username,
                text = text,
                channel = channel
            };
            SlackClient.PostMessage(API.toJson(payload));
        }

    }
}