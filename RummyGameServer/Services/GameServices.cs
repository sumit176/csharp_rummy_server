using System;
using System.Threading.Tasks;
using CommonData.Data.Request;
using RummyGameServer;

namespace RummyGameServer
{
    public class GameServices : IGameServices
    {
        public Game GetGame(string gameId)
        {
            throw new NotImplementedException();
        }

        public Game FindGame(GameJoinData gameJoinData)
        {
            throw new NotImplementedException();
        }

        public Game CreateGame(Game game)
        {
            throw new NotImplementedException();
        }

        public Game SaveGame(Game game)
        {
            throw new NotImplementedException();
        }
    }
}