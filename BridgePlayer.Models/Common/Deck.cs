namespace BridgePlayer.Models.Common;

/// <summary>
/// Represents a full bridge deal, containing the four hands dealt to North, East, South, and West.
/// </summary>
public readonly record struct Deck
{
    /// <summary>Gets the hand dealt to the North player.</summary>
    public readonly Hand North { get; }
    /// <summary>Gets the hand dealt to the East player.</summary>
    public readonly Hand East { get; }
    /// <summary>Gets the hand dealt to the South player.</summary>
    public readonly Hand South { get; }
    /// <summary>Gets the hand dealt to the West player.</summary>
    public readonly Hand West { get; }

    /// <summary>
    /// Initializes a new <see cref="Deck"/> with the four player hands.
    /// </summary>
    /// <param name="north">The 13-card hand for North.</param>
    /// <param name="east">The 13-card hand for East.</param>
    /// <param name="south">The 13-card hand for South.</param>
    /// <param name="west">The 13-card hand for West.</param>
    public Deck(Hand north, Hand east, Hand south, Hand west)
    {
        North = north;
        East = east;
        South = south;
        West = west;
    }
}