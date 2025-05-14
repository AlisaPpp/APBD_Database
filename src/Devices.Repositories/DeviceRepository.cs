using System.Data;
using System.Text.RegularExpressions;
using Devices.Entities;
using Microsoft.Data.SqlClient;
namespace Devices.Repositories;


public class DeviceRepository : IDeviceRepository
{
    private readonly string _connectionString;

    public DeviceRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Device> GetAllDevices()
    {
        List<Device> devices = [];

        string query = "SELECT * FROM Device";

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
                            IsTurnedOn = reader.GetBoolean(2),
                            RowVersion = (byte[])reader.GetValue(3)
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

        string getBaseQuery = "SELECT Id, Name, IsTurnedOn, RowVersion FROM Device WHERE Id = @Id";

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
        var rowVersion = (byte[])reader.GetValue(3);

        reader.Close();

        string swQuery = "SELECT BatteryLevel FROM Smartwatch WHERE DeviceId = @Id";
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
                    RowVersion = rowVersion,
                    BatteryLevel = swReader.GetInt32(0)
                };
            }
        }

        string pcQuery = "SELECT OperatingSystem FROM PersonalComputer WHERE DeviceId = @Id";
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
                    RowVersion = rowVersion,
                    OperatingSystem = pcReader.GetString(0)
                };
            }
        }

        string edQuery = "SELECT IpAddress, NetworkName FROM EmbeddedDevice WHERE DeviceId = @Id";
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
                    RowVersion = rowVersion,
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
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using var command = new SqlCommand
            {
                Connection = connection,
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                switch (device)
                {
                    case Smartwatch sw:
                        command.CommandText = "AddSmartwatch";
                        command.Parameters.AddWithValue("@DeviceId", sw.ID);
                        command.Parameters.AddWithValue("@Name", sw.Name);
                        command.Parameters.AddWithValue("@IsTurnedOn", sw.IsTurnedOn);
                        command.Parameters.AddWithValue("@BatteryLevel", sw.BatteryLevel);
                        break;
                    case PersonalComputer pc:
                        command.CommandText = "AddPersonalComputer";
                        command.Parameters.AddWithValue("@DeviceId", pc.ID);
                        command.Parameters.AddWithValue("@Name", pc.Name);
                        command.Parameters.AddWithValue("@IsTurnedOn", pc.IsTurnedOn);
                        command.Parameters.AddWithValue("@OperatingSystem", pc.OperatingSystem);
                        break;
                    case EmbeddedDevice ed:
                        command.CommandText = "AddEmbedded";
                        command.Parameters.AddWithValue("@DeviceId", ed.ID);
                        command.Parameters.AddWithValue("@Name", ed.Name);
                        command.Parameters.AddWithValue("@IsTurnedOn", ed.IsTurnedOn);
                        command.Parameters.AddWithValue("@IpAddress", ed.IpAddress);
                        command.Parameters.AddWithValue("@NetworkName", ed.NetworkName);
                        break;
                    default:
                        throw new ApplicationException("Unknown device");
                }

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                throw new Exception("Failed to execute stored procedure", ex);
            }
        }
    }

    public bool EditDevice(Device device)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();

        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            string updateDevice =
                "UPDATE Device SET Name = @Name, IsTurnedOn = @IsTurnedOn WHERE Id = @Id AND RowVersion = @RowVersion";

            byte[] newRowVersion;

            using (SqlCommand command = new SqlCommand(updateDevice, connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", device.ID);
                command.Parameters.AddWithValue("@Name", device.Name);
                command.Parameters.AddWithValue("@IsTurnedOn", device.IsTurnedOn);
                command.Parameters.AddWithValue("@RowVersion", SqlDbType.Timestamp).Value = device.RowVersion;

                if (command.ExecuteNonQuery() == 0)
                    throw new DBConcurrencyException("Device was updated or could not be found");

                using var versionCommand = new SqlCommand("SELECT RowVersion FROM Device WHERE Id = @Id", connection,
                    transaction);
                versionCommand.Parameters.AddWithValue("@Id", device.ID);
                newRowVersion = (byte[])versionCommand.ExecuteScalar();

            }

            if (device is Smartwatch sw)
            {
                string updateSmartwatch =
                    "UPDATE Smartwatch SET BatteryLevel = @BatteryLevel WHERE DeviceId = @DeviceId";
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
                    "UPDATE PersonalComputer SET OperatingSystem = @OperatingSystem WHERE DeviceId = @DeviceId";
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
                    "UPDATE EmbeddedDevice SET IpAddress = @IpAddress, NetworkName = @NetworkName WHERE DeviceId = @DeviceId";
                using (SqlCommand command = new SqlCommand(updateEmbeddedDevice, connection, transaction))
                {
                    command.Parameters.AddWithValue("@DeviceId", device.ID);
                    command.Parameters.AddWithValue("@IpAddress", ed.IpAddress);
                    command.Parameters.AddWithValue("@NetworkName", ed.NetworkName);
                    command.ExecuteNonQuery();
                }
            }

            transaction.Commit();
            device.RowVersion = newRowVersion;
            return true;
        }
        catch (DBConcurrencyException)
        {
            transaction.Rollback();
            return false;
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
                DELETE FROM Smartwatch WHERE DeviceId = @Id;
                DELETE FROM PersonalComputer WHERE DeviceId = @Id;
                DELETE FROM EmbeddedDevice WHERE DeviceId = @Id;";

            using (SqlCommand command = new SqlCommand(deleteSpecific, connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }

            using (SqlCommand command = new SqlCommand("DELETE FROM Device WHERE Id = @Id", connection, transaction))
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

    public void ValidateDevice(Device device)
    {
        if (string.IsNullOrWhiteSpace(device.Name))
            throw new ArgumentException("Device name cannot be empty.");

        switch (device)
        {
            case Smartwatch sw:
                ValidateSmartwatch(sw);
                break;
            case PersonalComputer pc:
                ValidatePersonalComputer(pc);
                break;
            case EmbeddedDevice ed:
                ValidateEmbeddedDevice(ed);
                break;
            default:
                throw new ArgumentException("Invalid Device Type");
        }
    }

    private void ValidateSmartwatch(Smartwatch smartwatch)
    {
        if (smartwatch.BatteryLevel < 0 || smartwatch.BatteryLevel > 100)
            throw new ArgumentException("Battery level must be between 0 and 100.");
        if (smartwatch.BatteryLevel < 11 && smartwatch.IsTurnedOn)
            throw new ArgumentException("Smartwatch can't be turned on when battery level is below 11");
    }

    private void ValidatePersonalComputer(PersonalComputer personalComputer)
    {
        if (string.IsNullOrWhiteSpace(personalComputer.OperatingSystem) && personalComputer.IsTurnedOn)
            throw new ArgumentException("There should be an operating system for PC to turn on");
    }

    private void ValidateEmbeddedDevice(EmbeddedDevice embeddedDevice)
    {
        if (!embeddedDevice.NetworkName.Contains("MD Ltd.") && embeddedDevice.IsTurnedOn)
            throw new ArgumentException("Embedded Device network must be MD Ltd. to be turned on");
        if (!Regex.IsMatch(embeddedDevice.IpAddress,
                @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"))
        {
            throw new ArgumentException("Invalid IP address.");
        }
    }
}