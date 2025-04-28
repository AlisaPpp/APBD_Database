using Devices.Entities;
using Microsoft.Data.SqlClient;

namespace Devices.Application;

public class DeviceManager : IDeviceManager
{
    private string _connectionString;

    public DeviceManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Device> GetAllDevices()
    {
        List<Device> devices = [];
        
        string query = "SELECT * FROM Devices";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var device = new Device
                        {
                            ID = reader.GetString(0),
                            Name = reader.GetString(1),
                            IsTurnedOn = reader.GetBoolean(2)
                        };
                        devices.Add(device);
                    }
                }
            }
            finally
            {
                reader.Close();
            }
        }
        return devices;
    }

    public Device? GetDeviceById(string id)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();

        string getBaseQuery = "SELECT Id, Name, IsTurnedOn FROM Devices WHERE Id = @Id";

        using SqlCommand baseCmd = new SqlCommand(getBaseQuery, connection);
        baseCmd.Parameters.AddWithValue("@Id", id);

        using SqlDataReader reader = baseCmd.ExecuteReader();
        
        if (!reader.Read())
        { 
            throw new ApplicationException("Device not found"); 
        }

        var baseId = reader.GetString(0);
        var name = reader.GetString(1);
        var isTurnedOn = reader.GetBoolean(2);

        reader.Close();
        
        string swQuery = "SELECT BatteryLevel FROM Smartwatches WHERE DeviceId = @Id";
        using (SqlCommand swCmd = new SqlCommand(swQuery, connection)) 
        {
            swCmd.Parameters.AddWithValue("@Id", id);
            using SqlDataReader swReader = swCmd.ExecuteReader();
            if (swReader.Read())
            {
            return new Smartwatch
                {
                    ID = baseId,
                    Name = name,
                    IsTurnedOn = isTurnedOn,
                    BatteryLevel = swReader.GetInt32(0)
                };
            }
        }

        string pcQuery = "SELECT OperatingSystem FROM PersonalComputers WHERE DeviceId = @Id";
        using (SqlCommand pcCmd = new SqlCommand(pcQuery, connection))
        {
            pcCmd.Parameters.AddWithValue("@Id", id);
            using SqlDataReader pcReader = pcCmd.ExecuteReader();
            if (pcReader.Read())
            {
                return new PersonalComputer
                {
                    ID = baseId,
                    Name = name,
                    IsTurnedOn = isTurnedOn,
                    OperatingSystem = pcReader.GetString(0)
                }; 
            }
        }

        string edQuery = "SELECT IpAddress, NetworkName FROM EmbeddedDevices WHERE DeviceId = @Id";
        using (SqlCommand edCmd = new SqlCommand(edQuery, connection))
        {
            edCmd.Parameters.AddWithValue("@Id", id);
            using SqlDataReader edReader = edCmd.ExecuteReader();
            if (edReader.Read())
            {
                return new EmbeddedDevice
                {
                    ID = baseId,
                    Name = name,
                    IsTurnedOn = isTurnedOn,
                    IpAddress = edReader.GetString(0),
                    NetworkName = edReader.GetString(1)
                };
            }
        }

        return new Device
        {
            ID = baseId,
            Name = name,
            IsTurnedOn = isTurnedOn
        };
    }
    
    public bool CreateDevice(Device device)
    {
        ValidateDevice(device);
        
        int countRowsAdd = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                string insertDevice = "INSERT INTO Devices (Id, Name, IsTurnedOn) VALUES (@Id, @Name, @IsTurnedOn)";

                using (SqlCommand command = new SqlCommand(insertDevice, connection, transaction))
                {
                    command.Parameters.AddWithValue("@Id", device.ID);
                    command.Parameters.AddWithValue("@Name", device.Name);
                    command.Parameters.AddWithValue("@IsTurnedOn", device.IsTurnedOn);

                    countRowsAdd += command.ExecuteNonQuery();
                }

                if (device is Smartwatch sw)
                {
                    string insertSmartwatch =
                        "INSERT INTO Smartwatches (DeviceId, BatteryLevel) VALUES (@DeviceId, @BatteryLevel)";
                    using (SqlCommand command = new SqlCommand(insertSmartwatch, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@DeviceId", device.ID);
                        command.Parameters.AddWithValue("@BatteryLevel", sw.BatteryLevel);
                        countRowsAdd += command.ExecuteNonQuery();
                    }
                }
                else if (device is PersonalComputer pc)
                {
                    string insertPersonalComputer =
                        "INSERT INTO PersonalComputers (DeviceId, OperatingSystem) VALUES (@DeviceId, @OperatingSystem)";
                    using (SqlCommand command = new SqlCommand(insertPersonalComputer, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@DeviceId", device.ID);
                        command.Parameters.AddWithValue("@OperatingSystem", pc.OperatingSystem);
                        countRowsAdd += command.ExecuteNonQuery();
                    }
                }
                else if (device is EmbeddedDevice ed)
                {
                    string insertEmbeddedDevice =
                        "INSERT INTO EmbeddedDevices (DeviceId, NetworkName, IpAddress) VALUES (@DeviceId, @IpAddress, @NetworkName)";
                    using (SqlCommand command = new SqlCommand(insertEmbeddedDevice, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@DeviceId", device.ID);
                        command.Parameters.AddWithValue("@IpAddress", ed.IpAddress);
                        command.Parameters.AddWithValue("@NetworkName", ed.NetworkName);
                        countRowsAdd += command.ExecuteNonQuery();
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid Device Type");
                }

                transaction.Commit();
            }
            catch 
            {
                transaction.Rollback();
                throw;
            }
        }
        return countRowsAdd > 0;
    }

    public bool EditDevice(Device device)
    {
        ValidateDevice(device);
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();
        
        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            string updateDevice = "UPDATE Devices SET Name = @Name, IsTurnedOn = @IsTurnedOn WHERE Id = @Id";

            using (SqlCommand command = new SqlCommand(updateDevice, connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", device.ID);
                command.Parameters.AddWithValue("@Name", device.Name);
                command.Parameters.AddWithValue("@IsTurnedOn", device.IsTurnedOn);
                if (command.ExecuteNonQuery() == 0)
                    throw new Exception("Device not found");
            }

            if (device is Smartwatch sw)
            {
                string updateSmartwatch =
                    "UPDATE Smartwatches SET BatteryLevel = @BatteryLevel WHERE DeviceId = @DeviceId";
                using (SqlCommand command = new SqlCommand(updateSmartwatch, connection, transaction))
                {
                    command.Parameters.AddWithValue("@DeviceId", sw.ID);
                    command.Parameters.AddWithValue("@BatteryLevel", sw.BatteryLevel);
                    command.ExecuteNonQuery();
                }
            }
            else if (device is PersonalComputer pc)
            {
                string updatePersonalComputer =
                    "UPDATE PersonalComputers SET OperatingSystem = @OperatingSystem WHERE DeviceId = @DeviceId";
                using (SqlCommand command = new SqlCommand(updatePersonalComputer, connection, transaction))
                {
                    command.Parameters.AddWithValue("@DeviceId", pc.ID);
                    command.Parameters.AddWithValue("@OperatingSystem", pc.OperatingSystem);
                    command.ExecuteNonQuery();
                }
            }
            else if (device is EmbeddedDevice ed)
            {
                string updateEmbeddedDevice =
                    "UPDATE EmbeddedDevices SET IpAddress = @IpAddress, NetworkName = @NetworkName WHERE DeviceId = @DeviceId";
                using (SqlCommand command = new SqlCommand(updateEmbeddedDevice, connection, transaction))
                {
                    command.Parameters.AddWithValue("@DeviceId", device.ID);
                    command.Parameters.AddWithValue("@IpAddress", ed.IpAddress);
                    command.Parameters.AddWithValue("@NetworkName", ed.NetworkName);
                    command.ExecuteNonQuery();
                }
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }
    
    public bool DeleteDevice(string id)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();
        SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            string deleteSpecific = @"
                DELETE FROM Smartwatches WHERE DeviceId = @Id;
                DELETE FROM PersonalComputers WHERE DeviceId = @Id;
                DELETE FROM EmbeddedDevices WHERE DeviceId = @Id;";

            using (SqlCommand command = new SqlCommand(deleteSpecific, connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }

            using (SqlCommand command = new SqlCommand("DELETE FROM Devices WHERE Id = @Id", connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", id);
                if (command.ExecuteNonQuery() == 0)
                {
                    transaction.Rollback();
                    return false;
                }
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
    
    private void ValidateDevice(Device device)
    {
        if (string.IsNullOrWhiteSpace(device.Name))
            throw new ArgumentException("Device name cannot be empty.");

        if (device is Smartwatch sw)
        {
            if (sw.BatteryLevel < 0 || sw.BatteryLevel > 100)
                throw new ArgumentException("Battery level must be between 0 and 100.");
            if (sw.BatteryLevel < 11)
                sw.IsTurnedOn = false;
        }
        else if (device is PersonalComputer pc)
        {
            if (string.IsNullOrWhiteSpace(pc.OperatingSystem))
            {
                pc.IsTurnedOn = false;
            }
        }
        else if (device is EmbeddedDevice ed)
        {
            if (ed.NetworkName != "MD Ltd.Wifi-1")
                ed.IsTurnedOn = false;
        }
    }
}
