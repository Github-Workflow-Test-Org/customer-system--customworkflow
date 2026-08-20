-- Customer Management System - Functions
-- SQL Server T-SQL functions for customer-related calculations and validations

USE [CustomerManagement]
GO

-- ============================================================================
-- Function: CalculateCustomerLifetimeValue
-- Purpose: Calculate total lifetime value of a customer
-- Safe implementation showing proper parameter handling
-- ============================================================================
CREATE OR ALTER FUNCTION dbo.CalculateCustomerLifetimeValue(@CustomerId INT)
RETURNS DECIMAL(10, 2)
AS
BEGIN
    DECLARE @LifetimeValue DECIMAL(10, 2);

    SELECT @LifetimeValue = ISNULL(SUM(o.[TotalAmount]), 0)
    FROM [dbo].[Orders] o
    WHERE o.[CustomerId] = @CustomerId
      AND o.[Status] IN ('DELIVERED', 'SHIPPED');

    RETURN @LifetimeValue;
END
GO

-- ============================================================================
-- Function: GetCustomerStatus
-- Purpose: Get human-readable status for a customer
-- Safe implementation using CASE statement
-- ============================================================================
CREATE OR ALTER FUNCTION dbo.GetCustomerStatus(@StatusCode CHAR(1))
RETURNS NVARCHAR(20)
AS
BEGIN
    DECLARE @StatusDescription NVARCHAR(20);

    SET @StatusDescription = CASE @StatusCode
        WHEN 'A' THEN 'Active'
        WHEN 'I' THEN 'Inactive'
        WHEN 'S' THEN 'Suspended'
        ELSE 'Unknown'
    END;

    RETURN @StatusDescription;
END
GO

-- ============================================================================
-- Function: ValidateCustomerAccess
-- Purpose: Validate if customer can access a resource
-- Mixed: Demonstrates both safe and vulnerable query patterns
-- ============================================================================
CREATE OR ALTER FUNCTION dbo.ValidateCustomerAccess(@CustomerId INT, @ResourceId NVARCHAR(100))
RETURNS BIT
AS
BEGIN
    DECLARE @HasAccess BIT = 0;
    DECLARE @CustomerStatus CHAR(1);

    -- Safe parameterized query
    SELECT @CustomerStatus = [Status]
    FROM [dbo].[Customers]
    WHERE [CustomerId] = @CustomerId;

    -- Check if customer is active
    IF @CustomerStatus = 'A'
    BEGIN
        SET @HasAccess = 1;
    END

    RETURN @HasAccess;
END
GO

-- ============================================================================
-- Function: GetCustomerDetails
-- Purpose: Retrieve formatted customer details as concatenated string
-- Safe implementation
-- ============================================================================
CREATE OR ALTER FUNCTION dbo.GetCustomerDetails(@CustomerId INT)
RETURNS NVARCHAR(500)
AS
BEGIN
    DECLARE @Details NVARCHAR(500);

    SELECT @Details = [FirstName] + ' ' + [LastName] + ' (' + [Email] + ')'
    FROM [dbo].[Customers]
    WHERE [CustomerId] = @CustomerId;

    RETURN ISNULL(@Details, 'Customer not found');
END
GO

-- ============================================================================
-- Function: CalculateOrderDiscount
-- Purpose: Calculate discount percentage based on order total
-- Safe implementation using numeric calculations
-- ============================================================================
CREATE OR ALTER FUNCTION dbo.CalculateOrderDiscount(@OrderTotal DECIMAL(10, 2))
RETURNS DECIMAL(5, 2)
AS
BEGIN
    DECLARE @Discount DECIMAL(5, 2) = 0;

    -- Tiered discount structure
    IF @OrderTotal >= 5000
        SET @Discount = 15;
    ELSE IF @OrderTotal >= 2000
        SET @Discount = 10;
    ELSE IF @OrderTotal >= 1000
        SET @Discount = 5;

    RETURN @Discount;
END
GO

-- ============================================================================
-- Function: GetSupportTicketPriority
-- Purpose: Convert priority number to text
-- Safe implementation using CASE
-- ============================================================================
CREATE OR ALTER FUNCTION dbo.GetSupportTicketPriority(@PriorityLevel INT)
RETURNS NVARCHAR(20)
AS
BEGIN
    DECLARE @PriorityText NVARCHAR(20);

    SET @PriorityText = CASE @PriorityLevel
        WHEN 1 THEN 'Critical'
        WHEN 2 THEN 'High'
        WHEN 3 THEN 'Medium'
        WHEN 4 THEN 'Low'
        ELSE 'Unknown'
    END;

    RETURN @PriorityText;
END
GO

-- ============================================================================
-- Function: CalculateDaysSinceRegistration
-- Purpose: Calculate days since customer registered
-- Safe implementation
-- ============================================================================
CREATE OR ALTER FUNCTION dbo.CalculateDaysSinceRegistration(@CustomerId INT)
RETURNS INT
AS
BEGIN
    DECLARE @DaysSince INT;

    SELECT @DaysSince = DATEDIFF(DAY, [RegistrationDate], GETDATE())
    FROM [dbo].[Customers]
    WHERE [CustomerId] = @CustomerId;

    RETURN ISNULL(@DaysSince, -1);
END
GO

-- ============================================================================
-- Function: GetOpenTicketCount
-- Purpose: Get count of open support tickets for customer
-- Safe implementation
-- ============================================================================
CREATE OR ALTER FUNCTION dbo.GetOpenTicketCount(@CustomerId INT)
RETURNS INT
AS
BEGIN
    DECLARE @TicketCount INT;

    SELECT @TicketCount = COUNT(*)
    FROM [dbo].[SupportTickets]
    WHERE [CustomerId] = @CustomerId
      AND [Status] IN ('OPEN', 'IN_PROGRESS');

    RETURN ISNULL(@TicketCount, 0);
END
GO
