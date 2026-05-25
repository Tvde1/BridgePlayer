using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;
using BridgePlayer.Models.LIN;

namespace BridgePlayer.Tests.LIN;

public class LinParserTokeniseTests
{
    [Fact]
    public void Tokenise_EmptyString_YieldsNoTokens()
    {
        var tokens = LinParser.Tokenise("").ToList();
        Assert.Empty(tokens);
    }

    [Fact]
    public void Tokenise_WhitespaceOnly_YieldsNoTokens()
    {
        var tokens = LinParser.Tokenise("   \n  ").ToList();
        Assert.Empty(tokens);
    }

    [Fact]
    public void Tokenise_SingleToken_ReturnsTagAndValue()
    {
        var tokens = LinParser.Tokenise("st||").ToList();
        Assert.Single(tokens);
        Assert.Equal("st", tokens[0].Tag);
        Assert.Equal("", tokens[0].Value);
    }

    [Fact]
    public void Tokenise_MultipleTokens_ReturnsAllPairs()
    {
        var tokens = LinParser.Tokenise("pn|South,West,North,East|sv|o|").ToList();
        Assert.Equal(2, tokens.Count);
        Assert.Equal(("pn", "South,West,North,East"), tokens[0]);
        Assert.Equal(("sv", "o"), tokens[1]);
    }

    [Fact]
    public void Tokenise_SkipsTagsWithWrongLength()
    {
        // "pg" is 2 chars and should be included; "abc" is 3 chars and should be skipped
        var tokens = LinParser.Tokenise("pg||abc|xyz|st||").ToList();
        Assert.Equal(2, tokens.Count);
        Assert.Equal("pg", tokens[0].Tag);
        Assert.Equal("st", tokens[1].Tag);
    }

    [Fact]
    public void Tokenise_SkipsLeadingWhitespace()
    {
        var tokens = LinParser.Tokenise("  st||").ToList();
        Assert.Single(tokens);
        Assert.Equal("st", tokens[0].Tag);
    }

    [Fact]
    public void Tokenise_HandlesNewlineBetweenTokens()
    {
        var tokens = LinParser.Tokenise("st||\npn|South,West,North,East|").ToList();
        Assert.Equal(2, tokens.Count);
    }
}

public class LinParserVulnerabilityTests
{
    [Theory]
    [InlineData("o", false, false)]
    [InlineData("O", false, false)]
    [InlineData("0", false, false)]
    [InlineData("",  false, false)]
    [InlineData("n", true,  false)]
    [InlineData("N", true,  false)]
    [InlineData("e", false, true)]
    [InlineData("E", false, true)]
    [InlineData("b", true,  true)]
    [InlineData("B", true,  true)]
    public void ParseVulnerability_KnownCodes_ReturnCorrectValue(string sv, bool ns, bool ew)
    {
        var vuln = LinParser.ParseVulnerability(sv);
        Assert.Equal(ns, vuln.NorthSouth);
        Assert.Equal(ew, vuln.EastWest);
    }

    [Fact]
    public void ParseVulnerability_UnknownCode_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => LinParser.ParseVulnerability("x"));
    }
}

public class LinParserCardTests
{
    [Theory]
    [InlineData("CA", Suit.Clubs,    Value.Ace)]
    [InlineData("HT", Suit.Hearts,   Value.Ten)]
    [InlineData("S9", Suit.Spades,   Value.Nine)]
    [InlineData("DK", Suit.Diamonds, Value.King)]
    [InlineData("c2", Suit.Clubs,    Value.Two)]   // lower-case suit
    [InlineData("hj", Suit.Hearts,   Value.Jack)]  // lower-case both
    public void ParseCard_ValidTokens_ReturnCorrectCard(string token, Suit expectedSuit, Value expectedValue)
    {
        var card = LinParser.ParseCard(token);
        Assert.Equal(expectedSuit,  card.Suit);
        Assert.Equal(expectedValue, card.Value);
    }

    [Fact]
    public void ParseCard_TooShort_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => LinParser.ParseCard("C"));
    }

    [Fact]
    public void ParseCard_UnknownSuit_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => LinParser.ParseCard("XA"));
    }

    [Fact]
    public void ParseCard_UnknownRank_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => LinParser.ParseCard("C1"));
    }
}

public class LinParserHoldingTests
{
    [Fact]
    public void ParseHolding_FullHand_Returns13Cards()
    {
        var hand = LinParser.ParseHolding("SAK32HAKQD32CA432");
        Assert.Equal(13, hand.Cards.Length);
    }

