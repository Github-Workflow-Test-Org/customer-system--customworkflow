-- Customer Management System - Stored Procedures
-- SQL Server T-SQL procedures for customer operations
-- Demonstrates security vulnerabilities for testing

USE [CustomerManagement]
GO

-- ============================================================================
-- Procedure: SearchCustomers
-- Purpose: Search for customers by dynamic criteria
-- Vulnerability: CWE-89 SQL Injection in WHERE clause concatenation
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SearchCustomers
    @SearchCriteria NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @SQL NVARCHAR(MAX);

    -- CWEID 89: SQL Injection vulnerability
    -- User input is concatenated directly into the WHERE clause
    -- Example: @SearchCriteria = "Email LIKE '%@example.com' OR 1=1--"
    SET @SQL = 'SELECT * FROM [dbo].[Customers] WHERE ' + @SearchCriteria;

    EXEC sp_executesql @SQL;
END
GO

-- ============================================================================
-- Procedure: CreateCustomer
-- Purpose: Create a new customer (Safe implementation)
-- Demonstrates proper parameterized approach
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.CreateCustomer
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Email NVARCHAR(100),
    @Phone NVARCHAR(20),
    @Country NVARCHAR(100),
    @City NVARCHAR(100),
    @PostalCode NVARCHAR(20),
    @NewCustomerId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO [dbo].[Customers]
            ([FirstName], [LastName], [Email], [Phone], [Country], [City], [PostalCode], [Status])
        VALUES
            (@FirstName, @LastName, @Email, @Phone, @Country, @City, @PostalCode, 'A');

        SET @NewCustomerId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- Procedure: UpdateCustomerSalutation
-- Purpose: Update customer contact information
-- Safe implementation with proper parameterization
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.UpdateCustomerInfo
    @CustomerId INT,
    @FirstName NVARCHAR(50) = NULL,
    @LastName NVARCHAR(50) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Phone NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[Customers]
    SET
        [FirstName] = ISNULL(@FirstName, [FirstName]),
        [LastName] = ISNULL(@LastName, [LastName]),
        [Email] = ISNULL(@Email, [Email]),
        [Phone] = ISNULL(@Phone, [Phone]),
        [UpdatedDate] = GETDATE()
    WHERE [CustomerId] = @CustomerId;
END
GO

-- ============================================================================
-- Procedure: ProcessOrder
-- Purpose: Process a customer order with dynamic item insertion
-- Vulnerability: CWE-89 SQL Injection through concatenated item insertions
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.ProcessOrder
    @CustomerId INT,
    @OrderTotal DECIMAL(10, 2),
    @ItemsJson NVARCHAR(MAX)  -- JSON array of items
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @OrderId INT;
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @ItemCount INT = 0;

    BEGIN TRY
        -- Insert main order record (safe)
        INSERT INTO [dbo].[Orders]
            ([CustomerId], [TotalAmount], [Status])
        VALUES
            (@CustomerId, @OrderTotal, 'PENDING');

        SET @OrderId = SCOPE_IDENTITY();

        -- CWEID 89: SQL Injection vulnerability
        -- Item data from JSON is concatenated into dynamic SQL
        -- Could contain malicious UNION queries or comments
        SET @SQL = 'INSERT INTO [dbo].[OrderItems] ([OrderId], [ProductName], [Quantity], [UnitPrice], [LineTotal]) VALUES '
                 + @ItemsJson;

        EXEC sp_executesql @SQL;

        -- Update order status (safe)
        UPDATE [dbo].[Orders]
        SET [Status] = 'PROCESSING'
        WHERE [OrderId] = @OrderId;

    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- Procedure: SendCustomerNotification
-- Purpose: Send email notification to customer
-- Vulnerability: CWE-88 Argument Injection in email parameters
-- Vulnerability: CWE-441 Unintended Web Service Proxy (HTTP call)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.SendCustomerNotification
    @CustomerId INT,
    @Subject NVARCHAR(200),
    @Body NVARCHAR(MAX),
    @NotificationType NVARCHAR(50) = 'EMAIL'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Email NVARCHAR(100);
    DECLARE @Phone NVARCHAR(20);
    DECLARE @HttpUrl NVARCHAR(500);
    DECLARE @Recipient NVARCHAR(500);

    -- Get customer contact info (safe)
    SELECT @Email = [Email], @Phone = [Phone]
    FROM [dbo].[Customers]
    WHERE [CustomerId] = @CustomerId;

    -- CWEID 88: Argument Injection vulnerability
    -- @Subject and @Body are concatenated without escaping
    -- Could contain special characters that break email parsing
    DECLARE @EmailCommand NVARCHAR(MAX);
    SET @EmailCommand = 'send_email -to "' + @Email + '" -subject "' + @Subject + '" -body "' + @Body + '"';

    -- Execute email (vulnerable to argument injection)
    EXEC xp_cmdshell @EmailCommand;

    -- CWEID 441: Unintended Web Service Proxy vulnerability
    -- External URL is contacted to send notification
    -- No validation of the destination service
    SET @HttpUrl = 'http://notification-service.example.com/notify?customerId=' + CAST(@CustomerId AS NVARCHAR(10))
                 + '&subject=' + @Subject;

    -- Log the notification (safe)
    INSERT INTO [dbo].[CommunicationLog]
        ([CustomerId], [MessageType], [Recipient], [Subject], [Body], [Status])
    VALUES
        (@CustomerId, @NotificationType, @Email, @Subject, @Body, 'SENT');
END
GO

