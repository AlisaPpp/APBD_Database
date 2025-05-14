INSERT INTO Device (Id, Name, IsTurnedOn) VALUES
    ('SW-1', 'Apple Watch SE6', 1),
    ('P-1', 'MacBook Pro', 0),
    ('ED-1', 'Sensor Node 01', 1);

INSERT INTO Smartwatch (DeviceId, BatteryLevel) VALUES
    ('SW-1', 67);

INSERT INTO PersonalComputer (DeviceId, OperatingSystem) VALUES
    ('P-1', 'macOS Ventura');

INSERT INTO EmbeddedDevice (DeviceId, IpAddress, NetworkName) VALUES
    ('ED-1', '192.168.1.42', 'MD Ltd.Wifi-1');

select * from device;

EXEC AddSmartwatch
     @DeviceId = 'SW-3',
     @Name = 'Test',
     @IsTurnedOn = 1,
     @BatteryLevel = 25;

delete from device where id = 'ED-2';
delete from EmbeddedDevice where id = 2;