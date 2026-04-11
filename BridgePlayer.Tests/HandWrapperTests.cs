using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;
using BridgePlayer.Models.Wrapper;

namespace BridgePlayer.Tests;

public class HandWrapperTests
{
    private static Hand MakeHand(byte spades, byte hearts, byte diamonds, byte clubs)
    {
        var cards = new Card[13];
        var idx = 0;
        for (var i = 0; i < spades; i++)   cards[idx++] = new Card(Suit.Spades,   Value.Two);
        for (var i = 0; i < hearts; i++)   cards[idx++] = new Card(Suit.Hearts,   Value.Two);
        for (var i = 0; i < diamonds; i++) cards[idx++] = new Card(Suit.Diamonds, Value.Two);
        for (var i = 0; i < clubs; i++)    cards[idx++] = new Card(Suit.Clubs,    Value.Two);
        return new Hand(new ReadOnlyMemory<Card>(cards));
    }

    [Fact]
    public void HandWrapper_Points_SumsAllHonours()
    {
        // A=4, K=3, Q=2, J=1 => 10 HCP total, padded with low cards to 13
        var cards = new Card[]
        {
            new(Suit.Spades,   Value.Ace),
            new(Suit.Hearts,   Value.King),
            new(Suit.Diamonds, Value.Queen),
            new(Suit.Clubs,    Value.Jack),
        }.Concat(Enumerable.Repeat(new Card(Suit.Clubs, Value.Two), 9)).ToArray();

        Assert.Equal(10, new HandWrapper(new Hand(new ReadOnlyMemory<Card>(cards))).Points);
    }

    [Fact]
    public void HandWrapper_SuitCounts_AreCorrect()
    {
        var wrapper = new HandWrapper(MakeHand(4, 3, 3, 3));
        Assert.Equal(4, wrapper.SuitCounts.Spades);
        Assert.Equal(3, wrapper.SuitCounts.Hearts);
        Assert.Equal(3, wrapper.SuitCounts.Diamonds);
        Assert.Equal(3, wrapper.SuitCounts.Clubs);
    }
}
