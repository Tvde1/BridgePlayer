namespace BridgePlayer.Models.LIN;

/// <summary>
/// Represents a single bid in the auction as decoded from a LIN <c>mb</c> tag.
/// </summary>
/// <param name="Value">
/// The bid string, e.g. <c>"p"</c> (pass), <c>"d"</c> (double), <c>"r"</c> (redouble),
/// <c>"1N"</c>, <c>"2H"</c>. The alert suffix <c>!</c> is stripped; see <paramref name="IsAlerted"/>.
/// </param>
/// <param name="IsAlerted">
/// <see langword="true"/> if the bid carried a <c>!</c> suffix in the LIN source.
/// </param>
/// <param name="Explanation">
/// The alert or announcement text from the subsequent <c>an</c> tag,
/// or <see langword="null"/> if none was present.
/// </param>
public readonly record struct LinBid(string Value, bool IsAlerted, string? Explanation = null);
