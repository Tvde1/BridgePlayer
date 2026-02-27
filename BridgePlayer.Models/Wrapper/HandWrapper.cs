using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;

namespace BridgePlayer.Models.Wrapper;

public readonly ref struct HandWrapper
{
    private readonly Hand _hand;

    public HandWrapper(Hand hand)
    {
        _hand = hand;

        var totalPoints = 0;

        byte spadeCount = 0;
        byte heartCount = 0;
        byte diamondCount = 0;
        byte clubCount = 0;

        for (var index = 0; index < _hand.Cards.Length; index++)
        {
            var c = _hand.Cards.Span[index];
            totalPoints += c.HCP;
            switch (c.Suit)
            {
                case Suit.Spades:
                    spadeCount++;
                    break;
                case Suit.Hearts:
                    heartCount++;
                    break;
                case Suit.Diamonds:
                    diamondCount++;
                    break;
                case Suit.Clubs:
                    clubCount++;
                    break;
            }
        }

        Points = totalPoints;
        SuitCounts = new SuitCounts(spadeCount, heartCount, diamondCount, clubCount);
    }

    public int Points { get; }

    public SuitCounts SuitCounts { get; }

    /// <summary>
    /// Returns true when the hand qualifies for a 1NT opening:
    /// 15–17 HCP, balanced distribution, and no 4-card major.
    /// </summary>
    public bool IsOneNoTrump() =>
        Points is >= 15 and <= 17 &&
        SuitCounts.IsBalanced() &&
        !SuitCounts.HasFourCardMajor();
}