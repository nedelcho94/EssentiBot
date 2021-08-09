using System;

namespace EssentiBot.Games.BlackJack
{
    class Game
    {
        public bool moreGames = true;
        //todo fix rest of game for discord

        public void PlayGame()
        {
            string choice = "";
            Console.Clear();
            Player player1 = new Player("Player");
            Player dealer = new Player("Dealer");

            Deck gameDeck = new Deck();

            gameDeck.CreateDeck(5);
            gameDeck.Shuffle(10);


            _GetStartingHand(player1, gameDeck);
            _GetStartingHand(dealer, gameDeck);

            _DisplayHand(dealer);
            _DisplayHand(player1);

            _CheckHand(player1);
            _CheckHand(dealer);

            if (!(player1.Busted || dealer.Busted))
            {
                _DrawPhase(player1, dealer, gameDeck);
            }

            _DecideWinner(player1, dealer);

            Console.Write("\n Play again? (Y/N): ");
            choice = Console.ReadLine();



            moreGames = choice.ToUpper() != "N";




        }

        //this doesnt seem to be the best way to decide the winner, works for now.
        private void _DecideWinner(Player Player, Player Dealer)
        {
            var winner = "";
            if (Player.handTotal == 21 && Dealer.handTotal == 21)
            {
                //Console.WriteLine("\n It's a tie!");
                winner = "It's a tie!";
            }
            else if (Player.handTotal == 21 && Dealer.handTotal != 21)
            {
                //Console.WriteLine($"\n {Player.name} wins!");
                winner = $"{Player.name} wins!";
            }
            else if (Player.handTotal != 21 && Dealer.handTotal == 21)
            {
                //Console.WriteLine($"\n {Dealer.name} wins!");
                winner = $"{Dealer.name} wins!";
            }
            else if (Player.handTotal > 21 && Dealer.handTotal <= 21)
            {
                //Console.WriteLine($"\n {Dealer.name} wins!");
                winner = $"{Dealer.name} wins!";
            }
            else if (Player.handTotal <= 21 && Dealer.handTotal > 21)
            {
                //Console.WriteLine($"\n {Player.name} wins!");
                winner = $"{Player.name} wins!";
            }
            else if (Player.handTotal > 21 && Dealer.handTotal > 21)
            {
                //Console.WriteLine($"\n Both Busted ");
                winner = $"Both Busted";
            }
            else if (Player.handTotal > Dealer.handTotal)
            {
                //Console.WriteLine($"\n {Player.name} wins!");
                winner = $"{Player.name} wins!";
            }
            else if (Player.handTotal < Dealer.handTotal)
            {
                //Console.WriteLine($"\n {Dealer.name} wins!");
                winner = $"{Dealer.name} wins!";
            }
            else if (Player.handTotal == Dealer.handTotal)
            {
                //Console.WriteLine("\n It's a tie!");
                winner = $"It's a tie!";
            }
        }

        private void _DrawPhase(Player Player, Player Dealer, Deck Deck)
        {

            while (Player.draw)
            {

                Console.Write("\n Would you like to (H)it or (S)tand: ");
                string choice = Console.ReadLine();

                Player.draw = choice.ToUpper() != "S";
                if (Player.draw)
                {
                    _CardDraw(Player, Deck);
                    _DisplayBoard(Player, Dealer);
                    _CheckHand(Player);
                }

            }

            if (!(Player.Busted))
            {
                Dealer.draw = Dealer.handTotal < 16;
                while (Dealer.draw)
                {

                    _CardDraw(Dealer, Deck);
                    _DisplayBoard(Player, Dealer);
                    _CheckHand(Dealer);

                    Dealer.draw = Dealer.handTotal < 16;
                }
                Console.Clear();
                _DisplayBoard(Player, Dealer);
                _CheckHand(Player);
                _CheckHand(Dealer);
            }
        }

        private void _DisplayBoard(Player Player, Player Dealer)
        {
            _DisplayHand(Dealer);
            _DisplayHand(Player);
        }

        private void _DisplayHand(Player Player)
        {
            Console.WriteLine("\n***********************\n" +
                $"*      {Player.name}Hand     *\n" +
                "***********************\n");

            foreach (var card in Player.hand)
            {
                Console.WriteLine($" - {card.rank} of {card.suit}");
            }
            Console.WriteLine($"\n Total: {Player.handTotal}");
        }

        private void _GetStartingHand(Player Player, Deck Deck)
        {
            Player.GetCards(Deck.deck);
            Player.GetCards(Deck.deck);
        }

        private void _CardDraw(Player Player, Deck Deck)
        {
            Console.Clear();
            Player.GetCards(Deck.deck);
        }

        private void _CheckHand(Player Player)
        {
            if (Player.handTotal > 21)
            {
                Console.WriteLine($"\n {Player.name} Busted!");
                Player.draw = false;
                Player.Busted = true;
            }
            else if (Player.handTotal == 21)
            {
                Console.WriteLine($"\n {Player.name} got a Blackjack!");
                Player.draw = false;
            }
        }
    }
}
