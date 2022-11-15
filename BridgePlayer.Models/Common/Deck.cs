namespace BridgePlayer.Models.Common;

public readonly record struct Deck
{
    public readonly Hand North { get; }
    public readonly Hand East { get; }
    public readonly Hand South { get; }
    public readonly Hand West { get; }

    public Deck(Hand north, Hand east, Hand south, Hand west)
    {
        North = north;
        East = east;
        South = south;
        West = west;
    }
}