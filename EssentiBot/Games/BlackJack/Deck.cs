using System;
using System.Collections.Generic;
using System.Linq;

namespace EssentiBot.Games.BlackJack
{
    class Deck
    {
        private int _deckCount;
        public List<Card> deck = new List<Card>();
        Random rnd = new Random();

        public void CreateDeck(int DeckCount)
        {
            //removes the remaining Contents of the deck, once a new deck is needed
            deck.Clear();

            _deckCount = DeckCount;

            //Itterate for the given amount of Decks
            for (int i = 0; i < _deckCount; i++)
            {
                //Itterate over Card Ranks
                for (int x = 0; x < 13; x++)
                {
                    //Itterate over Card Suites
                    for (int z = 0; z < 4; z++)
                    {
                        deck.Add(new Card(x, z));
                    }
                }
            }
        }

        public void Shuffle(int TimesToShuffle)
        {
            for (int i = 0; i < TimesToShuffle; i++)
            {
                //Fisher Yates Shuffle
                for (int cardIndex1 = deck.Count() - 1; cardIndex1 > 0; cardIndex1--)
                {
                    int cardIndex2 = rnd.Next(cardIndex1 + 1);
                    var tmp = deck[cardIndex1];
                    deck[cardIndex1] = deck[cardIndex2];
                    deck[cardIndex2] = tmp;
                }
            }
        }
    }
}
