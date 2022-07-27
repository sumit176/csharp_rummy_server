using System;
using System.Collections.Generic;
using CommonData.Enums;

namespace CommonData.Data.Response
{
    public sealed class GameJoinResData
    {
        public string GameId { get; set; }
        public List<string> Players { get; set; }
        public GameState GameState { get; set; }
    }
}