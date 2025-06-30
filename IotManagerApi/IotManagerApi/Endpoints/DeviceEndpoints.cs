using FastEndpoints;
using IotManagerApi.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Devices;

namespace IotManagerApi.Endpoints;

public class CreateDeviceEndpoint : Endpoint<CreateDeviceRequest, Created<string>>
{
    private readonly RegistryManager _registry;
    private readonly IotHubConfig _iotHubConfig;

    public CreateDeviceEndpoint(RegistryManager registry, IotHubConfig iotHubConfig)
    {
        _registry = registry;
        _iotHubConfig = iotHubConfig;
    }

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
        var createdDevice = await _registry.AddDeviceAsync(new Device(req.DeviceId), ct);
        var connectionString = $"HostName={_iotHubConfig.HostName};DeviceId={createdDevice.Id};SharedAccessKey={createdDevice.Authentication.SymmetricKey.PrimaryKey}";
        return TypedResults.Created($"/devices/{createdDevice.Id}", connectionString);
    }
}


public class ListDevicesEndpoint : Endpoint<CreateDeviceRequest, Ok<IEnumerable<DeviceDto>>>
{
    private readonly RegistryManager _registry;
    public ListDevicesEndpoint(RegistryManager registry)
    {
        _registry = registry;
    }

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
        var twins = await _registry.CreateQuery("select * from devices").GetNextAsTwinAsync();
        var result = twins.Select(t => t.ToDeviceData());
        return TypedResults.Ok(result);
    }
}

public class GetDeviceByIdEndpoint : Endpoint<string, IResult>
{
    private readonly RegistryManager _registry;

    public GetDeviceByIdEndpoint(RegistryManager registry)
    {
        _registry = registry;
    }

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
        var twin = await _registry.GetTwinAsync(deviceId, ct);
        return twin == null ? Results.NotFound() : Results.Ok(twin.ToDeviceData());
    }
}