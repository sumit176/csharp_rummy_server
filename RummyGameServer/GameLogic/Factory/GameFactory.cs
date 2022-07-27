using System.Collections.Generic;
using System.Linq;
using CommonData.Data.Request;
using CommonData.Enums;

namespace RummyGameServer.GameLogic.Factory
{
    public static class GameFactory
    {
        private static List<Game> _games;
        
        public static Game CreateOrJoinGame(GameJoinData gameJoinData)
        {
            if (_games == null)
            {
                _games = new List<Game>();
                return GetNewGame(gameJoinData);
            }

            var game = SearchGame(gameJoinData);
            if (game == null)
                return GetNewGame(gameJoinData);
            return game;
        }

        private static Game SearchGame(GameJoinData joinData)
        {
            return _games.FirstOrDefault(game => 
                (game.State == GameState.Waiting || game.State == GameState.Starting) 
                && !game.RoomFull 
                && game.GameType == joinData.GameType);
        }
        private static Game GetNewGame(GameJoinData gameJoin)
        {
            Game g = new Game(gameJoin.MaxPlayer, gameJoin.GameType);
            _games.Add(g);
            return g;
        }

        public static void Remove(this Game game)
        {
            _games.Remove(game);
        }
    }
}