    [Fact]
    public void ParseHolding_VoidSuit_ParsesCorrectly()
    {
        // Void in spades: 0+5+5+3 = 13 cards total
        var hand = LinParser.ParseHolding("SHAKQJTDAKQJTCAKQ");
        Assert.Equal(13, hand.Cards.Length);
        Assert.DoesNotContain(hand.Cards.ToArray(), c => c.Suit == Suit.Spades);
    }

    [Fact]
    public void ParseHolding_LowerCaseSuits_ParsesCorrectly()
    {
        var hand = LinParser.ParseHolding("sqjthjt98d98cjt98");
        Assert.Equal(13, hand.Cards.Length);
    }

    [Fact]
    public void ParseHolding_AllFourSuitsPresent_CardsHaveCorrectSuits()
    {
        // S:AK32 H:AKQ D:32 C:A432 = 4+3+2+4 = 13 cards
        var hand = LinParser.ParseHolding("SAK32HAKQD32CA432");
        var suits = hand.Cards.ToArray().Select(c => c.Suit).ToHashSet();
        Assert.Contains(Suit.Spades,   suits);
        Assert.Contains(Suit.Hearts,   suits);
        Assert.Contains(Suit.Diamonds, suits);
        Assert.Contains(Suit.Clubs,    suits);
    }
}

public class LinParserDealTests
{
    [Fact]
    public void ParseDeal_NorthDealer_CorrectDirection()
    {
        var deal = LinParser.ParseDeal("3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65");
        Assert.Equal(Direction.North, deal.Dealer);
    }

    [Theory]
    [InlineData('1', Direction.South)]
    [InlineData('2', Direction.West)]
    [InlineData('3', Direction.North)]
    [InlineData('4', Direction.East)]
    public void ParseDeal_DealerDigits_MapToCorrectDirections(char digit, Direction expected)
    {
        var deal = LinParser.ParseDeal($"{digit}SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65");
        Assert.Equal(expected, deal.Dealer);
    }

    [Fact]
    public void ParseDeal_ThreeHoldings_InfersEastHand()
    {
        var deal = LinParser.ParseDeal("3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65");
        Assert.Equal(13, deal.East.Cards.Length);
    }

    [Fact]
    public void ParseDeal_ThreeHoldings_AllCardsAccountedFor()
    {
        var deal = LinParser.ParseDeal("3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65");
        var allCards = deal.South.Cards.ToArray()
            .Concat(deal.West.Cards.ToArray())
            .Concat(deal.North.Cards.ToArray())
            .Concat(deal.East.Cards.ToArray())
            .ToList();
        Assert.Equal(52, allCards.Count);
        Assert.Equal(52, allCards.Distinct().Count());
    }

    [Fact]
    public void ParseDeal_FourHoldings_UsesExplicitEast()
    {
        // East holding explicitly given
        var deal = LinParser.ParseDeal("2SAK2HA95DAJ875C92,SQ765HKQ64D32CAK6,S9HJ872DQT64CQJ43,");
        Assert.Equal(13, deal.East.Cards.Length);
    }

    [Fact]
    public void ParseDeal_InvalidDealerDigit_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => LinParser.ParseDeal("5SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65"));
    }

    [Fact]
    public void ParseDeal_VulnerabilityIsPassedThrough()
    {
        var deal = LinParser.ParseDeal("3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65", Vulnerability.Both);
        Assert.Equal(Vulnerability.Both, deal.Vulnerability);
    }
}

public class LinParserParseBoardTests
{
    private const string IssueLin =
        "st||nt||sk||md|3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65|" +
        "mb|2d|mb|p|mb|3n|mb|p|mb|p|mb|p|";

    [Fact]
    public void ParseBoard_IssueSampleLin_ParsesCorrectly()
    {
        var board = LinParser.ParseBoard(IssueLin);

        Assert.Equal(Direction.North, board.Deal.Dealer);
        Assert.Equal(6, board.Bids.Count);
        Assert.Equal(13, board.Deal.South.Cards.Length);
        Assert.Equal(13, board.Deal.West.Cards.Length);
        Assert.Equal(13, board.Deal.North.Cards.Length);
        Assert.Equal(13, board.Deal.East.Cards.Length);
    }

    [Fact]
    public void ParseBoard_PlayerNames_ParsedCorrectly()
    {
        var lin = "pn|South,West,North,East|md|2SAK2HA95DAJ875C92,SQ765HKQ64D32CAK6,S9HJ872DQT64CQJ43,|";
        var board = LinParser.ParseBoard(lin);

        Assert.NotNull(board.PlayerNames);
        Assert.Equal(4, board.PlayerNames!.Count);
        Assert.Equal("South", board.PlayerNames[0]);
        Assert.Equal("East",  board.PlayerNames[3]);
    }

