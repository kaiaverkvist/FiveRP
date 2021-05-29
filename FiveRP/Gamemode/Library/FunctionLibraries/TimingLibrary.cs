using GTANetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FiveRP.Gamemode.Library.FunctionLibraries
{

    class TimingLibrary : Script
    {
        private class TimeEvent
        {
            public long TargetTime { get; set; }
            public long Length { get; set; }
            public Action TargetAction { get; set; }
            public int Id { get; set; }
            public bool Synchronous { get; set; }
            public TimeEvent(long targetTime, long length, Action targetAction, int id, bool synchronous)
            {
                TargetTime = targetTime;
                TargetAction = targetAction;
                Id = id;
                Synchronous = synchronous;
            }
        }

        public TimingLibrary()
        {
            API.onUpdate += OnUpdate;
        }

        private static Dictionary<int, TimeEvent> events = new Dictionary<int, TimeEvent>();
        private static int count = 0;

        public static int scheduleSyncAction(long timeFromNow, Action action)
        {
            lock (events)
            {
                events.Add(count, 
                    new TimeEvent(Environment.TickCount + timeFromNow, timeFromNow, action, count++, true));
            }
            return count - 1;
        }

        public static int scheduleAsyncAction(long timeFromNow, Action action)
        {
            lock (events)
            {
                events.Add(count,
                    new TimeEvent(Environment.TickCount + timeFromNow, timeFromNow, action, count++, false));
            }
            return count - 1;
        }

        public static bool CancelQueuedAction(int actionId)
        {
            bool r = events.ContainsKey(actionId);
            if (r) events.Remove(actionId);
            return r;
        }

        public static bool RestartActionQueue(int actionId)
        {
            bool r = events.ContainsKey(actionId);
            if (r)
            {
                TimeEvent t;
                events.TryGetValue(actionId, out t);
                t.TargetTime = Environment.TickCount + t.Length;
            }
            return r;
        }

        public void OnUpdate()
        {
            List<TimeEvent> copyList;
            lock (events)
            {
                copyList = events.Values.ToList();
            }
            foreach (var action in copyList)
            {
                if (Environment.TickCount > action.TargetTime)
                {
                    events.Remove(action.Id);
                    if (action.Synchronous)
                    {
                        action.TargetAction.Invoke();
                    } else
                    {
                        new Thread(new ThreadStart(action.TargetAction)).Start();
                    }
                }
            }
        }

    }
}
