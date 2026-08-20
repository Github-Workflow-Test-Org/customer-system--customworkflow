using CustomerManagement.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CustomerManagement.Repositories;

/// <summary>
/// Repository implementation for Customer data access
/// Uses ADO.NET with stored procedures
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Customer?> GetCustomerByIdAsync(int customerId)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "SELECT [CustomerId], [FirstName], [LastName], [Email], [Phone], [RegistrationDate], " +
                    "[Country], [City], [PostalCode], [Status], [CreatedDate], [UpdatedDate] " +
                    "FROM [dbo].[Customers] WHERE [CustomerId] = @CustomerId",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Customer
                            {
                                CustomerId = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                                RegistrationDate = reader.GetDateTime(5),
                                Country = reader.IsDBNull(6) ? null : reader.GetString(6),
                                City = reader.IsDBNull(7) ? null : reader.GetString(7),
                                PostalCode = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Status = reader.GetChar(9),
                                CreatedDate = reader.GetDateTime(10),
                                UpdatedDate = reader.GetDateTime(11)
                            };
                        }
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving customer {customerId}", ex);
        }
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        var customers = new List<Customer>();

        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "SELECT [CustomerId], [FirstName], [LastName], [Email], [Phone], [RegistrationDate], " +
                    "[Country], [City], [PostalCode], [Status], [CreatedDate], [UpdatedDate] " +
                    "FROM [dbo].[Customers] ORDER BY [CustomerId]",
                    connection))
                {
                    command.CommandType = CommandType.Text;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            customers.Add(new Customer
                            {
                                CustomerId = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                                RegistrationDate = reader.GetDateTime(5),
                                Country = reader.IsDBNull(6) ? null : reader.GetString(6),
                                City = reader.IsDBNull(7) ? null : reader.GetString(7),
                                PostalCode = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Status = reader.GetChar(9),
                                CreatedDate = reader.GetDateTime(10),
                                UpdatedDate = reader.GetDateTime(11)
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving customers", ex);
        }

        return customers;
    }

    public async Task<int> CreateCustomerAsync(Customer customer)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("dbo.CreateCustomer", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@FirstName", customer.FirstName);
                    command.Parameters.AddWithValue("@LastName", customer.LastName);
                    command.Parameters.AddWithValue("@Email", customer.Email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Country", customer.Country ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@City", customer.City ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PostalCode", customer.PostalCode ?? (object)DBNull.Value);

                    var outputParam = new SqlParameter("@NewCustomerId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputParam);

                    await command.ExecuteNonQueryAsync();

                    return (int)(outputParam.Value ?? -1);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error creating customer", ex);
        }
    }

    public async Task<bool> UpdateCustomerAsync(Customer customer)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("dbo.UpdateCustomerInfo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                    command.Parameters.AddWithValue("@FirstName", customer.FirstName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LastName", customer.LastName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Email", customer.Email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error updating customer {customer.CustomerId}", ex);
        }
    }

    public async Task<bool> DeleteCustomerAsync(int customerId)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "UPDATE [dbo].[Customers] SET [Status] = 'I', [UpdatedDate] = GETDATE() WHERE [CustomerId] = @CustomerId",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error deleting customer {customerId}", ex);
        }
    }

    public async Task<List<Customer>> SearchCustomersAsync(string searchCriteria)
    {
        var customers = new List<Customer>();

        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("dbo.SearchCustomers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    // CWEID 89: SQL Injection vulnerability
                    // searchCriteria is passed directly to stored procedure
                    // which concatenates it into a dynamic WHERE clause
                    command.Parameters.AddWithValue("@SearchCriteria", searchCriteria);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            customers.Add(new Customer
                            {
                                CustomerId = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                                RegistrationDate = reader.GetDateTime(5),
                                Country = reader.IsDBNull(6) ? null : reader.GetString(6),
                                City = reader.IsDBNull(7) ? null : reader.GetString(7),
                                PostalCode = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Status = reader.GetChar(9),
                                CreatedDate = reader.GetDateTime(10),
                                UpdatedDate = reader.GetDateTime(11)
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error searching customers", ex);
        }

        return customers;
    }
}

/// <summary>
/// Repository implementation for Audit Log data access
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly string _connectionString;

    public AuditLogRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<AuditLog?> GetAuditLogByIdAsync(int auditId)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "SELECT [AuditId], [TableName], [Operation], [RecordId], [Timestamp], [UserId], " +
                    "[OldValues], [NewValues], [IpAddress], [Details] " +
                    "FROM [dbo].[AuditLog] WHERE [AuditId] = @AuditId",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@AuditId", auditId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new AuditLog
                            {
                                AuditId = reader.GetInt32(0),
                                TableName = reader.GetString(1),
                                Operation = reader.GetString(2),
                                RecordId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                                Timestamp = reader.GetDateTime(4),
                                UserId = reader.IsDBNull(5) ? null : reader.GetString(5),
                                OldValues = reader.IsDBNull(6) ? null : reader.GetString(6),
                                NewValues = reader.IsDBNull(7) ? null : reader.GetString(7),
                                IpAddress = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Details = reader.IsDBNull(9) ? null : reader.GetString(9)
                            };
                        }
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving audit log {auditId}", ex);
        }
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(string tableName, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var auditLogs = new List<AuditLog>();

        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT [AuditId], [TableName], [Operation], [RecordId], [Timestamp], [UserId], " +
                           "[OldValues], [NewValues], [IpAddress], [Details] " +
                           "FROM [dbo].[AuditLog] WHERE [TableName] = @TableName";

                if (fromDate.HasValue)
                    query += " AND [Timestamp] >= @FromDate";
                if (toDate.HasValue)
                    query += " AND [Timestamp] <= @ToDate";

                query += " ORDER BY [Timestamp] DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@TableName", tableName);

                    if (fromDate.HasValue)
                        command.Parameters.AddWithValue("@FromDate", fromDate.Value);
                    if (toDate.HasValue)
                        command.Parameters.AddWithValue("@ToDate", toDate.Value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            auditLogs.Add(new AuditLog
                            {
                                AuditId = reader.GetInt32(0),
                                TableName = reader.GetString(1),
                                Operation = reader.GetString(2),
                                RecordId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                                Timestamp = reader.GetDateTime(4),
                                UserId = reader.IsDBNull(5) ? null : reader.GetString(5),
                                OldValues = reader.IsDBNull(6) ? null : reader.GetString(6),
                                NewValues = reader.IsDBNull(7) ? null : reader.GetString(7),
                                IpAddress = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Details = reader.IsDBNull(9) ? null : reader.GetString(9)
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving audit logs", ex);
        }

        return auditLogs;
    }

    public async Task<int> CreateAuditLogAsync(AuditLog auditLog)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("dbo.LogAuditAction", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@TableName", auditLog.TableName);
                    command.Parameters.AddWithValue("@Operation", auditLog.Operation);
                    command.Parameters.AddWithValue("@RecordId", auditLog.RecordId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UserId", auditLog.UserId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Details", auditLog.Details ?? (object)DBNull.Value);

                    var result = await command.ExecuteScalarAsync();
                    return (int?)result ?? -1;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error creating audit log", ex);
        }
    }
}
