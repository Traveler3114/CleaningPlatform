SET NOCOUNT ON;
GO

USE master;
GO

IF DB_ID('CleaningPlatformDB') IS NOT NULL
BEGIN
    ALTER DATABASE CleaningPlatformDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CleaningPlatformDB;
END;
GO

CREATE DATABASE CleaningPlatformDB;
GO
USE CleaningPlatformDB;
GO

-- ============================================================
-- TABLES (unchanged from your original – kept as is)
-- ============================================================

CREATE TABLE Employees (
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    Username            NVARCHAR(100)   NOT NULL UNIQUE,
    PasswordHash        NVARCHAR(255)   NOT NULL,
    SecurityStamp       NVARCHAR(100)   NOT NULL DEFAULT NEWID(),
    FirstName           NVARCHAR(100)   NOT NULL,
    LastName            NVARCHAR(100)   NOT NULL,
    Phone               NVARCHAR(50)    NULL,
    EmployeeCode        NVARCHAR(50)    NULL UNIQUE,
    HourlyRate          DECIMAL(10,2)   NULL,
    MaxJobsPerDay       INT             NULL DEFAULT 3,
    IsActive            BIT             NOT NULL DEFAULT 1,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);
GO
CREATE UNIQUE INDEX IX_Employees_Username ON Employees(Username);
GO

CREATE TABLE Clients (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    ClientName      NVARCHAR(200)   NOT NULL,
    Type            NVARCHAR(50)    NOT NULL,
    Oib             NVARCHAR(50)    NULL,
    PaymentTerms    NVARCHAR(100)   NULL,
    Notes           NVARCHAR(MAX)   NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CHK_Client_Type CHECK (Type IN ('Person', 'Business'))
);
GO
CREATE INDEX IX_Clients_Name ON Clients(ClientName);
CREATE INDEX IX_Clients_Type ON Clients(Type);
GO

