namespace BridgePlayer.Models.Enums;

/// <summary>
/// Represents the face value of a playing card, ranging from Two (2) through Ace (14).
/// </summary>
public enum Value : byte
{
    /// <summary>The Two (2).</summary>
    Two = 2,
    /// <summary>The Three (3).</summary>
    Three = 3,
    /// <summary>The Four (4).</summary>
    Four = 4,
    /// <summary>The Five (5).</summary>
    Five = 5,
    /// <summary>The Six (6).</summary>
    Six = 6,
    /// <summary>The Seven (7).</summary>
    Seven = 7,
    /// <summary>The Eight (8).</summary>
    Eight = 8,
    /// <summary>The Nine (9).</summary>
    Nine = 9,
    /// <summary>The Ten (10).</summary>
    Ten = 10,
    /// <summary>The Jack (J), worth 1 High Card Point.</summary>
    Jack = 11,
    /// <summary>The Queen (Q), worth 2 High Card Points.</summary>
    Queen = 12,
    /// <summary>The King (K), worth 3 High Card Points.</summary>
    King = 13,
    /// <summary>The Ace (A), worth 4 High Card Points.</summary>
    Ace = 14,
}
