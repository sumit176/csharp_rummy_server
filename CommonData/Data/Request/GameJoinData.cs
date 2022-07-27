using System;
using CommonData.Enums;

namespace CommonData.Data.Request
{
    public sealed class GameJoinData
    {
        public GameType GameType { get; set; }
        public int Lobby { get; set; }
        public MaxPlayers MaxPlayer { get; set; }

        public override string ToString()
        {
            return string.Format("GameType: {0} Lobby : {1} MaxPlayer : {2}", GameType, Lobby, MaxPlayer);
        }
    }
}