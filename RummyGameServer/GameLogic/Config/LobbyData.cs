using System;

namespace RummyGameServer.GameLogic.Config
{
    [Serializable]
    public class LobbyData
    {
        public int Id;
        public string Name;
        public double EntryPrice;
        public double EarnPrice;
        public double RakePercent; //amount to deduct from the earning
        public bool Locked;
    }
}