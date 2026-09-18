public static partial class AppConstants
{
    public static class Data
    {
        public const string DatabaseConnectionName = "DbConnection";

        public const string AdminSubjectId = "11111111-1111-4111-8111-111111111111";
        public static readonly Guid AdminActorId = Guid.Parse("3fba3307-da98-4573-9542-1ee925b43fd4");

        public const string DefaultFacilityCode = "DEFAULT";
        public static readonly Guid DefaultFacilityId = Guid.Parse("2658db5b-e9bf-4a6c-8b3e-9bc2740d39f3");

        public const int ActorDisplayNameMaxLength = 200;
        public const int ActorSubjectIdMaxLength = 255;
        public const int ActorKindMaxLength = 20;
        public const int FacilityCodeMaxLength = 50;
        public const int FacilityNameMaxLength = 200;
        public const int DefectDescriptionMaxLength = 500;
        public const int DefectResolutionMaxLength = 500;
        public const int DefectSeverityMaxLength = 20;
        public const int ErrorCodeDescriptionMaxLength = 500;
        public const int ErrorCodeSeverityMaxLength = 20;
        public const int ErrorCodeMaxLength = 50;
        public const int StationCodeMaxLength = 50;
        public const int StationNameMaxLength = 200;
        public const int CustomerCodeMaxLength = 50;
        public const int CustomerNameMaxLength = 200;
        public const int EscapeDescriptionMaxLength = 500;
        public const int EscapeResolutionMaxLength = 500;
        public const int EscapeSeverityMaxLength = 20;
        public const int EscapeAttributionBasisMaxLength = 20;
    }
}
