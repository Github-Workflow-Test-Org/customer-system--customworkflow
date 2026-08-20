using Microsoft.AspNetCore.Mvc;
using CustomerManagement.Models;
using CustomerManagement.Services;

namespace CustomerManagement.Controllers;

/// <summary>
/// REST API controller for customer operations
/// Provides endpoints for CRUD operations and search
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{customerId}")]
    public async Task<ActionResult<Customer>> GetCustomer(int customerId)
    {
        try
        {
            var customer = await _customerService.GetCustomerAsync(customerId);
            if (customer == null)
                return NotFound(new { message = $"Customer {customerId} not found" });

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Customer>>> GetAllCustomers()
    {
        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                return BadRequest(new { error = "First and last names are required" });

            var customerId = await _customerService.CreateCustomerAsync(
                request.FirstName, request.LastName, request.Email, request.Phone,
                request.Country, request.City, request.PostalCode);

            return CreatedAtAction(nameof(GetCustomer), new { customerId }, new { customerId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update customer information
    /// </summary>
    [HttpPut("{customerId}")]
    public async Task<ActionResult> UpdateCustomer(int customerId, [FromBody] UpdateCustomerRequest request)
    {
        try
        {
            var updated = await _customerService.UpdateCustomerAsync(
                customerId, request.FirstName, request.LastName, request.Email, request.Phone);

            if (!updated)
                return NotFound(new { message = $"Customer {customerId} not found" });

            return Ok(new { message = "Customer updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete (deactivate) a customer
    /// </summary>
    [HttpDelete("{customerId}")]
    public async Task<ActionResult> DeleteCustomer(int customerId)
    {
        try
        {
            var deleted = await _customerService.DeleteCustomerAsync(customerId);
            if (!deleted)
                return NotFound(new { message = $"Customer {customerId} not found" });

            return Ok(new { message = "Customer deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Search customers by criteria
    /// CWEID 89: SQL Injection vulnerability
    /// Criteria is passed to stored procedure which concatenates into WHERE clause
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<List<Customer>>> SearchCustomers([FromBody] SearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Criteria))
                return BadRequest(new { error = "Search criteria is required" });

            // CWEID 89: SQL Injection vulnerability
            // Example vulnerable input: "Email LIKE '%@example.com' OR 1=1--"
            var customers = await _customerService.SearchCustomersAsync(request.Criteria);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get customer lifetime value
    /// </summary>
    [HttpGet("{customerId}/lifetime-value")]
    public async Task<ActionResult<decimal>> GetLifetimeValue(int customerId)
    {
        try
        {
            var value = await _customerService.GetCustomerLifetimeValueAsync(customerId);
            return Ok(new { customerId, lifetimeValue = value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating lifetime value");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

/// <summary>
/// Request model for creating a customer
/// </summary>
public class CreateCustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
}

/// <summary>
/// Request model for updating a customer
/// </summary>
public class UpdateCustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

/// <summary>
/// Request model for searching customers
/// </summary>
public class SearchRequest
{
    public string Criteria { get; set; } = string.Empty;
}
