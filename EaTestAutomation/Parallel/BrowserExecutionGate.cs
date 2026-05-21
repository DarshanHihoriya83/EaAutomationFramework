using EAFramework.Config;
using EaFramework.Config;

namespace EaTestAutomation.Parallel
{
    /// <summary>
    /// Limits how many Playwright browser sessions run at once (from <c>MaxParallelBrowsers</c>).
    /// </summary>
    public static class BrowserExecutionGate
    {
        private static readonly Lazy<GateState> State = new(() =>
        {
            TestSettings settings = ConfigReader.ReadConfig();
            int limit = settings.EnableParallelExecution
                ? Math.Max(1, settings.MaxParallelBrowsers)
                : 1;

            return new GateState(new SemaphoreSlim(limit, limit), limit);
        });

        public static int MaxConcurrentBrowsers => State.Value.Limit;

        public static void Acquire()
        {
            State.Value.Semaphore.Wait();
        }

        public static void Release()
        {
            try
            {
                State.Value.Semaphore.Release();
            }
            catch (SemaphoreFullException)
            {
                // ignore imbalanced release during faulted teardown
            }
        }

        private sealed class GateState
        {
            public GateState(SemaphoreSlim semaphore, int limit)
            {
                Semaphore = semaphore;
                Limit = limit;
            }

            public SemaphoreSlim Semaphore { get; }
            public int Limit { get; }
        }
    }
}
