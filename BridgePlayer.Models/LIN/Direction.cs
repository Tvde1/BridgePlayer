namespace BridgePlayer.Models.LIN;

/// <summary>
/// Represents one of the four compass directions (seats) at a bridge table.
/// </summary>
/// <remarks>
/// The numeric values match the BBO dealer-digit encoding:
/// <c>((int)direction + 2) % 4 + 1</c> gives the LIN dealer digit.
/// </remarks>
public enum Direction : byte
{
    /// <summary>The South seat.</summary>
    South = 0,

    /// <summary>The West seat.</summary>
    West = 1,

    /// <summary>The North seat.</summary>
    North = 2,

    /// <summary>The East seat.</summary>
    East = 3,
}
