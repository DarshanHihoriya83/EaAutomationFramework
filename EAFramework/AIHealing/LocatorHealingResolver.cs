using Microsoft.Playwright;

namespace EAFramework.AIHealing
{
    /// <summary>
    /// Central AI self-healing entry for all framework locator actions.
    /// </summary>
    public static class LocatorHealingResolver
    {
        public static async Task<ILocator> ResolveAsync(ILocator locator)
        {
            if (HealingExecutionContext.ShouldSkipHealing)
            {
                return locator;
            }

            string? selector = LocatorSelectorHelper.TryGetSelector(locator);

            if (string.IsNullOrWhiteSpace(selector))
            {
                return locator;
            }

            return await HealingEngineCache
                .Get(locator.Page)
                .FindElementAsync(selector);
        }

        /// <summary>
        /// Resolves a locator for auto-retry attempts. When <paramref name="forceReHeal"/> is true,
        /// runs a fresh heal pass (still non-breaking for normal <see cref="ResolveAsync"/> calls).
        /// </summary>
        public static async Task<ILocator> ResolveForRetryAsync(
            ILocator locator,
            bool forceReHeal = false)
        {
            if (HealingExecutionContext.ShouldSkipHealing && !forceReHeal)
            {
                return locator;
            }

            string? selector = LocatorSelectorHelper.TryGetSelector(locator);

            if (string.IsNullOrWhiteSpace(selector))
            {
                return locator;
            }

            SelfHealingEngine engine = HealingEngineCache.Get(locator.Page);

            if (!forceReHeal)
            {
                return await engine.FindElementAsync(selector);
            }

            ILocator? resolved = await engine.TryFindElementAsync(selector);

            if (resolved != null)
            {
                return resolved;
            }

            return await engine.FindElementAsync(selector);
        }

        public static async Task RunAsync(
            ILocator locator,
            Func<ILocator, Task> action)
        {
            ILocator resolved = await ResolveAsync(locator);

            using (HealingExecutionContext.EnterSkipScope())
            {
                await action(resolved);
            }
        }

        public static async Task<T> RunAsync<T>(
            ILocator locator,
            Func<ILocator, Task<T>> action)
        {
            ILocator resolved = await ResolveAsync(locator);

            using (HealingExecutionContext.EnterSkipScope())
            {
                return await action(resolved);
            }
        }

        public static async Task WaitFirstThenHealAsync(
            ILocator locator,
            WaitForSelectorState state,
            int timeout)
        {
            try
            {
                await locator.First.WaitForAsync(new()
                {
                    State = state,
                    Timeout = timeout
                });
            }
            catch (TimeoutException)
            {
                ILocator healed = await ResolveAsync(locator);

                using (HealingExecutionContext.EnterSkipScope())
                {
                    await healed.First.WaitForAsync(new()
                    {
                        State = state,
                        Timeout = timeout
                    });
                }
            }
        }
    }
}
