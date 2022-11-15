using System.Diagnostics;

namespace BridgePlayer.Models.Common;

public readonly record struct Hand
{
    public readonly ReadOnlyMemory<Card> Cards { get; }

    public Hand(ReadOnlyMemory<Card> cards)
    {
        Cards = cards;
        Debug.Assert(cards.Length == 13);
    }
}
