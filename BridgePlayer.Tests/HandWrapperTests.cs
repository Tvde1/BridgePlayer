using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;
using BridgePlayer.Models.Wrapper;

namespace BridgePlayer.Tests;

public class HandWrapperTests
{
    /// <summary>
    /// Builds a 13-card hand from the provided list, padding remaining slots
    /// with 2-of-clubs (0 HCP) if fewer than 13 cards are given.
    /// </summary>
    private static Hand BuildHand(params Card[] cards)
    {
        var full = new Card[13];
        for (var i = 0; i < cards.Length && i < 13; i++)
            full[i] = cards[i];
        for (var i = cards.Length; i < 13; i++)
            full[i] = new Card(Suit.Clubs, Value.Two);
        return new Hand(new ReadOnlyMemory<Card>(full));
    }

    /// <summary>Creates a 13-card hand with exactly the specified suit distribution (all low cards, no HCP).</summary>
    private static Hand BuildDistribution(byte spades, byte hearts, byte diamonds, byte clubs)
    {
        var cards = new Card[13];
        var idx = 0;
        for (var i = 0; i < spades; i++)   cards[idx++] = new Card(Suit.Spades, Value.Two);
        for (var i = 0; i < hearts; i++)   cards[idx++] = new Card(Suit.Hearts, Value.Two);
        for (var i = 0; i < diamonds; i++) cards[idx++] = new Card(Suit.Diamonds, Value.Two);
        for (var i = 0; i < clubs; i++)    cards[idx++] = new Card(Suit.Clubs, Value.Two);
        return new Hand(new ReadOnlyMemory<Card>(cards));
    }

    [Fact]
    public void HandWrapper_Points_IsZeroForAllLowCards()
    {
        var hand = BuildHand();
        var wrapper = new HandWrapper(hand);
        Assert.Equal(0, wrapper.Points);
    }

    [Fact]
    public void HandWrapper_Points_CountsJackAsOne()
    {
        var hand = BuildHand(new Card(Suit.Spades, Value.Jack));
        var wrapper = new HandWrapper(hand);
        Assert.Equal(1, wrapper.Points);
    }

    [Fact]
    public void HandWrapper_Points_CountsQueenAsTwo()
    {
        var hand = BuildHand(new Card(Suit.Hearts, Value.Queen));
        var wrapper = new HandWrapper(hand);
        Assert.Equal(2, wrapper.Points);
    }

    [Fact]
    public void HandWrapper_Points_CountsKingAsThree()
    {
        var hand = BuildHand(new Card(Suit.Diamonds, Value.King));
        var wrapper = new HandWrapper(hand);
        Assert.Equal(3, wrapper.Points);
    }

    [Fact]
    public void HandWrapper_Points_CountsAceAsFour()
    {
        var hand = BuildHand(new Card(Suit.Clubs, Value.Ace));
        var wrapper = new HandWrapper(hand);
        Assert.Equal(4, wrapper.Points);
    }

    [Fact]
    public void HandWrapper_Points_SumsAllHonours()
    {
        // A=4, K=3, Q=2, J=1 => 10 HCP total
        var hand = BuildHand(
            new Card(Suit.Spades, Value.Ace),
            new Card(Suit.Hearts, Value.King),
            new Card(Suit.Diamonds, Value.Queen),
            new Card(Suit.Clubs, Value.Jack));
        var wrapper = new HandWrapper(hand);
        Assert.Equal(10, wrapper.Points);
    }

    [Fact]
    public void HandWrapper_SuitCounts_AreCorrect()
    {
        // 4 spades, 3 hearts, 3 diamonds, 3 clubs
        var hand = BuildDistribution(4, 3, 3, 3);
        var wrapper = new HandWrapper(hand);
        Assert.Equal(4, wrapper.SuitCounts.Spades);
        Assert.Equal(3, wrapper.SuitCounts.Hearts);
        Assert.Equal(3, wrapper.SuitCounts.Diamonds);
        Assert.Equal(3, wrapper.SuitCounts.Clubs);
    }

