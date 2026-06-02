namespace EAFramework.AIHealing
{
    /// <summary>
    /// Prevents double AI healing when <see cref="PageBase"/> already resolved a locator.
    /// </summary>
    internal static class HealingExecutionContext
    {
        private static readonly AsyncLocal<int> Depth = new();

        public static bool ShouldSkipHealing => Depth.Value > 0;

        public static IDisposable EnterSkipScope()
        {
            Depth.Value++;
            return new SkipScope();
        }

        private sealed class SkipScope : IDisposable
        {
            public void Dispose()
            {
                if (Depth.Value > 0)
                {
                    Depth.Value--;
                }
            }
        }
    }
}
