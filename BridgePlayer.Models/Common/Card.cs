using BridgePlayer.Models.Enums;

namespace BridgePlayer.Models.Common;

public readonly record struct Card(Suit Suit, Value Value)
{
    public byte HCP { get; } = (byte)Value >= (byte)Value.Jack
        ? (byte)((byte)Value - (byte)Value.Ten)
        : (byte)0;
}