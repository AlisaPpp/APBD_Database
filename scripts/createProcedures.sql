CREATE PROCEDURE AddEmbedded
    @DeviceId VARCHAR(50),
    @Name NVARCHAR(100),
    @IsTurnedOn BIT,
    @IpAddress VARCHAR(50),
    @NetworkName VARCHAR(100)
AS
BEGIN
    --SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert into Device table
        INSERT INTO Device (Id, Name, IsTurnedOn)
        VALUES (@DeviceId, @Name, @IsTurnedOn);

        -- Insert into Embedded table
        INSERT INTO EmbeddedDevice (IpAddress, NetworkName, DeviceId)
        VALUES (@IpAddress, @NetworkName, @DeviceId);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END


CREATE PROCEDURE AddSmartwatch
    @DeviceId VARCHAR(50),
    @Name NVARCHAR(100),
    @IsTurnedOn BIT,
    @BatteryLevel INT
AS
BEGIN
    --SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert into Device table
        INSERT INTO Device (Id, Name, IsTurnedOn)
        VALUES (@DeviceId, @Name, @IsTurnedOn);

        -- Insert into Smartwatch table
        INSERT INTO Smartwatch (BatteryLevel, DeviceId)
        VALUES (@BatteryLevel, @DeviceId);

    COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END


CREATE PROCEDURE AddPersonalComputer
    @DeviceId VARCHAR(50),
    @Name NVARCHAR(100),
    @IsTurnedOn BIT,
    @OperatingSystem VARCHAR(50)
AS
BEGIN
    --SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert into Device table
        INSERT INTO Device (Id, Name, IsTurnedOn)
        VALUES (@DeviceId, @Name, @IsTurnedOn);

        -- Insert into PersonalComputer table
        INSERT INTO PersonalComputer (OperatingSystem, DeviceId)
        VALUES (@OperatingSystem, @DeviceId);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

