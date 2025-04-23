
CREATE TABLE Devices (
                        Id NVARCHAR(5) PRIMARY KEY,
                        Name NVARCHAR(50),
                        IsTurnedOn BIT,
                        DeviceType NVARCHAR(50) NOT NULL 
);

CREATE TABLE Smartwatches (
                            DeviceId NVARCHAR(50) PRIMARY KEY FOREIGN KEY REFERENCES Devices(Id),
                            BatteryLevel INT
);

CREATE TABLE PersonalComputers (
                                DeviceId NVARCHAR(50) PRIMARY KEY FOREIGN KEY REFERENCES Devices(Id),
                                OperatingSystem NVARCHAR(100)
);

CREATE TABLE EmbeddedDevices (
                                DeviceId NVARCHAR(50) PRIMARY KEY FOREIGN KEY REFERENCES Devices(Id),
                                IpAddress NVARCHAR(50),
                                NetworkName NVARCHAR(100)
);
