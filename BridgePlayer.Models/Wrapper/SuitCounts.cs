using BridgePlayer.Models.Enums;

namespace BridgePlayer.Models.Wrapper;

public readonly ref struct SuitCounts
{
    public SuitCounts(byte spades, byte hearts, byte diamonds, byte clubs)
    {
        Spades = spades;
        Hearts = hearts;
        Diamonds = diamonds;
        Clubs = clubs;
    }

    public byte Spades { get; }
    public byte Hearts { get; }
    public byte Diamonds { get; }
    public byte Clubs { get; }

    public void Deconstruct(out byte longest, out byte secondLongest, out byte secondShortest, out byte shortest)
    {
        Span<byte> values = stackalloc byte[] { Spades, Hearts, Diamonds, Clubs };
        values.Sort();
        shortest = values[0];
        secondShortest = values[1];
        secondLongest = values[2];
        longest = values[3];
    }

    public int this[Suit suit] => suit switch
    {
        Suit.Spades => Spades,
        Suit.Hearts => Hearts,
        Suit.Diamonds => Diamonds,
        Suit.Clubs => Clubs,
        _ => throw new ArgumentOutOfRangeException(),
    };

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