CREATE TABLE Contacts (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    ClientId        INT             NOT NULL,
    ContactName     NVARCHAR(200)   NOT NULL,
    Role            NVARCHAR(100)   NULL,
    Phone           NVARCHAR(50)    NOT NULL,
    Email           NVARCHAR(255)   NULL,
    Address         NVARCHAR(500)   NULL,
    IsPrimary       BIT             NOT NULL DEFAULT 0,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Contact_Client FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_Contacts_ClientId ON Contacts(ClientId);
GO

CREATE TABLE Sites (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    ClientId        INT             NOT NULL,
    SiteName        NVARCHAR(200)   NOT NULL,
    Address         NVARCHAR(500)   NOT NULL,
    City            NVARCHAR(100)   NULL,
    PostalCode      NVARCHAR(20)    NULL,
    SiteType        NVARCHAR(50)    NULL,
    FloorAreaM2     DECIMAL(10,2)   NULL,
    AccessNotes     NVARCHAR(MAX)   NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Site_Client FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE,
    CONSTRAINT CHK_Site_Type CHECK (SiteType IN ('Office', 'Stairwell', 'Garage', 'Facility', 'Boat', 'Vehicle', 'Other') OR SiteType IS NULL)
);
GO
CREATE INDEX IX_Sites_ClientId ON Sites(ClientId);
CREATE INDEX IX_Sites_Type     ON Sites(SiteType);
GO

CREATE TABLE ServiceCatalog (
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    CatalogCode         NVARCHAR(10)    NOT NULL UNIQUE,
    Name                NVARCHAR(200)   NOT NULL,
    Category            NVARCHAR(100)   NULL,
    Unit                NVARCHAR(50)    NULL,
    BasePrice           DECIMAL(10,2)   NULL,
    ApproxTime          INT             NULL,
    ServiceType         NVARCHAR(50)    NOT NULL DEFAULT 'Vehicle',
    IsActive            BIT             NOT NULL DEFAULT 1,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CHK_ServiceCatalog_ServiceType CHECK (ServiceType IN ('Vehicle', 'SiteBased', 'Boat')),
    CONSTRAINT CHK_ServiceCatalog_Category CHECK (Category IN ('Stairs', 'Office', 'Private', 'Special', 'Carpet', 'Furniture', 'Exterior', 'Laundry', 'Vehicle', 'Boat') OR Category IS NULL)
);
GO
CREATE INDEX IX_ServiceCatalog_Code ON ServiceCatalog(CatalogCode);
GO

INSERT INTO ServiceCatalog (CatalogCode, Name, Category, Unit, BasePrice, ServiceType) VALUES
('A-01', N'Čišćenje/Održavanje stubišta - Osnovno', 'Stairs', 'Mj/objekt', 55.00, 'SiteBased'),
('A-02', N'Čišćenje/Održavanje stubišta - Premium', 'Stairs', 'Mj/objekt', 110.00, 'SiteBased'),
('A-03', N'Čišćenje/Održavanje stubišta - Gold', 'Stairs', 'Mj/objekt', 280.00, 'SiteBased'),
('A-04', N'Čišćenje/Održavanje Ureda - Osnovno', 'Office', 'Mj/ured', 500.00, 'SiteBased'),
('A-05', N'Čišćenje/Održavanje Ureda - Premium', 'Office', 'Mj/ured', 1500.00, 'SiteBased'),
('A-06', N'Čišćenje/Održavanje privatnog prostora - Osnovno', 'Private', 'Mj/objekt', NULL, 'SiteBased'),
('A-07', N'Čišćenje/Održavanje privatnog prostora - Detaljno', 'Private', 'Mj/objekt', NULL, 'SiteBased'),
('A-08', N'Post-construction čišćenje', 'Special', N'Po m²', 6.00, 'SiteBased'),
('A-09', N'Čišćenje garaže', 'Special', N'Po tretmanu', 200.00, 'SiteBased'),
('A-10', N'Dezinfekcija prostora (kemijska/ozon)', 'Special', N'Po m²', 6.00, 'SiteBased'),
('A-11', N'Industrijsko čišćenje hale', 'Special', N'Po tretmanu', 400.00, 'SiteBased'),
('A-101', N'Pranje tepiha - Vuna/Resice/Čupavi', 'Carpet', N'Po m²', 4.50, 'SiteBased'),
('A-102', N'Pranje tepiha', 'Carpet', N'Po m²', 5.00, 'SiteBased'),
('A-103', N'Najam i pranje tepiha (otirači)', 'Carpet', 'Mj/tepih', 12.00, 'SiteBased'),
('A-201', N'Kemijsko čišćenje - Kutna garnitura', 'Furniture', N'Po sjedalu', NULL, 'SiteBased'),
('A-202', N'Kemijsko čišćenje - Trosjed', 'Furniture', N'Po komadu', NULL, 'SiteBased'),
('A-203', N'Kemijsko čišćenje - Dvosjed', 'Furniture', N'Po komadu', NULL, 'SiteBased'),
('A-204', N'Kemijsko čišćenje - Fotelja', 'Furniture', N'Po komadu', NULL, 'SiteBased'),
('A-205', N'Kemijsko čišćenje - Madrac', 'Furniture', N'Po komadu', NULL, 'SiteBased'),
('A-301', N'Održavanje Eksterijera - Košenje trave', 'Exterior', N'Po satu/m²', 25.00, 'SiteBased'),
('A-302', N'Uređenje Eksterijera', 'Exterior', N'Po tretmanu', 180.00, 'SiteBased'),
('A-401', N'Pranje Uniformi', 'Laundry', 'Mj/ugovor', 180.00, 'SiteBased'),
('B-01', N'Kompletno Pranje vozila - LIM/OBD', 'Vehicle', N'Po vozilu', 16.00, 'Vehicle'),
('B-02', N'Kompletno Pranje vozila - SUV', 'Vehicle', N'Po vozilu', 24.00, 'Vehicle'),
('B-03', N'Kompletno Pranje vozila - Dostavno/Kombi', 'Vehicle', N'Po vozilu', 28.00, 'Vehicle'),
('B-04', N'Kompletno Pranje vozila - MINI', 'Vehicle', N'Po dolasku', 17.00, 'Vehicle'),
('B-05', N'Unutarnje Pranje vozila - LIM/OBD', 'Vehicle', N'Po vozilu', NULL, 'Vehicle'),
('B-06', N'Unutarnje Pranje vozila - SUV', 'Vehicle', N'Po vozilu', NULL, 'Vehicle'),
('B-07', N'Unutarnje Pranje vozila - Dostavno/Kombi', 'Vehicle', N'Po vozilu', NULL, 'Vehicle'),
('B-08', N'Unutarnje Pranje vozila - MINI', 'Vehicle', N'Po dolasku', NULL, 'Vehicle'),
('B-09', N'Vanjsko Pranje vozila - LIM/OBD', 'Vehicle', N'Po dolasku', 23.00, 'Vehicle'),
('B-10', N'Vanjsko Pranje vozila - SUV', 'Vehicle', N'Po dolasku', 28.00, 'Vehicle'),
('B-11', N'Vanjsko Pranje vozila - Dostavno/Kombi', 'Vehicle', N'Po dolasku', 42.00, 'Vehicle'),
('B-12', N'Vanjsko Pranje vozila - MINI', 'Vehicle', N'Po dolasku', NULL, 'Vehicle'),
('B-13', N'Pranje motora', 'Vehicle', N'Po vozilu', 20.00, 'Vehicle'),
('B-14', N'Kemijsko čišćenje sjedala/tapeciranog', 'Vehicle', N'Po kom', 63.00, 'Vehicle'),
('B-15', N'Detailing', 'Vehicle', N'Po vozilu', NULL, 'Vehicle'),
('B-101', N'Pranje jedrilice/broda - Osnovno', 'Boat', N'Po brodu', 320.00, 'Boat'),
('B-102', N'Pranje broda - Detailing', 'Boat', N'Po brodu', 900.00, 'Boat');
GO

CREATE TABLE Inventory (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    Name            NVARCHAR(200)   NOT NULL,
    Quantity        DECIMAL(10,2)   NOT NULL,
    Unit            NVARCHAR(20)    NOT NULL,
    Category        NVARCHAR(100)   NULL,
    Type            NVARCHAR(20)    NOT NULL DEFAULT 'Consumable',
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CHK_Inventory_Type CHECK (Type IN ('Consumable', 'Equipment'))
);
GO
CREATE INDEX IX_Inventory_Category ON Inventory(Category);
GO

CREATE TABLE ServiceInventoryRequirements (
    ServiceCatalogId    INT             NOT NULL,
    InventoryId         INT             NOT NULL,
    QuantityNeeded      DECIMAL(10,2)   NOT NULL,
    CONSTRAINT PK_SvcInvReq PRIMARY KEY (ServiceCatalogId, InventoryId),
    CONSTRAINT FK_SvcInvReq_Catalog  FOREIGN KEY (ServiceCatalogId) REFERENCES ServiceCatalog(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SvcInvReq_Inventory FOREIGN KEY (InventoryId)      REFERENCES Inventory(Id)
);
GO
CREATE INDEX IX_SvcInvReq_Inventory ON ServiceInventoryRequirements(InventoryId);
GO

-- ============================================================
-- INVENTORY SEED DATA
-- ============================================================

INSERT INTO Inventory (Name, Quantity, Unit, Category, Type) VALUES
('Miniwash Kärcher K2', 3, 'Piece', 'Strojevi za pranje', 'Equipment'),
('Miniwash Kärcher K4', 2, 'Piece', 'Strojevi za pranje', 'Equipment'),
(N'Usisavač vlage i prašine', 2, 'Piece', 'Strojevi za pranje', 'Equipment'),
(N'Šampon za pranje vozila 5L', 5, 'Liter', N'Kemijska sredstva', 'Consumable'),
('Sredstvo za stakla 1L', 3, 'Liter', N'Kemijska sredstva', 'Consumable'),
(N'Sredstvo za odmašćivanje 5L', 4, 'Liter', N'Kemijska sredstva', 'Consumable'),
(N'Šampon za tepihe 5L', 2, 'Liter', N'Kemijska sredstva', 'Consumable'),
('Mikrofiber krpe (paket 10)', 10, 'Piece', 'Pribor', 'Consumable'),
(N'Spužva za pranje (meka)', 15, 'Piece', 'Pribor', 'Consumable'),
(N'Četka za felge', 5, 'Piece', 'Pribor', 'Equipment'),
('Kanta za vodu 20L', 4, 'Piece', 'Pribor', 'Equipment'),
(N'Zaštitne rukavice', 20, 'Piece', 'Osobna zaštita', 'Consumable');
GO

-- ============================================================
-- SERVICE INVENTORY REQUIREMENTS SEED DATA
-- ============================================================

-- B-01 (Kompletno Pranje vozila - LIM/OBD, Id=23) -> Kärcher K2 (Id=1), Šampon 5L (Id=4), Mikrofiber krpe (Id=8), Spužva (Id=9)
INSERT INTO ServiceInventoryRequirements (ServiceCatalogId, InventoryId, QuantityNeeded)
SELECT sc.Id, i.Id, req.Qty
FROM ServiceCatalog sc
CROSS JOIN (VALUES ('Kärcher K2', 1), ('Šampon 5L', 0.2), ('Mikrofiber krpe', 2), ('Spužva', 1)) AS req(ItemName, Qty)
INNER JOIN Inventory i ON i.Name LIKE '%' + req.ItemName + '%'
WHERE sc.CatalogCode = 'B-01';
GO

-- B-02 (SUV, Id=24) -> Kärcher K4 (Id=2), Šampon 5L (Id=4), Mikrofiber krpe (Id=8), Spužva (Id=9)
INSERT INTO ServiceInventoryRequirements (ServiceCatalogId, InventoryId, QuantityNeeded)
SELECT sc.Id, i.Id, req.Qty
FROM ServiceCatalog sc
CROSS JOIN (VALUES ('Kärcher K4', 1), ('Šampon 5L', 0.3), ('Mikrofiber krpe', 3), ('Spužva', 1)) AS req(ItemName, Qty)
INNER JOIN Inventory i ON i.Name LIKE '%' + req.ItemName + '%'
WHERE sc.CatalogCode = 'B-02';
GO

-- B-101 (Pranje jedrilice/broda, Id=38) -> Kärcher K4 (Id=2), Šampon 5L (Id=4), Četka za felge (Id=10)
INSERT INTO ServiceInventoryRequirements (ServiceCatalogId, InventoryId, QuantityNeeded)
SELECT sc.Id, i.Id, req.Qty
FROM ServiceCatalog sc
CROSS JOIN (VALUES ('Kärcher K4', 1), ('Šampon 5L', 0.5), ('Četka', 1)) AS req(ItemName, Qty)
INNER JOIN Inventory i ON i.Name LIKE '%' + req.ItemName + '%'
WHERE sc.CatalogCode = 'B-101';
GO

CREATE TABLE Roles (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    Name            NVARCHAR(100)   NOT NULL UNIQUE,
    IsProtected     BIT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);
GO

INSERT INTO Roles (Name, IsProtected) VALUES
('Owner',      1),
('Admin',      1),
('Dispatcher', 1),
('Employee',   1),
('Finance',    1);
GO

CREATE TABLE RolePermissions (
    RoleId          INT             NOT NULL,
    PermissionKey   NVARCHAR(100)   NOT NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionKey),
    CONSTRAINT FK_RolePermission_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);
GO
GO

-- ============================================================
-- CORRECTED RolePermissions SEED (matching PermissionKeys.All)
-- ============================================================

-- Clear any previous permissions (fresh start)
DELETE FROM RolePermissions;
GO

-- Define the exact list of permission keys used in C# (PermissionKeys.All)
DECLARE @AllowedKeys TABLE (KeyName NVARCHAR(100));
INSERT INTO @AllowedKeys (KeyName) VALUES
    -- Pages
    ('pages.daily'), ('pages.bookings'), ('pages.schedule'), ('pages.users'),
    ('pages.roles'), ('pages.clients'), ('pages.kanban'), ('pages.sop'), ('pages.reports'), ('pages.inventory'),
    -- Bookings
    ('bookings.view'), ('bookings.create'), ('bookings.edit'), ('bookings.delete'),('bookings.progress'),
    -- Clients
    ('clients.view'), ('clients.create'), ('clients.edit'), ('clients.delete'),
    -- Invoices
    ('invoices.view'), ('invoices.create'), ('invoices.edit'),
    -- SOPs
    ('sops.view'), ('sops.manage'),
    -- Services
    ('services.view'), ('services.manage'),
    -- Inventory
    ('inventory.view'), ('inventory.manage'),
    -- Schedule
    ('schedule.view'), ('schedule.edit'),
    -- Users
    ('users.view'), ('users.create'), ('users.edit'),
    -- Roles
    ('roles.view'), ('roles.manage'),
    -- Reports
    ('reports.view'), ('reports.export');

-- Owner gets all keys
INSERT INTO RolePermissions (RoleId, PermissionKey)
SELECT r.Id, k.KeyName
FROM Roles r
CROSS JOIN @AllowedKeys k
WHERE r.Name = 'Owner'
AND NOT EXISTS (SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionKey = k.KeyName);

-- Admin gets all except user/role management and services.manage
INSERT INTO RolePermissions (RoleId, PermissionKey)
SELECT r.Id, k.KeyName
FROM Roles r
CROSS JOIN @AllowedKeys k
WHERE r.Name = 'Admin'
AND k.KeyName NOT IN ('pages.users','pages.roles','users.view','users.create','users.edit','roles.view','roles.manage','services.manage')
AND NOT EXISTS (SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionKey = k.KeyName);

-- Dispatcher: only operational pages and booking/schedule actions
INSERT INTO RolePermissions (RoleId, PermissionKey)
SELECT r.Id, k.KeyName
FROM Roles r
CROSS JOIN @AllowedKeys k
WHERE r.Name = 'Dispatcher'
AND k.KeyName IN ('pages.daily','pages.bookings','pages.clients','pages.kanban','pages.schedule',
                  'bookings.view','bookings.create','bookings.edit','schedule.edit')
AND NOT EXISTS (SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionKey = k.KeyName);

-- Employee: only daily view and kanban (personal)
INSERT INTO RolePermissions (RoleId, PermissionKey)
SELECT r.Id, k.KeyName
FROM Roles r
CROSS JOIN @AllowedKeys k
WHERE r.Name = 'Employee'
AND k.KeyName IN ('pages.daily','pages.kanban','bookings.progress')
AND NOT EXISTS (SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionKey = k.KeyName);

-- Finance: bookings, clients, reports, and ability to record payments (bookings.edit)
INSERT INTO RolePermissions (RoleId, PermissionKey)
SELECT r.Id, k.KeyName
FROM Roles r
CROSS JOIN @AllowedKeys k
WHERE r.Name = 'Finance'
AND k.KeyName IN ('pages.bookings','pages.clients','pages.reports','bookings.edit','reports.export')
AND NOT EXISTS (SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionKey = k.KeyName);
GO

-- Add RoleId to Employees after Roles table exists
ALTER TABLE Employees ADD RoleId INT NOT NULL DEFAULT 1;
ALTER TABLE Employees ADD CONSTRAINT FK_Employee_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id);
GO

