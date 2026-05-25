using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;

namespace BridgePlayer.Models.LIN;

/// <summary>
/// Parses LIN-format strings into strongly-typed bridge data models.
/// </summary>
/// <remarks>
/// This parser targets the Modern/Archive LIN flavour (one bid per <c>mb</c> tag) used by the
/// BBO HandViewer and Bridge Master interactive files. The compact Classic format
/// (multiple bids in a single <c>mb</c> tag) is not supported.
/// <para>
/// Unknown tags are silently skipped per the LIN specification.
/// </para>
/// </remarks>
public static class LinParser
{
    /// <summary>
    /// Tokenises a LIN string into a sequence of <c>(tag, value)</c> pairs.
    /// </summary>
    /// <param name="lin">A LIN token string.</param>
    /// <returns>A lazily-evaluated sequence of two-character tag and value pairs.</returns>
    public static IEnumerable<(string Tag, string Value)> Tokenise(string lin)
    {
        int pos = 0;

        // Skip leading whitespace.
        while (pos < lin.Length && char.IsWhiteSpace(lin[pos]))
            pos++;

        while (pos < lin.Length)
        {
            int firstPipe = lin.IndexOf('|', pos);
            if (firstPipe < 0) yield break;
            var tag = lin[pos..firstPipe];

            int secondPipe = lin.IndexOf('|', firstPipe + 1);
            if (secondPipe < 0) yield break;
            var value = lin[(firstPipe + 1)..secondPipe];

            pos = secondPipe + 1;

            // Skip whitespace between tokens.
            while (pos < lin.Length && char.IsWhiteSpace(lin[pos]))
                pos++;

            if (tag.Length == 2)
                yield return (tag, value);
        }
    }

    /// <summary>
    /// Parses a single-board LIN string into a <see cref="LinBoardRecord"/>.
    /// </summary>
    /// <param name="lin">A single-board LIN token string.</param>
    /// <returns>The parsed board record.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the LIN string contains no <c>md</c> tag or has an invalid format.
    /// </exception>
    public static LinBoardRecord ParseBoard(string lin)
    {
        LinDeal? deal = null;
        IReadOnlyList<string>? playerNames = null;
        var bids = new List<LinBid>();
        var play = new List<Card>();
        var commentary = new List<LinCommentary>();
        var vulnerability = Vulnerability.None;
        int mbCount = 0;
        int pcCount = 0;

        foreach (var (tag, value) in Tokenise(lin))
        {
            switch (tag)
            {
                case "pn":
                    playerNames = value.Split(',');
                    break;

                case "md":
                    deal = ParseDeal(value, vulnerability);
                    break;

                case "sv":
                    vulnerability = ParseVulnerability(value);
                    // Retroactively apply vulnerability if the deal was already parsed.
                    if (deal is not null)
                        deal = deal with { Vulnerability = vulnerability };
                    break;

                case "mb":
                    var isAlerted = value.EndsWith('!');
                    var bidValue = isAlerted ? value[..^1] : value;
                    bids.Add(new LinBid(bidValue, isAlerted));
                    mbCount++;
                    break;

                case "an":
                    // Attach explanation to the most recently seen bid.
                    if (bids.Count > 0)
                    {
                        var last = bids[^1];
                        bids[^1] = last with { Explanation = value };
                    }
                    break;

                case "pc":
                    if (value.Length >= 2)
                    {
                        play.Add(ParseCard(value));
                        pcCount++;
                    }
                    break;

                case "nt":
                    commentary.Add(CreateCommentary(mbCount, pcCount, value));
                    break;
            }
        }

        if (deal is null)
            throw new FormatException("LIN string does not contain a deal (md tag).");

        return new LinBoardRecord(deal, playerNames, bids, play, commentary);
    }

    /// <summary>
    /// Parses a LIN <c>md</c> tag value into a <see cref="LinDeal"/>.
    /// </summary>
    /// <param name="value">The raw value of the <c>md</c> tag (including the dealer digit).</param>
    /// <param name="vulnerability">The vulnerability to assign to the deal.</param>
    /// <returns>The parsed deal.</returns>
    /// <exception cref="FormatException">Thrown when the value is malformed.</exception>
    public static LinDeal ParseDeal(string value, Vulnerability vulnerability = default)
    {
        if (value.Length < 2)
            throw new FormatException($"Invalid LIN deal value: '{value}'");

        var dealer = DealerFromLinDigit(value[0]);
        var holdings = value[1..].Split(',');

        if (holdings.Length < 3)
            throw new FormatException($"LIN deal must contain at least 3 hand holdings: '{value}'");

        var south = ParseHolding(holdings[0]);
        var west  = ParseHolding(holdings[1]);
        var north = ParseHolding(holdings[2]);

        Hand east;
        if (holdings.Length >= 4 && !string.IsNullOrEmpty(holdings[3]))
            east = ParseHolding(holdings[3]);
        else
            east = InferFourthHand(south, west, north);

        return new LinDeal(dealer, vulnerability, south, west, north, east);
    }

