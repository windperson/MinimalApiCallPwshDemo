using DemoWebApi.DTOs;
using DemoWebApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddScoped<IPowerShellInvokeService, DuplicateFileFinderInvokeService>();
var app = builder.Build();

app.MapGet("/", () => "Web API call PowerShell script demo, use POST /api/find_duplicate to find duplicate files");

app.MapPost("/api/find_duplicate", async (IPowerShellInvokeService powerShellInvokeService, ApiInputDto input) =>
{
    var cts = new CancellationTokenSource();
    var result = await powerShellInvokeService.RunScriptAsync(input, cts.Token);
    return result;
});
app.Run();