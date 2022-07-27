using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonData.Data.Request;
using CommonData.Data.Response;
using CommonData.Enums;
using RummyGameServer.GameLogic.Interfaces;

namespace RummyGameServer
{
    public class Game : IGame, IDisposable
    {
        internal EventHandler OnGameComplete;
        internal EventHandler OnGameCancelled;
        internal EventHandler OnGameStarted;
        
        public string Id { get; private set; }
        public Dictionary<string, Player> Players { get; private set; }
        public GameState State { get; private set; }
        public GameType GameType { get; }
        public bool IsOpenToJoin { get; private set; }
        public bool RoomFull => _playersCount == Players.Count;
        
        private MaxPlayers _maxPlayers;
        private int _playersCount;
        
        //Decks for the game
        public Deck ClosedDeck { get; private set; }
        public Deck OpenDeck { get; private set; }

        private int currentTurnIndex = -1;
        private List<string> playerIds;
        
        public Game(MaxPlayers players, GameType gameType)
        {
            _maxPlayers = players;
            GameType = gameType;
            InitGameParams();
        }
        private void InitGameParams()
        {
            Id = Guid.NewGuid().ToString();
            Players = new Dictionary<string, Player>();
            State = GameState.Waiting;
            IsOpenToJoin = true;
            _playersCount = _maxPlayers == MaxPlayers.TwoPlayers ? 2 : 6;
            ClosedDeck = new Deck(DeckType.ClosedDeck);
            ClosedDeck.Shuffle();
            OpenDeck = new Deck(DeckType.OpenDeck);
            playerIds = new List<string>();
        }

        public void AddPlayer(Player player)
        {
            if (Players.Count < _playersCount && !Players.ContainsKey(player.ConnectionId))
                Players.Add(player.ConnectionId, player);
            else
            {
                Console.WriteLine("Unable to add players on the room!");
            }
        }
        
        public void RemovePlayer(Player player)
        {
            if (Players.Count > 0)
                Players.Remove(player.ConnectionId);
            if(Players.Count == 0)
                Dispose();
        }

        public bool HasPlayers() => Players.Count > 0;

        public void ReconnectPlayer(Player player)
        {
            player.PlayerStatus(ConnectionStatus.Connected);
        }

        public async void DisconnectPlayer(string cid)
        {
            var player = Players[cid];
            player?.PlayerStatus(ConnectionStatus.Disconncted);
        }

        private List<string> GetPlayerIds()
        {
            return Players.Select(p => p.Value.Id).ToList();
        }

        public string GetFirstPlayerTurn()
        {
            string id = null;
            int fValue = 0;
            
            foreach (var player in Players)
            {
                if (fValue < player.Value.FirstCard.FaceValue)
                {
                    id = player.Key;
                    fValue = player.Value.FirstCard.FaceValue;
                }
            }

            playerIds = Players.Keys.ToList();
            currentTurnIndex = playerIds.IndexOf(id);
            return id;
        }
        public GameJoinResData GetGameJoinResData()
        {
            return new GameJoinResData
            {
                GameId = Id,
                Players = GetPlayerIds(),
                GameState = State
            };
        }
        
        public bool CanStart()
        {
            if(_maxPlayers == MaxPlayers.TwoPlayers)
                return Players.Count == 2;
             
            return Players.Count > 2;
        }
        
        public async void Start()
        {
            State = GameState.Started;
            IsOpenToJoin = false;
            
            //Create hands with 13 card distribution
            DealCardToPlayers();
            
            //Set First random card to players for turn decider
            SetFirstRandomCardToPlayers();
            
            //Fire event
            OnGameStarted?.Invoke(this, EventArgs.Empty);
        }

        private void SetFirstRandomCardToPlayers()
        {
            foreach (var player in Players)
            {
                player.Value.SetFirstCard(ClosedDeck.GetRandom());
            }
        }

        private void DealCardToPlayers()
        {
            for (int i = 0; i < 13; i++)
            {
                foreach (var player in Players)
                {
                    player.Value.AddHandCard(ClosedDeck.Draw());
                }
            }
        }

        public async void Conclude()
        {
            
        }

        public async Task PlayerAction(string pId, PlayerActionData playerActionData, Action<Game, PlayerActionData, string> actionResult = null)
        {
            var result = string.Empty;
            switch (playerActionData.ActionType)
            {
                case PlayerActionType.PickCard:
                    result = AddCardToPlayerHand(pId, playerActionData.DeckType);
                    actionResult?.Invoke(this, playerActionData, result);
                    break;
                case PlayerActionType.DropCard:
                    result = RemoveCardFromPlayerAddToOpenDeck(pId, playerActionData.MoveId);
                    actionResult?.Invoke(this, playerActionData, result);
                    break;
                case PlayerActionType.ShowCard:
                    ValidateCards();
                    State = GameState.Finished;
                    break;
                case PlayerActionType.DropGame:
                    await RemovePlayerFromGame(pId);
                    break;
            }
        }

        private void ValidateCards()
        {
            //Write score validation logic
        }

        private string RemoveCardFromPlayerAddToOpenDeck(string playerId, string payload)
        {
            var player = Players[playerId];
            var card = player.GetAndRemoveCard(payload);
            ClosedDeck.AddCard(card);
            Console.WriteLine($"Player {playerId} has {player.GetHand().ToString()}");
            return card.Id;
        }

        private string AddCardToPlayerHand(string playerId, DeckType fromDeckType)
        {
            var player = Players[playerId];
            var deck = fromDeckType == DeckType.ClosedDeck ? ClosedDeck : OpenDeck;
            var card = deck.Draw();
            player.AddHandCard(card);
            Console.WriteLine($"Player {playerId} has {player.GetHand().ToString()}");
            return card.Id;
        }

        private async Task RemovePlayerFromGame(string pid)
        {
            Players.Remove(pid);
            if (State == GameState.Started && Players.Count <= 1)
            {
                State =  GameState.Finished;
                OnGameCancelled?.Invoke(this, EventArgs.Empty);
            }
        }
        public void Dispose()
        {
            State = GameState.Finished;
            Console.WriteLine("Disposing Game "+Id);
        }

        public async Task StartTurnTimer(Action<Game, string> callbackNextTurn, Action<Game, string, int> timeUpdate)
        {
            int time = Constants.TurnTime;
            timeUpdate?.Invoke(this, playerIds[currentTurnIndex], time);
            while (State == GameState.Started && Players.Count > 1)
            {
                await Task.Delay(1000);
                time--;
                // if(time % 2 == 0)
                //     timeUpdate?.Invoke(this, playerIds[currentTurnIndex], time);
                if (time == 0)
                {
                    //Change the turn and 
                    UpdateNextTurnIndex();
                    string nextId = playerIds[currentTurnIndex];
                    callbackNextTurn?.Invoke(this, nextId);
                    time = Constants.TurnTime;
                }
            }
        }

        private void UpdateNextTurnIndex()
        {
            if (currentTurnIndex < Players.Count - 1)
                currentTurnIndex++;
            else 
                currentTurnIndex = 0;
        }
    }
}