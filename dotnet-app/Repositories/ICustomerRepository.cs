using CustomerManagement.Models;

namespace CustomerManagement.Repositories;

/// <summary>
/// Repository interface for Customer data access
/// </summary>
public interface ICustomerRepository
{
    Task<Customer?> GetCustomerByIdAsync(int customerId);
    Task<List<Customer>> GetAllCustomersAsync();
    Task<int> CreateCustomerAsync(Customer customer);
    Task<bool> UpdateCustomerAsync(Customer customer);
    Task<bool> DeleteCustomerAsync(int customerId);
    Task<List<Customer>> SearchCustomersAsync(string searchCriteria);
}

/// <summary>
/// Repository interface for Audit Log data access
/// </summary>
public interface IAuditLogRepository
{
    Task<AuditLog?> GetAuditLogByIdAsync(int auditId);
    Task<List<AuditLog>> GetAuditLogsAsync(string tableName, DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> CreateAuditLogAsync(AuditLog auditLog);
}
