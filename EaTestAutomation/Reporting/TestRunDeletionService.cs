namespace EaTestAutomation.Reporting
{
    /// <summary>
    /// Removes a test run from registry JSON and deletes its artifact folder (video, trace, logs, screenshots, healing).
    /// </summary>
    public static class TestRunDeletionService
    {
        public static bool DeleteRun(string artifactFolder)
        {
            if (string.IsNullOrWhiteSpace(artifactFolder))
            {
                return false;
            }

            bool removedFromRegistry = false;
            bool removedArtifacts = false;

            foreach (string resultsPath in DashboardPaths.GetAllTestResultsJsonPaths())
            {
                if (TestRunRegistry.RemoveByArtifactFolder(resultsPath, artifactFolder))
                {
                    removedFromRegistry = true;
                }
            }

            foreach (string artifactsRoot in DashboardPaths.GetAllArtifactRoots())
            {
                string target = Path.Combine(artifactsRoot, artifactFolder);

                if (!Directory.Exists(target))
                {
                    continue;
                }

                try
                {
                    Directory.Delete(target, recursive: true);
                    removedArtifacts = true;
                }
                catch
                {
                    // continue other roots
                }
            }

            if (removedFromRegistry || removedArtifacts)
            {
                MasterDashboardGenerator.Generate();
                return true;
            }

            return false;
        }
    }
}
