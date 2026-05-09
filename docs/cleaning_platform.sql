IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'CleaningPlatformDB')
BEGIN
    ALTER DATABASE CleaningPlatformDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CleaningPlatformDB;
END
GO
CREATE DATABASE CleaningPlatformDB;
GO
USE CleaningPlatformDB;
GO
-- ============================================================
-- CLEANING PLATFORM – FULL SCHEMA (FIXED & ENHANCED)
-- Includes: Employees, Clients, Contacts, Sites, ServiceCatalog,
--           Bookings, BookingServices, child detail tables,
--           Roles, RolePermissions, WeeklySchedule, DateOverride,
--           Invoices, InvoiceLines, Payments, sequences, triggers.
-- ============================================================

-- ============================================================
-- 1. Employees
-- ============================================================
CREATE TABLE Employees (
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    Email               NVARCHAR(255)   NOT NULL UNIQUE,
    PasswordHash        NVARCHAR(255)   NOT NULL,
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
CREATE INDEX IX_Employees_Email ON Employees(Email);
GO

-- ============================================================
-- 2. Clients
-- ============================================================
CREATE TABLE Clients (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    ClientName      NVARCHAR(200)   NOT NULL,
    Type            NVARCHAR(50)    NOT NULL,   -- 'OneTime', 'RepeatIndividual', 'RepeatBusiness'
    Oib             NVARCHAR(50)    NULL,
    PaymentTerms    NVARCHAR(100)   NULL,       -- e.g. 'Net30', 'Net15', 'DueOnReceipt'
    Notes           NVARCHAR(MAX)   NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT CHK_Client_Type CHECK (Type IN ('OneTime', 'RepeatIndividual', 'RepeatBusiness'))
);
GO
CREATE INDEX IX_Clients_Name ON Clients(ClientName);
CREATE INDEX IX_Clients_Type ON Clients(Type);
GO

-- ============================================================
-- 3. Contacts
-- ============================================================
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

-- ============================================================
-- 4. Sites
-- ============================================================
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

-- ============================================================
-- 5. ServiceCatalog
-- ============================================================
CREATE TABLE ServiceCatalog (
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    CatalogCode         NVARCHAR(10)    NOT NULL UNIQUE,
    Name                NVARCHAR(200)   NOT NULL,
    Category            NVARCHAR(100)   NULL,
    Unit                NVARCHAR(50)    NULL,
    PriceMin            DECIMAL(10,2)   NULL,
    PriceMax            DECIMAL(10,2)   NULL,
    PriceAvg            DECIMAL(10,2)   NULL,
    DefaultMarginPct    DECIMAL(5,2)    NULL,
    IsActive            BIT             NOT NULL DEFAULT 1,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);
GO
CREATE INDEX IX_ServiceCatalog_Code ON ServiceCatalog(CatalogCode);
GO

-- Seed full catalog (26 services)
INSERT INTO ServiceCatalog (CatalogCode, Name, Category, Unit, PriceMin, PriceMax, PriceAvg, DefaultMarginPct) VALUES
('A-01', 'Čišćenje stubišta — Osnovno (1x/tjed.)',       'Stubišta',  'Mj/objekt',    36.00,    80.00,    55.00,  45.0),
('A-02', 'Čišćenje stubišta — Standard (+ tepih)',        'Stubišta',  'Mj/objekt',    80.00,   170.00,   110.00,  42.0),
('A-03', 'Čišćenje stubišta — Premium (veliki objekt)',   'Stubišta',  'Mj/objekt',   160.00,   580.00,   280.00,  40.0),
('A-04', 'Uredsko čišćenje — Osnovno',                   'Uredi',     'Mj/ured',     300.00,   700.00,   500.00,  40.0),
('A-05', 'Uredsko čišćenje — Veliki objekt',             'Uredi',     'Mj/ured',     700.00,  3275.00,  1500.00,  38.0),
('A-06', 'Pranje vozila — B2B ugovor (LIM/OBD)',         'Flota',     'Po vozilu',    13.50,    20.00,    16.00,  60.0),
('A-07', 'Pranje vozila — B2B ugovor (SUV)',             'Flota',     'Po vozilu',    20.00,    30.00,    24.00,  58.0),
('A-08', 'Pranje vozila — B2B ugovor (Dostavno/Kombi)',  'Flota',     'Po vozilu',    25.00,    35.00,    28.00,  55.0),
('B-01', 'Carwash — Kompletno LIM (manja)',              'Carwash',   'Po dolasku',   15.00,    20.00,    17.00,  62.0),
('B-02', 'Carwash — Kompletno SUV',                      'Carwash',   'Po dolasku',   20.00,    25.00,    23.00,  60.0),
('B-03', 'Carwash — Kompletno Terenski/Pick-up',         'Carwash',   'Po dolasku',   25.00,    30.00,    28.00,  58.0),
('B-04', 'Carwash — Teretno/Kombi',                      'Carwash',   'Po dolasku',   35.00,    50.00,    42.00,  50.0),
('B-05', 'Pranje motora',                                'Carwash',   'Po vozilu',    20.00,    20.00,    20.00,  55.0),
('B-06', 'Kemijsko čišćenje sjedala/tapeciranog',        'Carwash',   'Po kom',       55.00,    75.00,    63.00,  55.0),
('B-07', 'Pranje tepiha — auto',                         'Tepisi',    'Po m²',         4.00,     5.00,     4.50,  60.0),
('B-08', 'Pranje tepiha — podni',                        'Tepisi',    'Po m²',         4.00,     7.00,     5.00,  58.0),
('B-09', 'Post-construction čišćenje',                   'Specijal',  'Po m²',         4.00,     9.00,     6.00,  48.0),
('B-10', 'Čišćenje garaže',                              'Specijal',  'Po tretmanu',  50.00,   450.00,   200.00,  50.0),
('B-11', 'Dezinfekcija prostora (kemijska/ozon)',        'Specijal',  'Po m²',         4.00,    10.00,     6.00,  55.0),
('B-12', 'Industrijsko čišćenje hale',                   'Specijal',  'Po tretmanu', 300.00,   500.00,   400.00,  40.0),
('B-13', 'Pranje jedrilice/broda — osnovno',             'Brodovi',   'Po brodu',    200.00,   500.00,   320.00,  58.0),
('B-14', 'Pranje broda — detailing',                     'Brodovi',   'Po brodu',    500.00,  1500.00,   900.00,  62.0),
('C-01', 'Košenje trave — osnovno',                      'Upsell',    'Po satu/m²',   20.00,    30.00,    25.00,  45.0),
('C-02', 'Košenje + šiblje + odvoz',                     'Upsell',    'Po tretmanu', 100.00,   300.00,   180.00,  40.0),
('C-03', 'Najam i pranje tepiha (otirači)',               'Upsell',    'Mj/tepih',     10.00,    15.00,    12.00,  65.0),
('C-04', 'Uniforme — pranje hotelskih/medicinskih',      'Upsell',    'Mj/ugovor',   100.00,   300.00,   180.00,  50.0);
GO

-- ============================================================
-- 6. Roles & RolePermissions
-- ============================================================
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
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    RoleId          INT             NOT NULL,
    PermissionKey   NVARCHAR(100)   NOT NULL,
    CONSTRAINT FK_RolePermission_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_RolePermissions_RoleId ON RolePermissions(RoleId);
CREATE INDEX IX_RolePermissions_Key    ON RolePermissions(PermissionKey);
GO

INSERT INTO RolePermissions (RoleId, PermissionKey)
-- Admin
SELECT r.Id, 'clients.view'       FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'clients.manage'     FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'sites.view'         FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'sites.manage'       FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'bookings.view'      FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'bookings.manage'    FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'invoices.view'      FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'invoices.manage'    FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'reports.view'       FROM Roles r WHERE r.Name = 'Admin' UNION ALL
-- Dispatcher
SELECT r.Id, 'clients.view'       FROM Roles r WHERE r.Name = 'Dispatcher' UNION ALL
SELECT r.Id, 'sites.view'         FROM Roles r WHERE r.Name = 'Dispatcher' UNION ALL
SELECT r.Id, 'bookings.view'      FROM Roles r WHERE r.Name = 'Dispatcher' UNION ALL
SELECT r.Id, 'bookings.assign'    FROM Roles r WHERE r.Name = 'Dispatcher' UNION ALL
SELECT r.Id, 'bookings.manage'    FROM Roles r WHERE r.Name = 'Dispatcher' UNION ALL
-- Employee
SELECT r.Id, 'bookings.view'      FROM Roles r WHERE r.Name = 'Employee'   UNION ALL
-- Finance
SELECT r.Id, 'clients.view'       FROM Roles r WHERE r.Name = 'Finance'    UNION ALL
SELECT r.Id, 'invoices.view'      FROM Roles r WHERE r.Name = 'Finance'    UNION ALL
SELECT r.Id, 'invoices.manage'    FROM Roles r WHERE r.Name = 'Finance'    UNION ALL
SELECT r.Id, 'payments.manage'    FROM Roles r WHERE r.Name = 'Finance'    UNION ALL
SELECT r.Id, 'reports.view'       FROM Roles r WHERE r.Name = 'Finance'    UNION ALL
-- Owner gets all via application logic (all.access flag)
SELECT r.Id, 'all.access'         FROM Roles r WHERE r.Name = 'Owner';
GO

-- ============================================================
-- 7. Add RoleId to Employees
-- ============================================================
ALTER TABLE Employees ADD RoleId INT NOT NULL DEFAULT 1;
ALTER TABLE Employees ADD CONSTRAINT FK_Employee_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id);
GO

