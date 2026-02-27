using BridgePlayer.HandSimulator;
using BridgePlayer.Models.Common;
using BridgePlayer.Models.Wrapper;
using Spectre.Console;
using Spectre.Console.Rendering;

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

            // East: 15-17 HCP
            if (eastWrapper.Points is < 15 or > 17) return;

            // East: balanced hand – no void, no singleton, at most one doubleton
            var eastSuits = eastWrapper.SuitCounts;
            eastSuits.Deconstruct(out _, out _, out var eastSecondShortest, out var eastShortest);
            if (eastShortest < 2 || eastSecondShortest < 3) return;

            // East: no 4-card major (2D response to Stayman confirms no major)
            if (eastSuits.Spades >= 4 || eastSuits.Hearts >= 4) return;

            var westWrapper = new HandWrapper(deck.West);

            // West: 9-12 HCP
            if (westWrapper.Points is < 9 or > 12) return;

            // West: has a 4-card major (bid 2C Stayman looking for a major fit)
            var westSuits = westWrapper.SuitCounts;
            if (westSuits.Spades < 4 && westSuits.Hearts < 4) return;

            simData.HandsMatching++;

            var southWrapper = new HandWrapper(deck.South);
            var northWrapper = new HandWrapper(deck.North);

            var southSpades = southWrapper.SuitCounts.Spades;
            var northSpades = northWrapper.SuitCounts.Spades;
            var southHearts = southWrapper.SuitCounts.Hearts;
            var northHearts = northWrapper.SuitCounts.Hearts;

            // Spade analysis
            if (southSpades == 3)
            {
                simData.SouthHasThreeSpades++;
                // Assumption: with a 3-card major, partner has 5-card → 8-card fit
                if (northSpades >= 5)
                    simData.ThreeCardSpadeFitWithPartner++;
            }

            if (southSpades >= 4)
            {
                simData.SouthHasFourPlusSpades++;
                if (southSpades + northSpades >= 8)
                    simData.FourPlusCardSpadeFit++;
            }

            // Heart analysis
            if (southHearts == 3)
            {
                simData.SouthHasThreeHearts++;
                // Assumption: with a 3-card major, partner has 5-card → 8-card fit
                if (northHearts >= 5)
                    simData.ThreeCardHeartFitWithPartner++;
            }

            if (southHearts >= 4)
            {
                simData.SouthHasFourPlusHearts++;
                if (southHearts + northHearts >= 8)
                    simData.FourPlusCardHeartFit++;
            }
        });

        return new PrintableSimResult(await calc.Run(50_000_000, threadCount: 8));
    }

    internal record SimResult : ISimResult<SimResult>
    {
        public string Name { get; private init; } = null!;
        public long ElapsedMilliseconds { get; private init; }

        public long TotalHands { get; set; }
        public long HandsMatching { get; set; }

        public long SouthHasThreeSpades { get; set; }
        public long SouthHasFourPlusSpades { get; set; }
        public long ThreeCardSpadeFitWithPartner { get; set; }
        public long FourPlusCardSpadeFit { get; set; }

        public long SouthHasThreeHearts { get; set; }
        public long SouthHasFourPlusHearts { get; set; }
        public long ThreeCardHeartFitWithPartner { get; set; }
        public long FourPlusCardHeartFit { get; set; }

        public static SimResult Merge(string name, ICollection<SimResult> results, long elapsedMilliseconds)
        {
            return new()
            {
                Name = name,
                TotalHands = results.Sum(x => x.TotalHands),
                HandsMatching = results.Sum(x => x.HandsMatching),
                SouthHasThreeSpades = results.Sum(x => x.SouthHasThreeSpades),
                SouthHasFourPlusSpades = results.Sum(x => x.SouthHasFourPlusSpades),
                ThreeCardSpadeFitWithPartner = results.Sum(x => x.ThreeCardSpadeFitWithPartner),
                FourPlusCardSpadeFit = results.Sum(x => x.FourPlusCardSpadeFit),
                SouthHasThreeHearts = results.Sum(x => x.SouthHasThreeHearts),
                SouthHasFourPlusHearts = results.Sum(x => x.SouthHasFourPlusHearts),
                ThreeCardHeartFitWithPartner = results.Sum(x => x.ThreeCardHeartFitWithPartner),
                FourPlusCardHeartFit = results.Sum(x => x.FourPlusCardHeartFit),
                ElapsedMilliseconds = elapsedMilliseconds,
            };
        }

        public static SimResult New()
        {
            return new();
        }
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

            IRenderable spadeThreeChart = _result.SouthHasThreeSpades > 0
                ? new BreakdownChart()
                    .UseValueFormatter(val => $"{val} ({val / _result.SouthHasThreeSpades * 100:F2}%)")
                    .AddItem("Partner has 5+ spades (8-card fit)", _result.ThreeCardSpadeFitWithPartner, Color.DarkGreen)
                    .AddItem("No fit", _result.SouthHasThreeSpades - _result.ThreeCardSpadeFitWithPartner, Color.Grey)
                : new Markup("[grey]No data[/]");

            IRenderable spadeFourChart = _result.SouthHasFourPlusSpades > 0
                ? new BreakdownChart()
                    .UseValueFormatter(val => $"{val} ({val / _result.SouthHasFourPlusSpades * 100:F2}%)")
                    .AddItem("8-card spade fit", _result.FourPlusCardSpadeFit, Color.DarkGreen)
                    .AddItem("No fit", _result.SouthHasFourPlusSpades - _result.FourPlusCardSpadeFit, Color.Grey)
                : new Markup("[grey]No data[/]");

            IRenderable heartThreeChart = _result.SouthHasThreeHearts > 0
                ? new BreakdownChart()
                    .UseValueFormatter(val => $"{val} ({val / _result.SouthHasThreeHearts * 100:F2}%)")
                    .AddItem("Partner has 5+ hearts (8-card fit)", _result.ThreeCardHeartFitWithPartner, Color.Red)
                    .AddItem("No fit", _result.SouthHasThreeHearts - _result.ThreeCardHeartFitWithPartner, Color.Grey)
                : new Markup("[grey]No data[/]");

            IRenderable heartFourChart = _result.SouthHasFourPlusHearts > 0
                ? new BreakdownChart()
                    .UseValueFormatter(val => $"{val} ({val / _result.SouthHasFourPlusHearts * 100:F2}%)")
                    .AddItem("8-card heart fit", _result.FourPlusCardHeartFit, Color.Red)
                    .AddItem("No fit", _result.SouthHasFourPlusHearts - _result.FourPlusCardHeartFit, Color.Grey)
                : new Markup("[grey]No data[/]");

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
                .AddRow("[bold]Spades[/] - South has exactly 3 spades (partner assumed 5-card):")
                .AddRow(spadeThreeChart)
                .AddEmptyRow()
                .AddRow("[bold]Spades[/] - South has 4+ spades (8-card combined fit):")
                .AddRow(spadeFourChart)
                .AddEmptyRow()
                .AddRow("[bold]Hearts[/] - South has exactly 3 hearts (partner assumed 5-card):")
                .AddRow(heartThreeChart)
                .AddEmptyRow()
                .AddRow("[bold]Hearts[/] - South has 4+ hearts (8-card combined fit):")
                .AddRow(heartFourChart);

            return rows;
        }
    }
}
