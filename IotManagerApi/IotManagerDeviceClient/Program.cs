using IotManagerDeviceClient;
using System.Text.Json;

var connectionStringsJson = await File.ReadAllTextAsync("connectionstrings.json");
var connectionStrings = JsonSerializer.Deserialize<Dictionary<string, string>>(connectionStringsJson);
if (connectionStrings == null || connectionStrings.Count == 0)
{
    Console.Error.WriteLine("No connection strings found in 'connectionstrings.json'.");
    return;
}

Console.WriteLine("Press Control+C to quit the sample.");
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cts.Cancel();
    Console.WriteLine("Cancellation requested; will exit.");
};
var devices = connectionStrings.Select(x => new Device(x.Key, x.Value)).ToList();

foreach (var d in devices) { await d.InitAsync(cts.Token); }

while (!cts.IsCancellationRequested)
{
    await Task.Delay(1000);
}

foreach (var d in devices) { await d.OnClose(); }