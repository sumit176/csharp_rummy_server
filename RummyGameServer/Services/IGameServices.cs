using System.Collections.Generic;
using System.Threading.Tasks;
using CommonData.Data.Request;
using RummyGameServer;

namespace RummyGameServer
{
    public interface IGameServices
    {
        Game GetGame(string gameId);
        Game FindGame(GameJoinData gameJoinData);
        Game CreateGame(Game game);
        Game SaveGame(Game game);
    }
}