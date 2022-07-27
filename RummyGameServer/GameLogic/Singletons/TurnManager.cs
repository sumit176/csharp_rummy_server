using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RummyGameServer.GameLogic;
using RummyGameServer.Hubs;

namespace RummyGameServer
{
    public class TurnManager
    {
        public long TurnDuration;
        public float RemainingSecondsInTurn;
        public float ElapsedTimeInTurn;
        public bool IsCompletedByAll;
        public bool IsCompletedByMe;
        public bool IsOver;
        public int Turn;

        private List<Player> Players;
        private bool isRunning = false;
        
        public Timer pTimer { get; set; }

        public async void BeginTurn()
        {
            //Start the thread for the time
            pTimer = new Timer(OnTimeComplete, isRunning, TurnDuration, 1);
        }

        private void OnTimeComplete(object? state)
        {
            
        }

        public async Task<int> DoSomething(int time)
        {
            while (time > 0)
            {
                await Task.Delay(1000);
                Console.WriteLine($"Time is {time}");
                time--;
            }
            return time;
        }
        
        public async Task StartTimer(int time, Action<int> callback = null)
        {
            callback?.Invoke(time);
            await Task.Delay(time* 1000);
        }
    }
}