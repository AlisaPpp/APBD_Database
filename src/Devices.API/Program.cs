using System.Text.Json.Nodes;
using Devices.Entities;
using Devices.Application;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PersonalDatabase");

builder.Services.AddTransient<IDeviceManager, DeviceManager>(_ => new DeviceManager(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
app.MapGet("/api/devices/{id}", (IDeviceManager deviceManager, string id) =>
{
    try
    {
        var device = deviceManager.GetDeviceById(id);
        if (device == null)
        {
            return Results.NotFound($"Device with ID {id} not found.");
        }
        return Results.Ok(device);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

//Create the device
app.MapPost("/api/devices", async (HttpRequest request, IDeviceManager deviceManager) => 
{
    string? contentType = request.ContentType?.ToLower();
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    switch (contentType)
    {
        case "application/json":
        {
            using var reader = new StreamReader(request.Body);
            string rawJson = await reader.ReadToEndAsync();

            var json = JsonNode.Parse(rawJson);
            if (json is null)
                return Results.BadRequest("Invalid JSON.");

            string? deviceType = json["deviceType"]?.ToString();
            if (string.IsNullOrEmpty(deviceType))
                return Results.BadRequest("Missing deviceType.");

            string newId = await GenerateNextDeviceIdAsync(connection, deviceType);

            Device device;
            switch (deviceType)
            {
                case "SW":
                    device = new Smartwatch
                    {
                        Name = json["name"]?.ToString() ?? throw new Exception("Missing name."),
                        IsTurnedOn = bool.Parse(json["isTurnedOn"]?.ToString() ?? "false"),
                        BatteryLevel = int.Parse(json["batteryLevel"]?.ToString() ??
                                                 throw new Exception("Missing batteryLevel"))
                    };
                    break;
                case "P":
                    device = new PersonalComputer
                    {
                        Name = json["name"]?.ToString() ?? throw new Exception("Missing name."),
                        IsTurnedOn = bool.Parse(json["isTurnedOn"]?.ToString() ?? "false"),
                        OperatingSystem = json["operatingSystem"]?.ToString() ?? ""
                    };
                    break;
                case "ED":
                    device = new EmbeddedDevice
                    {
                        Name = json["name"]?.ToString() ?? throw new Exception("Missing name."),
                        IpAddress = json["ipAddress"]?.ToString() ?? throw new Exception("Missing ipAddress."),
                        NetworkName = json["networkName"]?.ToString() ?? throw new Exception("Missing networkName")
                    };
                    break;
                default:
                    return Results.BadRequest("Unknown deviceType.");
                }

            device.ID = newId;
            var success = deviceManager.CreateDevice(device);
            if (success)
                return Results.Created($"/api/devices/{device.ID}", device);
            else
                return Results.BadRequest("Failed to create device.");
        }

        case "text/plain":
        {
            using var reader = new StreamReader(request.Body);
            string rawText = await reader.ReadToEndAsync();

            var parts = rawText.Split(',');
            if (parts.Length < 2)
                return Results.BadRequest("Invalid plain text format.");

            string deviceType = parts[0].Trim(); 
            string[] deviceParts = parts.Skip(1).ToArray(); 

            try
            {
                string newId = await GenerateNextDeviceIdAsync(connection, deviceType);
                Device device = DeviceFactory.CreateDevice(deviceType, deviceParts);
                device.ID = newId;

                var success = deviceManager.CreateDevice(device);
                if (success)
                    return Results.Created($"/api/devices/{device.ID}", device);
                else
                    return Results.BadRequest("Failed to create device.");
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error parsing device: {ex.Message}");
            }
        }

        default:
            return Results.Conflict("Unsupported content type.");
    }
})
.Accepts<string>("application/json", ["text/plain"]);

//Edit the device
app.MapPut("/api/devices/{id}", async (HttpRequest request, string id, IDeviceManager deviceManager) =>
{
    string? contentType = request.ContentType?.ToLower();

    switch (contentType)
    {
        case "application/json":
        {
            using var reader = new StreamReader(request.Body);
            string rawJson = await reader.ReadToEndAsync();

            var json = JsonNode.Parse(rawJson);
            if (json is null)
                return Results.BadRequest("Invalid JSON.");

            string? deviceType = json["deviceType"]?.ToString();
            if (string.IsNullOrEmpty(deviceType))
                return Results.BadRequest("Missing deviceType.");

            Device device;
            switch (deviceType)
            {
                case "SW":
                    device = new Smartwatch
                    {
                        ID = id,
                        Name = json["name"]?.ToString() ?? throw new Exception("Missing name."),
                        IsTurnedOn = bool.Parse(json["isTurnedOn"]?.ToString() ?? "false"),
                        BatteryLevel = int.Parse(json["batteryLevel"]?.ToString() ?? throw new Exception("Missing batteryLevel"))
                    };
                    break;
                case "P":
                    device = new PersonalComputer
                    {
                        ID = id,
                        Name = json["name"]?.ToString() ?? throw new Exception("Missing name."),
                        IsTurnedOn = bool.Parse(json["isTurnedOn"]?.ToString() ?? "false"),
                        OperatingSystem = json["operatingSystem"]?.ToString() ?? ""
                    };
                    break;
                case "ED":
                    device = new EmbeddedDevice
                    {
                        ID = id,
                        Name = json["name"]?.ToString() ?? throw new Exception("Missing name."),
                        IpAddress = json["ipAddress"]?.ToString() ?? throw new Exception("Missing ipAddress."),
                        NetworkName = json["networkName"]?.ToString() ?? throw new Exception("Missing networkName")
                    };
                    break;
                default:
                    return Results.BadRequest("Unknown deviceType.");
            }

            var success = deviceManager.EditDevice(device);
            if (success)
                return Results.Ok(device); 
            else
                return Results.NotFound("Device not found or failed to update."); 
        }
        
        case "text/plain":
        {
            using var reader = new StreamReader(request.Body);
            string rawText = await reader.ReadToEndAsync();

            var parts = rawText.Split(',');
            if (parts.Length < 2)
                return Results.BadRequest("Invalid plain text format.");

            string deviceType = parts[0].Trim(); 
            string[] deviceParts = parts.Skip(1).ToArray(); 

            try
            {
                Device device = DeviceFactory.CreateDevice(deviceType, deviceParts);
                device.ID = id; 

                var success = deviceManager.EditDevice(device);
                if (success)
                    return Results.Ok(device); 
                else
                    return Results.NotFound("Device not found or failed to update."); 
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error parsing device: {ex.Message}"); 
            }
        }
        
        default:
            return Results.Conflict("Unsupported content type."); 
    }
});


//Delete the device
app.MapDelete("/api/devices/{id}", (string id, IDeviceManager deviceManager) =>
{
    if (deviceManager.DeleteDevice(id))
        return Results.Ok();
    
    return Results.NotFound();
});

static async Task<string> GenerateNextDeviceIdAsync(SqlConnection connection, string deviceType)
{
    string prefix = deviceType switch
    {
        "SW" => "SW-",
        "P" => "P-",
        "ED" => "ED-",
        _ => throw new Exception("Invalid device type")
    };

    var command = new SqlCommand(
        @"SELECT MAX(CAST(SUBSTRING(Id, LEN(@Prefix) + 1, LEN(Id)) AS INT)) 
          FROM Devices 
          WHERE Id LIKE @Prefix + '%'", connection);

    command.Parameters.AddWithValue("@Prefix", prefix);

    object result = await command.ExecuteScalarAsync();

    int nextNumber = 1; 
    if (result != DBNull.Value && result != null)
    {
        nextNumber = (int)result + 1;
    }

    return $"{prefix}{nextNumber}";
}


app.Run();

