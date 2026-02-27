using BridgePlayer.Dealer;
using BridgePlayer.Models.Common;
using System.Diagnostics;

namespace BridgePlayer.HandSimulator;

/// <summary>
/// Convenience factory for creating typed <see cref="SimRunner{TData}"/> instances.
/// </summary>
public static class SimRun
{
    /// <summary>
    /// Creates a new <see cref="SimRunner{TData}"/> with the given display <paramref name="name"/>
    /// and per-deal evaluation function <paramref name="func"/>.
    /// </summary>
    /// <typeparam name="TData">The result type that accumulates simulation statistics.</typeparam>
    /// <param name="name">A human-readable name for the simulation run (used in results output).</param>
    /// <param name="func">
    /// An action that is called once for every randomly generated <see cref="Deck"/>.
    /// It receives the deck and a mutable <typeparamref name="TData"/> instance to update.
    /// </param>
    /// <returns>A configured <see cref="SimRunner{TData}"/> ready to execute.</returns>
    public static SimRunner<TData> Create<TData>(string name, Action<Deck, TData> func)
        where TData : ISimResult<TData>
    {
        return SimRunner<TData>.Create(name, func);
    }
}

/// <summary>
/// Runs a bridge deal simulation in parallel across multiple threads,
/// accumulating results into a single <typeparamref name="TData"/> instance.
/// </summary>
/// <typeparam name="TData">
/// The result type that collects per-deal statistics and can be merged across threads.
/// Must implement <see cref="ISimResult{TSelf}"/>.
/// </typeparam>
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

    /// <summary>
    /// Executes the simulation by generating <paramref name="count"/> random deals spread across
    /// <paramref name="threadCount"/> parallel workers, then merges and returns the aggregated result.
    /// </summary>
    /// <param name="count">Total number of deals to simulate across all threads.</param>
    /// <param name="threadCount">Number of parallel threads to use (default: 6).</param>
    /// <returns>
    /// A <see cref="Task{TData}"/> that resolves to the merged simulation result,
    /// including elapsed wall-clock time.
    /// </returns>
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

/// <summary>
/// Defines the contract for a simulation result type that can be created fresh,
/// merged from multiple partial results, and report basic metadata.
/// </summary>
/// <typeparam name="TSelf">The implementing type (CRTP pattern for static abstract members).</typeparam>
public interface ISimResult<TSelf>
{
    /// <summary>Gets the human-readable name of the simulation run.</summary>
    public string Name { get; }

    /// <summary>Gets the wall-clock time in milliseconds taken by the simulation.</summary>
    public long ElapsedMilliseconds { get; }

    /// <summary>Creates a new, empty instance of <typeparamref name="TSelf"/> to accumulate results into.</summary>
    /// <returns>A zeroed-out <typeparamref name="TSelf"/> instance.</returns>
    public static abstract TSelf New();

    /// <summary>
    /// Merges a collection of partial results produced by individual threads into one aggregate result.
    /// </summary>
    /// <param name="name">The simulation name to assign to the merged result.</param>
    /// <param name="results">The per-thread partial results to combine.</param>
    /// <param name="elapsedMilliseconds">Total wall-clock time for the whole run.</param>
    /// <returns>A single <typeparamref name="TSelf"/> containing the combined statistics.</returns>
    public static abstract TSelf Merge(string name, ICollection<TSelf> results, long elapsedMilliseconds);
}
