using BridgePlayer.Models.Enums;

namespace BridgePlayer.Tests;

public class SuitTests
{
    [Fact]
    public void Suit_HasFourValues()
    {
        var suits = Enum.GetValues<Suit>();
        Assert.Equal(4, suits.Length);
    }

    [Fact]
    public void Suit_EnumValues_AreCorrect()
    {
        Assert.Equal((byte)1, (byte)Suit.Spades);
        Assert.Equal((byte)2, (byte)Suit.Hearts);
        Assert.Equal((byte)3, (byte)Suit.Diamonds);
        Assert.Equal((byte)4, (byte)Suit.Clubs);
    }

    [Fact]
    public void Suit_ContainsSpades()
    {
        Assert.Contains(Suit.Spades, Enum.GetValues<Suit>());
    }

    [Fact]
    public void Suit_ContainsHearts()
    {
        Assert.Contains(Suit.Hearts, Enum.GetValues<Suit>());
    }

    [Fact]
    public void Suit_ContainsDiamonds()
    {
        Assert.Contains(Suit.Diamonds, Enum.GetValues<Suit>());
    }

    [Fact]
    public void Suit_ContainsClubs()
    {
        Assert.Contains(Suit.Clubs, Enum.GetValues<Suit>());
    }
}
