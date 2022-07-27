using System;
using CommonData.Enums;

namespace RummyGameServer
{
    public class Card
    {
        public string Id;
        public int FaceValue;
        public Suits Suit;
        public string FaceId;
        public int Sequence;
        
        public bool IsJoker { get; private set; }
        public Card(string id, int faceValue, int sequence, string faceId, Suits suits)
        {
            Id = id;
            FaceValue = faceValue;
            FaceId = faceId;
            Suit = suits;
            Sequence = sequence;
            IsJoker = false;
        }

        public void SetJoker()
        {
            IsJoker = true;
        }

        public override string ToString()
        {
            return $"{GetSuitSymbol()} {FaceId}";
        }

        private string GetSuitSymbol()
        {
            switch (Suit)
            {
                case Suits.Hearts:
                    return "♥";
                case Suits.Clubs:
                    return "♣";
                case Suits.Spades:
                    return "♠";
                case Suits.Diamonds:
                    return "♦";
                case Suits.Joker:
                    return "J";
            }
            return "";
        }
    }
}