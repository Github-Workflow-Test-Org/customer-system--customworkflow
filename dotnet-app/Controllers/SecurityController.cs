using Microsoft.AspNetCore.Mvc;
using CustomerManagement.Services;

namespace CustomerManagement.Controllers;

/// <summary>
/// REST API controller for security operations
/// Provides endpoints for authentication, session management, and access validation
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SecurityController : ControllerBase
{
    private readonly ISecurityService _securityService;
    private readonly ILogger<SecurityController> _logger;

    public SecurityController(ISecurityService securityService, ILogger<SecurityController> logger)
    {
        _securityService = securityService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate customer with credentials
    /// CWEID 798: Hardcoded credentials vulnerability
    /// Service account credentials are hardcoded in the service layer
    /// </summary>
    [HttpPost("login")]
    public ActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Username and password are required" });

            // CWEID 798: Hardcoded credentials vulnerability
            // Service account password is hardcoded in SecurityService
            // Should use secure credential storage (Key Vault, Secrets Manager, etc.)
            var authenticated = _securityService.AuthenticateCustomer(request.Username, request.Password);

            if (!authenticated)
                return Unauthorized(new { error = "Invalid credentials" });

            // In a real implementation, create and return JWT token
            var token = Guid.NewGuid().ToString("N");
            return Ok(new { token, message = "Authentication successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new session for a customer
    /// </summary>
    [HttpPost("session")]
    public async Task<ActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            if (request.CustomerId <= 0)
                return BadRequest(new { error = "Valid customer ID is required" });

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = Request.Headers.UserAgent.ToString() ?? "unknown";

            var sessionToken = await _securityService.CreateSessionAsync(
                request.CustomerId, ipAddress, userAgent);

            return Ok(new { sessionToken, customerId = request.CustomerId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Validate customer access to a resource
    /// CWEID 89: SQL Injection vulnerability in access control
    /// Resource parameter could be concatenated into dynamic query
    /// </summary>
    [HttpPost("validate-access")]
    public async Task<ActionResult> ValidateAccess([FromBody] ValidateAccessRequest request)
    {
        try
        {
            if (request.CustomerId <= 0)
                return BadRequest(new { error = "Valid customer ID is required" });

            if (string.IsNullOrWhiteSpace(request.Resource))
                return BadRequest(new { error = "Resource is required" });

            // CWEID 89: SQL Injection vulnerability
            // Example vulnerable input: resource = "Report' OR '1'='1"
            // In vulnerable implementation, would be concatenated into query:
            // "SELECT * FROM Permissions WHERE CustomerId = " + customerId + " AND Resource = '" + resource + "'"
            var hasAccess = await _securityService.ValidateAccessAsync(request.CustomerId, request.Resource);

            return Ok(new
            {
                customerId = request.CustomerId,
                resource = request.Resource,
                hasAccess = hasAccess
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating access");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// End a customer session
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.SessionToken))
                return BadRequest(new { error = "Session token is required" });

            var success = await _securityService.EndSessionAsync(request.SessionToken);

            if (!success)
                return StatusCode(500, new { error = "Logout failed" });

            return Ok(new { message = "Logout successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging out");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

/// <summary>
/// Request model for login
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request model for creating a session
/// </summary>
public class CreateSessionRequest
{
    public int CustomerId { get; set; }
}

/// <summary>
/// Request model for validating access
/// </summary>
public class ValidateAccessRequest
{
    public int CustomerId { get; set; }
    public string Resource { get; set; } = string.Empty;
}

/// <summary>
/// Request model for logout
/// </summary>
public class LogoutRequest
{
    public string SessionToken { get; set; } = string.Empty;
}
