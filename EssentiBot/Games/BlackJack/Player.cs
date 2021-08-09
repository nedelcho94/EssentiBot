using System.Collections.Generic;

namespace EssentiBot.Games.BlackJack
{
    class Player
    {
        public int handTotal = 0;
        public string name;
        public bool draw = true;
        public bool Busted = false;
        public List<Card> hand = new List<Card>();

        public Player(string Name)
        {
            name = Name;
        }

        public void GetCards(List<Card> Deck)
        {
            hand.Add(Deck[0]);
            Deck.RemoveAt(0);
            _CalcHandTotal();
        }

        private void _CalcHandTotal()
        {
            handTotal = 0;
            int aceCount = 0;
            foreach (var card in hand)
            {
                if (card.rank == 0)
                {
                    aceCount++;
                }
                else if ((int)card.rank < 10)
                {
                    handTotal += (int)card.rank + 1;
                }
                else
                {
                    handTotal += 10;
                }

            }

            if (aceCount > 0)
            {
                if (handTotal <= (11 - aceCount))
                {
                    handTotal += (aceCount - 1) + 11;
                }
                else
                {
                    handTotal += aceCount;
                }
            }

        }
    }
}
