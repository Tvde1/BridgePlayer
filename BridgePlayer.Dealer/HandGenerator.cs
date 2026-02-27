using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;

namespace BridgePlayer.Dealer;

/// <summary>
/// Generates randomly shuffled bridge deals from a standard 52-card deck.
/// Each instance reuses the same underlying deck array for efficient allocation.
/// </summary>
public class HandGenerator
{
    private readonly Card[] _deck = CreateDeck().ToArray();
    private readonly Random _rng = new();

    /// <summary>
    /// Initializes a new <see cref="HandGenerator"/> with its own private random number generator.
    /// </summary>
    public HandGenerator()
    {
    }

    /// <summary>
    /// Shuffles the 52-card deck and returns a new <see cref="Deck"/> with four hands of 13 cards
    /// assigned to North, East, South, and West respectively.
    /// </summary>
    /// <returns>A freshly shuffled <see cref="Deck"/>.</returns>
    public Deck ShuffleNewDeck()
    {
        Shuffle();
        return new Deck(
            new Hand(new ReadOnlyMemory<Card>(_deck, 0, 13)),
            new Hand(new ReadOnlyMemory<Card>(_deck, 13, 13)),
            new Hand(new ReadOnlyMemory<Card>(_deck, 26, 13)),
            new Hand(new ReadOnlyMemory<Card>(_deck, 39, 13))
        );
    }

    /// <summary>Performs an in-place Fisher-Yates shuffle on the internal deck array.</summary>
    private void Shuffle()
    {
        var n = _deck.Length;
        while (n > 1)
        {
            var k = _rng.Next(n--);
            (_deck[k], _deck[n]) = (_deck[n], _deck[k]);
        }
    }

    /// <summary>Creates an ordered sequence of all 52 cards (all suits × all values).</summary>
    private static IEnumerable<Card> CreateDeck()
    {
        var suits = Enum.GetValues<Suit>();
        var values = Enum.GetValues<Value>();

        foreach (var suit in suits)
            foreach (var @value in values)
                yield return new Card(suit, @value);
    } 
}
