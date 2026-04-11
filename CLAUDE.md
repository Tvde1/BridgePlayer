# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run the console app (50M deal simulation of Gabor's Wacky Convention)
dotnet run --project BridgePlayer.Console

# Run benchmarks
dotnet run --project BridgePlayer.Benchmarks --configuration Release -- --exporters fullJson --filter '*'
```

There are no automated tests in this project — correctness is validated via the simulation output and benchmarks.

## Architecture

Dependency chain (inner → outer):

```
BridgePlayer.Models → BridgePlayer.Dealer → BridgePlayer.HandSimulator → BridgePlayer.Console
                                                                        → BridgePlayer.Benchmarks
```

**BridgePlayer.Models** — Core domain: `Card`, `Hand`, `Deck`, `Suit`, `Value` enums.  
`HandWrapper` and `SuitCounts` are `readonly ref struct` types — intentionally stack-allocated to avoid heap pressure in the hot simulation loop. This is a deliberate performance constraint; do not convert them to classes.

**BridgePlayer.Dealer** — `HandGenerator` shuffles a 52-card deck and splits it into four 13-card `Hand` instances (`deck.North/South/East/West`).

**BridgePlayer.HandSimulator** — `SimRunner<TData>` is the parallel engine. It spawns `threadCount` workers (default 6), each running `count/threadCount` deals, then merges thread-local `TData` results via `TData.Merge(...)`.  
`ISimResult<TSelf>` uses the CRTP pattern (C# static abstract members) — implementors must provide `static New()` and `static Merge(...)`.

**BridgePlayer.Console** — Wires up concrete simulations and renders results with Spectre.Console.

**BridgePlayer.Benchmarks** — BenchmarkDotNet benchmarks. CI runs these on every push/PR and tracks regressions (>10% slowdown fails the pipeline) via `benchmark-action/github-action-benchmark`, storing history on `gh-pages`.

## Adding a New Simulation

1. Create a `record` implementing `ISimResult<TSelf>` (see `SimRunner.cs` for the interface).
2. Call `SimRun.Create("name", (Deck deck, MyResult data) => { ... })` and `.Run(count)`.
3. Inside the lambda, wrap hands with `new HandWrapper(deck.South)` to access `.Points` and `.SuitCounts`.
