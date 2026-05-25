namespace BridgePlayer.Models.Enums;

/// <summary>
/// Represents one of the four compass directions (seats) at a bridge table.
/// </summary>
/// <remarks>
/// The numeric values match the BBO dealer-digit encoding:
/// <c>(int)direction + 1</c> gives the LIN dealer digit (South→1, West→2, North→3, East→4).
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
