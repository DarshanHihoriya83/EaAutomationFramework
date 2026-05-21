using EAFramework.Config;
using Microsoft.Playwright;

namespace EaTestAutomation.Parallel
{
    /// <summary>
    /// Runs multiple data-driven cases with separate browsers (respects <c>MaxParallelBrowsers</c>).
    /// </summary>
    public static class ParallelTestOrchestrator
    {
        public static async Task RunEmployeeCasesAsync<TCase>(
            TestSettings settings,
            string testMethodName,
            IEnumerable<TCase> cases,
            Func<TCase, int, string> identityFactory,
            Func<IPage, TCase, Task> executeAsync)
            where TCase : notnull
        {
            List<TCase> list = cases.ToList();

            if (list.Count == 0)
            {
                return;
            }

            if (!settings.EnableParallelExecution)
            {
                throw new InvalidOperationException(
                    "RunEmployeeCasesAsync requires EnableParallelExecution=true. Use sequential loop in test instead.");
            }

            int maxParallel = Math.Max(1, settings.MaxParallelBrowsers);
            using var limiter = new SemaphoreSlim(maxParallel, maxParallel);

            IEnumerable<Task> tasks = list.Select((item, index) =>
                RunCaseAsync(item, index, settings, testMethodName, identityFactory, executeAsync, limiter));

            await Task.WhenAll(tasks);
        }

        private static async Task RunCaseAsync<TCase>(
            TCase item,
            int index,
            TestSettings settings,
            string testMethodName,
            Func<TCase, int, string> identityFactory,
            Func<IPage, TCase, Task> executeAsync,
            SemaphoreSlim limiter)
        {
            await limiter.WaitAsync();

            string identity = identityFactory(item, index);

            try
            {
                await using ParallelTestSession session =
                    await ParallelTestSession.StartAsync(settings, $"{testMethodName}_{identity}");

                await executeAsync(session.Page, item);
            }
            finally
            {
                limiter.Release();
            }
        }
    }
}
