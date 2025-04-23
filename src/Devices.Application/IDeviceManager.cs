using Devices.Entities;
namespace Devices.Application;

public interface IDeviceManager
{
    IEnumerable<Device> GetAllDevices();
    bool CreateDevice(Device device);
}