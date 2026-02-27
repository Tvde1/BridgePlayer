using System.Diagnostics;

namespace BridgePlayer.Models.Common;

/// <summary>
/// Represents a bridge hand consisting of exactly 13 cards dealt to one player.
/// </summary>
public readonly record struct Hand
{
    /// <summary>
    /// Gets the 13 cards held in this hand.
    /// </summary>
    public readonly ReadOnlyMemory<Card> Cards { get; }

    /// <summary>
    /// Initializes a new <see cref="Hand"/> with the provided cards.
    /// </summary>
    /// <param name="cards">A memory segment containing exactly 13 <see cref="Card"/> values.</param>
    public Hand(ReadOnlyMemory<Card> cards)
    {
        Cards = cards;
        Debug.Assert(cards.Length == 13);
    }
}
