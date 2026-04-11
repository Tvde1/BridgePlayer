using BridgePlayer.HandSimulator;
using BridgePlayer.Models.Common;
using BridgePlayer.Models.Wrapper;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Runtime.InteropServices;

namespace BridgePlayer.Console.Sims;

/// <summary>
/// Simulates the scenario after opponents use Stayman:
///   E      S      W      N
///  (1NT) - pass - (2C!) - pass
///  (2D)  - pass - (3NT) - ...
///
/// East: 15-17 HCP, balanced hand, no 4-card major (shown by 2D response).
/// West: 9-12 HCP, has a 4-card major (bid 2C Stayman).
///
/// Determines which major South should lead, comparing the likelihood
/// of an 8-card fit in spades vs. hearts for the N/S partnership.
/// </summary>
internal static class StaymanAfterOpponents1NT
{
    public static async Task<PrintableSimResult> Run()
    {
        var calc = SimRun.Create("Stayman After Opponents 1NT", (Deck deck, SimResult simData) =>
        {
            simData.TotalHands++;

            var eastWrapper = new HandWrapper(deck.East);

            // East: 1NT opening (15-17 HCP, balanced, no 4-card major)
            if (!eastWrapper.IsOneNoTrump()) return;

            var westWrapper = new HandWrapper(deck.West);

            // West: 9-12 HCP, has a 4-card major (bid 2C Stayman)
            if (westWrapper.Points is < 9 or > 12) return;
            if (!westWrapper.SuitCounts.HasFourCardMajor()) return;

            simData.HandsMatching++;

            var southWrapper = new HandWrapper(deck.South);
            var northWrapper = new HandWrapper(deck.North);

            var southSpades = southWrapper.SuitCounts.Spades;
            var northSpades = northWrapper.SuitCounts.Spades;
            var southHearts = southWrapper.SuitCounts.Hearts;
            var northHearts = northWrapper.SuitCounts.Hearts;

            var key = (southSpades, southHearts);
            ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(simData.Holdings, key, out _);
            entry.Count++;
            if (southSpades + northSpades >= 8) entry.SpadeFit++;
            if (southHearts + northHearts >= 8) entry.HeartFit++;
        });

        return new PrintableSimResult(await calc.Run(50_000_000, threadCount: 8));
    }

    internal struct MajorFitCounts
    {
        /// <summary>Total occurrences of this (southSpades, southHearts) holding combination.</summary>
        public long Count;
        /// <summary>Hands where South and North together hold 8+ spades (8-card spade fit).</summary>
        public long SpadeFit;
        /// <summary>Hands where South and North together hold 8+ hearts (8-card heart fit).</summary>
        public long HeartFit;
    }

    internal record SimResult : ISimResult<SimResult>
    {
        public string Name { get; private init; } = null!;
        public long ElapsedMilliseconds { get; private init; }

        public long TotalHands { get; set; }
        public long HandsMatching { get; set; }

        // Key: (southSpades, southHearts); tracks fit counts for each South major-holding combination
        public Dictionary<(byte southSpades, byte southHearts), MajorFitCounts> Holdings { get; } = new();

        public static SimResult Merge(string name, ICollection<SimResult> results, long elapsedMilliseconds)
        {
            var merged = new SimResult
            {
                Name = name,
                TotalHands = results.Sum(x => x.TotalHands),
                HandsMatching = results.Sum(x => x.HandsMatching),
                ElapsedMilliseconds = elapsedMilliseconds,
            };

            foreach (var result in results)
            {
                foreach (var (key, data) in result.Holdings)
                {
                    ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(merged.Holdings, key, out _);
                    entry.Count += data.Count;
                    entry.SpadeFit += data.SpadeFit;
                    entry.HeartFit += data.HeartFit;
                }
            }

            return merged;
        }

        public static SimResult New() => new();
    }

    public record PrintableSimResult
    {
        private readonly SimResult _result;

        public PrintableSimResult(SimResult result)
        {
            _result = result;
        }

        public string Name => _result.Name;

        public IRenderable CreateRenderable()
        {
            var totalOverview = _result.TotalHands;
            var overview = new BreakdownChart()
                .UseValueFormatter(val => $"{val} ({val / totalOverview * 100:F2}%)")
                .AddItem("Matching hands", _result.HandsMatching, Color.Blue)
                .AddItem("Other", totalOverview - _result.HandsMatching, Color.Grey);

            var innerGrid = new Grid()
                .AddColumn()
                .AddColumn(new GridColumn().RightAligned())
                .AddRow($"Processed [yellow]{_result.TotalHands}[/] hands.",
                    $"Took [yellow]{_result.ElapsedMilliseconds / (double)1000:F2}[/] seconds.");

            innerGrid.Width = 1000;

            var rows = new Grid()
                .AddColumn()
                .AddRow(innerGrid)
                .AddEmptyRow()
                .AddRow("Hands matching bidding sequence (E: 1NT balanced no major, W: Stayman with major):")
                .AddRow(overview)
                .AddEmptyRow()
                .AddRow("[bold]South major holdings[/] - fit probability by (spades, hearts) count:");

            foreach (var ((spades, hearts), data) in _result.Holdings
                .OrderBy(x => x.Key.southSpades)
                .ThenBy(x => x.Key.southHearts))
            {
                if (data.Count == 0) continue;

                rows.AddEmptyRow();
                rows.AddRow($"South has [blue]{spades}♠[/]  [red]{hearts}♥[/]:");

                if (spades > 0)
                {
                    rows.AddRow("  ♠ Spade fit (N+S ≥ 8):");
                    rows.AddRow(new BreakdownChart()
                        .UseValueFormatter(val => $"{val} ({val / data.Count * 100:F2}%)")
                        .AddItem("8-card spade fit", data.SpadeFit, Color.DarkGreen)
                        .AddItem("No spade fit", data.Count - data.SpadeFit, Color.Grey));
                }

                if (hearts > 0)
                {
                    rows.AddRow("  ♥ Heart fit (N+S ≥ 8):");
                    rows.AddRow(new BreakdownChart()
                        .UseValueFormatter(val => $"{val} ({val / data.Count * 100:F2}%)")
                        .AddItem("8-card heart fit", data.HeartFit, Color.Red)
                        .AddItem("No heart fit", data.Count - data.HeartFit, Color.Grey));
                }
            }

            return rows;
        }
    }
}