-- ============================================================
-- 8. WeeklySchedule
-- ============================================================
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
(1, 8, 17, 3),   -- Monday
(2, 8, 17, 3),   -- Tuesday
(3, 8, 17, 3),   -- Wednesday
(4, 8, 17, 3),   -- Thursday
(5, 8, 17, 3),   -- Friday
(6, 9, 14, 1),   -- Saturday
(0, 0,  0, 0);   -- Sunday closed
GO

-- ============================================================
-- 9. DateOverride
-- ============================================================
CREATE TABLE DateOverride (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    Date            DATE            NOT NULL,
    StartHour       INT             NULL,
    EndHour         INT             NULL,
    Capacity        INT             NULL,
    IsFullyClosed   BIT             NOT NULL DEFAULT 0,
    CONSTRAINT CHK_DateOverride_Start CHECK (StartHour IS NULL OR (StartHour >= 0 AND StartHour <= 23)),
    CONSTRAINT CHK_DateOverride_End   CHECK (EndHour   IS NULL OR (EndHour   >= 0 AND EndHour   <= 23))
);
GO
CREATE UNIQUE INDEX IX_DateOverride_Date ON DateOverride(Date);
GO

-- ============================================================
-- 10. Bookings (with fixed CompletedAt constraint)
-- ============================================================
CREATE TABLE Bookings (
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    ClientId            INT             NOT NULL,
    SiteId              INT             NULL,
    AssignedEmployeeId  INT             NULL,
    ServiceType         NVARCHAR(50)    NOT NULL,           -- 'Vehicle', 'SiteBased', 'Boat'
    ScheduledDate       DATE            NOT NULL,
    ScheduledTimeSlot   TIME            NULL,
    Status              NVARCHAR(50)    NOT NULL DEFAULT 'Pending',
    Notes               NVARCHAR(MAX)   NULL,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CompletedAt         DATETIME2       NULL,

    CONSTRAINT FK_Booking_Client    FOREIGN KEY (ClientId)           REFERENCES Clients(Id),
    CONSTRAINT FK_Booking_Site      FOREIGN KEY (SiteId)             REFERENCES Sites(Id),
    CONSTRAINT FK_Booking_Employee  FOREIGN KEY (AssignedEmployeeId) REFERENCES Employees(Id),
    CONSTRAINT CHK_Booking_ServiceType CHECK (ServiceType IN ('Vehicle', 'SiteBased', 'Boat')),
    CONSTRAINT CHK_Booking_Status      CHECK (Status IN ('Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled')),
    -- Fixed: only requires CompletedAt when status is 'Completed' (does not force NULL otherwise)
    CONSTRAINT CHK_Booking_CompletedAt CHECK (Status != 'Completed' OR CompletedAt IS NOT NULL)
);
GO
CREATE INDEX IX_Bookings_ClientId      ON Bookings(ClientId);
CREATE INDEX IX_Bookings_SiteId        ON Bookings(SiteId);
CREATE INDEX IX_Bookings_Employee      ON Bookings(AssignedEmployeeId);
CREATE INDEX IX_Bookings_ServiceType   ON Bookings(ServiceType);
CREATE INDEX IX_Bookings_ScheduledDate ON Bookings(ScheduledDate);
CREATE INDEX IX_Bookings_Status        ON Bookings(Status);
GO