    [Fact]
    public void ParseBoard_AlertedBidWithAnnotation_ParsedCorrectly()
    {
        var lin = "md|2SAK2HA95DAJ875C92,SQ765HKQ64D32CAK6,S9HJ872DQT64CQJ43,|mb|2H!|an|transfer|";
        var board = LinParser.ParseBoard(lin);

        Assert.Single(board.Bids);
        var bid = board.Bids[0];
        Assert.Equal("2H",       bid.Value);
        Assert.True(bid.IsAlerted);
        Assert.Equal("transfer", bid.Explanation);
    }

    [Fact]
    public void ParseBoard_NonAlertedBidWithAnnotation_ParsedCorrectly()
    {
        var lin = "md|2SAK2HA95DAJ875C92,SQ765HKQ64D32CAK6,S9HJ872DQT64CQJ43,|mb|1N|an|15-17|";
        var board = LinParser.ParseBoard(lin);

        var bid = board.Bids[0];
        Assert.Equal("1N",   bid.Value);
        Assert.False(bid.IsAlerted);
        Assert.Equal("15-17", bid.Explanation);
    }

    [Fact]
    public void ParseBoard_Vulnerability_AppliedToDeal()
    {
        var lin = "sv|n|md|3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65|";
        var board = LinParser.ParseBoard(lin);

        Assert.True(board.Deal.Vulnerability.NorthSouth);
        Assert.False(board.Deal.Vulnerability.EastWest);
    }

    [Fact]
    public void ParseBoard_VulnerabilityAfterDeal_StillApplied()
    {
        var lin = "md|3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65|sv|b|";
        var board = LinParser.ParseBoard(lin);

        Assert.True(board.Deal.Vulnerability.NorthSouth);
        Assert.True(board.Deal.Vulnerability.EastWest);
    }

    [Fact]
    public void ParseBoard_PlayedCards_Recorded()
    {
        var lin = "md|3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65|" +
                  "mb|3n|mb|p|mb|p|mb|p|pg||pc|HJ|pc|H3|pc|HK|pc|H2|pg||";
        var board = LinParser.ParseBoard(lin);

        Assert.Equal(4, board.Play.Count);
        Assert.Equal(new Card(Suit.Hearts, Value.Jack),  board.Play[0]);
        Assert.Equal(new Card(Suit.Hearts, Value.Three), board.Play[1]);
        Assert.Equal(new Card(Suit.Hearts, Value.King),  board.Play[2]);
        Assert.Equal(new Card(Suit.Hearts, Value.Two),   board.Play[3]);
    }

    [Fact]
    public void ParseBoard_NoteBeforeBids_AnchoredToNullIndices()
    {
        var lin = "md|3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65|nt|Welcome!|";
        var board = LinParser.ParseBoard(lin);

        Assert.Single(board.Commentary);
        Assert.Null(board.Commentary[0].BidIndex);
        Assert.Null(board.Commentary[0].PlayIndex);
        Assert.Equal("Welcome!", board.Commentary[0].Text);
    }

    [Fact]
    public void ParseBoard_NoteAfterBid_AnchoredToBidIndex()
    {
        var lin = "md|3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65|mb|p|nt|Pass.|";
        var board = LinParser.ParseBoard(lin);

        Assert.Single(board.Commentary);
        Assert.Equal(0, board.Commentary[0].BidIndex);
        Assert.Null(board.Commentary[0].PlayIndex);
    }

    [Fact]
    public void ParseBoard_NoteAfterPlay_AnchoredToPlayIndex()
    {
        var lin = "md|3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65|" +
                  "mb|3n|mb|p|mb|p|mb|p|pg||pc|HJ|nt|West leads.|";
        var board = LinParser.ParseBoard(lin);

        Assert.Single(board.Commentary);
        Assert.Null(board.Commentary[0].BidIndex);
        Assert.Equal(0, board.Commentary[0].PlayIndex);
        Assert.Equal("West leads.", board.Commentary[0].Text);
    }

    [Fact]
    public void ParseBoard_NoDealTag_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => LinParser.ParseBoard("mb|p|mb|p|"));
    }

    [Fact]
    public void ParseBoard_UnknownTagsAreSkipped()
    {
        // "zz" is not a recognised tag; should not throw
        var lin = "zz|ignored|md|3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65|";
        var board = LinParser.ParseBoard(lin);
        Assert.NotNull(board);
    }

    [Fact]
    public void ParseBoard_PassAuction_AllBidsArePass()
    {
        var lin = "md|3SAK32HAKQD32CA432,sqjthjt98d98cjt98,S54H43DKQ7654CK65|mb|p|mb|p|mb|p|mb|p|";
        var board = LinParser.ParseBoard(lin);

        Assert.Equal(4, board.Bids.Count);
        Assert.All(board.Bids, b => Assert.Equal("p", b.Value));
    }
}
