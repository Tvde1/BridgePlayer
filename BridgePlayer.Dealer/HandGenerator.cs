using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;

namespace BridgePlayer.Dealer;

public class HandGenerator
{
    private readonly Card[] _deck = CreateDeck().ToArray();
    private readonly Random _rng = new();

    public HandGenerator()
    {
    }

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

    private void Shuffle()
    {
        var n = _deck.Length;
        while (n > 1)
        {
            var k = _rng.Next(n--);
            (_deck[k], _deck[n]) = (_deck[n], _deck[k]);
        }
    }

    private static IEnumerable<Card> CreateDeck()
    {
        var suits = Enum.GetValues<Suit>();
        var values = Enum.GetValues<Value>();

        foreach (var suit in suits)
            foreach (var @value in values)
                yield return new Card(suit, @value);
    } 
}