-- ============================================================
-- 11. BookingServices
-- ============================================================
CREATE TABLE BookingServices (
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    BookingId           INT             NOT NULL,
    ServiceCatalogId    INT             NOT NULL,
    EstimatedPrice      DECIMAL(10,2)   NULL,
    FinalPrice          DECIMAL(10,2)   NULL,
    Quantity            DECIMAL(10,2)   NOT NULL DEFAULT 1,
    Notes               NVARCHAR(MAX)   NULL,

    CONSTRAINT FK_BookingService_Booking FOREIGN KEY (BookingId)        REFERENCES Bookings(Id)       ON DELETE CASCADE,
    CONSTRAINT FK_BookingService_Catalog FOREIGN KEY (ServiceCatalogId) REFERENCES ServiceCatalog(Id)
);
GO
CREATE INDEX IX_BookingServices_Booking ON BookingServices(BookingId);
GO

-- ============================================================
-- 12. VehicleBookingDetails
-- ============================================================
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

-- ============================================================
-- 13. BoatBookingDetails
-- ============================================================
CREATE TABLE BoatBookingDetails (
    BookingId       INT             PRIMARY KEY,
    BoatType        NVARCHAR(100)   NOT NULL,
    LengthMeters    DECIMAL(5,2)    NULL,
    Notes           NVARCHAR(MAX)   NULL,
    CONSTRAINT FK_BoatDetail_Booking FOREIGN KEY (BookingId) REFERENCES Bookings(Id) ON DELETE CASCADE
);
GO

