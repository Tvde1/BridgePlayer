using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;

namespace BridgePlayer.Models.LIN;

/// <summary>
/// Represents a bridge deal as decoded from a LIN <c>md</c> tag.
/// </summary>
/// <param name="Dealer">The direction that dealt (and typically opens the bidding).</param>
/// <param name="Vulnerability">The vulnerability state for this deal.</param>
/// <param name="South">The 13-card hand held by South.</param>
/// <param name="West">The 13-card hand held by West.</param>
/// <param name="North">The 13-card hand held by North.</param>
/// <param name="East">The 13-card hand held by East.</param>
public sealed record LinDeal(
    Direction Dealer,
    Vulnerability Vulnerability,
    Hand South,
    Hand West,
    Hand North,
    Hand East);
