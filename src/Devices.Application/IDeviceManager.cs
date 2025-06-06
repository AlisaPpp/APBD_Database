using Devices.Entities;
namespace Devices.Application;

public interface IDeviceManager
{
    public IEnumerable<Device> GetAllDevices();
    public bool CreateDevice(Device device);
    public bool EditDevice(Device device);
    public bool DeleteDevice(string id);
    public Device? GetDeviceById(string id);
}