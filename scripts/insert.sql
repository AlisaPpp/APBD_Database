INSERT INTO Devices (Id, Name, IsTurnedOn) VALUES
    ('SW-1', 'Apple Watch SE6', 1),
    ('P-1', 'MacBook Pro', 0),
    ('ED-1', 'Sensor Node 01', 1);

INSERT INTO Smartwatches (DeviceId, BatteryLevel) VALUES
    ('SW-1', 67);

INSERT INTO PersonalComputers (DeviceId, OperatingSystem) VALUES
    ('P-1', 'macOS Ventura');

INSERT INTO EmbeddedDevices (DeviceId, IpAddress, NetworkName) VALUES
    ('ED-1', '192.168.1.42', 'MD Ltd.Wifi-1');
