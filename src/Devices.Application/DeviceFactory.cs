using Devices.Entities;
namespace Devices.Application;

public class DeviceFactory
{
    public static Device CreateDevice(string type, string[] parts)
    {
        return type switch
        {
            "SW" => new Smartwatch { ID = parts[0], Name = parts[1], BatteryLevel = int.Parse(parts[3].TrimEnd('%')), IsTurnedOn = bool.Parse(parts[2]) },
            "P" => new PersonalComputer { ID = parts[0], Name = parts[1], IsTurnedOn = bool.Parse(parts[2]), OperatingSystem = parts.Length > 3 ? parts[3] : "" },
            "ED" => new EmbeddedDevice { ID = parts[0], Name = parts[1], IpAddress = parts[2], NetworkName = parts[3] },
            _ => throw new Exception("Unknown device type")
        };
    }
}