-- ============================================================================
-- Procedure: ExportCustomerData
-- Purpose: Export customer data to file
-- Vulnerability: CWE-73 Path Manipulation in file operations
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.ExportCustomerData
    @FilePath NVARCHAR(500),
    @FileFormat NVARCHAR(20) = 'CSV'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @FullPath NVARCHAR(500);
    DECLARE @Query NVARCHAR(MAX);

    -- CWEID 73: Path Manipulation vulnerability
    -- @FilePath is used directly without validation
    -- Could traverse to system directories: ../../windows/system32/config/sam
    SET @FullPath = 'C:\CustomerExports\' + @FilePath;

    IF @FileFormat = 'CSV'
    BEGIN
        -- Construct BCP command with untrusted path
        DECLARE @BcpCommand NVARCHAR(MAX);
        SET @BcpCommand = 'bcp "SELECT * FROM [CustomerManagement].[dbo].[Customers]" queryout "' + @FullPath + '" -c -T';

        EXEC xp_cmdshell @BcpCommand;
    END
    ELSE IF @FileFormat = 'JSON'
    BEGIN
        -- Export as JSON (could also be manipulated)
        SET @Query = 'SELECT * FROM [dbo].[Customers] FOR JSON PATH';
        -- File write with untrusted path
        EXEC xp_cmdshell 'powershell.exe -Command "' + @Query + ' > ' + @FullPath + '"';
    END

    -- Log the export (safe)
    INSERT INTO [dbo].[AuditLog]
        ([TableName], [Operation], [Timestamp], [UserId], [Details])
    VALUES
        ('Customers', 'EXPORT', GETDATE(), SYSTEM_USER, 'Data exported to: ' + @FullPath);
END
GO

-- ============================================================================
-- Procedure: GetCustomerOrders
-- Purpose: Retrieve all orders for a customer (Safe implementation)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.GetCustomerOrders
    @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        o.[OrderId],
        o.[OrderDate],
        o.[TotalAmount],
        o.[Status],
        COUNT(oi.[OrderItemId]) AS [ItemCount]
    FROM [dbo].[Orders] o
    LEFT JOIN [dbo].[OrderItems] oi ON o.[OrderId] = oi.[OrderId]
    WHERE o.[CustomerId] = @CustomerId
    GROUP BY o.[OrderId], o.[OrderDate], o.[TotalAmount], o.[Status]
    ORDER BY o.[OrderDate] DESC;
END
GO

-- ============================================================================
-- Procedure: CreateSupportTicket
-- Purpose: Create a customer support ticket (Safe implementation)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.CreateSupportTicket
    @CustomerId INT,
    @Subject NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @Priority INT = 3,
    @TicketId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Priority NOT IN (1, 2, 3, 4)
        SET @Priority = 3;  -- Default to Medium

    INSERT INTO [dbo].[SupportTickets]
        ([CustomerId], [Subject], [Description], [Priority], [Status])
    VALUES
        (@CustomerId, @Subject, @Description, @Priority, 'OPEN');

    SET @TicketId = SCOPE_IDENTITY();
END
GO

-- ============================================================================
-- Procedure: LogAuditAction
-- Purpose: Log an audit action (Safe implementation)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.LogAuditAction
    @TableName NVARCHAR(50),
    @Operation NVARCHAR(10),
    @RecordId INT,
    @UserId NVARCHAR(100),
    @Details NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Operation NOT IN ('INSERT', 'UPDATE', 'DELETE')
    BEGIN
        RAISERROR('Invalid operation type', 16, 1);
        RETURN;
    END

    INSERT INTO [dbo].[AuditLog]
        ([TableName], [Operation], [RecordId], [Timestamp], [UserId], [Details])
    VALUES
        (@TableName, @Operation, @RecordId, GETDATE(), @UserId, @Details);
END
GO

-- ============================================================================
-- Procedure: ValidateCustomerSession
-- Purpose: Validate and create customer session
-- Vulnerability: CWE-798 Hardcoded credentials for database service account
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.ValidateCustomerSession
    @CustomerId INT,
    @IpAddress NVARCHAR(50),
    @UserAgent NVARCHAR(MAX),
    @SessionToken NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ServicePassword NVARCHAR(100);

    -- CWEID 798: Hardcoded credential
    -- Service account password is hardcoded in stored procedure
    -- This should never be in source code
    SET @ServicePassword = 'ServiceAcct@2024!#';

    -- Generate session token (simplified for example)
    SET @SessionToken = CONVERT(NVARCHAR(500), HASHBYTES('SHA2_256', CONVERT(NVARCHAR(MAX), @CustomerId) + @IpAddress));

    -- Create session record (safe)
    INSERT INTO [dbo].[UserSessions]
        ([CustomerId], [SessionToken], [IpAddress], [UserAgent], [IsActive])
    VALUES
        (@CustomerId, @SessionToken, @IpAddress, @UserAgent, 'Y');

    -- Log session creation
    EXEC dbo.LogAuditAction 'UserSessions', 'INSERT', @CustomerId, SYSTEM_USER, 'Session created from IP: ' + @IpAddress;
END
GO

-- ============================================================================
-- Procedure: EndCustomerSession
-- Purpose: End a customer session (Safe implementation)
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.EndCustomerSession
    @SessionToken NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[UserSessions]
    SET [LogoutTime] = GETDATE(), [IsActive] = 'N'
    WHERE [SessionToken] = @SessionToken;

    -- Log session termination
    EXEC dbo.LogAuditAction 'UserSessions', 'UPDATE', NULL, SYSTEM_USER, 'Session terminated';
END
GO
