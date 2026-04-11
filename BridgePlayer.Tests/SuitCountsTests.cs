using BridgePlayer.Models.Enums;
using BridgePlayer.Models.Wrapper;

namespace BridgePlayer.Tests;

public class SuitCountsTests
{
    [Fact]
    public void SuitCounts_StoresEachSuitCount()
    {
        var counts = new SuitCounts(5, 4, 3, 1);
        Assert.Equal(5, counts.Spades);
        Assert.Equal(4, counts.Hearts);
        Assert.Equal(3, counts.Diamonds);
        Assert.Equal(1, counts.Clubs);
    }

    [Fact]
    public void SuitCounts_Indexer_ReturnsCorrectCount()
    {
        var counts = new SuitCounts(5, 4, 3, 1);
        Assert.Equal(5, counts[Suit.Spades]);
        Assert.Equal(4, counts[Suit.Hearts]);
        Assert.Equal(3, counts[Suit.Diamonds]);
        Assert.Equal(1, counts[Suit.Clubs]);
    }

    [Fact]
    public void SuitCounts_Indexer_ThrowsForInvalidSuit()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SuitCounts(4, 3, 3, 3)[(Suit)99]);
    }

    [Fact]
    public void SuitCounts_HasCount_ReturnsTrueWhenExactMatch()
    {
        var counts = new SuitCounts(4, 3, 3, 3);
        Assert.True(counts.HasCount(4, out var suit));
        Assert.Equal(Suit.Spades, suit);
    }

    [Fact]
    public void SuitCounts_HasCount_ReturnsFalseWhenNoMatch()
    {
        var counts = new SuitCounts(4, 4, 4, 1);
        Assert.False(counts.HasCount(3, out _));
    }

    [Fact]
    public void SuitCounts_HasCount_PrefersHigherRankedSuit()
    {
        // Both Spades and Hearts have 4 cards; Spades (rank 1) should win
        var counts = new SuitCounts(4, 4, 3, 2);
        Assert.True(counts.HasCount(4, out var suit));
        Assert.Equal(Suit.Spades, suit);
    }

    [Fact]
    public void SuitCounts_HasMinimumCount_ReturnsTrueWhenMet()
    {
        var counts = new SuitCounts(5, 3, 3, 2);
        Assert.True(counts.HasMinimumCount(5, out var suit));
        Assert.Equal(Suit.Spades, suit);
    }

    [Fact]
    public void SuitCounts_HasMinimumCount_ReturnsFalseWhenNotMet()
    {
        var counts = new SuitCounts(4, 3, 3, 3);
        Assert.False(counts.HasMinimumCount(5, out _));
    }

    [Fact]
    public void SuitCounts_HasMinimumCount_PrefersHigherRankedSuit()
    {
        // Hearts (4) and Spades (4) both meet minimum of 4; Spades wins
        var counts = new SuitCounts(4, 4, 2, 3);
        Assert.True(counts.HasMinimumCount(4, out var suit));
        Assert.Equal(Suit.Spades, suit);
    }

    [Fact]
    public void SuitCounts_Deconstruct_OrdersFromLongestToShortest()
    {
        var counts = new SuitCounts(3, 5, 2, 3);
        counts.Deconstruct(out var longest, out var secondLongest, out var secondShortest, out var shortest);
        Assert.Equal(5, longest);
        Assert.Equal(3, secondLongest);
        Assert.Equal(3, secondShortest);
        Assert.Equal(2, shortest);
    }

    [Fact]
    public void SuitCounts_IsBalanced_ReturnsTrueFor4333()
    {
        var counts = new SuitCounts(4, 3, 3, 3);
        Assert.True(counts.IsBalanced());
    }

    [Fact]
    public void SuitCounts_IsBalanced_ReturnsTrueFor4432()
    {
        var counts = new SuitCounts(4, 4, 3, 2);
        Assert.True(counts.IsBalanced());
    }

    [Fact]
    public void SuitCounts_IsBalanced_ReturnsTrueFor5332()
    {
        var counts = new SuitCounts(5, 3, 3, 2);
        Assert.True(counts.IsBalanced());
    }

    [Fact]
    public void SuitCounts_IsBalanced_ReturnsFalseForSingleton()
    {
        var counts = new SuitCounts(5, 4, 3, 1);
        Assert.False(counts.IsBalanced());
    }

    [Fact]
    public void SuitCounts_IsBalanced_ReturnsFalseForVoid()
    {
        var counts = new SuitCounts(6, 4, 3, 0);
        Assert.False(counts.IsBalanced());
    }

    [Fact]
    public void SuitCounts_IsBalanced_ReturnsFalseForTwoDoubletons()
    {
        var counts = new SuitCounts(5, 4, 2, 2);
        Assert.False(counts.IsBalanced());
    }

    [Fact]
    public void SuitCounts_HasFourCardMajor_ReturnsTrueWhenSpadesHasFour()
    {
        var counts = new SuitCounts(4, 3, 3, 3);
        Assert.True(counts.HasFourCardMajor());
    }

    [Fact]
    public void SuitCounts_HasFourCardMajor_ReturnsTrueWhenHeartsHasFour()
    {
        var counts = new SuitCounts(3, 4, 3, 3);
        Assert.True(counts.HasFourCardMajor());
    }

    [Fact]
    public void SuitCounts_HasFourCardMajor_ReturnsFalseWhenNoMajorHasFour()
    {
        var counts = new SuitCounts(3, 3, 4, 3);
        Assert.False(counts.HasFourCardMajor());
    }
}
