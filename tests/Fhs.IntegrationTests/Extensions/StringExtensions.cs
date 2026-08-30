namespace Fhs.IntegrationTests.Extensions;

internal static class StringExtensions
{
    extension(string value)
    {
        public string ToCamelCase()
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length == 1)
                return value.ToLower();

            return $"{char.ToLower(value[0])}{value.Substring(1)}";
        }
    }
}