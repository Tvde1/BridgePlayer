using BridgePlayer.Models.Enums;

namespace BridgePlayer.Models.Wrapper;

/// <summary>
/// A stack-allocated, read-only snapshot of the number of cards held in each suit within a single hand.
/// </summary>
public readonly ref struct SuitCounts
{
    /// <summary>
    /// Initializes a new <see cref="SuitCounts"/> with explicit per-suit card counts.
    /// </summary>
    /// <param name="spades">Number of spade cards.</param>
    /// <param name="hearts">Number of heart cards.</param>
    /// <param name="diamonds">Number of diamond cards.</param>
    /// <param name="clubs">Number of club cards.</param>
    public SuitCounts(byte spades, byte hearts, byte diamonds, byte clubs)
    {
        Spades = spades;
        Hearts = hearts;
        Diamonds = diamonds;
        Clubs = clubs;
    }

    /// <summary>Gets the number of spade cards in the hand.</summary>
    public byte Spades { get; }
    /// <summary>Gets the number of heart cards in the hand.</summary>
    public byte Hearts { get; }
    /// <summary>Gets the number of diamond cards in the hand.</summary>
    public byte Diamonds { get; }
    /// <summary>Gets the number of club cards in the hand.</summary>
    public byte Clubs { get; }

    /// <summary>
    /// Deconstructs the suit counts into four values ordered from longest to shortest suit.
    /// </summary>
    /// <param name="longest">The count of cards in the longest suit.</param>
    /// <param name="secondLongest">The count of cards in the second-longest suit.</param>
    /// <param name="secondShortest">The count of cards in the second-shortest suit.</param>
    /// <param name="shortest">The count of cards in the shortest suit.</param>
    public void Deconstruct(out byte longest, out byte secondLongest, out byte secondShortest, out byte shortest)
    {
        Span<byte> values = stackalloc byte[] { Spades, Hearts, Diamonds, Clubs };
        values.Sort();
        shortest = values[0];
        secondShortest = values[1];
        secondLongest = values[2];
        longest = values[3];
    }

    /// <summary>
    /// Gets the card count for the specified <paramref name="suit"/>.
    /// </summary>
    /// <param name="suit">The suit to look up.</param>
    /// <returns>The number of cards held in <paramref name="suit"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="suit"/> is not a valid <see cref="Suit"/> value.</exception>
    public int this[Suit suit] => suit switch
    {
        Suit.Spades => Spades,
        Suit.Hearts => Hearts,
        Suit.Diamonds => Diamonds,
        Suit.Clubs => Clubs,
        _ => throw new ArgumentOutOfRangeException(),
    };

    /// <summary>
    /// Returns <see langword="true"/> if any suit has exactly <paramref name="count"/> cards,
    /// and sets <paramref name="suit"/> to that suit.
    /// When multiple suits share the count, the highest-ranked suit (Spades first) is returned.
    /// </summary>
    /// <param name="count">The exact card count to search for.</param>
    /// <param name="suit">
    /// When this method returns <see langword="true"/>, the first suit found with the given count;
    /// otherwise, the default value of <see cref="Suit"/>.
    /// </param>
    /// <returns><see langword="true"/> if a suit with the exact count exists; otherwise <see langword="false"/>.</returns>
    public bool HasCount(int count, out Suit suit)
    {
        if (Spades == count)
        {
            suit = Suit.Spades;
            return true;
        }

        if (Hearts == count)
        {
            suit = Suit.Hearts;
            return true;
        }

        if (Diamonds == count)
        {
            suit = Suit.Diamonds;
            return true;
        }

        if (Clubs == count)
        {
            suit = Suit.Clubs;
            return true;
        }

        suit = default;
        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> if any suit has at least <paramref name="count"/> cards,
    /// and sets <paramref name="suit"/> to that suit.
    /// When multiple suits qualify, the highest-ranked suit (Spades first) is returned.
    /// </summary>
    /// <param name="count">The minimum card count to search for.</param>
    /// <param name="suit">
    /// When this method returns <see langword="true"/>, the first suit found meeting the minimum count;
    /// otherwise, the default value of <see cref="Suit"/>.
    /// </param>
    /// <returns><see langword="true"/> if a suit with at least the given count exists; otherwise <see langword="false"/>.</returns>
    public bool HasMinimumCount(int count, out Suit suit)
    {
        if (Spades >= count)
        {
            suit = Suit.Spades;
            return true;
        }

        if (Hearts >= count)
        {
            suit = Suit.Hearts;
            return true;
        }

        if (Diamonds >= count)
        {
            suit = Suit.Diamonds;
            return true;
        }

        if (Clubs >= count)
        {
            suit = Suit.Clubs;
            return true;
        }

        suit = default;
        return false;
    }

    internal (byte, byte, byte, byte) ToTuple()
    {
        Deconstruct(out var a, out var b, out var c, out var d);
        return (a, b, c, d);
    }
}
