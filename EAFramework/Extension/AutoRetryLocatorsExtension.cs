using EAFramework.AIHealing;
using Microsoft.Playwright;

namespace EAFramework.Extension
{
    /// <summary>
    /// Auto-retry locator actions with AI self-healing for Dynamics 365, SAP-style, and other ERP UIs.
    /// Additive extension — does not change existing ClickExAsync / FillExAsync behavior.
    /// </summary>
    public static class AutoRetryLocatorsExtension
    {
        private static readonly string[] ErpBlockingOverlaySelectors =
        {
            ".loadingSpinner",
            ".spinner",
            ".ms-Overlay",
            ".blockOverlay",
            ".modal-backdrop",
            "[data-id='loading-spinner']",
            "[role='progressbar']",
            "[aria-busy='true']",
            ".fa-spinner",
            ".k-loading-mask",
            ".busy-indicator"
        };

        #region ===== OPTIONS =====

        public sealed class AutoRetryLocatorOptions
        {
            public int MaxAttempts { get; set; } = 3;

            public int DelayBetweenAttemptsMs { get; set; } = 1000;

            public int ActionTimeoutMs { get; set; } = 60000;

            public bool WaitForErpOverlayBeforeAttempt { get; set; } = true;

            public int ErpOverlayTimeoutMs { get; set; } = 60000;

            /// <summary>Short settle delay after overlay clears (ERP forms often re-render).</summary>
            public int PostOverlaySettleMs { get; set; } = 300;

            public bool ReHealLocatorOnRetry { get; set; } = true;

            public bool ClearBadHealMappingOnFailure { get; set; } = true;

            public bool UseDynamicsClickFallback { get; set; } = true;

            public bool UseDynamicsFillFallback { get; set; } = true;
        }

        public static AutoRetryLocatorOptions DynamicsDefaults() => new()
        {
            MaxAttempts = 4,
            DelayBetweenAttemptsMs = 1500,
            ActionTimeoutMs = 90000,
            WaitForErpOverlayBeforeAttempt = true,
            ErpOverlayTimeoutMs = 90000,
            PostOverlaySettleMs = 500,
            ReHealLocatorOnRetry = true,
            ClearBadHealMappingOnFailure = true,
            UseDynamicsClickFallback = true,
            UseDynamicsFillFallback = true
        };

        public static AutoRetryLocatorOptions StandardDefaults() => new();

        #endregion

        #region ===== ERP READY =====

        public static async Task WaitForErpReadyAsync(
            this IPage page,
            int overlayTimeoutMs = 60000,
            int postSettleMs = 300)
        {
            await page.WaitForErpOverlaysHiddenAsync(overlayTimeoutMs);

            if (postSettleMs > 0)
            {
                await page.WaitForTimeoutAsync(postSettleMs);
            }
        }

        public static async Task WaitForErpOverlaysHiddenAsync(
            this IPage page,
            int timeoutMs = 60000)
        {
            foreach (string selector in ErpBlockingOverlaySelectors)
            {
                ILocator overlay = page.Locator(selector);

                try
                {
                    if (await overlay.CountAsync() == 0)
                    {
                        continue;
                    }

                    await overlay.First.WaitForAsync(new()
                    {
                        State = WaitForSelectorState.Hidden,
                        Timeout = timeoutMs
                    });
                }
                catch
                {
                    // Overlay selector not present on this ERP skin — continue.
                }
            }
        }

        #endregion

        #region ===== AUTO RETRY — STRING SELECTOR =====

        public static Task AutoRetryClickAsync(
            this IPage page,
            string selector,
            AutoRetryLocatorOptions? options = null) =>
            page.Locator(selector).AutoRetryClickAsync(options);

        public static Task AutoRetryFillAsync(
            this IPage page,
            string selector,
            string value,
            AutoRetryLocatorOptions? options = null) =>
            page.Locator(selector).AutoRetryFillAsync(value, options);

        public static Task AutoRetryDynamicsClickAsync(
            this IPage page,
            string selector,
            AutoRetryLocatorOptions? options = null) =>
            page.Locator(selector).AutoRetryDynamicsClickAsync(options);

        public static Task AutoRetryDynamicsFillAsync(
            this IPage page,
            string selector,
            string value,
            AutoRetryLocatorOptions? options = null) =>
            page.Locator(selector).AutoRetryDynamicsFillAsync(value, options ?? DynamicsDefaults());

        #endregion

        #region ===== AUTO RETRY — ILOCATOR =====

