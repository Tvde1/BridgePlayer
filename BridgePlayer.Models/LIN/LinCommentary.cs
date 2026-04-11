namespace BridgePlayer.Models.LIN;

/// <summary>
/// Represents a <c>nt</c> commentary note attached to a position in the auction or play sequence.
/// </summary>
/// <param name="BidIndex">
/// Zero-based index of the bid after which this note appears,
/// or <see langword="null"/> when the note is in the play phase.
/// </param>
/// <param name="PlayIndex">
/// Zero-based index of the played card after which this note appears,
/// or <see langword="null"/> when the note is in the auction phase.
/// </param>
/// <param name="Text">The note text. May contain suit symbols encoded as <c>!s !h !d !c</c>.</param>
public readonly record struct LinCommentary(int? BidIndex, int? PlayIndex, string Text);
