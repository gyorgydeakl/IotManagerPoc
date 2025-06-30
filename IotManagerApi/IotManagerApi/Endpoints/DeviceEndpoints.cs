using FastEndpoints;
using IotManagerApi.Config;
using IotManagerApi.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Devices;

namespace IotManagerApi.Endpoints;

public class CreateDeviceEndpoint(RegistryManager registry, IotHubConfig iotHubConfig)
    : Endpoint<CreateDeviceRequest, Created<string>>
{
    public override void Configure()
    {
        base.Configure();
        Post("/devices");
        AllowAnonymous();
        Description(x => x
            .WithName("CreateDevice")
            .WithSummary("Creates a new IoT device")
            .WithOpenApi());
    }

    public override async Task<Created<string>> ExecuteAsync(CreateDeviceRequest req, CancellationToken ct)
    {
        var createdDevice = await registry.AddDeviceAsync(new Device(req.DeviceId), ct);
        var connectionString = $"HostName={iotHubConfig.HostName};DeviceId={createdDevice.Id};SharedAccessKey={createdDevice.Authentication.SymmetricKey.PrimaryKey}";
        return TypedResults.Created($"/devices/{createdDevice.Id}", connectionString);
    }
}


public class ListDevicesEndpoint(RegistryManager registry) : Endpoint<CreateDeviceRequest, Ok<IEnumerable<DeviceDto>>>
{
    public override void Configure()
    {
        base.Configure();
        Post("/devices");
        AllowAnonymous();
        Description(x => x
            .WithName("ListDevices")
            .WithSummary("Lists all IoT devices")
            .WithOpenApi());
    }

    public override async Task<Ok<IEnumerable<DeviceDto>>> ExecuteAsync(CreateDeviceRequest req, CancellationToken ct)
    {
        var twins = await registry.CreateQuery("select * from devices").GetNextAsTwinAsync();
        var result = twins.Select(t => t.ToDeviceData());
        return TypedResults.Ok(result);
    }
}

public class GetDeviceByIdEndpoint(RegistryManager registry) : Endpoint<string, IResult>
{
    public override void Configure()
    {
        base.Configure();
        Get("/devices/{deviceId}");
        AllowAnonymous();
        Description(x => x
            .WithName("GetDeviceById")
            .WithSummary("Retrieves a specific IoT device by its ID")
            .WithOpenApi());
    }

    public override async Task<IResult> ExecuteAsync(string deviceId, CancellationToken ct)
    {
        var twin = await registry.GetTwinAsync(deviceId, ct);
        return twin == null ? Results.NotFound() : Results.Ok(twin.ToDeviceData());
    }
}