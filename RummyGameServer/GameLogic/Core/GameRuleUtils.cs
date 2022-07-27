using System.Collections.Generic;
using System.Linq;
using CommonData.Enums;

namespace RummyGameServer
{
    public static class GameRuleUtils
    {
        /// <summary>
        /// Rule For Set
        /// 1: All card has to be from different suit
        /// 2: All card should have same face
        /// 3: 2 same face with one Joker
        /// 4: Number of cards 3 or 4
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsASet(this List<Card> list)
        {
            int jokerCount = list.Count(c => c.IsJoker);
            HashSet<Suits> suitsSet = new HashSet<Suits>();
            
            if (list.Count > 2 && list.Count < 5 && (jokerCount == 0 || jokerCount == 1))
            {
                var faceId = list[0].FaceId;
                foreach (var card in list)
                {
                    if (suitsSet.Add(card.Suit)) { }
                    else 
                        return false;

                    if (!card.IsJoker && card.FaceId != faceId)
                        return false;
                }
            }
            else 
                return false;

            return true;
        }
        
        /// <summary>
        /// Rule For Pure Sequence
        /// 1: All card has to be from same suit
        /// 2: All card should have incremental face value
        /// 3: No Joker allowed
        /// 4: Number of cards 3 or 4
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsAPureSequence(this List<Card> list)
        {
            int jokerCount = list.Count(c => c.IsJoker);
            if (jokerCount > 0)
                return false;
            
            if (list.Count > 2 && list.Count < 5)
            {
                var currentSequence = list[0].Sequence;
                var suit = list[0].Suit;
                
                foreach (var card in list)
                {
                    if (card.Suit == suit && card.Sequence == currentSequence)
                    {
                        currentSequence += 1;
                    }
                    else 
                        return false;
                }
            }
            else 
                return false;

            return true;
        }
        
        /// <summary>
        /// Rule For Impure Sequence
        /// 1: All card has to be from same suit
        /// 2: All card should have incremental face value
        /// 3: Joker will act as a required card in place
        /// 4: Number of cards 3 or 4
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsAImpureSequence(this List<Card> list)
        {
            int jokerCount = list.Count(c => c.IsJoker);
            if (jokerCount > 1)
                return false;
            
            if (list.Count > 2 && list.Count < 5)
            {
                var currentSequence = list[0].Sequence;
                var suit = list[0].Suit;
                
                foreach (var card in list)
                {
                    if (card.IsJoker || card.Suit == suit && card.Sequence == currentSequence)
                    {
                        currentSequence += 1;
                    }
                    else 
                        return false;
                }
            }
            else 
                return false;

            return true;
        }
        
        /// <summary>
        /// Calculate the score and return
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int CalculateScore(this List<Card> list)
        {
            return list.Sum(c => c.FaceValue);
        }
        
    }
}