using BridgePlayer.Models.Enums;

namespace BridgePlayer.Models.Common;

/// <summary>
/// Represents a single playing card, defined by its <see cref="Suit"/> and <see cref="Value"/>.
/// </summary>
public readonly record struct Card(Suit Suit, Value Value)
{
    /// <summary>
    /// Gets the High Card Point (HCP) value of this card.
    /// Jack = 1, Queen = 2, King = 3, Ace = 4; all other cards = 0.
    /// </summary>
    public byte HCP { get; } = (byte)Value >= (byte)Value.Jack
        ? (byte)((byte)Value - (byte)Value.Ten)
        : (byte)0;
}