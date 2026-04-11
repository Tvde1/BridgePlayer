using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;

namespace BridgePlayer.Tests;

public class CardTests
{
    [Theory]
    [InlineData(Value.Two, 0)]
    [InlineData(Value.Three, 0)]
    [InlineData(Value.Four, 0)]
    [InlineData(Value.Five, 0)]
    [InlineData(Value.Six, 0)]
    [InlineData(Value.Seven, 0)]
    [InlineData(Value.Eight, 0)]
    [InlineData(Value.Nine, 0)]
    [InlineData(Value.Ten, 0)]
    [InlineData(Value.Jack, 1)]
    [InlineData(Value.Queen, 2)]
    [InlineData(Value.King, 3)]
    [InlineData(Value.Ace, 4)]
    public void Card_HCP_IsCorrectForEachValue(Value value, int expectedHcp)
    {
        var card = new Card(Suit.Clubs, value);
        Assert.Equal(expectedHcp, card.HCP);
    }
}
