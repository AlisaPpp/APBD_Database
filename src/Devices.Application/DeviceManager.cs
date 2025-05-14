using Devices.Entities;
using Devices.Repositories;

namespace Devices.Application;

public class DeviceManager : IDeviceManager
{
    private readonly IDeviceRepository _deviceRepository;

    public DeviceManager(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public IEnumerable<Device> GetAllDevices()
    {
        return _deviceRepository.GetAllDevices();
    }

    public bool CreateDevice(Device device)
    {
        _deviceRepository.ValidateDevice(device);
        return _deviceRepository.CreateDevice(device);
    }

    public bool EditDevice(Device device)
    {
        _deviceRepository.ValidateDevice(device);
        return _deviceRepository.EditDevice(device);
    }

    public bool DeleteDevice(string id)
    {
        return _deviceRepository.DeleteDevice(id);
    }

    public Device? GetDeviceById(string id)
    {
        return _deviceRepository.GetDeviceById(id);
    }
    
}
