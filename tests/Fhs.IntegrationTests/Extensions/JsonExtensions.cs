using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FHS.Api.Data.Entities;
using FHS.Api.Interfaces;

namespace Fhs.IntegrationTests.Extensions;

internal static class JsonExtensions
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    extension(HttpResponseMessage response)
    {
        public Task<T?> ReadAsync<T>(CancellationToken ct) =>
            response.Content.ReadFromJsonAsync<T>(Options, ct);
    }

    extension(OutboxMessage message)
    {
        public IDomainEvent DeserializePayload()
        {
            var eventType = typeof(IDomainEvent).Assembly.GetType(message.Type)
            ?? throw new InvalidOperationException(
                $"Outbox row names type '{message.Type}', which does not exist in FHS.Api."
            );

            return (IDomainEvent)JsonSerializer.Deserialize(message.Payload, eventType, Options)!;
        }
    }
}