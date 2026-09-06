public static partial class AppConstants
{
    public static class Cors
    {
        private const string CorsSectionName = "Cors";

        public static readonly string AllowedOriginsPropertyName = $"{CorsSectionName}:AllowedOrigins";

        public const string SpaPolicy = "SpaCors";
    }
}
