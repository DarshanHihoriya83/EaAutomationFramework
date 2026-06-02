using EAFramework.AIHealing;
using Microsoft.Playwright;

namespace EAFramework.Extension
{
    public static class ScrollExtension
    {
        #region ===== SCROLL INTO VIEW =====

        public static async Task ScrollIntoViewExAsync(
            this ILocator locator)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);

            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Attached
            });

            await locator.ScrollIntoViewIfNeededAsync();
        }

        #endregion

        #region ===== SCROLL TO TOP =====

        public static async Task ScrollToTopAsync(
            this IPage page)
        {
            await page.EvaluateAsync(
                @"() =>
                {
                    window.scrollTo(0, 0);
                }");
        }

        #endregion

        #region ===== SCROLL TO BOTTOM =====

        public static async Task ScrollToBottomAsync(
            this IPage page)
        {
            await page.EvaluateAsync(
                @"() =>
                {
                    window.scrollTo(
                        0,
                        document.body.scrollHeight);
                }");
        }

        #endregion

        #region ===== SCROLL BY PIXELS =====

        public static async Task ScrollByAsync(
            this IPage page,
            int xPixels,
            int yPixels)
        {
            await page.EvaluateAsync(
                @"([x, y]) =>
                {
                    window.scrollBy(x, y);
                }",
                new[] { xPixels, yPixels });
        }

        #endregion

        #region ===== SCROLL HORIZONTAL =====

        public static async Task ScrollHorizontalAsync(
            this ILocator locator,
            int scrollAmount)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);
            var page = locator.Page;

            var elementHandle =
                await locator.ElementHandleAsync();

            if (elementHandle != null)
            {
                await page.EvaluateAsync(
                    @"([element, amount]) =>
                    {
                        element.scrollLeft += amount;
                    }",
                    new object[]
                    {
                        elementHandle,
                        scrollAmount
                    });
            }
        }

        #endregion

        #region ===== SCROLL VERTICAL =====

        public static async Task ScrollVerticalAsync(
            this ILocator locator,
            int scrollAmount)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);
            var page = locator.Page;

            var elementHandle =
                await locator.ElementHandleAsync();

            if (elementHandle != null)
            {
                await page.EvaluateAsync(
                    @"([element, amount]) =>
                    {
                        element.scrollTop += amount;
                    }",
                    new object[]
                    {
                        elementHandle,
                        scrollAmount
                    });
            }
        }

        #endregion

        #region ===== SCROLL UNTIL ELEMENT VISIBLE =====

        public static async Task ScrollUntilVisibleAsync(
            this IPage page,
            string selector,
            int maxScrollAttempts = 20)
        {
            var locator = page.Locator(selector);

            for (int attempt = 0;
                 attempt < maxScrollAttempts;
                 attempt++)
            {
                if (await locator.IsVisibleAsync())
                {
                    return;
                }

                await page.Mouse.WheelAsync(0, 1000);

                await page.WaitForTimeoutAsync(500);
            }

            throw new Exception(
                $"Element not visible after scrolling: {selector}");
        }

        #endregion

        #region ===== SCROLL USING MOUSE WHEEL =====

        public static async Task MouseWheelScrollAsync(
            this IPage page,
            int deltaX,
            int deltaY)
        {
            await page.Mouse.WheelAsync(deltaX, deltaY);
        }

        #endregion

        #region ===== SCROLL INSIDE GRID =====

        public static async Task ScrollGridAsync(
            this ILocator grid,
            int scrollAmount = 1000)
        {
            await grid.ScrollVerticalAsync(scrollAmount);
        }

        #endregion

        #region ===== DYNAMICS 365 FORM SCROLL =====

        public static async Task ScrollDynamicsFormAsync(
            this IPage page)
        {
            var formContainer = page
                .Locator("[data-id='form-container']");

            if (await formContainer.CountAsync() > 0)
            {
                await formContainer.ScrollVerticalAsync(1500);
            }
            else
            {
                await page.ScrollByAsync(0, 1500);
            }
        }

        #endregion

        #region ===== SCROLL TO ELEMENT AND CLICK =====

        public static async Task ScrollAndClickAsync(
            this ILocator locator)
        {
            await locator.ScrollIntoViewExAsync();

            await locator.ClickAsync();
        }

        #endregion

        #region ===== SCROLL TO ELEMENT AND FILL =====

        public static async Task ScrollAndFillAsync(
            this ILocator locator,
            string value)
        {
            await locator.ScrollIntoViewExAsync();

            await locator.FillAsync(value);
        }

        #endregion

        #region ===== INFINITE SCROLL =====

        public static async Task InfiniteScrollAsync(
            this IPage page,
            int scrollCount = 10)
        {
            for (int i = 0; i < scrollCount; i++)
            {
                await page.Mouse.WheelAsync(0, 2000);

                await page.WaitForTimeoutAsync(1000);
            }
        }

        #endregion

        #region ===== SCROLL UNTIL TEXT FOUND =====

        public static async Task ScrollUntilTextVisibleAsync(
            this IPage page,
            string text,
            int maxScrollAttempts = 20)
        {
            var locator = page.GetByText(text);

            for (int attempt = 0;
                 attempt < maxScrollAttempts;
                 attempt++)
            {
                if (await locator.IsVisibleAsync())
                {
                    return;
                }

                await page.Mouse.WheelAsync(0, 1000);

                await page.WaitForTimeoutAsync(500);
            }

            throw new Exception(
                $"Text not found after scrolling: {text}");
        }

        #endregion

        #region ===== SCROLL TO LAST GRID ROW =====

        public static async Task ScrollToLastGridRowAsync(
            this ILocator table)
        {
            var lastRow = table
                .Locator("tr")
                .Last;

            await lastRow.ScrollIntoViewExAsync();
        }

        #endregion
    }
}