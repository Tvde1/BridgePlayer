using BridgePlayer.Models.Enums;

namespace BridgePlayer.Tests;

public class SuitTests
{
    [Theory]
    [InlineData(Suit.Spades, 1)]
    [InlineData(Suit.Hearts, 2)]
    [InlineData(Suit.Diamonds, 3)]
    [InlineData(Suit.Clubs, 4)]
    public void Suit_ByteValue_IsCorrect(Suit suit, byte expected)
    {
        Assert.Equal(expected, (byte)suit);
    }
}
