using FastEndpoints;
using IotManagerApi;
using IotManagerApi.Config;
using IotManagerApi.Database;
using IotManagerApi.Services;
using Microsoft.Azure.Devices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<IotHubConfig>(builder.Configuration.GetSection(nameof(IotHubConfig)));
builder.Services.Configure<DbConfig>(builder.Configuration.GetSection(nameof(DbConfig)));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(sp =>
{
    var iotHubConfig = sp.GetRequiredService<IOptions<IotHubConfig>>();
    return RegistryManager.CreateFromConnectionString(iotHubConfig.Value.ConnectionString);
});
builder.Services.AddSingleton(sp =>
{
    var iotHubConfig = sp.GetRequiredService<IOptions<IotHubConfig>>();
    return JobClient.CreateFromConnectionString(iotHubConfig.Value.ConnectionString);
});
builder.Services.AddSingleton<BatchJobService>();
builder.Services.AddFastEndpoints();
builder.Services.AddDbContext<IotManagerDbContext>(options =>
{
    var connectionString = builder.Configuration.GetSection(nameof(DbConfig)).Get<DbConfig>()!.ConnectionString;
    options.UseSqlServer(connectionString);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAngularDev");
app.UseHttpsRedirection();

app.UseFastEndpoints();

await app.RunAsync();