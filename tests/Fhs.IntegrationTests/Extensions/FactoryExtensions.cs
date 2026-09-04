namespace Fhs.IntegrationTests.Extensions;

internal static class TestFactoryExtensions
{
    extension(FhsApiFactory factory)
    {
        public HttpClient CreateAdminClient() => factory.CreateClientAs(AppConstants.Data.AdminSubjectId, AppConstants.Auth.AdminRole);

        public HttpClient CreateLineOperatorClient() => factory.CreateClientAs(AppConstants.Data.LineOperatorSubjectId);
    }
}
