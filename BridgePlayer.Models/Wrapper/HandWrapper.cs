using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;

namespace BridgePlayer.Models.Wrapper;

/// <summary>
/// A lightweight, stack-allocated wrapper around a <see cref="Hand"/> that pre-computes
/// commonly needed statistics such as High Card Points and per-suit card counts.
/// </summary>
public readonly ref struct HandWrapper
{
    private readonly Hand _hand;

    /// <summary>
    /// Initializes a new <see cref="HandWrapper"/> and computes HCP and suit-count statistics
    /// for the provided <paramref name="hand"/>.
    /// </summary>
    /// <param name="hand">The bridge hand to analyse.</param>
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

    /// <summary>
    /// Gets the total High Card Points (HCP) held in this hand.
    /// </summary>
    public int Points { get; }

    /// <summary>
    /// Gets the number of cards held in each suit for this hand.
    /// </summary>
    public SuitCounts SuitCounts { get; }
}