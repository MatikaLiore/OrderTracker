using OrderTracker.Api.Domain;

namespace OrderTracker.Api.Contracts;

public sealed record SubmitOrderResult(Order Order, bool Created);
