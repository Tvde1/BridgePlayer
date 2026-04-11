using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;

namespace BridgePlayer.Tests;

public class CardTests
{
    [Fact]
    public void Card_StoresSuit()
    {
        var card = new Card(Suit.Hearts, Value.Two);
        Assert.Equal(Suit.Hearts, card.Suit);
    }

    [Fact]
    public void Card_StoresValue()
    {
        var card = new Card(Suit.Spades, Value.Ace);
        Assert.Equal(Value.Ace, card.Value);
    }

    [Theory]
    [InlineData(Value.Two, 0)]
    [InlineData(Value.Three, 0)]
    [InlineData(Value.Four, 0)]
    [InlineData(Value.Five, 0)]
    [InlineData(Value.Six, 0)]
    [InlineData(Value.Seven, 0)]
    [InlineData(Value.Eight, 0)]
    [InlineData(Value.Nine, 0)]
    [InlineData(Value.Ten, 0)]
    [InlineData(Value.Jack, 1)]
    [InlineData(Value.Queen, 2)]
    [InlineData(Value.King, 3)]
    [InlineData(Value.Ace, 4)]
    public void Card_HCP_IsCorrectForEachValue(Value value, int expectedHcp)
    {
        var card = new Card(Suit.Clubs, value);
        Assert.Equal(expectedHcp, card.HCP);
    }

    [Fact]
    public void Card_HCP_IsIndependentOfSuit()
    {
        Assert.Equal(new Card(Suit.Spades, Value.Ace).HCP, new Card(Suit.Hearts, Value.Ace).HCP);
        Assert.Equal(new Card(Suit.Diamonds, Value.King).HCP, new Card(Suit.Clubs, Value.King).HCP);
    }

    [Fact]
    public void Card_Equality_SameSuitAndValue()
    {
        var a = new Card(Suit.Spades, Value.Ace);
        var b = new Card(Suit.Spades, Value.Ace);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Card_Inequality_DifferentSuit()
    {
        var a = new Card(Suit.Spades, Value.Ace);
        var b = new Card(Suit.Hearts, Value.Ace);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Card_Inequality_DifferentValue()
    {
        var a = new Card(Suit.Spades, Value.Ace);
        var b = new Card(Suit.Spades, Value.King);
        Assert.NotEqual(a, b);
    }
}
