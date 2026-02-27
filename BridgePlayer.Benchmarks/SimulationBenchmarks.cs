using BenchmarkDotNet.Attributes;
using BridgePlayer.Dealer;
using BridgePlayer.HandSimulator;
using BridgePlayer.Models.Common;
using BridgePlayer.Models.Enums;
using BridgePlayer.Models.Wrapper;

namespace BridgePlayer.Benchmarks;

[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 5)]
[MemoryDiagnoser]
public class SimulationBenchmarks
{
    private HandGenerator _handGenerator = null!;
    private SimRunner<GaborWackyResult> _sim = null!;

    [GlobalSetup]
    public void Setup()
    {
        _handGenerator = new HandGenerator();
        _sim = SimRun.Create<GaborWackyResult>("GaborWacky", static (deck, result) =>
        {
            result.TotalHands++;

            var southWrapper = new HandWrapper(deck.South);

            if (southWrapper.Points is < 4 or > 11)
                return;

            if (!southWrapper.SuitCounts.HasCount(6, out _))
                return;

            if (!southWrapper.SuitCounts.HasCount(4, out var fourCardSuit))
                return;

            result.HandsPreempted++;

            var northWrapper = new HandWrapper(deck.North);

            switch (northWrapper.SuitCounts[fourCardSuit])
            {
                case 0:
                    result.HandsWithPartnerVoid++;
                    break;
                case 1:
                    result.HandsWithPartnerSingleton++;
                    break;
                case > 4:
                    result.HandsWithPartnerFit++;
                    break;
            }
        });
    }

    /// <summary>
    /// Benchmarks the raw deck shuffle operation.
    /// </summary>
    [Benchmark]
    public Deck ShuffleDeck()
    {
        return _handGenerator.ShuffleNewDeck();
    }

    /// <summary>
    /// Benchmarks shuffling one deck and evaluating the GaborWacky convention for that hand.
    /// </summary>
    [Benchmark]
    public bool GaborWackyOneDeck()
    {
        var deck = _handGenerator.ShuffleNewDeck();
        var southWrapper = new HandWrapper(deck.South);

        if (southWrapper.Points is < 4 or > 11)
            return false;

        if (!southWrapper.SuitCounts.HasCount(6, out _))
            return false;

        if (!southWrapper.SuitCounts.HasCount(4, out var fourCardSuit))
            return false;

        var northWrapper = new HandWrapper(deck.North);
        return northWrapper.SuitCounts[fourCardSuit] <= 1;
    }

    /// <summary>
    /// Benchmarks the full SimRunner mechanism running 10,000 iterations on a single thread.
    /// This measures the throughput of the simulation engine end-to-end.
    /// </summary>
    [Benchmark]
    public async Task GaborWackySimulation()
    {
        await _sim.Run(10_000, threadCount: 1);
    }

    internal record GaborWackyResult : ISimResult<GaborWackyResult>
    {
        public string Name { get; private init; } = null!;
        public long ElapsedMilliseconds { get; private init; }

        public long TotalHands { get; set; }
        public long HandsPreempted { get; set; }
        public long HandsWithPartnerVoid { get; set; }
        public long HandsWithPartnerSingleton { get; set; }
        public long HandsWithPartnerFit { get; set; }

        public static GaborWackyResult New() => new();

        public static GaborWackyResult Merge(string name, ICollection<GaborWackyResult> results, long elapsedMilliseconds)
        {
            return new()
            {
                Name = name,
                TotalHands = results.Sum(x => x.TotalHands),
                HandsPreempted = results.Sum(x => x.HandsPreempted),
                HandsWithPartnerVoid = results.Sum(x => x.HandsWithPartnerVoid),
                HandsWithPartnerSingleton = results.Sum(x => x.HandsWithPartnerSingleton),
                HandsWithPartnerFit = results.Sum(x => x.HandsWithPartnerFit),
                ElapsedMilliseconds = elapsedMilliseconds,
            };
        }
    }
}
