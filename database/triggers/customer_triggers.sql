-- Customer Management System - Triggers
-- SQL Server T-SQL triggers for audit, validation, and data integrity

USE [CustomerManagement]
GO

-- ============================================================================
-- Trigger: trg_Customer_Audit
-- Purpose: Audit all INSERT, UPDATE, DELETE operations on Customers table
-- Action: Logs changes to AuditLog table
-- ============================================================================
CREATE OR ALTER TRIGGER dbo.trg_Customer_Audit
ON [dbo].[Customers]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Log INSERT operations
    IF EXISTS (SELECT * FROM inserted) AND NOT EXISTS (SELECT * FROM deleted)
    BEGIN
        INSERT INTO [dbo].[AuditLog]
            ([TableName], [Operation], [RecordId], [Timestamp], [UserId], [Details])
        SELECT
            'Customers' AS [TableName],
            'INSERT' AS [Operation],
            i.[CustomerId] AS [RecordId],
            GETDATE() AS [Timestamp],
            SYSTEM_USER AS [UserId],
            'New customer: ' + i.[FirstName] + ' ' + i.[LastName] AS [Details]
        FROM inserted i;
    END

    -- Log UPDATE operations
    ELSE IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
    BEGIN
        INSERT INTO [dbo].[AuditLog]
            ([TableName], [Operation], [RecordId], [Timestamp], [UserId], [Details])
        SELECT
            'Customers' AS [TableName],
            'UPDATE' AS [Operation],
            i.[CustomerId] AS [RecordId],
            GETDATE() AS [Timestamp],
            SYSTEM_USER AS [UserId],
            'Updated customer: ' + i.[FirstName] + ' ' + i.[LastName] AS [Details]
        FROM inserted i;
    END

    -- Log DELETE operations
    ELSE IF EXISTS (SELECT * FROM deleted) AND NOT EXISTS (SELECT * FROM inserted)
    BEGIN
        INSERT INTO [dbo].[AuditLog]
            ([TableName], [Operation], [RecordId], [Timestamp], [UserId], [Details])
        SELECT
            'Customers' AS [TableName],
            'DELETE' AS [Operation],
            d.[CustomerId] AS [RecordId],
            GETDATE() AS [Timestamp],
            SYSTEM_USER AS [UserId],
            'Deleted customer: ' + d.[FirstName] + ' ' + d.[LastName] AS [Details]
        FROM deleted d;
    END
END
GO

-- ============================================================================
-- Trigger: trg_Order_Validate
-- Purpose: Validate order data before INSERT/UPDATE
-- Action: Ensures order total matches sum of order items
-- ============================================================================
CREATE OR ALTER TRIGGER dbo.trg_Order_Validate
ON [dbo].[Orders]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderId INT;
    DECLARE @TotalAmount DECIMAL(10, 2);
    DECLARE @CalculatedTotal DECIMAL(10, 2);

    DECLARE order_cursor CURSOR FOR
    SELECT DISTINCT i.[OrderId], i.[TotalAmount]
    FROM inserted i;

    OPEN order_cursor;

    FETCH NEXT FROM order_cursor INTO @OrderId, @TotalAmount;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Calculate actual item total
        SELECT @CalculatedTotal = ISNULL(SUM(oi.[LineTotal]), 0)
        FROM [dbo].[OrderItems] oi
        WHERE oi.[OrderId] = @OrderId;

        -- If totals don't match, raise error (data validation only, no security issue)
        IF @CalculatedTotal > 0 AND ABS(@CalculatedTotal - @TotalAmount) > 0.01
        BEGIN
            RAISERROR('Order total does not match line items', 16, 1);
            ROLLBACK;
            RETURN;
        END

        FETCH NEXT FROM order_cursor INTO @OrderId, @TotalAmount;
    END

    CLOSE order_cursor;
    DEALLOCATE order_cursor;
END
GO

-- ============================================================================
-- Trigger: trg_SupportTicket_ChangeLog
-- Purpose: Log changes to support tickets
-- Action: Tracks status changes and assignments
-- ============================================================================
CREATE OR ALTER TRIGGER dbo.trg_SupportTicket_ChangeLog
ON [dbo].[SupportTickets]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[AuditLog]
        ([TableName], [Operation], [RecordId], [Timestamp], [UserId], [Details])
    SELECT
        'SupportTickets' AS [TableName],
        'UPDATE' AS [Operation],
        i.[TicketId] AS [RecordId],
        GETDATE() AS [Timestamp],
        SYSTEM_USER AS [UserId],
        'Status: ' + ISNULL(d.[Status], 'NULL') + ' -> ' + ISNULL(i.[Status], 'NULL') +
        ', Assigned: ' + ISNULL(d.[AssignedTo], 'Unassigned') + ' -> ' + ISNULL(i.[AssignedTo], 'Unassigned') AS [Details]
    FROM inserted i
    JOIN deleted d ON i.[TicketId] = d.[TicketId]
    WHERE ISNULL(i.[Status], '') <> ISNULL(d.[Status], '')
       OR ISNULL(i.[AssignedTo], '') <> ISNULL(d.[AssignedTo], '');
