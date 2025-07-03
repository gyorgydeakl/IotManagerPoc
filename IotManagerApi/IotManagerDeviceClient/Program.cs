using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

const string deviceConnectionString = "HostName=cmgtestIoTHub2.azure-devices.net;DeviceId=device01;SharedAccessKey=YI5OZ9kAamztXRtvHU+JJW7LhiCpaCHBytyF3NJLCaE=";
var sampleRunningTime = TimeSpan.FromHours(8);
var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);

Console.WriteLine("Press Control+C to quit the sample.");
using var cts = new CancellationTokenSource(sampleRunningTime);
Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    cts.Cancel();
    Console.WriteLine("Cancellation requested; will exit.");
};

await deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChangedAsync, null, cts.Token);
await deviceClient.UpdateReportedPropertiesAsync(new TwinCollection
{
    ["DateTimeLastAppLaunch"] = DateTime.UtcNow
}, cts.Token);

var initialTwinValue = await deviceClient.GetTwinAsync();
await OnDesiredPropertyChangedAsync(initialTwinValue.Properties.Desired, null);

while (!cts.IsCancellationRequested)
{
    await Task.Delay(1000);
}

// unsubscribe a callback for properties.
await deviceClient.SetDesiredPropertyUpdateCallbackAsync(null, null);

return;

async Task OnDesiredPropertyChangedAsync(TwinCollection desiredProperties, object? userContext)
{
    var newReportedProperties = new TwinCollection();

    Console.WriteLine("\tDesired properties requested:");
    Console.WriteLine($"\t{desiredProperties.ToJson()}");

    foreach (KeyValuePair<string, object> desiredProperty in desiredProperties)
    {
        Console.WriteLine($"Setting {desiredProperty.Key} to {desiredProperty.Value}.");
        newReportedProperties[desiredProperty.Key] = desiredProperty.Value;
    }

    Console.WriteLine("\tAlso setting current time as reported property");
    newReportedProperties["DateTimeLastDesiredPropertyChangeReceived"] = DateTime.UtcNow;

    await deviceClient.UpdateReportedPropertiesAsync(newReportedProperties);
}