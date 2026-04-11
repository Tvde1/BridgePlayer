using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;
using BridgePlayer.Models.Wrapper;

namespace BridgePlayer.Tests.Bidding;

public class OneNoTrumpTests
{
    /// <summary>
    /// Builds a 13-card hand from a 4-character distribution string (S-H-D-C order)
    /// and a target HCP count. Honours are placed greedily into clubs and diamonds first
    /// so that major-suit counts remain exactly as specified.
    /// </summary>
    private static Hand BuildHand(string distribution, int targetHcp)
    {
        var sp = (byte)(distribution[0] - '0');
        var h  = (byte)(distribution[1] - '0');
        var d  = (byte)(distribution[2] - '0');
        var c  = (byte)(distribution[3] - '0');

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
            [Suit.Spades] = sp, [Suit.Hearts] = h,
            [Suit.Diamonds] = d, [Suit.Clubs] = c,
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
            var suitHonours = honours.Where(card => card.Suit == suit).ToList();
            foreach (var card in suitHonours) cards[idx++] = card;
            for (var i = suitHonours.Count; i < capacity[suit]; i++)
                cards[idx++] = new Card(suit, Value.Two);
        }

        return new Hand(new ReadOnlyMemory<Card>(cards));
    }

    [Theory]
    [InlineData("2353", 15, true)]    // 2-3-5-3 (S-H-D-C), 15 HCP, no 4-card major
    [InlineData("3343", 16, true)]    // 3-3-4-3, 16 HCP, no 4-card major
    [InlineData("3343", 17, true)]    // 3-3-4-3, 17 HCP, upper bound
    [InlineData("2353", 14, false)]   // 14 HCP — too few
    [InlineData("2353", 18, false)]   // 18 HCP — too many
    [InlineData("4333", 16, false)]   // 4-card spade (major)
    [InlineData("3433", 16, false)]   // 4-card heart (major)
    [InlineData("6232", 16, false)]   // unbalanced (6-card suit)
    public void IsOneNoTrump(string distribution, int hcp, bool expected)
    {
        var hand = BuildHand(distribution, hcp);
        Assert.Equal(expected, new HandWrapper(hand).IsOneNoTrump());
    }
}
