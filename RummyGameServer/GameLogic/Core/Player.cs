using System.Collections.Generic;
using CommonData.Enums;
using RummyGameServer.GameLogic;

namespace RummyGameServer
{
    public class Player
    {
        public string Id { get; private set; }
        public string ConnectionId { get; private set; }
        public PlayerActionType ActionType { get; set; }
        public ConnectionStatus Connection { get; set; }

        private Hand _hand;
        public List<Round> Rounds;
        public Card FirstCard { get; private set; }

        public Player(string id, string connectionId)
        {
            Id = id;
            ConnectionId = connectionId;
            Rounds = new List<Round>();
            Connection = ConnectionStatus.Connected;
            _hand = new Hand();
        }

        public void AddHandCard(Card card)
        {
            _hand.AddCard(card);
        }

        public Hand GetHand() => _hand;

        public void PlayerStatus(ConnectionStatus status)
        {
            Connection = status;
        }

        public void SetFirstCard(Card firstCard)
        {
            FirstCard = firstCard;
        }

        public Card GetAndRemoveCard(string cardId)
        {
            var card = _hand.Cards.Find(c => c.Id == cardId);
            _hand.Cards.Remove(card);
            return card;
        }
    }
}