END
GO

-- ============================================================================
-- Trigger: trg_CommunicationLog_Status
-- Purpose: Track communication log changes
-- Action: Logs message type changes and failures
-- ============================================================================
CREATE OR ALTER TRIGGER dbo.trg_CommunicationLog_Status
ON [dbo].[CommunicationLog]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Log when a message fails
    INSERT INTO [dbo].[AuditLog]
        ([TableName], [Operation], [RecordId], [Timestamp], [UserId], [Details])
    SELECT
        'CommunicationLog' AS [TableName],
        CASE WHEN i.[Status] = 'FAILED' THEN 'FAILED_MESSAGE' ELSE 'MESSAGE_SENT' END AS [Operation],
        i.[LogId] AS [RecordId],
        GETDATE() AS [Timestamp],
        SYSTEM_USER AS [UserId],
        i.[MessageType] + ' to ' + ISNULL(i.[Recipient], 'UNKNOWN') + ': ' + ISNULL(i.[ErrorMessage], 'No error') AS [Details]
    FROM inserted i
    WHERE i.[Status] IN ('FAILED', 'BOUNCED');
END
GO

-- ============================================================================
-- Trigger: trg_OrderItem_SummarizeOrder
-- Purpose: Update order total when order items change
-- Action: Recalculates order total from line items
-- ============================================================================
CREATE OR ALTER TRIGGER dbo.trg_OrderItem_SummarizeOrder
ON [dbo].[OrderItems]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderId INT;

    -- Get affected order IDs (from inserted or deleted)
    DECLARE affected_orders CURSOR FOR
    SELECT DISTINCT [OrderId] FROM inserted
    UNION
    SELECT DISTINCT [OrderId] FROM deleted;

    OPEN affected_orders;

    FETCH NEXT FROM affected_orders INTO @OrderId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Update order total with sum of line items
        UPDATE [dbo].[Orders]
        SET [TotalAmount] = (
            SELECT ISNULL(SUM(oi.[LineTotal]), 0)
            FROM [dbo].[OrderItems] oi
            WHERE oi.[OrderId] = @OrderId
        ),
        [UpdatedDate] = GETDATE()
        WHERE [OrderId] = @OrderId;

        -- Log the update
        INSERT INTO [dbo].[AuditLog]
            ([TableName], [Operation], [RecordId], [Timestamp], [UserId], [Details])
        VALUES
            ('Orders', 'AUTO_TOTAL_UPDATE', @OrderId, GETDATE(), SYSTEM_USER, 'Order total recalculated from items');

        FETCH NEXT FROM affected_orders INTO @OrderId;
    END

    CLOSE affected_orders;
    DEALLOCATE affected_orders;
END
GO

-- ============================================================================
-- Trigger: trg_Customer_Deactivation
-- Purpose: Cascade operations when customer is suspended or deactivated
-- Action: Logs deactivation and related cleanup operations
-- ============================================================================
CREATE OR ALTER TRIGGER dbo.trg_Customer_Deactivation
ON [dbo].[Customers]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if status changed to Suspended or Inactive
    INSERT INTO [dbo].[AuditLog]
        ([TableName], [Operation], [RecordId], [Timestamp], [UserId], [Details])
    SELECT
        'Customers' AS [TableName],
        'DEACTIVATION' AS [Operation],
        i.[CustomerId] AS [RecordId],
        GETDATE() AS [Timestamp],
        SYSTEM_USER AS [UserId],
        'Customer status changed to: ' + dbo.GetCustomerStatus(i.[Status]) AS [Details]
    FROM inserted i
    JOIN deleted d ON i.[CustomerId] = d.[CustomerId]
    WHERE i.[Status] IN ('S', 'I')
      AND d.[Status] <> i.[Status];

    -- Invalidate active sessions for deactivated customers
    UPDATE [dbo].[UserSessions]
    SET [IsActive] = 'N', [LogoutTime] = GETDATE()
    WHERE [CustomerId] IN (
        SELECT [CustomerId] FROM inserted WHERE [Status] IN ('S', 'I')
    )
    AND [IsActive] = 'Y';
END
GO
