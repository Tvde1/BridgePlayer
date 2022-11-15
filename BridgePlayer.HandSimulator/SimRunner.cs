using BridgePlayer.Dealer;
using BridgePlayer.Models.Common;
using System.Diagnostics;

namespace BridgePlayer.HandSimulator;

public static class SimRun
{
    public static SimRunner<TData> Create<TData>(string name, Action<Deck, TData> func)
        where TData : ISimResult<TData>
    {
        return SimRunner<TData>.Create(name, func);
    }
}

public class SimRunner<TData>
    where TData : ISimResult<TData>
{
    private readonly string _name;
    private readonly Action<Deck, TData> _func;

    private SimRunner(string name, Action<Deck, TData> func)
    {
        _name = name;
        _func = func;
    }

    internal static SimRunner<TData> Create(string name, Action<Deck, TData> func) =>
        new SimRunner<TData>(name, func);

    public Task<TData> Run(int count, int threadCount = 6)
    {
        count /= threadCount;
        var results = new List<TData>(threadCount);

        var actions = Enumerable.Repeat(() =>
        {
            var result = TData.New();
            var handGenerator = new HandGenerator();
            {
                for (var runCycle = 0; runCycle < count; runCycle++)
                {
                    var newDeck = handGenerator.ShuffleNewDeck();
                    _func(newDeck, result);
                }
            }
            results.Add(result);
        }, threadCount).ToArray();

        var sw = Stopwatch.StartNew();
        Parallel.Invoke(new ParallelOptions
        {
            MaxDegreeOfParallelism = threadCount,
        }, actions);
        sw.Stop();

        return Task.FromResult(TData.Merge(_name, results, sw.ElapsedMilliseconds));
    }
}

public interface ISimResult<TSelf>
{
    public string Name { get; }
    public long ElapsedMilliseconds { get; }

    public static abstract TSelf New();
    public static abstract TSelf Merge(string name, ICollection<TSelf> results, long elapsedMilliseconds);
}
