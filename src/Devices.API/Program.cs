using Devices.Entities;
using Devices.Application;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("UniversityDatabase");

builder.Services.AddTransient<IDeviceManager, DeviceManager>(_ => new DeviceManager(connectionString));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Get short info about all the devices
app.MapGet("/api/devices", (IDeviceManager deviceManager) =>
{
    try
    {
        return Results.Ok(deviceManager.GetAllDevices());
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

//Get detailed info about the device by id


app.Run();

