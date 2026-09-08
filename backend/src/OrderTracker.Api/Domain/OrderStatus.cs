using System.Text.Json.Serialization;

namespace OrderTracker.Api.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    Pending,
    Confirmed,
    Fulfilled,
    Cancelled
}