-- ============================================================
-- 14. Invoice Number Sequence
-- ============================================================
CREATE SEQUENCE InvoiceNumberSeq
    START WITH 1
    INCREMENT BY 1
    NO CYCLE;
GO

-- ============================================================
-- 15. Invoices (renumbered, with trigger for auto-number)
-- ============================================================
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

CREATE TRIGGER trg_Invoices_AutoNumber
ON Invoices
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Invoices (
        InvoiceNumber, ClientId, IssueDate, DueDate, SubTotal,
        DiscountAmount, VatPct, VatAmount, TotalAmount, Status,
        Notes, CreatedByEmployeeId, CreatedAt, UpdatedAt
    )
    SELECT
        CONCAT(
            'INV-',
            YEAR(GETUTCDATE()),
            '-',
            FORMAT(NEXT VALUE FOR InvoiceNumberSeq, '0000')
        ),
        ClientId, IssueDate, DueDate, SubTotal,
        DiscountAmount, VatPct, VatAmount, TotalAmount, Status,
        Notes, CreatedByEmployeeId, CreatedAt, UpdatedAt
    FROM inserted;
END;
GO

CREATE INDEX IX_Invoices_ClientId  ON Invoices(ClientId);
CREATE INDEX IX_Invoices_Status    ON Invoices(Status);
CREATE INDEX IX_Invoices_IssueDate ON Invoices(IssueDate);
CREATE INDEX IX_Invoices_DueDate   ON Invoices(DueDate);
GO

