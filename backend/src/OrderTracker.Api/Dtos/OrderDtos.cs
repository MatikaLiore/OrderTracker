using System.ComponentModel.DataAnnotations;

namespace OrderTracker.Api.Dtos;

public sealed class CreateOrderRequest
{
    [MaxLength(80, ErrorMessage = "Client reference is too long (80 characters max).")]
    public string? ClientReference { get; set; }

    [MaxLength(1000, ErrorMessage = "Notes are too long (1000 characters max).")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "At least one menu item is required.")]
    [MinLength(1, ErrorMessage = "At least one menu item is required.")]
    public List<OrderLineRequest> LineItems { get; set; } = [];
}

public sealed class OrderLineRequest
{
    [Required(ErrorMessage = "Please select a dish from the menu.")]
    public Guid MenuItemId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive whole number.")]
    public int Quantity { get; set; }
}

public sealed class ChangeStatusRequest
{
    [Required]
    public Models.OrderStatus Status { get; set; }
}

public sealed class MenuItemDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>Result of placing an order. ConflictMessage means HTTP 409.</summary>
public sealed class SubmitResult
{
    public Models.Order? Order { get; init; }
    public bool Created { get; init; }
    public string? ConflictMessage { get; init; }
    public bool HasConflict => ConflictMessage is not null;
}
