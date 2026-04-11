using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;

namespace BridgePlayer.Tests;

public class HandTests
{
    private static Hand MakeFullHand()
    {
        var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
        var values = Enum.GetValues<Value>();
        var cards = new Card[13];
        for (var i = 0; i < 13; i++)
            cards[i] = new Card(suits[i % 4], values[i % values.Length]);
        return new Hand(new ReadOnlyMemory<Card>(cards));
    }

    [Fact]
    public void Hand_HasThirteenCards()
    {
        Assert.Equal(13, MakeFullHand().Cards.Length);
    }

    [Fact]
    public void Hand_StoresProvidedCards()
    {
        var cards = new Card[13];
        Array.Fill(cards, new Card(Suit.Spades, Value.Ace));

        var hand = new Hand(new ReadOnlyMemory<Card>(cards));

        for (var i = 0; i < 13; i++)
            Assert.Equal(new Card(Suit.Spades, Value.Ace), hand.Cards.Span[i]);
    }
}
