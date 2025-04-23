using System.Text.RegularExpressions;
namespace Devices.Entities;

public class EmbeddedDevice : Device
{
    private string ipAddress;
    public string NetworkName { get; set; }

    public string IpAddress
    {
        get => ipAddress;
        set
        {
            if(!Regex.IsMatch(value, @"^([0-9]{1,3}\.){3}[0-9]{1,3}$"))
                throw new ArgumentException("Invalid IP Address");
            
            string[] octets = value.Split('.');
            foreach (var octet in octets)
            {
                try
                {
                    int num = int.Parse(octet);
                    if (num < 0 || num > 255)
                        throw new ArgumentException("Each octet of the IP address must be between 0 and 255.");
                }
                catch (FormatException)
                {
                    throw new ArgumentException("Invalid IP Address. Octet is not a valid number.");
                }
            }
            ipAddress = value;
        }
    }
}