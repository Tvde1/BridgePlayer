# BridgePlayer

A .NET library and console application for running Monte-Carlo simulations over randomly generated bridge deals.

## Overview

BridgePlayer lets you define a predicate over a randomly shuffled bridge deal and run it millions of times in parallel to gather statistical insight.  
The canonical example bundled with the project simulates *Gabor's Wacky Convention* and reports how often the convention's preemptive opening bid is available, and how well partner's hand supports it.

## Projects

| Project | Description |
|---|---|
| `BridgePlayer.Models` | Core domain types: `Card`, `Hand`, `Deck`, `Suit`, `Value`, `HandWrapper`, `SuitCounts`. |
| `BridgePlayer.Dealer` | `HandGenerator` — shuffles a 52-card deck and splits it into four 13-card hands. |
| `BridgePlayer.HandSimulator` | `SimRunner<TData>` — parallel simulation engine; `ISimResult<TSelf>` — result contract. |
| `BridgePlayer.Console` | Console entry point; shows built-in simulations using [Spectre.Console](https://spectreconsole.net/). |

## Getting Started

### Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download) or later

### Build

```bash
dotnet build
```

### Run the console app

```bash
dotnet run --project BridgePlayer.Console
```

The console app runs the *Gabor's Wacky Convention* simulation over 50 million randomly shuffled deals and displays a breakdown of the results.

## Writing Your Own Simulation

1. Implement `ISimResult<TSelf>` to define what statistics you want to collect:

```csharp
record MyResult : ISimResult<MyResult>
{
    public string Name { get; private init; } = null!;
    public long ElapsedMilliseconds { get; private init; }
    public long MatchingHands { get; set; }

    public static MyResult New() => new();

    public static MyResult Merge(string name, ICollection<MyResult> results, long elapsedMs) => new()
    {
        Name = name,
        ElapsedMilliseconds = elapsedMs,
        MatchingHands = results.Sum(r => r.MatchingHands),
    };
}
```

2. Create a runner with `SimRun.Create` and call `Run`:

```csharp
var runner = SimRun.Create("My Simulation", (Deck deck, MyResult data) =>
{
    var south = new HandWrapper(deck.South);
    if (south.Points >= 12)
        data.MatchingHands++;
});

var result = await runner.Run(count: 1_000_000, threadCount: 4);
Console.WriteLine($"{result.MatchingHands} hands matched out of 1 000 000.");
```

## Architecture

```
BridgePlayer.Console
    └── BridgePlayer.HandSimulator   (SimRunner / ISimResult)
            └── BridgePlayer.Dealer  (HandGenerator)
                    └── BridgePlayer.Models  (Card / Hand / Deck / Suit / Value)
```

`HandWrapper` and `SuitCounts` are `ref struct` types, keeping per-deal analysis entirely on the stack for maximum throughput during simulation.

## License

See [LICENSE](LICENSE) for details.
