using EAFramework.AIHealing;
using Microsoft.Playwright;

namespace EAFramework.Extension
{
    public static class WaitExtension
    {
        #region ===== PAGE LOAD WAIT =====

        public static async Task WaitForPageLoadAsync(
            this IPage page)
        {
            await page.WaitForLoadStateAsync(
                LoadState.DOMContentLoaded);
        }

        #endregion

        #region ===== NETWORK IDLE WAIT =====

        public static async Task WaitForNetworkIdleAsync(
            this IPage page)
        {
            await page.WaitForLoadStateAsync(
                LoadState.NetworkIdle);
        }

        #endregion

        #region ===== WAIT FOR TIMEOUT =====

        public static async Task WaitAsync(
            this IPage page,
            int milliseconds)
        {
            await page.WaitForTimeoutAsync(milliseconds);
        }

        #endregion

        #region ===== WAIT FOR ELEMENT VISIBLE =====

        public static Task WaitForVisibleAsync(
            this ILocator locator,
            int timeout = 30000) =>
            LocatorHealingResolver.WaitFirstThenHealAsync(
                locator,
                WaitForSelectorState.Visible,
                timeout);

        #endregion

        #region ===== WAIT FOR ELEMENT HIDDEN =====

        public static Task WaitForHiddenAsync(
            this ILocator locator,
            int timeout = 30000) =>
            LocatorHealingResolver.WaitFirstThenHealAsync(
                locator,
                WaitForSelectorState.Hidden,
                timeout);

        #endregion

        #region ===== WAIT FOR ELEMENT ATTACHED =====

        public static Task WaitForAttachedAsync(
            this ILocator locator,
            int timeout = 30000) =>
            LocatorHealingResolver.WaitFirstThenHealAsync(
                locator,
                WaitForSelectorState.Attached,
                timeout);

        #endregion

        #region ===== WAIT FOR ELEMENT DETACHED =====

        public static Task WaitForDetachedAsync(
            this ILocator locator,
            int timeout = 30000) =>
            LocatorHealingResolver.WaitFirstThenHealAsync(
                locator,
                WaitForSelectorState.Detached,
                timeout);

        #endregion

        #region ===== WAIT FOR ENABLED =====

        public static Task WaitForEnabledAsync(
            this ILocator locator,
            int timeout = 30000) =>
            LocatorHealingResolver.RunAsync(locator, async resolved =>
            {
                for (int second = 0; second < timeout / 1000; second++)
                {
                    if (await resolved.IsEnabledAsync())
                    {
                        return;
                    }

                    await Task.Delay(1000);
                }

                throw new TimeoutException(
                    "Element not enabled within timeout.");
            });

        #endregion

        #region ===== WAIT FOR DISABLED =====

        public static async Task WaitForDisabledAsync(
            this ILocator locator,
            int timeout = 30000)
        {
            for (int second = 0;
                 second < timeout / 1000;
                 second++)
            {
                if (await locator.IsDisabledAsync())
                {
                    return;
                }

                await Task.Delay(1000);
            }

            throw new TimeoutException(
                "Element not disabled within timeout.");
        }

        #endregion

        #region ===== WAIT FOR TEXT =====

        public static async Task WaitForTextAsync(
            this ILocator locator,
            string expectedText,
            int timeout = 30000)
        {
            for (int second = 0;
                 second < timeout / 1000;
                 second++)
            {
                var actualText =
                    await locator.InnerTextAsync();

                if (actualText.Contains(expectedText))
                {
                    return;
                }

                await Task.Delay(1000);
            }

            throw new TimeoutException(
                $"Expected text '{expectedText}' not found.");
        }

        #endregion

        #region ===== WAIT FOR URL =====

        public static async Task WaitForUrlContainsAsync(
            this IPage page,
            string partialUrl,
            int timeout = 30000)
        {
            for (int second = 0;
                 second < timeout / 1000;
                 second++)
            {
                if (page.Url.Contains(partialUrl))
                {
                    return;
                }

                await Task.Delay(1000);
            }

            throw new TimeoutException(
                $"URL does not contain '{partialUrl}'.");
        }

        #endregion

        #region ===== WAIT FOR TITLE =====

        public static async Task WaitForTitleAsync(
            this IPage page,
            string expectedTitle,
            int timeout = 30000)
        {
            for (int second = 0;
                 second < timeout / 1000;
                 second++)
            {
                var title = await page.TitleAsync();

                if (title.Contains(expectedTitle))
                {
                    return;
                }

                await Task.Delay(1000);
            }

            throw new TimeoutException(
                $"Page title does not contain '{expectedTitle}'.");
        }

        #endregion

        #region ===== DYNAMICS SPINNER WAIT =====

        public static async Task WaitForDynamicsSpinnerAsync(
            this IPage page,
            int timeout = 60000)
        {
            var spinnerLocators = new List<string>
            {
                ".loadingSpinner",
                ".spinner",
                "[data-id='loading-spinner']",
                "[role='progressbar']"
            };

            foreach (var spinnerSelector in spinnerLocators)
            {
                var spinner = page.Locator(spinnerSelector);

                try
                {
                    if (await spinner.CountAsync() > 0)
                    {
                        await spinner.WaitForAsync(new()
                        {
                            State = WaitForSelectorState.Hidden,
                            Timeout = timeout
                        });
                    }
                }
                catch
                {
                }
            }
        }

        #endregion

        #region ===== WAIT FOR FRAME =====

        public static async Task<IFrame?> WaitForFrameAsync(
            this IPage page,
            string frameName,
            int timeout = 30000)
        {
            for (int second = 0;
                 second < timeout / 1000;
                 second++)
            {
                var frame = page
                    .Frames
                    .FirstOrDefault(f =>
                        f.Name.Contains(frameName));

                if (frame != null)
                {
                    return frame;
                }

                await Task.Delay(1000);
            }

            throw new TimeoutException(
                $"Frame not found: {frameName}");
        }

        #endregion

        #region ===== WAIT FOR GRID LOAD =====

        public static async Task WaitForGridLoadAsync(
            this ILocator grid,
            int timeout = 30000)
        {
            await grid.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeout
            });

            await Task.Delay(2000);
        }

        #endregion

        #region ===== WAIT FOR ELEMENT COUNT =====

        public static async Task WaitForElementCountAsync(
            this ILocator locator,
            int expectedCount,
            int timeout = 30000)
        {
            for (int second = 0;
                 second < timeout / 1000;
                 second++)
            {
                int actualCount =
                    await locator.CountAsync();

                if (actualCount == expectedCount)
                {
                    return;
                }

                await Task.Delay(1000);
            }

            throw new TimeoutException(
                $"Expected count {expectedCount} not matched.");
        }

        #endregion

        #region ===== SMART WAIT =====

        public static async Task SmartWaitAsync(
            this IPage page)
        {
            await page.WaitForPageLoadAsync();

            await page.WaitForNetworkIdleAsync();

            await page.WaitForDynamicsSpinnerAsync();
        }

        #endregion

        #region ===== WAIT UNTIL CLICKABLE =====

        public static async Task WaitUntilClickableAsync(
            this ILocator locator,
            int timeout = 30000)
        {
            await locator.WaitForVisibleAsync(timeout);

            await locator.WaitForEnabledAsync(timeout);
        }

        #endregion

        #region ===== WAIT FOR LOOKUP SEARCH =====

        public static async Task WaitForLookupResultsAsync(
            this IPage page,
            int timeout = 30000)
        {
            var results = page
                .Locator("[role='listbox']");

            await results.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeout
            });
        }

        #endregion
    }
}