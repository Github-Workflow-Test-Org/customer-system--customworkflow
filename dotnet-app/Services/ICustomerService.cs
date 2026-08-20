using CustomerManagement.Models;

namespace CustomerManagement.Services;

/// <summary>
/// Service interface for customer business logic
/// </summary>
public interface ICustomerService
{
    Task<Customer?> GetCustomerAsync(int customerId);
    Task<List<Customer>> GetAllCustomersAsync();
    Task<int> CreateCustomerAsync(string firstName, string lastName, string? email, string? phone, string? country, string? city, string? postalCode);
    Task<bool> UpdateCustomerAsync(int customerId, string firstName, string lastName, string? email, string? phone);
    Task<bool> DeleteCustomerAsync(int customerId);
    Task<List<Customer>> SearchCustomersAsync(string searchCriteria);
    Task<decimal> GetCustomerLifetimeValueAsync(int customerId);
}

/// <summary>
/// Service interface for audit operations
/// </summary>
public interface IAuditService
{
    Task<List<AuditLog>> GetAuditLogsAsync(string tableName, DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> ExportAuditDataAsync(string filePath);
    Task<bool> SendAuditEmailAsync(string recipient, string subject, string body);
}

/// <summary>
/// Service interface for security operations
/// </summary>
public interface ISecurityService
{
    bool AuthenticateCustomer(string username, string password);
    Task<string> CreateSessionAsync(int customerId, string ipAddress, string userAgent);
    Task<bool> ValidateAccessAsync(int customerId, string resource);
    Task<bool> EndSessionAsync(string sessionToken);
}
