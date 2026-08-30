namespace Fhs.IntegrationTests.Extensions;

internal static class TestFactoryExtensions
{
    extension(FhsApiFactory factory)
    {
        public HttpClient AdminClient() => factory.CreateClientAs(AppConstants.Data.AdminSubjectId, AppConstants.Auth.AdminRole);

        public HttpClient LineOperatorClient() => factory.CreateClientAs(AppConstants.Data.LineOperatorSubjectId);
    }
}
