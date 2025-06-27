using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace IotManagerApi;

public static class Endpoints
{
    public static void RegisterAppEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/devices", async (RegistryManager registry) =>
            {
                var twins = await registry.CreateQuery("select * from devices").GetNextAsTwinAsync();
                var result = twins.Select(t => t.ToDeviceData());
                return Results.Ok(result);
            })
            .WithName("GetDevices")
            .WithOpenApi();
        endpoints.MapGet("/devices/{deviceId}", async (RegistryManager registry, string deviceId) =>
            {
                var twin = await registry.GetTwinAsync(deviceId);
                return twin == null ? Results.NotFound() : Results.Ok(twin.ToDeviceData());
            })
            .WithName("GetDeviceById")
            .WithOpenApi();
        endpoints.MapPost("/devices", async (RegistryManager registry, IConfiguration cfg, [FromBody] CreateDeviceRequest device) =>
            {
                var createdDevice = await registry.AddDeviceAsync(new Device(device.DeviceId));
                var connectionString = $"HostName={cfg["IotHubHostName"]};DeviceId={createdDevice.Id};SharedAccessKey={createdDevice.Authentication.SymmetricKey.PrimaryKey}";
                return Results.Created($"/devices/{createdDevice.Id}", connectionString);
            })
            .WithName("CreateDevice")
            .WithOpenApi();
    }
}

public record CreateDeviceRequest
{
    public required string DeviceId { get; init; }
    public required Twin Twin { get; init; }
};