CREATE TABLE WeeklySchedule (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    DayOfWeek       INT             NOT NULL,
    StartHour       INT             NOT NULL,
    EndHour         INT             NOT NULL,
    Capacity        INT             NOT NULL DEFAULT 1,
    CONSTRAINT CHK_WeeklySchedule_Day   CHECK (DayOfWeek BETWEEN 0 AND 6),
    CONSTRAINT CHK_WeeklySchedule_Hours CHECK (
        (StartHour = 0 AND EndHour = 0) OR
        (StartHour >= 0 AND EndHour <= 24 AND StartHour < EndHour)
    )
);
GO
CREATE UNIQUE INDEX IX_WeeklySchedule_DayOfWeek ON WeeklySchedule(DayOfWeek);
GO

INSERT INTO WeeklySchedule (DayOfWeek, StartHour, EndHour, Capacity) VALUES
(1, 8, 17, 3),
(2, 8, 17, 3),
(3, 8, 17, 3),
(4, 8, 17, 3),
(5, 8, 17, 3),
(6, 9, 14, 1),
(0, 0,  0, 0);
GO

CREATE TABLE DateOverride (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    Date            DATE            NOT NULL,
    StartHour       INT             NULL,
    EndHour         INT             NULL,
    Capacity        INT             NULL,
    IsFullyClosed   BIT             NOT NULL DEFAULT 0,
    CONSTRAINT CHK_DateOverride_Start CHECK (StartHour IS NULL OR (StartHour >= 0 AND StartHour <= 23)),
    CONSTRAINT CHK_DateOverride_End   CHECK (EndHour   IS NULL OR (EndHour   >= 0 AND EndHour   <= 24))
);
GO
CREATE UNIQUE INDEX IX_DateOverride_Date ON DateOverride(Date);
GO

CREATE TABLE BookingRequests (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    ContactName     NVARCHAR(200)   NOT NULL,
    Phone           NVARCHAR(50)    NOT NULL,
    Email           NVARCHAR(255)   NOT NULL,
    Notes           NVARCHAR(MAX)   NULL,
    EstimatedPrice  DECIMAL(10,2)   NULL,
    AdminNotes      NVARCHAR(MAX)   NULL,
    Status          NVARCHAR(50)    NOT NULL DEFAULT 'New',
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CHK_BookingRequest_Status CHECK (Status IN ('New', 'AdminReviewed', 'SentToCustomer', 'CustomerConfirmed', 'Cancelled', 'Converted'))
);
GO

