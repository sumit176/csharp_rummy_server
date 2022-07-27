using System;
using System.Collections.Generic;
using System.Linq;
using CommonData.Data.Response;
using CommonData.Enums;

namespace RummyGameServer
{
    public class Hand
    {
       public List<Card> Cards{ get; private set; }
       public List<Group> CardGroups { get; private set; }

       public Hand()
       {
          Cards = new List<Card>();
       }
       
       internal CardsResponseData ToResponseData()
       {
          var response = new CardsResponseData();
          var list = Cards.Select(c => c.Id).ToList();
          response.CardIds = list.Count == 0 ? new List<string>() : list;
          return response;
       }

        public void Clear()
        {
           Cards.Clear();
        }

        public void AddCard(Card c)
        {
           Cards.Add(c);
        }
        
        public void SortBySuit()
        {
           //add cards to groups and let client know the sorting
           Dictionary<Suits, List<Card>> groupBySuits = new Dictionary<Suits, List<Card>>();
           CardGroups = new List<Group>();
           
           //Order cards by sequence
           Cards.OrderBy(c => c.Sequence);
           
           //Sort card by Suit type
           foreach (var card in Cards)
           {
              if (groupBySuits.ContainsKey(card.Suit))
              {
                 groupBySuits[card.Suit].Add(card);
              }
              else
              {
                 groupBySuits.Add(card.Suit, new List<Card>{card});
              }
           }
           
           //Create Group object and add data to CardGroups
           foreach (var groupBySuit in groupBySuits)
           {
              Group g = new Group
              {
                 Cards = groupBySuit.Value
              };
              CardGroups.Add(g);
           }
           
           Console.WriteLine($"Total Groups {groupBySuits.Count}");
        }

        public override string ToString()
        {
           return string.Join('|', Cards);
        }
    }
}