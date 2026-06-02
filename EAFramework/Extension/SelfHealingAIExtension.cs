using EAFramework.AIHealing;
using Microsoft.Playwright;

namespace EAFramework.Extension
{
    /// <summary>
    /// AI self-healing extensions for <see cref="IPage"/> and chained <see cref="ILocator"/> objects.
    /// Resolves Playwright selector chains, applies healing strategies, and manages the healing store.
    /// </summary>
    public static class SelfHealingAIExtension
    {
        #region ===== ENGINE =====

        public static SelfHealingEngine GetHealingEngine(this IPage page) =>
            HealingEngineCache.Get(page);

        public static AIHealingService GetAIHealingService(this IPage page) =>
            new AIHealingService(page);

        #endregion

        #region ===== SELECTOR =====

        public static string? GetLocatorSelector(this ILocator locator) =>
            LocatorSelectorHelper.TryGetSelector(locator);

        #endregion

        #region ===== RESOLVE =====

        public static Task<ILocator> FindWithHealingAsync(
            this IPage page,
            string selector) =>
            page.GetHealingEngine().FindElementAsync(selector);

        public static Task<ILocator> ResolveWithHealingAsync(this ILocator locator) =>
            LocatorHealingResolver.ResolveAsync(locator);

        public static async Task<ILocator> HealingLocatorAsync(
            this IPage page,
            string selector) =>
            await page.FindWithHealingAsync(selector);

        #endregion

        #region ===== CLICK =====

        public static async Task HealingClickAsync(
            this IPage page,
            string selector,
            int timeout = 30000)
        {
            ILocator locator = await page.FindWithHealingAsync(selector);
            await locator.ClickExAsync(timeout);
        }

        public static Task HealingClickExAsync(
            this ILocator locator,
            int timeout = 30000) =>
            locator.ClickExAsync(timeout);

        public static async Task HealingForceClickAsync(
            this ILocator locator,
            int timeout = 30000)
        {
            locator = await locator.ResolveWithHealingAsync();
            await locator.ForceClickAsync(timeout);
        }

        #endregion

        #region ===== FILL =====

        public static async Task HealingFillAsync(
            this IPage page,
            string selector,
            string value,
            int timeout = 30000)
        {
            ILocator locator = await page.FindWithHealingAsync(selector);
            await locator.FillExAsync(value, timeout);
        }

        public static Task HealingFillExAsync(
            this ILocator locator,
            string value,
            int timeout = 30000) =>
            locator.FillExAsync(value, timeout);

        public static async Task HealingClearAndFillAsync(
            this ILocator locator,
            string value)
        {
            locator = await locator.ResolveWithHealingAsync();
            await locator.ClearAndFillAsync(value);
        }

        #endregion

        #region ===== WAIT =====

        public static async Task WaitForVisibleWithHealingAsync(
            this ILocator locator,
            int timeout = 30000)
        {
            locator = await locator.ResolveWithHealingAsync();

            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeout
            });
        }

        public static async Task<bool> IsVisibleWithHealingAsync(
            this ILocator locator,
            int timeout = 5000)
        {
            try
            {
                await locator.WaitForVisibleWithHealingAsync(timeout);
                return await locator.IsVisibleAsync();
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region ===== STORE & REPORT =====

        public static IReadOnlyDictionary<string, string> GetHealedMappings(this IPage page) =>
            page.GetHealingEngine().GetHealedMappings();

        public static int GetHealingCount(this IPage page) =>
            page.GetHealedMappings().Count;

        public static void RemoveHealedMapping(this IPage page, string originalSelector) =>
            page.GetHealingEngine().RemoveHealedMapping(originalSelector);

        public static void ClearHealingStore(this IPage page) =>
            page.GetHealingEngine().ClearHealedMappings();

        public static void ExportHealingReport(this IPage page, string? reportPath = null)
        {
            reportPath ??= HealingPaths.ResolveAutoHealReportPath();
            page.GetHealingEngine().ExportHealingReport(reportPath);
        }

        public static async Task<bool> VerifyHealingAsync(
            this IPage page,
            string selector)
        {
            ILocator locator = await page.FindWithHealingAsync(selector);
            return await locator.CountAsync() > 0;
        }

        #endregion

        #region ===== AUTO RETRY (DELEGATES) =====

        public static Task AutoRetryClickWithHealingAsync(
            this ILocator locator,
            AutoRetryLocatorsExtension.AutoRetryLocatorOptions? options = null) =>
            locator.AutoRetryClickAsync(options);

        public static Task AutoRetryDynamicsClickWithHealingAsync(
            this ILocator locator) =>
            locator.AutoRetryDynamicsClickAsync(
                AutoRetryLocatorsExtension.DynamicsDefaults());

        #endregion
    }
}
