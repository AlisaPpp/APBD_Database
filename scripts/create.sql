DROP TABLE Device;
DROP TABLE Smartwatch;
DROP TABLE PersonalComputer;
DROP TABLE EmbeddedDevice;

CREATE TABLE Device (
                        Id NVARCHAR(20) PRIMARY KEY,
                        Name NVARCHAR(50),
                        IsTurnedOn BIT
);

CREATE TABLE Smartwatch (
                            Id INT PRIMARY KEY IDENTITY(1, 1),
                            BatteryLevel INT,
                            DeviceId NVARCHAR(20) FOREIGN KEY REFERENCES Device(Id)

);

CREATE TABLE PersonalComputer (
                                Id INT PRIMARY KEY IDENTITY(1, 1),
                                OperatingSystem NVARCHAR(80),
                                DeviceId NVARCHAR(20) FOREIGN KEY REFERENCES Device(Id)

);

CREATE TABLE EmbeddedDevice (
                                Id INT PRIMARY KEY IDENTITY(1, 1),
                                IpAddress NVARCHAR(20),
                                NetworkName NVARCHAR(50),
                                DeviceId NVARCHAR(20) FOREIGN KEY REFERENCES Device(Id)

);
