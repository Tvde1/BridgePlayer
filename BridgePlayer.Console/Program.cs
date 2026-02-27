using BridgePlayer.Console.Sims;
using Spectre.Console;

var console = AnsiConsole.Create(new AnsiConsoleSettings
{
    Interactive = InteractionSupport.Detect,
    Ansi = AnsiSupport.Detect,
    ColorSystem = ColorSystemSupport.Detect,
});

var res1 = await console.Status()
    .Spinner(Spinner.Known.Pong)
    .StartAsync("Running Gabor's Wacky Convention simulation", async ctx =>
    {
        return await GaborWackyConvention.Run();
    });

console.Write(new Panel(res1.CreateRenderable())
    .Border(BoxBorder.Rounded)
    .Header(res1.Name)
    .Padding(new Padding(2, 1)));

var res2 = await console.Status()
    .Spinner(Spinner.Known.Pong)
    .StartAsync("Running Stayman After Opponents 1NT simulation", async ctx =>
    {
        return await StaymanAfterOpponents1NT.Run();
    });

console.Write(new Panel(res2.CreateRenderable())
    .Border(BoxBorder.Rounded)
    .Header(res2.Name)
    .Padding(new Padding(2, 1)));