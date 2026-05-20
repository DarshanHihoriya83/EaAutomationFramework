using System.Reflection;
using EAFramework.Reporting;
using Xunit.Sdk;

namespace EaTestAutomation.Reporting
{
    /// <summary>
    /// Creates <c>Artifacts/{class}_{method}_{date}_{time}/</c> before each test method.
    /// For data-driven tests, call <see cref="Base.BaseTest.BindArtifactToTestCase"/> first in the test body.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class PlaywrightTestArtifactAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            string identity =
                $"{methodUnderTest.DeclaringType?.Name ?? "Test"}_{methodUnderTest.Name}";

            string artifactRoot =
                ArtifactDirectoryBuilder.CreateTestRunDirectory(identity);

            TestArtifactScope.Begin(identity, artifactRoot);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            TestArtifactScope.Clear();
        }
    }
}
