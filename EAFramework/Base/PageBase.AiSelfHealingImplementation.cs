using EAFramework.AIHealing;
using Microsoft.Playwright;

namespace EAFramework.Base
{
    public partial class PageBase
    {
        #region ===== AI SELF-HEALING IMPLEMENTATION =====

        /// <summary>Resolves a string selector through <see cref="SelfHealingEngine"/>.</summary>
        protected Task<ILocator> AiResolveAsync(string selector) =>
            _healingEngine.FindElementAsync(selector);

        /// <summary>Resolves a chained <see cref="ILocator"/> through <see cref="SelfHealingEngine"/>.</summary>
        protected Task<ILocator> AiResolveAsync(ILocator locator) =>
            LocatorHealingResolver.ResolveAsync(locator);

        /// <summary>Highlight + action on a healed string selector.</summary>
        protected async Task AiExecuteAsync(
            string selector,
            Func<ILocator, Task> action)
        {
            ILocator resolved = await AiResolveAsync(selector);
            await HighlightElementAsync(resolved);

            using (HealingExecutionContext.EnterSkipScope())
            {
                await action(resolved);
            }
        }

        /// <summary>Highlight + action on a healed chained locator.</summary>
        protected async Task AiExecuteAsync(
            ILocator locator,
            Func<ILocator, Task> action)
        {
            ILocator resolved = await AiResolveAsync(locator);
            await HighlightElementAsync(resolved);

            using (HealingExecutionContext.EnterSkipScope())
            {
                await action(resolved);
            }
        }

        /// <summary>Action on a healed string selector (no highlight).</summary>
        protected async Task AiExecuteAsync(
            string selector,
            Func<ILocator, Task> action,
            bool highlight)
        {
            if (highlight)
            {
                await AiExecuteAsync(selector, action);
                return;
            }

            ILocator resolved = await AiResolveAsync(selector);

            using (HealingExecutionContext.EnterSkipScope())
            {
                await action(resolved);
            }
        }

        /// <summary>Action on a healed chained locator (no highlight).</summary>
        protected async Task AiExecuteAsync(
            ILocator locator,
            Func<ILocator, Task> action,
            bool highlight)
        {
            if (highlight)
            {
                await AiExecuteAsync(locator, action);
                return;
            }

            ILocator resolved = await AiResolveAsync(locator);

            using (HealingExecutionContext.EnterSkipScope())
            {
                await action(resolved);
            }
        }

        /// <summary>Func returning a value on a healed string selector.</summary>
        protected async Task<T> AiExecuteAsync<T>(
            string selector,
            Func<ILocator, Task<T>> action)
        {
            ILocator resolved = await AiResolveAsync(selector);

            using (HealingExecutionContext.EnterSkipScope())
            {
                return await action(resolved);
            }
        }

        /// <summary>Func returning a value on a healed chained locator.</summary>
        protected async Task<T> AiExecuteAsync<T>(
            ILocator locator,
            Func<ILocator, Task<T>> action)
        {
            ILocator resolved = await AiResolveAsync(locator);

            using (HealingExecutionContext.EnterSkipScope())
            {
                return await action(resolved);
            }
        }

        /// <summary>Func returning a value on a healed string selector (optional highlight).</summary>
        protected async Task<T> AiExecuteAsync<T>(
            string selector,
            Func<ILocator, Task<T>> action,
            bool highlight)
        {
            ILocator resolved = await AiResolveAsync(selector);

            if (highlight)
            {
                await HighlightElementAsync(resolved);
            }

            using (HealingExecutionContext.EnterSkipScope())
            {
                return await action(resolved);
            }
        }

        /// <summary>Func returning a value on a healed chained locator (optional highlight).</summary>
        protected async Task<T> AiExecuteAsync<T>(
            ILocator locator,
            Func<ILocator, Task<T>> action,
            bool highlight)
        {
            ILocator resolved = await AiResolveAsync(locator);

            if (highlight)
            {
                await HighlightElementAsync(resolved);
            }

            using (HealingExecutionContext.EnterSkipScope())
            {
                return await action(resolved);
            }
        }

        /// <summary>
        /// Waits for the locator state first; runs AI healing only if the wait times out.
        /// </summary>
        protected Task AiWaitForAsync(
            string selector,
            WaitForSelectorState state,
            int timeout) =>
            LocatorHealingResolver.WaitFirstThenHealAsync(
                _page.Locator(selector),
                state,
                timeout);

        /// <summary>
        /// Waits on a chained locator first; runs AI healing only if the wait times out.
        /// </summary>
        protected Task AiWaitForAsync(
            ILocator locator,
            WaitForSelectorState state,
            int timeout) =>
            LocatorHealingResolver.WaitFirstThenHealAsync(locator, state, timeout);

        #endregion
    }
}
