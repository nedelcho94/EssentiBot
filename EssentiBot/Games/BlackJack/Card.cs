namespace EssentiBot.Games.BlackJack
{
    enum Suit
    {
        Spades,
        Hearths,
        Clubs,
        Diamonds,
    };

    enum Rank
    {
        Ace,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Quen,
        King,
    };


    class Card
    {
        public Rank rank;
        public Suit suit;

        public Card(int Rank, int Suit)
        {
            rank = (Rank)Rank;
            suit = (Suit)Suit;
        }
    }
}
