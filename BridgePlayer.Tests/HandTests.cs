using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;

namespace BridgePlayer.Tests;

public class HandTests
{
    private static Card[] MakeCards(int count, Suit suit = Suit.Spades, Value startValue = Value.Two)
    {
        var cards = new Card[count];
        var values = Enum.GetValues<Value>();
        for (var i = 0; i < count; i++)
            cards[i] = new Card(suit, values[i % values.Length]);
        return cards;
    }

    private static Hand MakeFullHand()
    {
        var cards = new Card[13];
        var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
        var values = Enum.GetValues<Value>();
        for (var i = 0; i < 13; i++)
            cards[i] = new Card(suits[i % 4], values[i % values.Length]);
        return new Hand(new ReadOnlyMemory<Card>(cards));
    }

    [Fact]
    public void Hand_HasThirteenCards()
    {
        var hand = MakeFullHand();
        Assert.Equal(13, hand.Cards.Length);
    }

    [Fact]
    public void Hand_StoresProvidedCards()
    {
        var cards = new Card[13];
        for (var i = 0; i < 13; i++)
            cards[i] = new Card(Suit.Spades, Value.Ace);

        var hand = new Hand(new ReadOnlyMemory<Card>(cards));

        for (var i = 0; i < 13; i++)
            Assert.Equal(new Card(Suit.Spades, Value.Ace), hand.Cards.Span[i]);
    }

    [Fact]
    public void Hand_Equality_SameCards()
    {
        var cards = new Card[13];
        for (var i = 0; i < 13; i++)
            cards[i] = new Card(Suit.Hearts, Value.King);

        var memory = new ReadOnlyMemory<Card>(cards);
        var hand1 = new Hand(memory);
        var hand2 = new Hand(memory);

        Assert.Equal(hand1, hand2);
    }

    [Fact]
    public void Hand_Inequality_DifferentCards()
    {
        var cards1 = new Card[13];
        var cards2 = new Card[13];
        for (var i = 0; i < 13; i++)
        {
            cards1[i] = new Card(Suit.Spades, Value.Ace);
            cards2[i] = new Card(Suit.Hearts, Value.Two);
        }

        var hand1 = new Hand(new ReadOnlyMemory<Card>(cards1));
        var hand2 = new Hand(new ReadOnlyMemory<Card>(cards2));

        Assert.NotEqual(hand1, hand2);
    }
}
