using Microsoft.AspNetCore.Mvc;
using OrderTracker.Api.Dtos;
using OrderTracker.Api.Models;
using OrderTracker.Api.Services;

namespace OrderTracker.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly OrderService _orders;

    public OrdersController(OrderService orders) => _orders = orders;

    [HttpPost]
    public ActionResult<Order> PlaceOrder([FromBody] CreateOrderRequest request)
    {
        var result = _orders.PlaceOrder(request);

        if (result.HasConflict)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate client reference",
                Detail = result.ConflictMessage
            });
        }

        if (result.Created)
            return CreatedAtAction(nameof(GetById), new { id = result.Order!.Id }, result.Order);

        return Ok(result.Order);
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<Order>> List() => Ok(_orders.ListOrders());

    [HttpGet("{id:guid}")]
    public ActionResult<Order> GetById(Guid id) => Ok(_orders.GetOrder(id));

    [HttpPost("{id:guid}/status")]
    public ActionResult<Order> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request) =>
        Ok(_orders.ChangeStatus(id, request.Status));
}

[ApiController]
[Route("api/menu")]
public sealed class MenuController : ControllerBase
{
    private readonly OrderService _orders;

    public MenuController(OrderService orders) => _orders = orders;

    [HttpGet]
    public ActionResult<IReadOnlyList<MenuItemDto>> List() => Ok(_orders.ListMenu());
}
