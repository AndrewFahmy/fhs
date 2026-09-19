namespace FHS.IntegrationTests;

/// <summary>
/// Identities and reference rows the suite owns. The API's migration seeds only the bootstrap
/// system administrator; everything a test authenticates as or binds data to beyond that is
/// created by <see cref="FhsApiFactory"/>, not borrowed from production seed data.
/// </summary>
internal static class TestData
{
    public const string LineOperatorSubjectId = "test-line-operator";
    public static readonly Guid LineOperatorActorId = Guid.Parse("a5a5a5a5-0000-4000-8000-000000000002");
}
