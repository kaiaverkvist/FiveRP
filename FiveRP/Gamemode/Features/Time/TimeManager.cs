using System;
using System.Timers;
using GTANetworkServer;

namespace FiveRP.Gamemode.Managers
{
    public class TimeManager : Script
    {
        public delegate void TimeEvent(DateTime time);
        public static event TimeEvent OnTimeUpdate;

        private Timer _timeTimer;

        public TimeManager()
        {
            API.onResourceStart += StartTimeTimer;
            API.onResourceStop += OnResourceStop;
        }

        private void OnResourceStop()
        {
            _timeTimer.Dispose();
        }

        private void StartTimeTimer()
        {
            // Don't change the timescale without making sure a minute is only added for each minute passed.
            _timeTimer = new Timer(60 * 1000);
            _timeTimer.Elapsed += HandleTime;
            _timeTimer.AutoReset = true;
            _timeTimer.Enabled = true;

            HandleTime(null, null);
        }

        private void HandleTime(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var date = DateTime.Now;

            API.setTime(date.Hour, date.Minute);

            // Call the event.
            OnTimeUpdate?.Invoke(date);
        }

        [Command("time", Group = "Player Commands")]
        public void TimeCommand(Client sender)
        {
            sender.sendChatMessage($"Current time: {DateTime.Now}");
        }
    }
}
