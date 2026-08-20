using Microsoft.AspNetCore.Mvc;
using CustomerManagement.Models;
using CustomerManagement.Services;

namespace CustomerManagement.Controllers;

/// <summary>
/// REST API controller for audit operations
/// Provides endpoints for audit log retrieval, export, and notifications
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get audit logs for a specific table
    /// </summary>
    [HttpGet("logs/{tableName}")]
    public async Task<ActionResult<List<AuditLog>>> GetAuditLogs(
        string tableName,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required" });

            var auditLogs = await _auditService.GetAuditLogsAsync(tableName, fromDate, toDate);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Export audit data to file
    /// CWEID 73: Path Manipulation vulnerability
    /// filePath is used directly without validation
    /// </summary>
    [HttpPost("export")]
    public async Task<ActionResult> ExportAuditData([FromBody] ExportRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
                return BadRequest(new { error = "File path is required" });

            // CWEID 73: Path Manipulation vulnerability
            // Example vulnerable input: "../../windows/system32/config/sam"
            // Should validate against whitelist of allowed directories
            var success = await _auditService.ExportAuditDataAsync(request.FilePath);

            if (!success)
                return StatusCode(500, new { error = "Export failed" });

            return Ok(new { message = "Audit data exported successfully", filePath = request.FilePath });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized export attempt");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit data");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Send audit log via email
    /// CWEID 88: Argument Injection vulnerability
    /// recipient, subject, and body are passed without proper escaping
    /// </summary>
    [HttpPost("send-email")]
    public async Task<ActionResult> SendAuditEmail([FromBody] SendEmailRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Recipient))
                return BadRequest(new { error = "Recipient email is required" });

            if (string.IsNullOrWhiteSpace(request.Subject))
                return BadRequest(new { error = "Subject is required" });

            // CWEID 88: Argument Injection vulnerability
            // Example vulnerable input: recipient = "test@example.com\" -bcc attacker@evil.com \""
            // Subject and body are concatenated into command without escaping
            var success = await _auditService.SendAuditEmailAsync(
                request.Recipient,
                request.Subject,
                request.Body ?? string.Empty);

            if (!success)
                return StatusCode(500, new { error = "Email send failed" });

            return Ok(new { message = "Email sent successfully", recipient = request.Recipient });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending audit email");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get audit summary for a time period
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult> GetAuditSummary(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            // Get audit logs for relevant tables
            var customerLogs = await _auditService.GetAuditLogsAsync("Customers", fromDate, toDate);
            var orderLogs = await _auditService.GetAuditLogsAsync("Orders", fromDate, toDate);

            var summary = new
            {
                period = new { from = fromDate, to = toDate },
                totals = new
                {
                    customerChanges = customerLogs.Count,
                    orderChanges = orderLogs.Count
                },
                operations = new
                {
                    inserts = customerLogs.Count(l => l.Operation == "INSERT"),
                    updates = customerLogs.Count(l => l.Operation == "UPDATE"),
                    deletes = customerLogs.Count(l => l.Operation == "DELETE")
                }
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit summary");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

/// <summary>
/// Request model for exporting audit data
/// </summary>
public class ExportRequest
{
    public string FilePath { get; set; } = string.Empty;
    public string Format { get; set; } = "CSV";
}

/// <summary>
/// Request model for sending audit email
/// </summary>
public class SendEmailRequest
{
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Body { get; set; }
}
