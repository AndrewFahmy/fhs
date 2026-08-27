public static partial class AppConstants
{
    public static class Auth
    {
        private const string AuthenticationSectionName = "Authentication";

        public static readonly string AuthorityPropertyName = $"{AuthenticationSectionName}:Authority";
        
        public static readonly string AudiencePropertyName = $"{AuthenticationSectionName}:Audience";
    }
}
