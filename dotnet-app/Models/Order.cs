namespace CustomerManagement.Models;

/// <summary>
/// Represents a customer order
/// </summary>
public class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "PENDING";  // PENDING, PROCESSING, SHIPPED, DELIVERED, CANCELLED
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

/// <summary>
/// Represents an individual item in an order
/// </summary>
public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
