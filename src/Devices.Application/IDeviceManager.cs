using Devices.Entities;
namespace Devices.Application;

public interface IDeviceManager
{
    IEnumerable<Device> GetAllDevices();
    bool CreateDevice(Device device);
    bool EditDevice(Device device);
    bool DeleteDevice(string id);
    Device? GetDeviceById(string id);
}