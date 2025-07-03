using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

namespace IotManagerDeviceClient;

public class Device
{
    public Device(string id, string connectionString)
    {
        Id = id;
        Client = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
    }

    public string Id { get; }
    public DeviceClient Client { get; }

    public async Task InitAsync(CancellationToken ct = default)
    {
        await Client.UpdateReportedPropertiesAsync(new TwinCollection
        {
            ["DateTimeLastAppLaunch"] = DateTime.UtcNow
        }, ct);

        var initialTwinValue = await Client.GetTwinAsync(ct);
        await OnDesiredPropertyChangedAsync(initialTwinValue.Properties.Desired, null);

        await Client.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChangedAsync, null, ct);
    }
    
    private async Task OnDesiredPropertyChangedAsync(TwinCollection desiredProperties, object? userContext)
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

        await Client.UpdateReportedPropertiesAsync(newReportedProperties);
    }

    public async Task OnClose(CancellationToken ct = default)
    {
        await Client.SetDesiredPropertyUpdateCallbackAsync(null, null, ct);
        await Client.CloseAsync(ct);
    }
}