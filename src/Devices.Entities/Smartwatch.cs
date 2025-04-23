namespace Devices.Entities;

public class Smartwatch : Device
{
    private int _batteryLevel;
    
    public int BatteryLevel
    {
        get => _batteryLevel;
        set
        {
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException("Battery level must be between 0 and 100");
            _batteryLevel = value;
        }
    }
}