    [Fact]
    public void HandWrapper_IsOneNoTrump_ReturnsTrueFor15HcpBalancedNoMajor()
    {
        // 15 HCP, 5-3-3-2 balanced (Diamonds=5, Hearts=3, Clubs=3, Spades=2), no 4-card major
        // A+K in spades=7, Q+J in hearts=3, A in diamonds=4, J in clubs=1 => total 15
        var hand = new Hand(new ReadOnlyMemory<Card>(new Card[]
        {
            new(Suit.Spades,   Value.Ace),
            new(Suit.Spades,   Value.King),
            new(Suit.Hearts,   Value.Queen),
            new(Suit.Hearts,   Value.Jack),
            new(Suit.Hearts,   Value.Two),
            new(Suit.Diamonds, Value.Ace),
            new(Suit.Diamonds, Value.Two),
            new(Suit.Diamonds, Value.Three),
            new(Suit.Diamonds, Value.Four),
            new(Suit.Diamonds, Value.Five),
            new(Suit.Clubs,    Value.Jack),
            new(Suit.Clubs,    Value.Two),
            new(Suit.Clubs,    Value.Three),
        }));
        // Spades=2, Hearts=3, Diamonds=5, Clubs=3 — balanced, no 4-card major, 15 HCP
        var wrapper = new HandWrapper(hand);
        Assert.Equal(15, wrapper.Points);
        Assert.True(wrapper.IsOneNoTrump());
    }

    [Fact]
    public void HandWrapper_IsOneNoTrump_ReturnsTrueFor17HcpBalancedNoMajor()
    {
        // 17 HCP, 4-3-3-3 balanced, no 4-card major (Diamonds=4)
        // A+K+Q in diamonds=9, A+K in clubs=7, J in hearts=1 => 17
        var hand = new Hand(new ReadOnlyMemory<Card>(new Card[]
        {
            new(Suit.Spades,   Value.Two),
            new(Suit.Spades,   Value.Three),
            new(Suit.Spades,   Value.Four),
            new(Suit.Hearts,   Value.Jack),
            new(Suit.Hearts,   Value.Two),
            new(Suit.Hearts,   Value.Three),
            new(Suit.Diamonds, Value.Ace),
            new(Suit.Diamonds, Value.King),
            new(Suit.Diamonds, Value.Queen),
            new(Suit.Diamonds, Value.Two),
            new(Suit.Clubs,    Value.Ace),
            new(Suit.Clubs,    Value.King),
            new(Suit.Clubs,    Value.Two),
        }));
        // Spades=3, Hearts=3, Diamonds=4, Clubs=3 — balanced, no 4-card major, 17 HCP
        var wrapper = new HandWrapper(hand);
        Assert.Equal(17, wrapper.Points);
        Assert.True(wrapper.IsOneNoTrump());
    }

    [Fact]
    public void HandWrapper_IsOneNoTrump_ReturnsFalseWhenPointsBelowRange()
    {
        // 14 HCP, balanced, no 4-card major
        var hand = new Hand(new ReadOnlyMemory<Card>(new Card[]
        {
            new(Suit.Spades,   Value.Ace),
            new(Suit.Spades,   Value.King),
            new(Suit.Hearts,   Value.Queen),
            new(Suit.Hearts,   Value.Jack),
            new(Suit.Hearts,   Value.Two),
            new(Suit.Diamonds, Value.King),
            new(Suit.Diamonds, Value.Two),
            new(Suit.Diamonds, Value.Three),
            new(Suit.Diamonds, Value.Four),
            new(Suit.Diamonds, Value.Five),
            new(Suit.Clubs,    Value.Jack),
            new(Suit.Clubs,    Value.Two),
            new(Suit.Clubs,    Value.Three),
        }));
        // Spades=2, Hearts=3, Diamonds=5, Clubs=3 — balanced, no 4-card major, 14 HCP
        var wrapper = new HandWrapper(hand);
        Assert.Equal(14, wrapper.Points);
        Assert.False(wrapper.IsOneNoTrump());
    }

