using Devices.Entities;
namespace Devices.Repositories;

public interface IDeviceRepository
{
    public IEnumerable<Device> GetAllDevices();
    public bool CreateDevice(Device device);
    public bool EditDevice(Device device);
    public bool DeleteDevice(string id);
    public Device? GetDeviceById(string id);
    public void ValidateDevice(Device device);
}