-- ============================================================
-- 16. InvoiceLines (new table)
-- ============================================================
CREATE TABLE InvoiceLines (
    Id          INT             PRIMARY KEY IDENTITY(1,1),
    InvoiceId   INT             NOT NULL,
    Description NVARCHAR(500)   NOT NULL,
    Quantity    DECIMAL(10,2)   NOT NULL DEFAULT 1,
    UnitPrice   DECIMAL(10,2)   NOT NULL,
    DiscountPct DECIMAL(5,2)    NULL DEFAULT 0,
    VatPct      DECIMAL(5,2)    NOT NULL DEFAULT 0,
    SourceType  NVARCHAR(50)    NULL,      -- 'Booking', 'Manual', 'CreditNote'
    SourceId    INT             NULL,      -- BookingId if SourceType = 'Booking'
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

-- ============================================================
-- 17. InvoiceBookings (kept for backward compatibility / quick lookup)
-- ============================================================
CREATE TABLE InvoiceBookings (
    Id          INT     PRIMARY KEY IDENTITY(1,1),
    InvoiceId   INT     NOT NULL,
    BookingId   INT     NOT NULL,

    CONSTRAINT FK_InvoiceBooking_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
    CONSTRAINT FK_InvoiceBooking_Booking FOREIGN KEY (BookingId) REFERENCES Bookings(Id),
    CONSTRAINT UQ_InvoiceBooking UNIQUE (BookingId)
);
GO
CREATE INDEX IX_InvoiceBookings_InvoiceId ON InvoiceBookings(InvoiceId);
CREATE INDEX IX_InvoiceBookings_BookingId ON InvoiceBookings(BookingId);
GO

-- ============================================================
-- 18. Payments
-- ============================================================
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
-- 19. View – vw_Bookings (updated to include InvoiceLines? Not needed, remains same)
-- ============================================================
CREATE VIEW vw_Bookings AS
SELECT
    b.Id                                                    AS BookingId,
    c.ClientName,
    c.Type                                                  AS ClientType,
    cont.ContactName                                        AS PrimaryContact,
    cont.Phone                                              AS ContactPhone,
    b.ServiceType,
    b.ScheduledDate,
    b.ScheduledTimeSlot,
    b.Status,
    b.Notes,
    s.SiteName,
    s.Address                                               AS SiteAddress,
    s.City                                                  AS SiteCity,
    s.SiteType,
    s.FloorAreaM2,
    (
        SELECT SUM(bs.EstimatedPrice * bs.Quantity)
        FROM BookingServices bs
        WHERE bs.BookingId = b.Id
    )                                                       AS EstimatedTotal,
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
    END                                                     AS FinalTotal,
    e.FirstName + ' ' + e.LastName                         AS AssignedEmployee,
    (
        SELECT COUNT(*)
        FROM BookingServices bs
        WHERE bs.BookingId = b.Id
    )                                                       AS ServiceCount,
    (
        SELECT STRING_AGG(sc.CatalogCode + ' (' + CAST(bs.Quantity AS NVARCHAR) + ')', ', ')
        FROM BookingServices bs
        INNER JOIN ServiceCatalog sc ON bs.ServiceCatalogId = sc.Id
        WHERE bs.BookingId = b.Id
    )                                                       AS ServiceItems,
    inv.InvoiceNumber,
    inv.Status                                              AS InvoiceStatus,
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
LEFT  JOIN Employees  e     ON b.AssignedEmployeeId = e.Id
LEFT  JOIN InvoiceBookings ib  ON ib.BookingId = b.Id
LEFT  JOIN Invoices    inv  ON inv.Id = ib.InvoiceId
LEFT  JOIN VehicleBookingDetails v   ON b.Id = v.BookingId
LEFT  JOIN BoatBookingDetails    bt  ON b.Id = bt.BookingId;
GO

-- ============================================================
-- 20. View – vw_InvoiceSummary (fixed DaysOverdue)
-- ============================================================
CREATE VIEW vw_InvoiceSummary AS
SELECT
    i.Id                                        AS InvoiceId,
    i.InvoiceNumber,
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
    CASE
        WHEN i.Status = 'Paid'        THEN 0
        WHEN i.DueDate < CAST(GETUTCDATE() AS DATE)
             AND i.Status NOT IN ('Paid','WrittenOff') THEN 1
        ELSE 0
    END                                         AS IsOverdue,
    -- Fixed: returns 0 instead of negative days
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

-- ============================================================
-- 21. Seed: Owner employee
-- ============================================================
INSERT INTO Employees (Email, PasswordHash, FirstName, LastName, Phone, EmployeeCode, HourlyRate, MaxJobsPerDay, IsActive, CreatedAt, UpdatedAt, RoleId)
VALUES (
    'owner@cleaningplatform.com',
    '$2y$10$qZKh.FlEZrHNSyAcazlNdOyBMHA.SJSfnLDoPtuFKt9Mrj99tdNEe',
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