using System;
using System.Collections.Generic;
using CommonData.Enums;

namespace RummyGameServer
{
    public class DeckFactory
    {
        private static List<Card> cards;
        private static string[] faceIds = {"A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"};
        private static int[] faceValues = {10, 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10};

        public static List<Card> Create()
        {
            cards = new List<Card>();

            for (int i = 0; i < 13; i++)
            {
                string cId = (i + 1) + "" + (int) Suits.Clubs;
                string dId = (i + 1) + "" + (int) Suits.Diamonds;
                string hId = (i + 1) + "" + (int) Suits.Hearts;
                string sId = (i + 1) + "" + (int) Suits.Clubs;

                Card club = new Card(cId, faceValues[i],i, faceIds[i], Suits.Clubs);
                Card diamond = new Card(dId, faceValues[i], i,faceIds[i], Suits.Diamonds);
                Card heart = new Card(hId, faceValues[i], i,faceIds[i], Suits.Hearts);
                Card spade = new Card(sId, faceValues[i], i,faceIds[i], Suits.Spades);
                cards.AddRange(new[] {club, spade, diamond, heart});
            }

            //Include Joker to the deck
            Card joker = new Card("999", 0, -1,"Joker", Suits.Joker);
            Card joker1 = new Card("998", 0, -1,"Joker", Suits.Joker);

            cards.AddRange(new[] {joker, joker1});
            
            return cards;
        }
    }
}