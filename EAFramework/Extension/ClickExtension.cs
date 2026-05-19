using EAFramework.AIHealing;
using Microsoft.Playwright;

namespace EAFramework.Extension
{
    public static class ClickExtension
    {
        #region ===== NORMAL CLICK =====

        public static async Task ClickExAsync(
            this ILocator locator,
            int timeout = 30000)
        {
            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeout
            });

            await locator.ScrollIntoViewIfNeededAsync();

            await locator.ClickAsync(new()
            {
                Timeout = timeout
            });
        }

        #endregion

        #region ===== FORCE CLICK =====

        public static async Task ForceClickAsync(
            this ILocator locator,
            int timeout = 30000)
        {
            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeout
            });

            await locator.ScrollIntoViewIfNeededAsync();

            await locator.ClickAsync(new()
            {
                Force = true,
                Timeout = timeout
            });
        }

        #endregion

        #region ===== JAVASCRIPT CLICK =====

        public static async Task JsClickAsync(
            this ILocator locator)
        {
            var page = locator.Page;

            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Attached
            });

            var elementHandle = await locator.ElementHandleAsync();

            if (elementHandle != null)
            {
                await page.EvaluateAsync(
                    "(element) => element.click()",
                    elementHandle);
            }
        }

        #endregion

        #region ===== RETRY CLICK =====

        public static async Task RetryClickAsync(
            this ILocator locator,
            int retryCount = 3,
            int delayMilliseconds = 1000)
        {
            Exception? lastException = null;

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    await locator.ClickExAsync();
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    await Task.Delay(delayMilliseconds);
                }
            }

            throw new Exception(
                $"Retry click failed after {retryCount} attempts.",
                lastException);
        }

        #endregion

        #region ===== AI SELF HEALING CLICK =====

        public static async Task HealingClickAsync(
            this IPage page,
            string selector,
            int timeout = 30000)
        {
            var healingEngine = new SelfHealingEngine(page);

            var locator = await healingEngine
                .FindElementAsync(selector);

            await locator.ClickExAsync(timeout);
        }

        #endregion

        #region ===== DYNAMICS 365 CLICK =====

        public static async Task DynamicsClickAsync(
            this ILocator locator,
            int timeout = 60000)
        {
            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeout
            });

            await locator.ScrollIntoViewIfNeededAsync();

            try
            {
                await locator.ClickAsync(new()
                {
                    Timeout = timeout
                });
            }
            catch
            {
                try
                {
                    await locator.ClickAsync(new()
                    {
                        Force = true,
                        Timeout = timeout
                    });
                }
                catch
                {
                    await locator.JsClickAsync();
                }
            }
        }

        #endregion

        #region ===== DOUBLE CLICK =====

        public static async Task DoubleClickExAsync(
            this ILocator locator,
            int timeout = 30000)
        {
            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeout
            });

            await locator.ScrollIntoViewIfNeededAsync();

            await locator.DblClickAsync(new()
            {
                Timeout = timeout
            });
        }

        #endregion

        #region ===== RIGHT CLICK =====

        public static async Task RightClickAsync(
            this ILocator locator,
            int timeout = 30000)
        {
            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeout
            });

            await locator.ScrollIntoViewIfNeededAsync();

            await locator.ClickAsync(new()
            {
                Button = MouseButton.Right,
                Timeout = timeout
            });
        }

        #endregion

        #region ===== CLICK WITH WAIT =====

        public static async Task ClickAndWaitAsync(
            this ILocator locator,
            int waitMilliseconds = 2000)
        {
            await locator.ClickExAsync();

            await Task.Delay(waitMilliseconds);
        }

        #endregion

        #region ===== CLICK IF EXISTS =====

        public static async Task<bool> ClickIfExistsAsync(
            this ILocator locator)
        {
            try
            {
                if (await locator.CountAsync() > 0)
                {
                    if (await locator.IsVisibleAsync())
                    {
                        await locator.ClickExAsync();
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region ===== GRID ROW CLICK =====

        public static async Task ClickGridRowByTextAsync(
            this ILocator table,
            string rowText)
        {
            var row = table
                .Locator("tr")
                .Filter(new()
                {
                    HasText = rowText
                });

            await row.ClickExAsync();
        }

        #endregion

        #region ===== GRID BUTTON CLICK =====

        public static async Task ClickButtonInsideRowAsync(
            this ILocator table,
            string rowText,
            string buttonText)
        {
            var button = table
                .Locator("tr")
                .Filter(new()
                {
                    HasText = rowText
                })
                .Locator("button")
                .Filter(new()
                {
                    HasText = buttonText
                });

            await button.ClickExAsync();
        }

        #endregion

        #region ===== DYNAMICS COMMAND BAR =====

        public static async Task ClickCommandBarButtonAsync(
            this IPage page,
            string buttonName)
        {
            var button = page
                .GetByRole(AriaRole.Button,
                    new() { Name = buttonName });

            await button.DynamicsClickAsync();
        }

        #endregion

        #region ===== TAB CLICK =====

        public static async Task ClickTabAsync(
            this IPage page,
            string tabName)
        {
            var tab = page
                .Locator("[role='tab']")
                .Filter(new()
                {
                    HasText = tabName
                });

            await tab.DynamicsClickAsync();
        }

        #endregion
    }
}