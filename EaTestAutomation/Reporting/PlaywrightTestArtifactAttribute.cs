using System.Reflection;
using EAFramework.Reporting;
using Xunit.Sdk;

namespace EaTestAutomation.Reporting
{
    /// <summary>
    /// Sets test identity before each method. Artifact folders are created lazily in
    /// <see cref="Base.BaseTest.BindArtifactToTestCase"/> or <see cref="Base.BaseTest"/> session init —
    /// not here — so data-driven tests do not spawn empty duplicate folders.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class PlaywrightTestArtifactAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            string identity =
                $"{methodUnderTest.DeclaringType?.Name ?? "Test"}_{methodUnderTest.Name}";

            TestArtifactScope.Begin(identity, null);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            TestArtifactScope.Clear();
        }
    }
}
