using System.Text.Json.Serialization;

namespace OrderTracker.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    Pending,
    Confirmed,
    Ready,
    Done,
    Cancelled
}
