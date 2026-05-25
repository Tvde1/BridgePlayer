using BridgePlayer.Models.Common;

namespace BridgePlayer.Models.LIN;

/// <summary>
/// Represents a fully-parsed LIN board, including the deal, bidding, play, and teaching annotations.
/// </summary>
/// <param name="Deal">The deal (hands and vulnerability) for this board.</param>
/// <param name="PlayerNames">
/// The player names in South, West, North, East order as given by the <c>pn</c> tag,
/// or <see langword="null"/> if the tag was absent.
/// </param>
/// <param name="Bids">
/// The auction, one <see cref="LinBid"/> per <c>mb</c> tag, in order.
/// Each bid carries its alert flag and optional explanation.
/// </param>
/// <param name="Play">
/// The played cards, one <see cref="Card"/> per <c>pc</c> tag, in trick-then-clockwise order.
/// </param>
/// <param name="Commentary">
/// The teaching notes (<c>nt</c> tags) in source order, each anchored to an auction or play index.
/// </param>
public sealed record LinBoardRecord(
    LinDeal Deal,
    IReadOnlyList<string>? PlayerNames,
    IReadOnlyList<LinBid> Bids,
    IReadOnlyList<Card> Play,
    IReadOnlyList<LinCommentary> Commentary);
