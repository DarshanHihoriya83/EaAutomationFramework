using System.Text;

namespace EAFramework.Reporting
{
    /// <summary>
    /// Builds per-test artifact directories under <c>Artifacts/</c> (test identity + date/time + uniqueness).
    /// </summary>
    public static class ArtifactDirectoryBuilder
    {
        private const int MaxFolderNameLength = 180;

        public static string EnsureArtifactRoot()
        {
            string runtime = Path.Combine(AppContext.BaseDirectory, "Artifacts");

            if (Directory.Exists(runtime))
            {
                return Path.GetFullPath(runtime);
            }

            string cwd = Path.Combine(Directory.GetCurrentDirectory(), "Artifacts");
            Directory.CreateDirectory(cwd);
            return Path.GetFullPath(cwd);
        }

        /// <summary>
        /// Creates <c>Artifacts/{sanitizedName}_{yyyyMMdd_HHmmssfff}_{guid}</c> with standard subfolders.
        /// </summary>
        public static string CreateTestRunDirectory(string testIdentitySegment)
        {
            string root = EnsureArtifactRoot();
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
            string unique = Guid.NewGuid().ToString("N")[..8];
            string folder = $"{Sanitize(testIdentitySegment)}_{stamp}_{unique}";
            if (folder.Length > MaxFolderNameLength)
            {
                folder = folder[..MaxFolderNameLength];
            }

            string full = Path.Combine(root, folder);
            Directory.CreateDirectory(full);
            Directory.CreateDirectory(Path.Combine(full, "screenshots"));
            Directory.CreateDirectory(Path.Combine(full, "video"));
            Directory.CreateDirectory(Path.Combine(full, "trace"));
            Directory.CreateDirectory(Path.Combine(full, "logs"));
            Directory.CreateDirectory(Path.Combine(full, "healing"));
            return full;
        }

        public static string Sanitize(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "TestRun";
            }

            var sb = new StringBuilder(name.Length);
            foreach (char c in name.Trim())
            {
                if (char.IsLetterOrDigit(c) || c is '_' or '-' or '.')
                {
                    sb.Append(c);
                }
                else if (c is ' ' or '(' or ')' or ',' or ':')
                {
                    sb.Append('_');
                }
            }

            return sb.Length > 0 ? sb.ToString() : "TestRun";
        }
    }
}
