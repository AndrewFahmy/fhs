using FHS.Api.Data.Entities;
using Fhs.IntegrationTests.Interfaces;

namespace Fhs.IntegrationTests.Snapshots;

internal sealed record CustomerSnapshot(string Code, string Name, bool IsActive) : ISnapshotModel
{
    public static Type RelatedType { get; } = typeof(Customer);

    public static string[] ExcludedProperties { get; } = [nameof(Customer.Id), nameof(Customer.Version)];

    public static CustomerSnapshot From(Customer customer) =>
        new(customer.Code, customer.Name, customer.IsActive);
}
