using FastEndpoints;
using IotManagerApi;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<IotHubConfig>(builder.Configuration.GetSection(nameof(IotHubConfig)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton((IOptions<IotHubConfig> cfg) => RegistryManager.CreateFromConnectionString(cfg.Value.ConnectionString));
builder.Services.AddFastEndpoints();

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

app.Run();