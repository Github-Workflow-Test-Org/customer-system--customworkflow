namespace CustomerManagement.Models;

/// <summary>
/// Represents a customer in the system
/// </summary>
public class Customer
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public char Status { get; set; } = 'A';  // A=Active, I=Inactive, S=Suspended
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
