using Microsoft.AspNetCore.Mvc;
using OrderTracker.Api.Contracts;
using OrderTracker.Api.Services;

namespace OrderTracker.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly OrderService _orders;

    public OrdersController(OrderService orders)
    {
        _orders = orders;
    }

    [HttpPost]
    public ActionResult<Domain.Order> Submit([FromBody] CreateOrderRequest request)
    {
        var result = _orders.Submit(request);
        if (result.Created)
            return CreatedAtAction(nameof(GetById), new { id = result.Order.Id }, result.Order);

        return Ok(result.Order);
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<Domain.Order>> List() =>
        Ok(_orders.List());

    [HttpGet("{id:guid}")]
    public ActionResult<Domain.Order> GetById(Guid id) =>
        Ok(_orders.Get(id));

    [HttpPost("{id:guid}/status")]
    public ActionResult<Domain.Order> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request) =>
        Ok(_orders.ChangeStatus(id, request.Status));
}
