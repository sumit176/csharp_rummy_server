using System;
using System.Threading.Tasks;
using CommonData.Data.Request;
using CommonData.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RummyGameServer.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class GameHub : Hub
    {
        private readonly GameManager _gameManager;
        
        public GameHub(GameManager gameManager)
        {
            _gameManager = gameManager;
        }
        
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Player connected with {Context.ConnectionId}");
            // Console.WriteLine($"Player Name {Context.User.Claims.to}");
            Clients.Client(Context.ConnectionId).SendAsync("Connected", $"{Context.ConnectionId} joined");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Player disconnected with {Context.ConnectionId} {exception?.Message}");
            await _gameManager.PlayerDisconnected(Context);
            await Clients.Client(Context.ConnectionId).SendAsync("Disconnected", $"{Context.ConnectionId} Left");
        }
        
        public async void JoinGame(GameJoinData joinData)
        {
            await _gameManager.CreateOrJoinGame(joinData, Context);
        }

        public async void ExitFromGame()
        {
            await _gameManager.LeaveGame(Context);
        }
        
        public async void RejoinGame(string gameId)
        {
            await _gameManager.RejoinGame(Context, gameId);
        }

        public async void PlayerMove(PlayerActionData actionData)
        {
            await _gameManager.GamePlayAction(Context, actionData);
        }
    }
}