CREATE TABLE BookingRequestServices (
    BookingRequestId INT            NOT NULL,
    ServiceCatalogId INT            NOT NULL,
    CONSTRAINT PK_BookingRequestServices PRIMARY KEY (BookingRequestId, ServiceCatalogId),
    CONSTRAINT FK_BookingRequestService_Request FOREIGN KEY (BookingRequestId)
        REFERENCES BookingRequests(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BookingRequestService_Catalog FOREIGN KEY (ServiceCatalogId)
        REFERENCES ServiceCatalog(Id)
);
GO

CREATE TABLE Bookings (
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    ClientId            INT             NOT NULL,
    SiteId              INT             NULL,
    ServiceType         NVARCHAR(50)    NOT NULL,
    ScheduledDate       DATE            NOT NULL,
    ScheduledTimeSlot   TIME            NULL,
    Status              NVARCHAR(50)    NOT NULL DEFAULT 'Pending',
    Notes               NVARCHAR(MAX)   NULL,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CompletedAt         DATETIME2       NULL,
    RecurringScheduleId INT             NULL,
    CONSTRAINT FK_Booking_Client    FOREIGN KEY (ClientId)           REFERENCES Clients(Id),
    CONSTRAINT FK_Booking_Site      FOREIGN KEY (SiteId)             REFERENCES Sites(Id),
    CONSTRAINT CHK_Booking_ServiceType CHECK (ServiceType IN ('Vehicle', 'SiteBased', 'Boat')),
    CONSTRAINT CHK_Booking_Status      CHECK (Status IN ('Pending', 'InProgress', 'Completed', 'Cancelled')),
    CONSTRAINT CHK_Booking_CompletedAt CHECK (Status != 'Completed' OR CompletedAt IS NOT NULL)
);
GO
CREATE INDEX IX_Bookings_ClientId      ON Bookings(ClientId);
CREATE INDEX IX_Bookings_SiteId        ON Bookings(SiteId);
CREATE INDEX IX_Bookings_ServiceType   ON Bookings(ServiceType);
CREATE INDEX IX_Bookings_ScheduledDate ON Bookings(ScheduledDate);
CREATE INDEX IX_Bookings_Status        ON Bookings(Status);
GO

CREATE TABLE BookingAssignments (
    BookingId   INT         NOT NULL,
    EmployeeId  INT         NOT NULL,
    AssignedAt  DATETIME2   NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BookingAssignments PRIMARY KEY (BookingId, EmployeeId),
    CONSTRAINT FK_BookingAssignment_Booking  FOREIGN KEY (BookingId)  REFERENCES Bookings(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BookingAssignment_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);
GO
CREATE INDEX IX_BookingAssignments_EmployeeId ON BookingAssignments(EmployeeId);
GO

CREATE TABLE BookingServices (
    BookingId           INT             NOT NULL,
    ServiceCatalogId    INT             NOT NULL,
    EstimatedPrice      DECIMAL(10,2)   NULL,
    FinalPrice          DECIMAL(10,2)   NULL,
    Quantity            DECIMAL(10,2)   NOT NULL DEFAULT 1,
    Notes               NVARCHAR(MAX)   NULL,
    CONSTRAINT PK_BookingServices PRIMARY KEY (BookingId, ServiceCatalogId),
    CONSTRAINT FK_BookingService_Booking FOREIGN KEY (BookingId)        REFERENCES Bookings(Id)       ON DELETE CASCADE,
    CONSTRAINT FK_BookingService_Catalog FOREIGN KEY (ServiceCatalogId) REFERENCES ServiceCatalog(Id)
);
GO

-- ============================================================
-- RECURRING SCHEDULES
-- ============================================================

CREATE TABLE RecurringSchedules (
    Id                          INT             PRIMARY KEY IDENTITY(1,1),
    SourceBookingId             INT             NOT NULL,
    Frequency                   NVARCHAR(20)    NOT NULL,
    DayOfWeek                   INT             NULL,
    DayOfMonth                  INT             NULL,
    AutoGenerateWeeksAhead      INT             NOT NULL DEFAULT 4,
    IsActive                    BIT             NOT NULL DEFAULT 1,
    EndsOn                      DATE            NULL,
    CreatedAt                   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt                   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_RecurringSchedule_SourceBooking FOREIGN KEY (SourceBookingId) REFERENCES Bookings(Id),
    CONSTRAINT CHK_RecurringSchedule_Frequency CHECK (Frequency IN ('Weekly', 'Biweekly', 'Monthly')),
    CONSTRAINT CHK_RecurringSchedule_DayOfWeek CHECK (DayOfWeek IS NULL OR (DayOfWeek >= 0 AND DayOfWeek <= 6)),
    CONSTRAINT CHK_RecurringSchedule_DayOfMonth CHECK (DayOfMonth IS NULL OR (DayOfMonth >= 1 AND DayOfMonth <= 28)),
    CONSTRAINT CHK_RecurringSchedule_AutoGenerateWeeksAhead CHECK (AutoGenerateWeeksAhead BETWEEN 1 AND 52)
);
GO
CREATE INDEX IX_RecurringSchedules_SourceBookingId ON RecurringSchedules(SourceBookingId);
CREATE INDEX IX_RecurringSchedules_IsActive ON RecurringSchedules(IsActive);
GO

ALTER TABLE Bookings ADD CONSTRAINT FK_Booking_RecurringSchedule FOREIGN KEY (RecurringScheduleId) REFERENCES RecurringSchedules(Id) ON DELETE SET NULL;
GO
CREATE INDEX IX_Bookings_RecurringScheduleId ON Bookings(RecurringScheduleId);
GO

CREATE TABLE VehicleBookingDetails (
    BookingId       INT             PRIMARY KEY,
    LicensePlate    NVARCHAR(20)    NOT NULL,
    CarModel        NVARCHAR(100)   NULL,
    Notes           NVARCHAR(MAX)   NULL,
    CONSTRAINT FK_VehicleDetail_Booking FOREIGN KEY (BookingId) REFERENCES Bookings(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_VehicleDetails_License ON VehicleBookingDetails(LicensePlate);
GO

CREATE TABLE BoatBookingDetails (
    BookingId       INT             PRIMARY KEY,
    BoatType        NVARCHAR(100)   NOT NULL,
    LengthMeters    DECIMAL(5,2)    NULL,
    Notes           NVARCHAR(MAX)   NULL,
    CONSTRAINT FK_BoatDetail_Booking FOREIGN KEY (BookingId) REFERENCES Bookings(Id) ON DELETE CASCADE
);
GO

CREATE SEQUENCE InvoiceNumberSeq
    START WITH 1
    INCREMENT BY 1
    NO CYCLE;
GO

CREATE TABLE Invoices (
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    InvoiceNumber       NVARCHAR(50)    NOT NULL UNIQUE,
    ClientId            INT             NOT NULL,
    IssueDate           DATE            NOT NULL DEFAULT CAST(GETUTCDATE() AS DATE),
    DueDate             DATE            NOT NULL,
    SubTotal            DECIMAL(10,2)   NOT NULL DEFAULT 0,
    DiscountAmount      DECIMAL(10,2)   NOT NULL DEFAULT 0,
    VatPct              DECIMAL(5,2)    NOT NULL DEFAULT 0,
    VatAmount           DECIMAL(10,2)   NOT NULL DEFAULT 0,
    TotalAmount         DECIMAL(10,2)   NOT NULL DEFAULT 0,
    Status              NVARCHAR(50)    NOT NULL DEFAULT 'Draft',
    Notes               NVARCHAR(MAX)   NULL,
    CreatedByEmployeeId INT             NULL,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Invoice_Client      FOREIGN KEY (ClientId)            REFERENCES Clients(Id),
    CONSTRAINT FK_Invoice_CreatedBy   FOREIGN KEY (CreatedByEmployeeId) REFERENCES Employees(Id),
    CONSTRAINT CHK_Invoice_Status     CHECK (Status IN ('Draft', 'Sent', 'PartiallyPaid', 'Paid', 'Overdue', 'WrittenOff')),
    CONSTRAINT CHK_Invoice_DueDate    CHECK (DueDate >= IssueDate),
    CONSTRAINT CHK_Invoice_SubTotal   CHECK (SubTotal >= 0),
    CONSTRAINT CHK_Invoice_Discount   CHECK (DiscountAmount >= 0),
    CONSTRAINT CHK_Invoice_VatPct     CHECK (VatPct >= 0 AND VatPct <= 100),
    CONSTRAINT CHK_Invoice_Total      CHECK (TotalAmount >= 0)
);
GO

CREATE INDEX IX_Invoices_ClientId  ON Invoices(ClientId);
CREATE INDEX IX_Invoices_Status    ON Invoices(Status);
CREATE INDEX IX_Invoices_IssueDate ON Invoices(IssueDate);
CREATE INDEX IX_Invoices_DueDate   ON Invoices(DueDate);
GO

CREATE TABLE InvoiceLines (
    Id          INT             PRIMARY KEY IDENTITY(1,1),
    InvoiceId   INT             NOT NULL,
    Description NVARCHAR(500)   NOT NULL,
    Quantity    DECIMAL(10,2)   NOT NULL DEFAULT 1,
    UnitPrice   DECIMAL(10,2)   NOT NULL,
    DiscountPct DECIMAL(5,2)    NULL DEFAULT 0,
    VatPct      DECIMAL(5,2)    NOT NULL DEFAULT 0,
    SourceType  NVARCHAR(50)    NULL,
    SourceId    INT             NULL,
    CreatedAt   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_InvoiceLine_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
    CONSTRAINT CHK_InvoiceLine_Source CHECK (SourceType IN ('Booking', 'Manual', 'CreditNote') OR SourceType IS NULL),
    CONSTRAINT CHK_InvoiceLine_QtyPrice CHECK (Quantity > 0 AND UnitPrice >= 0),
    CONSTRAINT CHK_InvoiceLine_Discount CHECK (DiscountPct >= 0 AND DiscountPct <= 100),
    CONSTRAINT CHK_InvoiceLine_Vat CHECK (VatPct >= 0 AND VatPct <= 100)
);
GO
CREATE INDEX IX_InvoiceLines_InvoiceId ON InvoiceLines(InvoiceId);
CREATE INDEX IX_InvoiceLines_Source    ON InvoiceLines(SourceType, SourceId);
GO

CREATE TABLE InvoiceBookings (
    InvoiceId   INT     NOT NULL,
    BookingId   INT     NOT NULL PRIMARY KEY,
    CONSTRAINT FK_InvoiceBooking_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
    CONSTRAINT FK_InvoiceBooking_Booking FOREIGN KEY (BookingId) REFERENCES Bookings(Id)
);
GO
CREATE INDEX IX_InvoiceBookings_InvoiceId ON InvoiceBookings(InvoiceId);
GO

CREATE TABLE Payments (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    InvoiceId       INT             NOT NULL,
    PaymentDate     DATE            NOT NULL,
    Amount          DECIMAL(10,2)   NOT NULL,
    Method          NVARCHAR(50)    NOT NULL DEFAULT 'BankTransfer',
    Reference       NVARCHAR(200)   NULL,
    Notes           NVARCHAR(MAX)   NULL,
    RecordedBy      INT             NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Payment_Invoice    FOREIGN KEY (InvoiceId)   REFERENCES Invoices(Id)   ON DELETE CASCADE,
    CONSTRAINT FK_Payment_RecordedBy FOREIGN KEY (RecordedBy)  REFERENCES Employees(Id),
    CONSTRAINT CHK_Payment_Amount    CHECK (Amount > 0),
    CONSTRAINT CHK_Payment_Method    CHECK (Method IN ('BankTransfer', 'Cash', 'Card', 'Other'))
);
GO
CREATE INDEX IX_Payments_InvoiceId ON Payments(InvoiceId);
CREATE INDEX IX_Payments_Date      ON Payments(PaymentDate);
GO

-- ============================================================
-- SEED DATA (Employees, Clients, Sites, etc.)
-- ============================================================

INSERT INTO Employees (Username, PasswordHash, SecurityStamp, FirstName, LastName, Phone, EmployeeCode, HourlyRate, MaxJobsPerDay, IsActive, CreatedAt, UpdatedAt, RoleId)
VALUES (
    'owner',
    '$2y$10$qZKh.FlEZrHNSyAcazlNdOyBMHA.SJSfnLDoPtuFKt9Mrj99tdNEe',
    NEWID(),
    'Owner',
    'User',
    NULL,
    'EMP-001',
    NULL,
    NULL,
    1,
    '2026-01-01T00:00:00Z',
    '2026-01-01T00:00:00Z',
    (SELECT Id FROM Roles WHERE Name = 'Owner')
);
GO

WITH N AS (
    SELECT TOP (50) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects
),
Seeded AS (
    SELECT
        n,
        LOWER(
            LEFT(CHOOSE((n % 10) + 1, 'Matej','Ana','Ivan','Luka','Sara','Petra','Niko','Tina','Filip','Maja'), 1)
            + CHOOSE((n % 10) + 1, 'Kovac','Horvat','Novak','Maric','Klaric','Botic','Skoko','Jukic','Peric','Skoric')
        ) AS BaseUsername,
        ROW_NUMBER() OVER (
            PARTITION BY LOWER(
                LEFT(CHOOSE((n % 10) + 1, 'Matej','Ana','Ivan','Luka','Sara','Petra','Niko','Tina','Filip','Maja'), 1)
                + CHOOSE((n % 10) + 1, 'Kovac','Horvat','Novak','Maric','Klaric','Botic','Skoko','Jukic','Peric','Skoric')
            )
            ORDER BY n
        ) AS rn
    FROM N
)
INSERT INTO Employees (Username, PasswordHash, FirstName, LastName, Phone, EmployeeCode, HourlyRate, MaxJobsPerDay, IsActive, RoleId)
SELECT
    CASE WHEN rn = 1 THEN BaseUsername ELSE BaseUsername + CAST(rn AS NVARCHAR(10)) END,
    '$2y$10$qZKh.FlEZrHNSyAcazlNdOyBMHA.SJSfnLDoPtuFKt9Mrj99tdNEe',
    CHOOSE((n % 10) + 1, 'Matej','Ana','Ivan','Luka','Sara','Petra','Niko','Tina','Filip','Maja'),
    CHOOSE((n % 10) + 1, 'Kovac','Horvat','Novak','Maric','Klaric','Botic','Skoko','Jukic','Peric','Skoric'),
    CONCAT('+385 9', RIGHT('0000000' + CAST(n AS VARCHAR(7)), 7)),
    CONCAT('EMP-', RIGHT('000' + CAST(n + 1 AS VARCHAR(3)), 3)),
    CAST(25 + (n % 15) AS DECIMAL(10,2)),
    2 + (n % 4),
    CASE WHEN n % 12 = 0 THEN 0 ELSE 1 END,
    (SELECT Id FROM Roles WHERE Name = CHOOSE((n % 4) + 1, 'Admin', 'Dispatcher', 'Employee', 'Finance'))
FROM Seeded;
GO

WITH N AS (
    SELECT TOP (40) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects
)
INSERT INTO Clients (ClientName, Type, Oib, PaymentTerms, Notes, IsActive)
SELECT
    CONCAT('Client ', n),
    CASE WHEN n % 2 = 0 THEN 'Person' ELSE 'Business' END,   -- alternate Person/Business
    RIGHT('00000000000' + CAST(10000000000 + n AS VARCHAR(11)), 11),
    CASE WHEN n % 4 = 0 THEN 'Net30' WHEN n % 3 = 0 THEN 'Net15' WHEN n % 5 = 0 THEN 'DueOnReceipt' ELSE NULL END,
    CONCAT('Mock client notes for client ', n),
    CASE WHEN n % 15 = 0 THEN 0 ELSE 1 END
FROM N;
GO

INSERT INTO Contacts (ClientId, ContactName, Role, Phone, Email, Address, IsPrimary, IsActive)
SELECT
    c.Id,
    CONCAT('Contact ', c.Id),
    'Primary',
    CONCAT('+385 98', RIGHT('000000' + CAST(c.Id AS VARCHAR(6)), 6)),
    CONCAT('contact', c.Id, '@client.com'),
    CONCAT('Main Street ', c.Id, ', Zagreb'),
    1,
    1
FROM Clients c;
GO

INSERT INTO Contacts (ClientId, ContactName, Role, Phone, Email, Address, IsPrimary, IsActive)
SELECT
    c.Id,
    CONCAT('Alt Contact ', c.Id),
    'Secondary',
    CONCAT('+385 99', RIGHT('000000' + CAST(c.Id AS VARCHAR(6)), 6)),
    CONCAT('alt', c.Id, '@client.com'),
    CONCAT('Second Street ', c.Id, ', Split'),
    0,
    1
FROM Clients c
WHERE c.Id % 3 = 0;
GO

WITH SiteNums AS (
    SELECT 1 AS s UNION ALL SELECT 2 UNION ALL SELECT 3
)
INSERT INTO Sites (ClientId, SiteName, Address, City, PostalCode, SiteType, FloorAreaM2, AccessNotes, IsActive)
SELECT
    c.Id,
    CONCAT('Site ', c.Id, '-', s.s),
    CONCAT('Site Address ', c.Id, '-', s.s),
    CHOOSE((c.Id % 5) + 1, 'Zagreb', 'Split', 'Rijeka', 'Osijek', 'Zadar'),
    RIGHT('10000' + CAST((c.Id * 7 + s.s) AS VARCHAR(5)), 5),
    CHOOSE(((c.Id + s.s) % 7) + 1, 'Office', 'Stairwell', 'Garage', 'Facility', 'Boat', 'Vehicle', 'Other'),
    CAST(50 + (c.Id * 3 + s.s * 10) AS DECIMAL(10,2)),
    CONCAT('Access notes for site ', c.Id, '-', s.s),
    1
FROM Clients c
JOIN SiteNums s ON s.s <= CASE WHEN c.Id % 4 = 0 THEN 3 WHEN c.Id % 2 = 0 THEN 2 ELSE 1 END;
GO

WITH N AS (
    SELECT TOP (20) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects
)
INSERT INTO DateOverride (Date, StartHour, EndHour, Capacity, IsFullyClosed)
SELECT
    DATEADD(DAY, n * 5, '2026-01-05'),
    CASE WHEN n % 5 = 0 THEN NULL ELSE 9 END,
    CASE WHEN n % 5 = 0 THEN NULL ELSE 14 END,
    CASE WHEN n % 4 = 0 THEN NULL ELSE 2 END,
    CASE WHEN n % 5 = 0 THEN 1 ELSE 0 END
FROM N;
GO

WITH N AS (
    SELECT TOP (120) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects
)
INSERT INTO Bookings (
    ClientId, SiteId, ServiceType, ScheduledDate, ScheduledTimeSlot, Status, Notes, CreatedAt, UpdatedAt, CompletedAt, RecurringScheduleId
)
SELECT
    ((n - 1) % (SELECT COUNT(*) FROM Clients)) + 1,
    CASE WHEN n % 5 = 0 THEN NULL ELSE ((n - 1) % (SELECT COUNT(*) FROM Sites)) + 1 END,
    CHOOSE((n % 3) + 1, 'Vehicle', 'SiteBased', 'Boat'),
    DATEADD(DAY, (n % 60) - 20, CAST('2026-05-01' AS DATE)),
    CASE WHEN n % 4 = 0 THEN NULL ELSE CAST(CONCAT(8 + (n % 8), ':00') AS TIME) END,
    s.Status,
    CONCAT('Mock booking note ', n),
    DATEADD(DAY, -1 * (n % 30), GETUTCDATE()),
    GETUTCDATE(),
    CASE WHEN s.Status = 'Completed' THEN DATEADD(DAY, -1, GETUTCDATE()) ELSE NULL END,
    NULL
FROM N
CROSS APPLY (SELECT CHOOSE((n % 4) + 1, 'Pending', 'InProgress', 'Completed', 'Cancelled') AS Status) s;
GO

INSERT INTO BookingAssignments (BookingId, EmployeeId, AssignedAt)
SELECT
    b.Id,
    ((b.Id - 1) % (SELECT COUNT(*) FROM Employees)) + 1,
    GETUTCDATE()
FROM Bookings b
WHERE b.Id % 7 <> 0;
GO

WITH S AS (
    SELECT 1 AS s UNION ALL SELECT 2
)
INSERT INTO BookingServices (BookingId, ServiceCatalogId, EstimatedPrice, FinalPrice, Quantity, Notes)
SELECT
    b.Id,
    ((b.Id + s.s) % (SELECT COUNT(*) FROM ServiceCatalog)) + 1,
    CAST(20 + ((b.Id + s.s) % 50) * 5 AS DECIMAL(10,2)),
    CASE WHEN b.Status = 'Completed' THEN CAST(25 + ((b.Id + s.s) % 50) * 5 AS DECIMAL(10,2)) ELSE NULL END,
    CAST(1 + (b.Id % 3) AS DECIMAL(10,2)),
    CONCAT('Service note for booking ', b.Id, ' item ', s.s)
FROM Bookings b
CROSS JOIN S;
GO

INSERT INTO VehicleBookingDetails (BookingId, LicensePlate, CarModel, Notes)
SELECT
    b.Id,
    CONCAT('ZG', RIGHT('0000' + CAST(b.Id AS VARCHAR(4)), 4), 'CP'),
    CHOOSE((b.Id % 6) + 1, 'Skoda Octavia', 'VW Golf', 'Toyota Corolla', 'Renault Clio', 'Audi A4', 'Ford Focus'),
    CONCAT('Vehicle details for booking ', b.Id)
FROM Bookings b
WHERE b.ServiceType = 'Vehicle';
GO

INSERT INTO BoatBookingDetails (BookingId, BoatType, LengthMeters, Notes)
SELECT
    b.Id,
    CHOOSE((b.Id % 5) + 1, 'Sailboat', 'Yacht', 'Motorboat', 'Catamaran', 'Fishing'),
    CAST(6 + (b.Id % 12) AS DECIMAL(5,2)),
    CONCAT('Boat details for booking ', b.Id)
FROM Bookings b
WHERE b.ServiceType = 'Boat';
GO

-- ============================================================
-- INVOICE SEED (unchanged)
-- ============================================================

DECLARE @InvoiceMap TABLE (BookingId INT, InvoiceId INT);

WITH BookingTotals AS (
    SELECT
        b.Id                                                        AS BookingId,
        b.ClientId,
        aa.EmployeeId                                               AS CreatedByEmployeeId,
        b.ScheduledDate,
        b.Status,
        SUM(bs.EstimatedPrice * bs.Quantity)                        AS SubTotal,
        ROW_NUMBER() OVER (ORDER BY b.Id)                           AS RowNum
    FROM Bookings b
    OUTER APPLY (
        SELECT TOP 1 ba.EmployeeId
        FROM BookingAssignments ba
        WHERE ba.BookingId = b.Id
        ORDER BY ba.EmployeeId
    ) aa
    JOIN BookingServices bs ON bs.BookingId = b.Id
    WHERE b.Id % 2 = 0
    GROUP BY b.Id, b.ClientId, aa.EmployeeId, b.ScheduledDate, b.Status
)
INSERT INTO Invoices (
    InvoiceNumber, ClientId, IssueDate, DueDate, SubTotal, DiscountAmount,
    VatPct, VatAmount, TotalAmount, Status, Notes, CreatedByEmployeeId
)
SELECT
    CONCAT('INV-', YEAR(ScheduledDate), '-', RIGHT('0000' + CAST(RowNum AS NVARCHAR(10)), 4)),
    ClientId,
    ScheduledDate,
    DATEADD(DAY, 15, ScheduledDate),
    SubTotal,
    CASE WHEN BookingId % 5 = 0 THEN 10 ELSE 0 END,
    25,
    ROUND((SubTotal - CASE WHEN BookingId % 5 = 0 THEN 10 ELSE 0 END) * 0.25, 2),
    ROUND((SubTotal - CASE WHEN BookingId % 5 = 0 THEN 10 ELSE 0 END) * 1.25, 2),
    CHOOSE((BookingId % 5) + 1, 'Draft', 'Sent', 'PartiallyPaid', 'Paid', 'Overdue'),
    CONCAT('Invoice for booking ', BookingId),
    CreatedByEmployeeId
FROM BookingTotals;

INSERT INTO @InvoiceMap (BookingId, InvoiceId)
SELECT
    bt.BookingId,
    i.Id
FROM Invoices i
JOIN (
    SELECT
        b.Id                                        AS BookingId,
        b.ScheduledDate,
        ROW_NUMBER() OVER (ORDER BY b.Id)           AS RowNum
    FROM Bookings b
    JOIN BookingServices bs ON bs.BookingId = b.Id
    WHERE b.Id % 2 = 0
    GROUP BY b.Id, b.ScheduledDate
) bt ON i.InvoiceNumber = CONCAT('INV-', YEAR(bt.ScheduledDate), '-', RIGHT('0000' + CAST(bt.RowNum AS NVARCHAR(10)), 4));

INSERT INTO InvoiceBookings (InvoiceId, BookingId)
SELECT InvoiceId, BookingId
FROM @InvoiceMap;

INSERT INTO InvoiceLines (InvoiceId, Description, Quantity, UnitPrice, DiscountPct, VatPct, SourceType, SourceId)
SELECT
    im.InvoiceId,
    CONCAT('Service ', sc.CatalogCode, ' - ', sc.Name),
    bs.Quantity,
    CAST(CASE WHEN bs.EstimatedPrice IS NULL THEN 0 ELSE bs.EstimatedPrice END / NULLIF(bs.Quantity, 0) AS DECIMAL(10,2)),
    CASE WHEN bs.BookingId % 6 = 0 THEN 5 ELSE 0 END,
    25,
    'Booking',
    bs.BookingId
FROM @InvoiceMap im
JOIN BookingServices bs ON bs.BookingId = im.BookingId
JOIN ServiceCatalog sc ON sc.Id = bs.ServiceCatalogId;

INSERT INTO InvoiceLines (InvoiceId, Description, Quantity, UnitPrice, DiscountPct, VatPct, SourceType, SourceId)
SELECT
    i.Id,
    'Administrative fee',
    1,
    15.00,
    0,
    25,
    'Manual',
    NULL
FROM Invoices i
WHERE i.Id % 4 = 0;

INSERT INTO Payments (InvoiceId, PaymentDate, Amount, Method, Reference, Notes, RecordedBy)
SELECT
    i.Id,
    DATEADD(DAY, 2, i.IssueDate),
    CASE WHEN i.Status = 'PartiallyPaid' THEN ROUND(i.TotalAmount * 0.5, 2) ELSE i.TotalAmount END,
    CHOOSE((i.Id % 4) + 1, 'BankTransfer', 'Cash', 'Card', 'Other'),
    CONCAT('PAY-', i.InvoiceNumber),
    CONCAT('Payment for invoice ', i.InvoiceNumber),
    i.CreatedByEmployeeId
FROM Invoices i
WHERE i.Status IN ('Paid', 'PartiallyPaid');
GO

-- ============================================================
-- SOP MODULE (unchanged)
-- ============================================================

CREATE TABLE SopTemplates (
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    ServiceCatalogId    INT             NULL,
    Name                NVARCHAR(200)   NOT NULL,
    ServiceType         NVARCHAR(50)    NOT NULL,
    Description         NVARCHAR(MAX)   NULL,
    IsActive            BIT             NOT NULL DEFAULT 1,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_SopTemplate_ServiceCatalog FOREIGN KEY (ServiceCatalogId)
        REFERENCES ServiceCatalog(Id) ON DELETE SET NULL,
    CONSTRAINT CHK_SopTemplate_ServiceType CHECK (ServiceType IN ('Vehicle','SiteBased','Boat','Generic'))
);
GO
CREATE INDEX IX_SopTemplates_ServiceCatalog ON SopTemplates(ServiceCatalogId);
CREATE INDEX IX_SopTemplates_ServiceType    ON SopTemplates(ServiceType);
GO

CREATE TABLE ChecklistItems (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    SopTemplateId   INT             NOT NULL,
    ItemText        NVARCHAR(500)   NOT NULL,
    SortOrder       INT             NOT NULL DEFAULT 0,
    IsRequired      BIT             NOT NULL DEFAULT 1,
    CONSTRAINT FK_ChecklistItem_SopTemplate FOREIGN KEY (SopTemplateId)
        REFERENCES SopTemplates(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_ChecklistItems_SopTemplate ON ChecklistItems(SopTemplateId);
GO

CREATE TABLE BookingSopAssignments (
    BookingId           INT             NOT NULL,
    SopTemplateId       INT             NOT NULL,
    CustomInstructions  NVARCHAR(MAX)   NULL,
    AssignedAt          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BookingSopAssignments PRIMARY KEY (BookingId, SopTemplateId),
    CONSTRAINT FK_BookingSop_Booking FOREIGN KEY (BookingId)
        REFERENCES Bookings(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BookingSop_Sop     FOREIGN KEY (SopTemplateId)
        REFERENCES SopTemplates(Id)
);
GO
CREATE INDEX IX_BookingSop_Sop ON BookingSopAssignments(SopTemplateId);
GO

CREATE TABLE ChecklistResponses (
    BookingId           INT             NOT NULL,
    SopTemplateId       INT             NOT NULL,
    ChecklistItemId     INT             NOT NULL,
    IsCompleted         BIT             NOT NULL DEFAULT 0,
    CompletedAt         DATETIME2       NULL,
    Notes               NVARCHAR(500)   NULL,
    CONSTRAINT PK_ChecklistResponses PRIMARY KEY (BookingId, SopTemplateId, ChecklistItemId),
    CONSTRAINT FK_ChecklistResponse_Assignment FOREIGN KEY (BookingId, SopTemplateId)
        REFERENCES BookingSopAssignments(BookingId, SopTemplateId) ON DELETE CASCADE,
    CONSTRAINT FK_ChecklistResponse_Item       FOREIGN KEY (ChecklistItemId)
        REFERENCES ChecklistItems(Id)
);
GO

-- SOP SEED DATA
INSERT INTO SopTemplates (ServiceCatalogId, Name, ServiceType, Description) VALUES
(1,  'Stubište osnovno čišćenje', 'SiteBased', 'Standardni postupak za tjedno čišćenje stubišta'),
(4,  'Uredsko osnovno čišćenje', 'SiteBased', 'Osnovno čišćenje uredskih prostora'),
(23, 'Carwash LIM komplet', 'Vehicle', 'Kompletno vanjsko i unutarnje pranje manjeg vozila'),
(36, 'Kemijsko čišćenje sjedala', 'Vehicle', 'Dubinsko kemijsko čišćenje tapeciranih površina'),
(3,  'Stubište premium', 'SiteBased', 'Premium čišćenje velikih stambenih objekata');

INSERT INTO ChecklistItems (SopTemplateId, ItemText, SortOrder, IsRequired) VALUES
(1, 'Pomesti sve stepenice i podeste', 1, 1),
(1, 'Oprati podove mokrom krpom', 2, 1),
(1, 'Očistiti rukohvate i ograde', 3, 1),
(1, 'Provjeriti i zamijeniti pregorjele žarulje', 4, 0),
(1, 'Isprazniti pepeljare i kante za smeće', 5, 1),
(2, 'Isprazniti košarice za papir', 1, 1),
(2, 'Obrisati sve radne površine', 2, 1),
(2, 'Usisati podove i tepihe', 3, 1),
(2, 'Očistiti staklene površine', 4, 0),
(2, 'Dezinficirati kvake i telefone', 5, 1),
(3, 'Predpranje vozila visokotlačnim čistačem', 1, 1),
(3, 'Ručno pranje karoserije spužvom', 2, 1),
(3, 'Čišćenje felgi i guma', 3, 1),
(3, 'Usisavanje unutrašnjosti', 4, 1),
(3, 'Brisanje plastičnih dijelova i stakala iznutra', 5, 1),
(3, 'Nanošenje voska (ako je ugovoreno)', 6, 0),
(4, 'Usisavanje sjedala prije tretmana', 1, 1),
(4, 'Nanošenje kemijskog sredstva', 2, 1),
(4, 'Strojno ribanje i ekstrakcija', 3, 1),
(4, 'Provjera rezultata i po potrebi ponoviti', 4, 0),
(5, 'Pomesti sve etaže i podeste', 1, 1),
(5, 'Strojno ribanje podova', 2, 1),
(5, 'Očistiti prozore i prozorske klupčice', 3, 1),
(5, 'Očistiti ulazna vrata i okvire', 4, 1),
(5, 'Provjera sigurnosne rasvjete', 5, 0);

INSERT INTO BookingSopAssignments (BookingId, SopTemplateId, CustomInstructions)
SELECT b.Id, st.Id, NULL
FROM Bookings b
CROSS JOIN (SELECT Id FROM SopTemplates WHERE ServiceType = 'SiteBased') st
WHERE b.Id BETWEEN 1 AND 15
  AND b.ServiceType = 'SiteBased'
  AND b.Status NOT IN ('Cancelled')
  AND b.Id % 2 = 0;

INSERT INTO BookingSopAssignments (BookingId, SopTemplateId, CustomInstructions)
SELECT b.Id, st.Id, 'Preskočiti vosak - klijent ne želi'
FROM Bookings b
CROSS JOIN (SELECT Id FROM SopTemplates WHERE Name = 'Carwash LIM komplet') st
WHERE b.ServiceType = 'Vehicle'
  AND b.Id BETWEEN 5 AND 10;

INSERT INTO ChecklistResponses (BookingId, SopTemplateId, ChecklistItemId, IsCompleted, CompletedAt, Notes)
SELECT
    bsa.BookingId,
    bsa.SopTemplateId,
    ci.Id,
    CASE WHEN b.Status = 'Completed' THEN 1 ELSE 0 END,
    CASE WHEN b.Status = 'Completed' THEN DATEADD(HOUR, -1, GETUTCDATE()) ELSE NULL END,
    CASE WHEN b.Status = 'InProgress' THEN 'U tijeku' ELSE NULL END
FROM BookingSopAssignments bsa
INNER JOIN Bookings b ON bsa.BookingId = b.Id
INNER JOIN ChecklistItems ci ON ci.SopTemplateId = bsa.SopTemplateId
WHERE b.Id IN (SELECT BookingId FROM BookingSopAssignments);

UPDATE cr
SET Notes = 'Klijent tražio preskakanje - uredski tepisi već oprani jučer'
FROM ChecklistResponses cr
INNER JOIN BookingSopAssignments bsa ON bsa.BookingId = cr.BookingId AND bsa.SopTemplateId = cr.SopTemplateId
WHERE bsa.BookingId = 6
  AND cr.ChecklistItemId = (SELECT Id FROM ChecklistItems WHERE ItemText LIKE '%tepih%');
GO

-- ============================================================
-- DASHBOARD VIEWS (unchanged)
-- ============================================================

CREATE VIEW vw_MonthlyRevenue AS
SELECT
    YEAR(i.IssueDate)  AS Year,
    MONTH(i.IssueDate) AS Month,
    COUNT(*)           AS InvoiceCount,
    SUM(i.SubTotal)    AS TotalSubTotal,
    SUM(i.DiscountAmount) AS TotalDiscount,
    SUM(i.VatAmount)   AS TotalVat,
    SUM(i.TotalAmount) AS TotalRevenue
FROM Invoices i
WHERE i.Status NOT IN ('WrittenOff')
GROUP BY YEAR(i.IssueDate), MONTH(i.IssueDate);
GO

CREATE VIEW vw_TopClients AS
SELECT
    c.Id           AS ClientId,
    c.ClientName,
    COUNT(i.Id)    AS InvoiceCount,
    SUM(i.TotalAmount) AS TotalBilled,
    ISNULL(SUM(p.AmountPaid), 0) AS TotalPaid
FROM Clients c
INNER JOIN Invoices i ON i.ClientId = c.Id
LEFT JOIN (
    SELECT InvoiceId, SUM(Amount) AS AmountPaid
    FROM Payments
    GROUP BY InvoiceId
) p ON p.InvoiceId = i.Id
WHERE i.IssueDate >= DATEADD(YEAR, -1, CAST(GETUTCDATE() AS DATE))
GROUP BY c.Id, c.ClientName;
GO

CREATE VIEW vw_EmployeeUtilization AS
SELECT
    e.Id           AS EmployeeId,
    e.FirstName + ' ' + e.LastName AS EmployeeName,
    COUNT(DISTINCT CASE WHEN b.Id IS NOT NULL THEN ba.BookingId END) AS JobsAssigned,
    COUNT(DISTINCT CASE WHEN b.Status = 'Completed' THEN b.Id END) AS JobsCompleted,
    COUNT(DISTINCT b.ScheduledDate) AS DaysActive
FROM Employees e
LEFT JOIN BookingAssignments ba ON ba.EmployeeId = e.Id
LEFT JOIN Bookings b ON b.Id = ba.BookingId
    AND b.ScheduledDate >= DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE))
WHERE e.IsActive = 1
GROUP BY e.Id, e.FirstName, e.LastName;
GO

CREATE VIEW vw_JobCompletionRate AS
SELECT
    YEAR(b.ScheduledDate)  AS Year,
    MONTH(b.ScheduledDate) AS Month,
    COUNT(*)               AS TotalJobs,
    SUM(CASE WHEN b.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedJobs,
    ROUND(
        CAST(SUM(CASE WHEN b.Status = 'Completed' THEN 1 ELSE 0 END) AS FLOAT) /
        NULLIF(COUNT(*), 0) * 100, 1
    ) AS CompletionRatePct
FROM Bookings b
WHERE b.ScheduledDate >= DATEADD(MONTH, -12, CAST(GETUTCDATE() AS DATE))
GROUP BY YEAR(b.ScheduledDate), MONTH(b.ScheduledDate);
GO

CREATE VIEW vw_OverdueInvoiceSummary AS
SELECT
    SUM(i.TotalAmount)                         AS TotalOverdueAmount,
    COUNT(*)                                   AS OverdueInvoiceCount,
    AVG(i.TotalAmount)                         AS AvgOverdueAmount,
    MAX(DATEDIFF(DAY, i.DueDate, CAST(GETUTCDATE() AS DATE))) AS MaxDaysOverdue
FROM Invoices i
WHERE i.DueDate < CAST(GETUTCDATE() AS DATE)
  AND i.Status NOT IN ('Paid','WrittenOff');
GO

CREATE VIEW vw_BookingSopStatus AS
SELECT
    b.Id                                              AS BookingId,
    st.Id                                             AS SopTemplateId,
    st.Name                                           AS SopName,
    COUNT(ci.Id)                                      AS TotalItems,
    SUM(CASE WHEN cr.IsCompleted = 1 THEN 1 ELSE 0 END) AS CompletedItems,
    SUM(CASE WHEN cr.IsCompleted = 0 OR cr.IsCompleted IS NULL THEN 1 ELSE 0 END) AS IncompleteItems,
    MIN(CASE WHEN cr.IsCompleted = 1 THEN cr.CompletedAt END) AS FirstCompletedAt,
    MAX(cr.CompletedAt)                               AS LastCompletedAt
FROM Bookings b
INNER JOIN BookingSopAssignments bsa ON bsa.BookingId = b.Id
INNER JOIN SopTemplates st ON st.Id = bsa.SopTemplateId
INNER JOIN ChecklistItems ci ON ci.SopTemplateId = st.Id
LEFT JOIN ChecklistResponses cr ON cr.BookingId = b.Id
    AND cr.SopTemplateId = bsa.SopTemplateId
    AND cr.ChecklistItemId = ci.Id
GROUP BY b.Id, st.Id, st.Name;
GO

CREATE VIEW vw_Bookings AS
SELECT
    b.Id                                                        AS BookingId,
    b.ClientId,
    c.ClientName,
    c.Type                                                      AS ClientType,
    cont.ContactName                                            AS PrimaryContact,
    cont.Phone                                                  AS ContactPhone,
    b.ServiceType,
    b.ScheduledDate,
    b.ScheduledTimeSlot,
    b.Status,
    b.Notes,
    s.SiteName,
    s.Address                                                   AS SiteAddress,
    s.City                                                      AS SiteCity,
    s.SiteType,
    s.FloorAreaM2,
    (
        SELECT SUM(bs.EstimatedPrice * bs.Quantity)
        FROM BookingServices bs
        WHERE bs.BookingId = b.Id
    )                                                           AS EstimatedTotal,
    CASE
        WHEN (
            SELECT COUNT(*)
            FROM BookingServices bs
            WHERE bs.BookingId = b.Id AND bs.FinalPrice IS NULL
        ) = 0
        THEN (
            SELECT SUM(bs.FinalPrice * bs.Quantity)
            FROM BookingServices bs
            WHERE bs.BookingId = b.Id
        )
        ELSE NULL
    END                                                         AS FinalTotal,
    (
        SELECT STRING_AGG(LTRIM(RTRIM(ea.FirstName + ' ' + ea.LastName)), ', ')
        FROM BookingAssignments ba
        INNER JOIN Employees ea ON ea.Id = ba.EmployeeId
        WHERE ba.BookingId = b.Id
    )                                                           AS AssignedEmployee,
    (
        SELECT COUNT(*)
        FROM BookingServices bs
        WHERE bs.BookingId = b.Id
    )                                                           AS ServiceCount,
    (
        SELECT STRING_AGG(sc.CatalogCode + ' (' + CAST(bs.Quantity AS NVARCHAR) + ')', ', ')
        FROM BookingServices bs
        INNER JOIN ServiceCatalog sc ON bs.ServiceCatalogId = sc.Id
        WHERE bs.BookingId = b.Id
    )                                                           AS ServiceItems,
    inv.InvoiceNumber,
    inv.Status                                                  AS InvoiceStatus,
    b.CreatedAt,
    b.CompletedAt,
    v.LicensePlate,
    v.CarModel,
    bt.BoatType,
    bt.LengthMeters
FROM Bookings b
INNER JOIN Clients    c     ON b.ClientId = c.Id
LEFT  JOIN Sites      s     ON b.SiteId   = s.Id
LEFT  JOIN Contacts   cont  ON cont.ClientId = c.Id AND cont.IsPrimary = 1
LEFT  JOIN InvoiceBookings ib  ON ib.BookingId = b.Id
LEFT  JOIN Invoices    inv  ON inv.Id = ib.InvoiceId
LEFT  JOIN VehicleBookingDetails v   ON b.Id = v.BookingId
LEFT  JOIN BoatBookingDetails    bt  ON b.Id = bt.BookingId;
GO

CREATE VIEW vw_InvoiceSummary AS
SELECT
    i.Id                                        AS InvoiceId,
    i.InvoiceNumber,
    i.ClientId,
    c.ClientName,
    i.IssueDate,
    i.DueDate,
    i.SubTotal,
    i.DiscountAmount,
    i.VatAmount,
    i.TotalAmount,
    i.Status,
    ISNULL(paid.AmountPaid, 0)                  AS AmountPaid,
    i.TotalAmount - ISNULL(paid.AmountPaid, 0)  AS AmountOutstanding,
    CAST(CASE
        WHEN i.Status = 'Paid'        THEN 0
        WHEN i.DueDate < CAST(GETUTCDATE() AS DATE)
             AND i.Status NOT IN ('Paid','WrittenOff') THEN 1
        ELSE 0
    END AS BIT)                                 AS IsOverdue,
    CASE
        WHEN i.DueDate < CAST(GETUTCDATE() AS DATE)
        THEN DATEDIFF(DAY, i.DueDate, CAST(GETUTCDATE() AS DATE))
        ELSE 0
    END                                         AS DaysOverdue,
    e.FirstName + ' ' + e.LastName              AS CreatedBy
FROM Invoices i
INNER JOIN Clients   c ON i.ClientId = c.Id
LEFT  JOIN Employees e ON i.CreatedByEmployeeId = e.Id
LEFT  JOIN (
    SELECT InvoiceId, SUM(Amount) AS AmountPaid
    FROM Payments
    GROUP BY InvoiceId
) paid ON paid.InvoiceId = i.Id;
GO