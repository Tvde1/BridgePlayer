using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;
using BridgePlayer.Models.Wrapper;

namespace BridgePlayer.Tests;

public class HandWrapperTests
{
    /// <summary>
    /// Builds a 13-card hand with the given suit distribution and target HCP.
    /// Honours are placed greedily into minors first (Clubs → Diamonds → Hearts → Spades)
    /// using the highest available honours, so suit counts remain exactly as requested.
    /// </summary>
    private static Hand BuildHandWithHcp(byte spades, byte hearts, byte diamonds, byte clubs, int targetHcp)
    {
        var pool = new (Suit Suit, Value Value, int Hcp)[]
        {
            (Suit.Clubs,    Value.Ace,   4), (Suit.Clubs,    Value.King,  3),
            (Suit.Clubs,    Value.Queen, 2), (Suit.Clubs,    Value.Jack,  1),
            (Suit.Diamonds, Value.Ace,   4), (Suit.Diamonds, Value.King,  3),
            (Suit.Diamonds, Value.Queen, 2), (Suit.Diamonds, Value.Jack,  1),
            (Suit.Hearts,   Value.Ace,   4), (Suit.Hearts,   Value.King,  3),
            (Suit.Hearts,   Value.Queen, 2), (Suit.Hearts,   Value.Jack,  1),
            (Suit.Spades,   Value.Ace,   4), (Suit.Spades,   Value.King,  3),
            (Suit.Spades,   Value.Queen, 2), (Suit.Spades,   Value.Jack,  1),
        };

        var capacity = new Dictionary<Suit, int>
        {
            [Suit.Spades] = spades, [Suit.Hearts] = hearts,
            [Suit.Diamonds] = diamonds, [Suit.Clubs] = clubs,
        };
        var used = new Dictionary<Suit, int>
        {
            [Suit.Spades] = 0, [Suit.Hearts] = 0,
            [Suit.Diamonds] = 0, [Suit.Clubs] = 0,
        };

        var honours = new List<Card>();
        var hcpLeft = targetHcp;
        foreach (var (suit, value, hcp) in pool)
        {
            if (hcpLeft <= 0) break;
            if (used[suit] < capacity[suit] && hcpLeft >= hcp)
            {
                honours.Add(new Card(suit, value));
                used[suit]++;
                hcpLeft -= hcp;
            }
        }

        var cards = new Card[13];
        var idx = 0;
        foreach (var suit in new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs })
        {
            var suitHonours = honours.Where(c => c.Suit == suit).ToList();
            foreach (var c in suitHonours) cards[idx++] = c;
            for (var i = suitHonours.Count; i < capacity[suit]; i++)
                cards[idx++] = new Card(suit, Value.Two);
        }

        return new Hand(new ReadOnlyMemory<Card>(cards));
    }

    [Fact]
    public void HandWrapper_Points_SumsAllHonours()
    {
        // A=4, K=3, Q=2, J=1 => 10 HCP total
        var hand = BuildHandWithHcp(spades: 4, hearts: 3, diamonds: 3, clubs: 3, targetHcp: 10);
        Assert.Equal(10, new HandWrapper(hand).Points);
    }

    [Fact]
    public void HandWrapper_SuitCounts_AreCorrect()
    {
        var hand = BuildHandWithHcp(spades: 4, hearts: 3, diamonds: 3, clubs: 3, targetHcp: 0);
        var wrapper = new HandWrapper(hand);
        Assert.Equal(4, wrapper.SuitCounts.Spades);
        Assert.Equal(3, wrapper.SuitCounts.Hearts);
        Assert.Equal(3, wrapper.SuitCounts.Diamonds);
        Assert.Equal(3, wrapper.SuitCounts.Clubs);
    }

    [Theory]
    [InlineData(15, 2, 3, 5, 3, true)]    // 15 HCP, 5-3-3-2, no 4-card major
    [InlineData(16, 3, 3, 4, 3, true)]    // 16 HCP, 4-3-3-3, no 4-card major
    [InlineData(17, 3, 3, 4, 3, true)]    // 17 HCP, 4-3-3-3, no 4-card major
    [InlineData(14, 2, 3, 5, 3, false)]   // 14 HCP — too few
    [InlineData(18, 2, 3, 5, 3, false)]   // 18 HCP — too many
    [InlineData(16, 4, 3, 3, 3, false)]   // 4-card spade major
    [InlineData(16, 3, 4, 3, 3, false)]   // 4-card heart major
    [InlineData(16, 6, 2, 3, 2, false)]   // unbalanced (two doubletons)
    public void HandWrapper_IsOneNoTrump(int hcp, byte spades, byte hearts, byte diamonds, byte clubs, bool expected)
    {
        var hand = BuildHandWithHcp(spades, hearts, diamonds, clubs, hcp);
        Assert.Equal(expected, new HandWrapper(hand).IsOneNoTrump());
    }
}
