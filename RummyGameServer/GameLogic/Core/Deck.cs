using System;
using System.Collections.Generic;
using System.Linq;
using CommonData.Data.Response;
using CommonData.Enums;

namespace RummyGameServer
{
    public class Deck
    {
        private readonly List<Card> _cards;
        private readonly DeckType _deckType;
        public Deck(DeckType deckType)
        {
            _deckType = deckType;
            _cards = deckType == DeckType.OpenDeck ? new List<Card>() : DeckFactory.Create();
        }
        internal void Shuffle()
        {
            _cards.Shuffle();
        }

        internal Card Draw()
        {
            var card = _cards[^1];
            _cards.Remove(card);
            return card;
        }

        internal string GetCard()
        {
            return _cards.Count == 0 ? String.Empty : _cards[^1].Id;
        }
        
        internal CardsResponseData ToResponseData()
        {
            var response = new CardsResponseData();
            var list = _cards.Select(c => c.Id).ToList();
            response.CardIds = list.Count == 0 ? new List<string>() : list;
            return response;
        }

        internal Card GetRandom()
        {
            List<Card> list = _cards.FindAll(c => c.Id != "999" || c.Id != "998").ToList();
            Random r = new Random();
            return list[r.Next(list.Count)];
        }

        public Card GetAndRemove(string payload)
        {
            var card = _cards.Find(c => c.Id == payload);
            _cards.Remove(card);
            return card;
        }

        public void AddCard(Card card)
        {
            _cards.Add(card);
        }
    }
}