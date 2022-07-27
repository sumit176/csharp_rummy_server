using System;

namespace CommonData.Data.Response
{
    public sealed class TurnTimeSyncData
    {
        public string UserId { get; set; }
        public float TotalTime { get; set; }
        public float TimeLeft { get; set; }

        public override string ToString()
        {
            return string.Format("UserId : {0} TotalTime : {1} TimeLeft {2}", UserId, TotalTime, TimeLeft);
        }
    }
}