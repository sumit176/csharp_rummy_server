using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonData.Data.Request;
using CommonData.Data.Response;
using CommonData.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RummyGameServer.GameLogic.Factory;
using RummyGameServer.Hubs;

namespace RummyGameServer
{
    public class GameManager
    {
        private readonly Dictionary<string, Game> _clientGameMap; //Player id game mapping
        private readonly IHubContext<GameHub> _gameHub;
        private readonly ILogger<GameManager> _logger;
        private readonly TurnManager _turnManager;

        public GameManager(IHubContext<GameHub> gameHub, TurnManager turnManager, ILogger<GameManager> logger)
        {
            _gameHub = gameHub;
            _logger = logger;
            _clientGameMap = new Dictionary<string, Game>();
            _turnManager = turnManager;
        }

        public async Task CreateOrJoinGame(GameJoinData gameJoinData, HubCallerContext context)
        {
            Console.WriteLine($"{context.ConnectionId} has requested for the game join {gameJoinData.ToString()}" );
            var game = GameFactory.CreateOrJoinGame(gameJoinData); //Look for already joined games as well
            if (!_clientGameMap.ContainsKey(context.ConnectionId))
            {
                _clientGameMap.Add(context.ConnectionId, game);
                await _gameHub.Groups.AddToGroupAsync(context.ConnectionId, game.Id);
            }

            if (game != null)
            {
                _logger.LogInformation("Adding Player {Player} to the game {Game}",context.ConnectionId, game.Id);
                game.AddPlayer(new Player(context.ConnectionId, context.ConnectionId));
            }
            else
            {
                _logger.LogError("Game Not Found for the Connection {ConnectionId}",context.ConnectionId);
                return;
            }
            
            //Send the Game Join data to every connecting player
            await _gameHub.Clients.Group(game.Id).SendAsync(Constants.JoinGame, game.GetGameJoinResData());
            CheckForGameReady(game);
        }

        private async void CheckForGameReady(Game game)
        {
            if (game.CanStart() && game.State == GameState.Waiting)
            {
                //Send player for starting game in time
                await _turnManager.StartTimer(Constants.GameWaitTime, async (time) =>
                {
                    await _gameHub.Clients.Group(game.Id).SendAsync(Constants.WaitingForStart, time);
                });
                
                //Once the initial 10 seconds time complete
                //then change the game status to start
                game.Start();
                
                //Now send the start game event
                await _gameHub.Clients.Group(game.Id).SendAsync(Constants.StartGame, game.GetGameJoinResData());
                
                //Turn Decide
                await TurnDecide(game);
                
                //Send the Initial Game Data to all the players with their hands data
                await SendGameSyncResponseToAll(game);
                
                //Start the turn
                await game.StartTurnTimer(OnTurnChange, OnTimerUpdate);
            }
        }

        private async void OnTimerUpdate(Game game, string playerId, int timeLeft)
        {
            await SendTurnSync(timeLeft, playerId, game);
        }

        private async void OnTurnChange(Game game,string playerId)
        {
            await SendTurnSync(Constants.TurnTime, playerId, game);
        }

        private async Task SendTurnSync(int timeLeft, string playerId, Game game)
        {
            var turnSyncData = new TurnTimeSyncData
            {
                TimeLeft = timeLeft, TotalTime = Constants.TurnTime, UserId = playerId
            };
            await _gameHub.Clients.Group(game.Id).SendAsync(Constants.TurnSync, turnSyncData);
        }
        private async Task SendGameSyncResponseToAll(Game game)
        {
            GameSyncResponse syncResponse = new GameSyncResponse
            {
                ClosedDeck = game.ClosedDeck.GetCard(),
                OpenDeck = game.OpenDeck.GetCard(),
                PlayerTurn = game.GetFirstPlayerTurn()
            };

            foreach (var player in game.Players)
            {
                syncResponse.PlayerHand = game.Players[player.Key].GetHand().ToResponseData();
                await _gameHub.Clients.Client(player.Value.ConnectionId).SendAsync(Constants.DealCards, syncResponse);
            }
        }
        
        private async Task SendGameSyncResponseToPlayer(Game game, string playerId)
        {
            GameSyncResponse syncResponse = new GameSyncResponse
            {
                ClosedDeck = game.ClosedDeck.GetCard(),
                OpenDeck = game.OpenDeck.GetCard(),
                PlayerHand = game.Players[playerId].GetHand().ToResponseData()
            };
            await _gameHub.Clients.Client(game.Players[playerId].ConnectionId).SendAsync(Constants.JoinGame, syncResponse);
        }
        
