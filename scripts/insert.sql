INSERT INTO Devices (Id, Name, IsTurnedOn, DeviceType) VALUES
    ('SW-1', 'Apple Watch SE6', 1, 'Smartwatch'),
    ('P-1', 'MacBook Pro', 0, 'PersonalComputer'),
    ('ED-1', 'Sensor Node 01', 1, 'EmbeddedDevice');

INSERT INTO Smartwatches (DeviceId, BatteryLevel) VALUES
    ('SW-1', 67);

INSERT INTO PersonalComputers (DeviceId, OperatingSystem) VALUES
    ('P-1', 'macOS Ventura');

INSERT INTO EmbeddedDevices (DeviceId, IpAddress, NetworkName) VALUES
    ('ED-1', '192.168.1.42', 'MD Ltd.Wifi-1');
