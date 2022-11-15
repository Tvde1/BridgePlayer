using System;
using BridgePlayer.HandSimulator;
using BridgePlayer.Models.Common;
using BridgePlayer.Models.Wrapper;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BridgePlayer.Console.Sims;

internal static class GaborWackyConvention
{
    public static async Task<PrintableSimResult> Run()
    {
        var calc = SimRun.Create("Gabor's Wacky Convention", (Deck deck, SimResult simData) =>
        {
            simData.TotalHands++;

            var southWrapper = new HandWrapper(deck.South);

            var hasEnoughPoints = southWrapper.Points is >= 4 and <= 11;

            if (!hasEnoughPoints)
            {
                return;
            }

            var hasSixCard = southWrapper.SuitCounts.HasCount(6, out _);

            if (!hasSixCard)
            {
                return;
            }

            var hasFourCard = southWrapper.SuitCounts.HasCount(4, out var fourCardSuit);
            if (!hasFourCard)
            {
                return;
            }

            simData.HandsPreempted++;

            var northWrapper = new HandWrapper(deck.North);

            if (northWrapper.SuitCounts[fourCardSuit] == 1)
            {
            }

            switch (northWrapper.SuitCounts[fourCardSuit])
            {
                case 0:
                    simData.HandsWithPartnerVoid++;
                    break;
                case 1:
                    simData.HandsWithPartnerSingleton++;
                    break;
                case > 4:
                    simData.HandsWithPartnerFit++;
                    break;
            }
        });

        return new PrintableSimResult(await calc.Run(50_000_000, threadCount: 8));
    }

    internal record SimResult : ISimResult<SimResult>
    {
        public string Name { get; private init; } = null!;
        public long ElapsedMilliseconds { get; private init; }

        public long TotalHands { get; set; }
        public long HandsPreempted { get; set; }
        public long HandsWithPartnerVoid { get; set; }
        public long HandsWithPartnerSingleton { get; set; }
        public long HandsWithPartnerFit { get; set; }

        public static SimResult Merge(string name, ICollection<SimResult> results, long elapsedMilliseconds)
        {
            return new()
            {
                Name = name,
                TotalHands = results.Sum(x => x.TotalHands),
                HandsPreempted = results.Sum(x => x.HandsPreempted),
                HandsWithPartnerVoid = results.Sum(x => x.HandsWithPartnerVoid),
                HandsWithPartnerSingleton = results.Sum(x => x.HandsWithPartnerSingleton),
                HandsWithPartnerFit = results.Sum(x => x.HandsWithPartnerFit),
                ElapsedMilliseconds = elapsedMilliseconds
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
                .AddItem("Preempt", _result.HandsPreempted, Color.Blue)
                .AddItem("No preempt", totalOverview - _result.HandsPreempted, Color.Grey);

            var totalSuccess = _result.HandsPreempted;
            var successHands = new BreakdownChart()
                .UseValueFormatter(val => $"{val} ({val / totalSuccess * 100:F2}%)")
                .AddItem("Void", _result.HandsWithPartnerVoid, Color.DarkGreen)
                .AddItem("Singleton", _result.HandsWithPartnerSingleton, Color.Green)
                .AddItem("Other", totalSuccess - _result.HandsWithPartnerVoid - _result.HandsWithPartnerSingleton - _result.HandsWithPartnerFit, Color.Grey)
                .AddItem("Fit", _result.HandsWithPartnerFit, Color.Gold3);


            var innerGrid = new Grid()
                .AddColumn()
                .AddColumn(new GridColumn().RightAligned())
                .AddRow($"Processed [yellow]{_result.TotalHands}[/] hands.",
                    $"Took [yellow]{_result.ElapsedMilliseconds / (double)1000:F2}[/] seconds.");

            innerGrid.Width = 1000;

            var rows = new Grid()
                .AddColumn()
                .AddRow(innerGrid)
                //.AddRow(new Panel(innerGrid).Border(BoxBorder.None).Padding(new Padding(0)).Expand())
                .AddEmptyRow()
                .AddRow("Hands that offered a possible preempt:")
                .AddRow(overview)
                .AddEmptyRow()
                .AddRow("Partner's holding in the 4-card:")
                .AddRow(successHands);

            return rows;
        }
    }
}
