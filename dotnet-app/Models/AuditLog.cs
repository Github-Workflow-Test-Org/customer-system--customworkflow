namespace CustomerManagement.Models;

/// <summary>
/// Represents an audit log entry
/// </summary>
public class AuditLog
{
    public int AuditId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;  // INSERT, UPDATE, DELETE
    public int? RecordId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? Details { get; set; }
}

/// <summary>
/// Represents a support ticket
/// </summary>
public class SupportTicket
{
    public int TicketId { get; set; }
    public int CustomerId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "OPEN";  // OPEN, IN_PROGRESS, RESOLVED, CLOSED
    public int Priority { get; set; } = 3;  // 1=Critical, 2=High, 3=Medium, 4=Low
    public DateTime CreatedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public string? AssignedTo { get; set; }
}

/// <summary>
/// Represents a communication log entry
/// </summary>
public class CommunicationLog
{
    public int LogId { get; set; }
    public int? CustomerId { get; set; }
    public string MessageType { get; set; } = string.Empty;  // EMAIL, SMS, PUSH, WEBHOOK
    public string? Recipient { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string Status { get; set; } = "SENT";  // SENT, FAILED, BOUNCED
    public DateTime SentDate { get; set; }
    public string? ErrorMessage { get; set; }
}
