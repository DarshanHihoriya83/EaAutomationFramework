using System.Text;

namespace EAFramework.Utilities
{
    /// <summary>
    /// Builds unique usernames and emails for registration and other data-driven tests.
    /// </summary>
    public static class UniqueTestDataGenerator
    {
        public static string CreateUniqueSuffix() =>
            $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Environment.TickCount64 % 100000:D5}";

        public static string CreateUniqueUsername(string baseUsername)
        {
            string safe = SanitizeUsername(baseUsername);
            return $"{safe}_{CreateUniqueSuffix()}";
        }

        public static string CreateUniqueEmail(string baseEmail, string? fallbackUsername = null)
        {
            if (string.IsNullOrWhiteSpace(baseEmail) || !baseEmail.Contains('@', StringComparison.Ordinal))
            {
                string user = SanitizeUsername(fallbackUsername ?? "user");
                return $"{user}_{CreateUniqueSuffix()}@test.com";
            }

            int atIndex = baseEmail.IndexOf('@');
            string localPart = baseEmail[..atIndex];
            string domain = baseEmail[(atIndex + 1)..];

            return $"{SanitizeUsername(localPart)}+{CreateUniqueSuffix()}@{domain}";
        }

        public static (string Username, string Email) CreateUniqueRegistrationPair(
            string baseUsername,
            string baseEmail)
        {
            string suffix = CreateUniqueSuffix();
            string username = $"{SanitizeUsername(baseUsername)}_{suffix}";
            string email = CreateUniqueEmail(baseEmail, baseUsername);
            return (username, email);
        }

        private static string SanitizeUsername(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "user";
            }

            StringBuilder builder = new(value.Length);

            foreach (char character in value.Trim())
            {
                if (char.IsLetterOrDigit(character) || character is '_' or '.')
                {
                    builder.Append(character);
                }
            }

            string result = builder.ToString();

            if (result.Length == 0)
            {
                return "user";
            }

            return result.Length > 32 ? result[..32] : result;
        }
    }
}