    [Fact]
    public void HandWrapper_IsOneNoTrump_ReturnsFalseWhenPointsAboveRange()
    {
        // 18 HCP, balanced, no 4-card major
        var hand = new Hand(new ReadOnlyMemory<Card>(new Card[]
        {
            new(Suit.Spades,   Value.Ace),
            new(Suit.Spades,   Value.King),
            new(Suit.Hearts,   Value.Ace),
            new(Suit.Hearts,   Value.Queen),
            new(Suit.Hearts,   Value.Two),
            new(Suit.Diamonds, Value.Ace),
            new(Suit.Diamonds, Value.Two),
            new(Suit.Diamonds, Value.Three),
            new(Suit.Diamonds, Value.Four),
            new(Suit.Diamonds, Value.Five),
            new(Suit.Clubs,    Value.Jack),
            new(Suit.Clubs,    Value.Two),
            new(Suit.Clubs,    Value.Three),
        }));
        // Spades=2, Hearts=3, Diamonds=5, Clubs=3 — balanced, no 4-card major, 18 HCP
        var wrapper = new HandWrapper(hand);
        Assert.Equal(18, wrapper.Points);
        Assert.False(wrapper.IsOneNoTrump());
    }

    [Fact]
    public void HandWrapper_IsOneNoTrump_ReturnsFalseWhenHandIsUnbalanced()
    {
        // 16 HCP, but 6-3-2-2 distribution (two doubletons, not balanced), no 4-card major
        var hand = new Hand(new ReadOnlyMemory<Card>(new Card[]
        {
            new(Suit.Spades,   Value.Ace),
            new(Suit.Spades,   Value.King),
            new(Suit.Spades,   Value.Two),
            new(Suit.Hearts,   Value.Two),
            new(Suit.Hearts,   Value.Three),
            new(Suit.Diamonds, Value.Ace),
            new(Suit.Diamonds, Value.King),
            new(Suit.Diamonds, Value.Two),
            new(Suit.Diamonds, Value.Three),
            new(Suit.Diamonds, Value.Four),
            new(Suit.Diamonds, Value.Five),
            new(Suit.Clubs,    Value.Queen),
            new(Suit.Clubs,    Value.Two),
        }));
        // Spades=3, Hearts=2, Diamonds=6, Clubs=2 — not balanced (two doubletons), 16 HCP
        var wrapper = new HandWrapper(hand);
        Assert.Equal(16, wrapper.Points);
        Assert.False(wrapper.IsOneNoTrump());
    }

    [Fact]
    public void HandWrapper_IsOneNoTrump_ReturnsFalseWhenFourCardMajorPresent()
    {
        // 15 HCP, balanced 4-3-3-3 but 4-card spade suit
        var hand = new Hand(new ReadOnlyMemory<Card>(new Card[]
        {
            new(Suit.Spades,   Value.Ace),
            new(Suit.Spades,   Value.King),
            new(Suit.Spades,   Value.Queen),
            new(Suit.Spades,   Value.Two),
            new(Suit.Hearts,   Value.Jack),
            new(Suit.Hearts,   Value.Two),
            new(Suit.Hearts,   Value.Three),
            new(Suit.Diamonds, Value.Ace),
            new(Suit.Diamonds, Value.Two),
            new(Suit.Diamonds, Value.Three),
            new(Suit.Clubs,    Value.Jack),
            new(Suit.Clubs,    Value.Two),
            new(Suit.Clubs,    Value.Three),
        }));
        // Spades=4, Hearts=3, Diamonds=3, Clubs=3 — balanced but 4-card major present, 15 HCP
        var wrapper = new HandWrapper(hand);
        Assert.Equal(15, wrapper.Points);
        Assert.False(wrapper.IsOneNoTrump());
    }
}
