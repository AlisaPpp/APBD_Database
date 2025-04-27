DROP TABLE Devices;
DROP TABLE Smartwatches;
DROP TABLE PersonalComputers;
DROP TABLE EmbeddedDevices;

CREATE TABLE Devices (
                        Id NVARCHAR(20) PRIMARY KEY,
                        Name NVARCHAR(50),
                        IsTurnedOn BIT
);

CREATE TABLE Smartwatches (
                            Id INT PRIMARY KEY IDENTITY(1, 1),
                            BatteryLevel INT,
                            DeviceId NVARCHAR(20) FOREIGN KEY REFERENCES Devices(Id)

);

CREATE TABLE PersonalComputers (
                                Id INT PRIMARY KEY IDENTITY(1, 1),
                                OperatingSystem NVARCHAR(80),
                                DeviceId NVARCHAR(20) FOREIGN KEY REFERENCES Devices(Id)

);

CREATE TABLE EmbeddedDevices (
                                Id INT PRIMARY KEY IDENTITY(1, 1),
                                IpAddress NVARCHAR(20),
                                NetworkName NVARCHAR(50),
                                DeviceId NVARCHAR(20) FOREIGN KEY REFERENCES Devices(Id)

);
