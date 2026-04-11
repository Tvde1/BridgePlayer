using BridgePlayer.Models.Enums;
using BridgePlayer.Models.Wrapper;

namespace BridgePlayer.Tests;

public class SuitCountsTests
{
    [Theory]
    [InlineData(5, 4, 3, 1, Suit.Spades, 5)]
    [InlineData(5, 4, 3, 1, Suit.Hearts, 4)]
    [InlineData(5, 4, 3, 1, Suit.Diamonds, 3)]
    [InlineData(5, 4, 3, 1, Suit.Clubs, 1)]
    public void SuitCounts_Indexer_ReturnsCorrectCount(byte sp, byte h, byte d, byte c, Suit suit, int expected)
    {
        Assert.Equal(expected, new SuitCounts(sp, h, d, c)[suit]);
    }

    [Fact]
    public void SuitCounts_Indexer_ThrowsForInvalidSuit()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SuitCounts(4, 3, 3, 3)[(Suit)99]);
    }

    [Theory]
    [InlineData(4, 3, 3, 3, 4, true, Suit.Spades)]   // spades has 4
    [InlineData(3, 4, 3, 3, 4, true, Suit.Hearts)]    // hearts has 4, not spades
    [InlineData(4, 4, 3, 2, 4, true, Suit.Spades)]    // both have 4; higher-ranked suit wins
    [InlineData(4, 4, 4, 1, 3, false, Suit.Spades)]   // no suit has exactly 3
    public void SuitCounts_HasCount_ReturnsMatchingSuit(byte sp, byte h, byte d, byte c, int count, bool expectedHit, Suit expectedSuit)
    {
        var found = new SuitCounts(sp, h, d, c).HasCount(count, out var suit);
        Assert.Equal(expectedHit, found);
        if (expectedHit) Assert.Equal(expectedSuit, suit);
    }

    [Theory]
    [InlineData(5, 3, 3, 2, 5, true, Suit.Spades)]    // spades meets minimum
    [InlineData(3, 5, 3, 2, 5, true, Suit.Hearts)]    // hearts meets minimum, not spades
    [InlineData(4, 4, 2, 3, 4, true, Suit.Spades)]    // both ≥ 4; higher-ranked suit wins
    [InlineData(4, 3, 3, 3, 5, false, Suit.Spades)]   // no suit has ≥ 5
    public void SuitCounts_HasMinimumCount_ReturnsMatchingSuit(byte sp, byte h, byte d, byte c, int count, bool expectedHit, Suit expectedSuit)
    {
        var found = new SuitCounts(sp, h, d, c).HasMinimumCount(count, out var suit);
        Assert.Equal(expectedHit, found);
        if (expectedHit) Assert.Equal(expectedSuit, suit);
    }

    [Theory]
    [InlineData(3, 5, 2, 3,  5, 3, 3, 2)]   // already-sorted input
    [InlineData(4, 4, 3, 2,  4, 4, 3, 2)]   // two suits tied
    [InlineData(1, 5, 4, 3,  5, 4, 3, 1)]   // ascending input
    public void SuitCounts_Deconstruct_OrdersLongestToShortest(
        byte sp, byte h, byte d, byte c,
        byte longest, byte secondLongest, byte secondShortest, byte shortest)
    {
        new SuitCounts(sp, h, d, c).Deconstruct(
            out var resultLongest, out var resultSecondLongest,
            out var resultSecondShortest, out var resultShortest);
        Assert.Equal(longest, resultLongest);
        Assert.Equal(secondLongest, resultSecondLongest);
        Assert.Equal(secondShortest, resultSecondShortest);
        Assert.Equal(shortest, resultShortest);
    }

    [Theory]
    [InlineData(4, 3, 3, 3, true)]    // 4-3-3-3
    [InlineData(4, 4, 3, 2, true)]    // 4-4-3-2
    [InlineData(5, 3, 3, 2, true)]    // 5-3-3-2
    [InlineData(5, 4, 3, 1, false)]   // singleton
    [InlineData(6, 4, 3, 0, false)]   // void
    [InlineData(5, 4, 2, 2, false)]   // two doubletons
    public void SuitCounts_IsBalanced(byte sp, byte h, byte d, byte c, bool expected)
    {
        Assert.Equal(expected, new SuitCounts(sp, h, d, c).IsBalanced());
    }

    [Theory]
    [InlineData(4, 3, 3, 3, true)]    // 4-card spades
    [InlineData(3, 4, 3, 3, true)]    // 4-card hearts
    [InlineData(3, 3, 4, 3, false)]   // 4-card minor only
    [InlineData(3, 3, 3, 3, false)]   // no 4-card suit
    public void SuitCounts_HasFourCardMajor(byte sp, byte h, byte d, byte c, bool expected)
    {
        Assert.Equal(expected, new SuitCounts(sp, h, d, c).HasFourCardMajor());
    }
}