    /// <summary>
    /// Parses a LIN holding string (e.g. <c>"SAKQHJ9D54C32"</c>) into a <see cref="Hand"/>.
    /// </summary>
    /// <param name="holding">The holding string, ordered S H D C with ranks high-to-low.</param>
    /// <returns>A <see cref="Hand"/> containing the cards described by the holding.</returns>
    /// <exception cref="FormatException">Thrown when an unrecognised rank character is encountered.</exception>
    public static Hand ParseHolding(string holding)
    {
        var cards = new List<Card>(13);
        Suit? currentSuit = null;

        foreach (var ch in holding)
        {
            switch (char.ToUpperInvariant(ch))
            {
                case 'S': currentSuit = Suit.Spades;   break;
                case 'H': currentSuit = Suit.Hearts;   break;
                case 'D': currentSuit = Suit.Diamonds; break;
                case 'C': currentSuit = Suit.Clubs;    break;
                default:
                    if (currentSuit.HasValue)
                        cards.Add(new Card(currentSuit.Value, ParseRank(char.ToUpperInvariant(ch))));
                    break;
            }
        }

        return new Hand(cards.ToArray());
    }

    /// <summary>
    /// Parses a two-character LIN card token (e.g. <c>"CA"</c>, <c>"HT"</c>) into a <see cref="Card"/>.
    /// </summary>
    /// <param name="token">A two-character string: suit letter followed by rank character.</param>
    /// <returns>The parsed card.</returns>
    /// <exception cref="FormatException">Thrown when the suit or rank character is unrecognised.</exception>
    public static Card ParseCard(string token)
    {
        if (token.Length < 2)
            throw new FormatException($"Invalid LIN card token: '{token}'");

        var suit = char.ToUpperInvariant(token[0]) switch
        {
            'S' => Suit.Spades,
            'H' => Suit.Hearts,
            'D' => Suit.Diamonds,
            'C' => Suit.Clubs,
            _   => throw new FormatException($"Unknown suit character: '{token[0]}'"),
        };

        var value = ParseRank(char.ToUpperInvariant(token[1]));
        return new Card(suit, value);
    }

    /// <summary>
    /// Parses a LIN vulnerability string into a <see cref="Vulnerability"/> value.
    /// </summary>
    /// <param name="sv">The raw value of the <c>sv</c> tag.</param>
    /// <returns>The corresponding <see cref="Vulnerability"/>.</returns>
    /// <exception cref="FormatException">Thrown when the value is not a recognised vulnerability code.</exception>
    public static Vulnerability ParseVulnerability(string sv) => sv.ToLowerInvariant() switch
    {
        "n"               => Vulnerability.NsOnly,
        "e"               => Vulnerability.EwOnly,
        "b"               => Vulnerability.Both,
        "o" or "0" or ""  => Vulnerability.None,
        _                 => throw new FormatException($"Unknown LIN vulnerability code: '{sv}'"),
    };

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a <see cref="LinCommentary"/> anchored to the correct auction or play position.
    /// </summary>
    /// <param name="mbCount">Number of <c>mb</c> tags seen so far.</param>
    /// <param name="pcCount">Number of <c>pc</c> tags seen so far.</param>
    /// <param name="text">The note text.</param>
    private static LinCommentary CreateCommentary(int mbCount, int pcCount, string text)
    {
        if (pcCount > 0)
        {
            // Note appears during the play phase.
            return new LinCommentary(BidIndex: null, PlayIndex: pcCount - 1, Text: text);
        }

        if (mbCount > 0)
        {
            // Note appears during the auction.
            return new LinCommentary(BidIndex: mbCount - 1, PlayIndex: null, Text: text);
        }

        // Note appears before any bids (pre-auction).
        return new LinCommentary(BidIndex: null, PlayIndex: null, Text: text);
    }

    private static Direction DealerFromLinDigit(char c) => c switch
    {
        '1' => Direction.South,
        '2' => Direction.West,
        '3' => Direction.North,
        '4' => Direction.East,
        _   => throw new FormatException($"Invalid LIN dealer digit: '{c}'"),
    };

    private static Hand InferFourthHand(Hand south, Hand west, Hand north)
    {
        // Build the full 52-card deck then remove the three known hands.
        var remaining = new HashSet<Card>(52);
        foreach (Suit suit in Enum.GetValues<Suit>())
            foreach (Value value in Enum.GetValues<Value>())
                remaining.Add(new Card(suit, value));

        foreach (var card in south.Cards.Span) remaining.Remove(card);
        foreach (var card in west.Cards.Span)  remaining.Remove(card);
        foreach (var card in north.Cards.Span) remaining.Remove(card);

        return new Hand(remaining.ToArray());
    }

    private static Value ParseRank(char c) => c switch
    {
        'A' => Value.Ace,
        'K' => Value.King,
        'Q' => Value.Queen,
        'J' => Value.Jack,
        'T' => Value.Ten,
        '9' => Value.Nine,
        '8' => Value.Eight,
        '7' => Value.Seven,
        '6' => Value.Six,
        '5' => Value.Five,
        '4' => Value.Four,
        '3' => Value.Three,
        '2' => Value.Two,
        _   => throw new FormatException($"Unknown LIN rank character: '{c}'"),
    };
}
