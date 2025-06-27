using Microsoft.Azure.Devices;

const string hostName = "cmgtestIoTHub2.azure-devices.net";
const string hubConnectionString = $"HostName={hostName};SharedAccessKeyName=iothubowner;SharedAccessKey=+6Og9v6MwNPazLiM7yqchGQOvl5Zs61tFAIoTCGrceU=";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(RegistryManager.CreateFromConnectionString(hubConnectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/devices", async (RegistryManager registry) =>
    {
        var twins = await registry.CreateQuery("select * from devices").GetNextAsTwinAsync();
        var result = twins.Select(t => new
        {
            t.DeviceId,
            t.ModelId,
            t.ConnectionState,
            t.Status,
            t.StatusReason,
            t.LastActivityTime,
            t.Capabilities,
            Tags = t.Tags.ToJson(),
            Properties = new
            {
                Desired = t.Properties.Desired.ToJson(),
                Reported = t.Properties.Reported.ToJson()
            }
        });
        return Results.Ok(result);
    })
    .WithName("GetDevices")
    .WithOpenApi();

app.Run();