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
-- CLEANING PLATFORM – FULL SCHEMA (FIXED & ENHANCED)
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
    CONSTRAINT CHK_Client_Type CHECK (Type IN ('OneTime', 'RepeatIndividual', 'RepeatBusiness'))
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
SELECT r.Id, 'clients.view'       FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'clients.manage'     FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'sites.view'         FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'sites.manage'       FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'bookings.view'      FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'bookings.manage'    FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'invoices.view'      FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'invoices.manage'    FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'reports.view'       FROM Roles r WHERE r.Name = 'Admin' UNION ALL
SELECT r.Id, 'clients.view'       FROM Roles r WHERE r.Name = 'Dispatcher' UNION ALL
SELECT r.Id, 'sites.view'         FROM Roles r WHERE r.Name = 'Dispatcher' UNION ALL
SELECT r.Id, 'bookings.view'      FROM Roles r WHERE r.Name = 'Dispatcher' UNION ALL
SELECT r.Id, 'bookings.assign'    FROM Roles r WHERE r.Name = 'Dispatcher' UNION ALL
SELECT r.Id, 'bookings.manage'    FROM Roles r WHERE r.Name = 'Dispatcher' UNION ALL
SELECT r.Id, 'bookings.view'      FROM Roles r WHERE r.Name = 'Employee'   UNION ALL
SELECT r.Id, 'clients.view'       FROM Roles r WHERE r.Name = 'Finance'    UNION ALL
SELECT r.Id, 'invoices.view'      FROM Roles r WHERE r.Name = 'Finance'    UNION ALL
SELECT r.Id, 'invoices.manage'    FROM Roles r WHERE r.Name = 'Finance'    UNION ALL
SELECT r.Id, 'payments.manage'    FROM Roles r WHERE r.Name = 'Finance'    UNION ALL
SELECT r.Id, 'reports.view'       FROM Roles r WHERE r.Name = 'Finance'    UNION ALL
SELECT r.Id, 'all.access'         FROM Roles r WHERE r.Name = 'Owner';
GO

INSERT INTO RolePermissions (RoleId, PermissionKey)
SELECT Id, 'actions.booking.assign' FROM Roles WHERE Name = 'Owner';
GO

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
    CONSTRAINT CHK_DateOverride_End   CHECK (EndHour   IS NULL OR (EndHour   >= 0 AND EndHour   <= 23))
);
GO
CREATE UNIQUE INDEX IX_DateOverride_Date ON DateOverride(Date);
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
    CONSTRAINT FK_Booking_Client    FOREIGN KEY (ClientId)           REFERENCES Clients(Id),
    CONSTRAINT FK_Booking_Site      FOREIGN KEY (SiteId)             REFERENCES Sites(Id),
    CONSTRAINT CHK_Booking_ServiceType CHECK (ServiceType IN ('Vehicle', 'SiteBased', 'Boat')),
    CONSTRAINT CHK_Booking_Status      CHECK (Status IN ('Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled')),
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
    Id          INT         PRIMARY KEY IDENTITY(1,1),
    BookingId   INT         NOT NULL,
    EmployeeId  INT         NOT NULL,
    AssignedAt  DATETIME2   NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_BookingAssignment_Booking  FOREIGN KEY (BookingId)  REFERENCES Bookings(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BookingAssignment_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT UQ_BookingAssignment UNIQUE (BookingId, EmployeeId)
);
GO
CREATE INDEX IX_BookingAssignments_BookingId  ON BookingAssignments(BookingId);
CREATE INDEX IX_BookingAssignments_EmployeeId ON BookingAssignments(EmployeeId);
GO

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

ALTER TABLE Invoices
ADD CONSTRAINT DF_Invoices_InvoiceNumber
DEFAULT (CONCAT('INV-', YEAR(GETUTCDATE()), '-', FORMAT(NEXT VALUE FOR InvoiceNumberSeq, '0000')))
FOR InvoiceNumber;
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

CREATE VIEW vw_Bookings AS
SELECT
    b.Id                                                    AS BookingId,
    b.ClientId,                                             AS ClientId,
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
    (
        SELECT STRING_AGG(LTRIM(RTRIM(ea.FirstName + ' ' + ea.LastName)), ', ')
        FROM BookingAssignments ba
        INNER JOIN Employees ea ON ea.Id = ba.EmployeeId
        WHERE ba.BookingId = b.Id
    )                                                       AS AssignedEmployee,
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
LEFT  JOIN InvoiceBookings ib  ON ib.BookingId = b.Id
LEFT  JOIN Invoices    inv  ON inv.Id = ib.InvoiceId
LEFT  JOIN VehicleBookingDetails v   ON b.Id = v.BookingId
LEFT  JOIN BoatBookingDetails    bt  ON b.Id = bt.BookingId;
GO

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
-- SEED DATA
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
    CHOOSE((n % 3) + 1, 'OneTime', 'RepeatIndividual', 'RepeatBusiness'),
    RIGHT('00000000000' + CAST(10000000000 + n AS VARCHAR(11)), 11),
    CHOOSE((n % 4) + 1, 'Net30', 'Net15', 'DueOnReceipt', 'Net45'),
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
    ClientId, SiteId, ServiceType, ScheduledDate, ScheduledTimeSlot, Status, Notes, CreatedAt, UpdatedAt, CompletedAt
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
    CASE WHEN s.Status = 'Completed' THEN DATEADD(DAY, -1, GETUTCDATE()) ELSE NULL END
FROM N
CROSS APPLY (SELECT CHOOSE((n % 5) + 1, 'Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled') AS Status) s;
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

-- Invoice creation + mapping
DECLARE @InvoiceMap TABLE (BookingId INT, InvoiceId INT);

WITH BookingTotals AS (
    SELECT
        b.Id AS BookingId,
        b.ClientId,
        aa.EmployeeId AS CreatedByEmployeeId,
        b.ScheduledDate,
        b.Status,
        SUM(bs.EstimatedPrice * bs.Quantity) AS SubTotal
    FROM Bookings b
    OUTER APPLY (
        SELECT TOP 1 ba.EmployeeId
        FROM BookingAssignments ba
        WHERE ba.BookingId = b.Id
        ORDER BY ba.Id
    ) aa
    JOIN BookingServices bs ON bs.BookingId = b.Id
    WHERE b.Id % 2 = 0
    GROUP BY b.Id, b.ClientId, aa.EmployeeId, b.ScheduledDate, b.Status
)
MERGE Invoices AS target
USING BookingTotals AS src
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (
        ClientId, IssueDate, DueDate, SubTotal, DiscountAmount,
        VatPct, VatAmount, TotalAmount, Status, Notes, CreatedByEmployeeId
    )
    VALUES (
        src.ClientId,
        src.ScheduledDate,
        DATEADD(DAY, 15, src.ScheduledDate),
        src.SubTotal,
        CASE WHEN src.BookingId % 5 = 0 THEN 10 ELSE 0 END,
        25,
        ROUND((src.SubTotal - CASE WHEN src.BookingId % 5 = 0 THEN 10 ELSE 0 END) * 0.25, 2),
        ROUND((src.SubTotal - CASE WHEN src.BookingId % 5 = 0 THEN 10 ELSE 0 END) * 1.25, 2),
        CHOOSE((src.BookingId % 5) + 1, 'Draft', 'Sent', 'PartiallyPaid', 'Paid', 'Overdue'),
        CONCAT('Invoice for booking ', src.BookingId),
        src.CreatedByEmployeeId
    )
OUTPUT src.BookingId, inserted.Id INTO @InvoiceMap (BookingId, InvoiceId);

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