        private async Task SendCardPickDropResponseToPlayer(Game game, string connectionId, string cardId, PlayerActionType actionType)
        {
            CardPickDropResponse playerResponse = new CardPickDropResponse
            {
                ClosedDeck = game.ClosedDeck.GetCard(),
                OpenDeck = game.OpenDeck.GetCard(),
                CardId = cardId,
                PlayerAction = actionType,
                PlayerTurn = connectionId
            };
            await _gameHub.Clients.Client(connectionId).SendAsync(Constants.PickOrDropCard, playerResponse);

            //List other players
            var connectionIds = game.Players.Select(p => p.Key).ToList();
            //remove current player
            connectionIds.Remove(connectionId);
            //make card id empty for other players
            playerResponse.CardId = String.Empty;
            
            await _gameHub.Clients.Clients(connectionIds).SendAsync(Constants.PickOrDropCard, playerResponse);
        }

        private async Task TurnDecide(Game game)
        {
            TurnDeciderData data = new TurnDeciderData {PlayerCardMap = new Dictionary<string, string>()};
            foreach (var player in game.Players)
            {
                data.PlayerCardMap.Add(player.Key, player.Value.FirstCard.Id);
            }
            await _gameHub.Clients.Group(game.Id).SendAsync(Constants.TurnDecide, data);
            await Task.Delay(3000);
        }
        
        public async Task RejoinGame(HubCallerContext context, string gameId)
        {
            //Optimise the code
            var runningGame = _clientGameMap.Values.Where(g => g.Id == gameId);
            var games = runningGame as Game[] ?? runningGame.ToArray();
            if (!games.Any())
            {
                //Send no game running with the game id
            }
            else
            {
                //return the game sync state
                await SendGameSyncResponseToPlayer(games[0], context.ConnectionId);
            }
        }
        
        public async Task LeaveGame(HubCallerContext context)
        {
            var runningGame = _clientGameMap[context.ConnectionId];
            if (runningGame != null)
            {
                await runningGame.PlayerAction(context.ConnectionId, null);
                _logger.Log(LogLevel.Debug, "Player {ConnectionId} has left the Game {GameId}", context.ConnectionId,
                    runningGame.Id);
            }
            else
            {
                _logger.Log(LogLevel.Debug,"No Running game for Player {ConnectionId}",context.ConnectionId);
                return;
            }
            if (_clientGameMap != null && _clientGameMap.ContainsKey(context.ConnectionId))
                _clientGameMap.Remove(context.ConnectionId);
            await _gameHub.Clients.Group(runningGame.Id).SendAsync(Constants.ExitGame, true);
            runningGame.Dispose();
        }

        public async Task GamePlayAction(HubCallerContext context, PlayerActionData data)
        {
            var runningGame = _clientGameMap[context.ConnectionId];
            if (runningGame != null)
            {
                await runningGame.PlayerAction(context.ConnectionId, data, async (game, actionData, cardId) =>
                {
                    await SendCardPickDropResponseToPlayer(game, context.ConnectionId, cardId, data.ActionType);
                });
                _logger.Log(LogLevel.Information, "Player Action {Action} with data {Data}", data.ActionType, data.MoveId);
            }
            else
            {
                _logger.Log(LogLevel.Error, "No Running game for Player {ConnectionId}", context.ConnectionId);
            }
        }

        public async Task PlayerDisconnected(HubCallerContext context)
        {
            var runningGame = _clientGameMap[context.ConnectionId];
            if (runningGame != null)
            {
                runningGame.DisconnectPlayer(context.ConnectionId);
                Console.WriteLine($"Player {context.ConnectionId} has left the Game {runningGame.Id}");
                await WaitForPlayerReconnection(runningGame.Players[context.ConnectionId], runningGame);
            }
            else
            {
                Console.WriteLine($"No Running game for Player {context.ConnectionId}");
            }
        }
        private async Task WaitForPlayerReconnection(Player player, Game game)
        {
            int timeout = Constants.PlayerReconnectionTimeout;
            while (player.Connection == ConnectionStatus.Disconncted && timeout > 0)
            {
                await Task.Delay(1000);
                timeout--;
            }
            if(player.Connection == ConnectionStatus.Disconncted)
                game.RemovePlayer(player);
            if (!game.HasPlayers())
            {
                game.Remove();
            }
        }
    }
}