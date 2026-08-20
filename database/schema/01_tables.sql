-- Customer Management System - Table Definitions
-- SQL Server T-SQL Schema for customer, order, and support operations

USE [CustomerManagement]
GO

-- Customers table
CREATE TABLE [dbo].[Customers] (
    [CustomerId] INT PRIMARY KEY IDENTITY(1000, 1),
    [FirstName] NVARCHAR(50) NOT NULL,
    [LastName] NVARCHAR(50) NOT NULL,
    [Email] NVARCHAR(100) UNIQUE,
    [Phone] NVARCHAR(20),
    [RegistrationDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [Country] NVARCHAR(100),
    [City] NVARCHAR(100),
    [PostalCode] NVARCHAR(20),
    [Status] CHAR(1) NOT NULL DEFAULT 'A', -- A=Active, I=Inactive, S=Suspended
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- Orders table
CREATE TABLE [dbo].[Orders] (
    [OrderId] INT PRIMARY KEY IDENTITY(1000, 1),
    [CustomerId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Customers]([CustomerId]),
    [OrderDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [TotalAmount] DECIMAL(10, 2) NOT NULL,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'PENDING', -- PENDING, PROCESSING, SHIPPED, DELIVERED, CANCELLED
    [ShippingAddress] NVARCHAR(500),
    [BillingAddress] NVARCHAR(500),
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- Order Items table
CREATE TABLE [dbo].[OrderItems] (
    [OrderItemId] INT PRIMARY KEY IDENTITY(1, 1),
    [OrderId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Orders]([OrderId]),
    [ProductName] NVARCHAR(200) NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPrice] DECIMAL(10, 2) NOT NULL,
    [LineTotal] DECIMAL(10, 2) NOT NULL
)
GO

-- Support Tickets table
CREATE TABLE [dbo].[SupportTickets] (
    [TicketId] INT PRIMARY KEY IDENTITY(1, 1),
    [CustomerId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Customers]([CustomerId]),
    [Subject] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX),
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'OPEN', -- OPEN, IN_PROGRESS, RESOLVED, CLOSED
    [Priority] INT DEFAULT 3, -- 1=Critical, 2=High, 3=Medium, 4=Low
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [ResolvedDate] DATETIME,
    [AssignedTo] NVARCHAR(100)
)
GO

-- Communication Log table
CREATE TABLE [dbo].[CommunicationLog] (
    [LogId] INT PRIMARY KEY IDENTITY(1, 1),
    [CustomerId] INT FOREIGN KEY REFERENCES [dbo].[Customers]([CustomerId]),
    [MessageType] NVARCHAR(50) NOT NULL, -- EMAIL, SMS, PUSH, WEBHOOK
    [Recipient] NVARCHAR(500),
    [Subject] NVARCHAR(200),
    [Body] NVARCHAR(MAX),
    [Status] NVARCHAR(20) DEFAULT 'SENT', -- SENT, FAILED, BOUNCED
    [SentDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [ErrorMessage] NVARCHAR(MAX)
)
GO

-- System Credentials table (for demonstrating CWE-798)
CREATE TABLE [dbo].[SystemCredentials] (
    [CredentialId] INT PRIMARY KEY IDENTITY(1, 1),
    [ServiceName] NVARCHAR(100) NOT NULL,
    [Username] NVARCHAR(100),
    [EncryptedPassword] VARBINARY(MAX),
    [ApiKey] NVARCHAR(500),
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [LastModifiedDate] DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- Audit Log table
CREATE TABLE [dbo].[AuditLog] (
    [AuditId] INT PRIMARY KEY IDENTITY(1, 1),
    [TableName] NVARCHAR(50) NOT NULL,
    [Operation] NVARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    [RecordId] INT,
    [Timestamp] DATETIME NOT NULL DEFAULT GETDATE(),
    [UserId] NVARCHAR(100),
    [OldValues] NVARCHAR(MAX),
    [NewValues] NVARCHAR(MAX),
    [IpAddress] NVARCHAR(50),
    [Details] NVARCHAR(MAX)
)
GO

-- User Sessions table
CREATE TABLE [dbo].[UserSessions] (
    [SessionId] INT PRIMARY KEY IDENTITY(1, 1),
    [CustomerId] INT FOREIGN KEY REFERENCES [dbo].[Customers]([CustomerId]),
    [SessionToken] NVARCHAR(500) NOT NULL UNIQUE,
    [LoginTime] DATETIME NOT NULL DEFAULT GETDATE(),
    [LogoutTime] DATETIME,
    [IpAddress] NVARCHAR(50),
    [UserAgent] NVARCHAR(MAX),
    [IsActive] CHAR(1) NOT NULL DEFAULT 'Y'
)
GO
