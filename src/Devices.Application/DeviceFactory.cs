using Devices.Entities;
namespace Devices.Application;

public class DeviceFactory
{
    public static Device CreateDevice(string type, string[] parts)
    {
        return type switch
        {
            "SW" => new Smartwatch 
            { 
                Name = parts[0], 
                IsTurnedOn = bool.Parse(parts[1]), 
                BatteryLevel = int.Parse(parts[2]) 
            },
            "P" => new PersonalComputer 
            { 
                Name = parts[0], 
                IsTurnedOn = bool.Parse(parts[1]), 
                OperatingSystem = parts.Length > 2 ? parts[2] : "" 
            },
            "ED" => new EmbeddedDevice 
            { 
                Name = parts[0], 
                IpAddress = parts[1], 
                NetworkName = parts[2] 
            },
            _ => throw new Exception("Unknown device type")
        };
    }
}