        public static async Task AutoRetryClickAsync(
            this ILocator locator,
            AutoRetryLocatorOptions? options = null)
        {
            options ??= StandardDefaults();

            await AutoRetryLocatorExecutor.ExecuteAsync(
                locator,
                options,
                async (target, opt) =>
                {
                    await target.ClickExAsync(opt.ActionTimeoutMs);
                });
        }

        public static async Task AutoRetryFillAsync(
            this ILocator locator,
            string value,
            AutoRetryLocatorOptions? options = null)
        {
            options ??= StandardDefaults();

            await AutoRetryLocatorExecutor.ExecuteAsync(
                locator,
                options,
                async (target, opt) =>
                {
                    await target.FillExAsync(value, opt.ActionTimeoutMs);
                });
        }

        public static async Task AutoRetryDynamicsClickAsync(
            this ILocator locator,
            AutoRetryLocatorOptions? options = null)
        {
            options ??= DynamicsDefaults();

            await AutoRetryLocatorExecutor.ExecuteAsync(
                locator,
                options,
                async (target, opt) =>
                {
                    await target.DynamicsClickAsync(opt.ActionTimeoutMs);
                });
        }

        public static async Task AutoRetryDynamicsFillAsync(
            this ILocator locator,
            string value,
            AutoRetryLocatorOptions? options = null)
        {
            options ??= DynamicsDefaults();

            await AutoRetryLocatorExecutor.ExecuteAsync(
                locator,
                options,
                async (target, opt) =>
                {
                    if (opt.UseDynamicsFillFallback)
                    {
                        await target.DynamicsFillAsync(value, opt.ActionTimeoutMs);
                    }
                    else
                    {
                        await target.FillExAsync(value, opt.ActionTimeoutMs);
                    }
                });
        }

        public static async Task AutoRetrySelectByTextAsync(
            this ILocator locator,
            string visibleText,
            AutoRetryLocatorOptions? options = null)
        {
            options ??= DynamicsDefaults();

            await AutoRetryLocatorExecutor.ExecuteAsync(
                locator,
                options,
                async (target, _) =>
                {
                    await target.SelectByTextAsync(visibleText);
                });
        }

        public static async Task<ILocator> AutoRetryResolveAsync(
            this ILocator locator,
            AutoRetryLocatorOptions? options = null)
        {
            options ??= StandardDefaults();
            IPage page = locator.Page;

            if (options.WaitForErpOverlayBeforeAttempt)
            {
                await page.WaitForErpReadyAsync(
                    options.ErpOverlayTimeoutMs,
                    options.PostOverlaySettleMs);
            }

            return await LocatorHealingResolver.ResolveForRetryAsync(
                locator,
                forceReHeal: true);
        }

        #endregion
    }

    /// <summary>
    /// Executes locator actions with retry, ERP overlay waits, and optional re-healing.
    /// </summary>
    internal static class AutoRetryLocatorExecutor
    {
        public static async Task ExecuteAsync(
            ILocator locator,
            AutoRetryLocatorsExtension.AutoRetryLocatorOptions options,
            Func<ILocator, AutoRetryLocatorsExtension.AutoRetryLocatorOptions, Task> action)
        {
            IPage page = locator.Page;
            Exception? lastException = null;
            string? selectorPath = LocatorSelectorHelper.TryGetSelector(locator);

            for (int attempt = 1; attempt <= options.MaxAttempts; attempt++)
            {
                try
                {
                    if (options.WaitForErpOverlayBeforeAttempt)
                    {
                        await page.WaitForErpReadyAsync(
                            options.ErpOverlayTimeoutMs,
                            options.PostOverlaySettleMs);
                    }

                    bool forceReHeal = options.ReHealLocatorOnRetry && attempt > 1;

                    ILocator target = await LocatorHealingResolver.ResolveForRetryAsync(
                        locator,
                        forceReHeal);

                    using (HealingExecutionContext.EnterSkipScope())
                    {
                        await action(target, options);
                    }

                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (options.ClearBadHealMappingOnFailure
                        && !string.IsNullOrWhiteSpace(selectorPath))
                    {
                        HealingEngineCache
                            .Get(page)
                            .RemoveHealedMapping(selectorPath);
                    }

                    if (attempt >= options.MaxAttempts)
                    {
                        break;
                    }

                    await page.WaitForTimeoutAsync(options.DelayBetweenAttemptsMs);
                }
            }

            throw new InvalidOperationException(
                $"Auto-retry failed after {options.MaxAttempts} attempt(s) "
                + $"for locator '{selectorPath ?? locator.ToString()}'.",
                lastException);
        }
    }
}
