using BridgePlayer.Console.Sims;
using Spectre.Console;

var console = AnsiConsole.Create(new AnsiConsoleSettings
{
    Interactive = InteractionSupport.Detect,
    Ansi = AnsiSupport.Detect,
    ColorSystem = ColorSystemSupport.Detect,
});

var res = await console.Status()
    .Spinner(Spinner.Known.Pong)
    .StartAsync("Running simulation", async ctx =>
    {
        return await GaborWackyConvention.Run();
    });

console.Write(new Panel(res.CreateRenderable())
    .Border(BoxBorder.Rounded)
    .Header(res.Name)
    .Padding(new Padding(2, 1)));