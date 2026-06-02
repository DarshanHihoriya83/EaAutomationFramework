using EAFramework.AIHealing;
using Microsoft.Playwright;

namespace EAFramework.Extension
{
    public static class FillExtension
    {
        #region ===== NORMAL FILL =====

        public static async Task FillExAsync(
            this ILocator locator,
            string value,
            int timeout = 30000)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);

            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeout
            });

            await locator.ScrollIntoViewIfNeededAsync();

            await locator.FillAsync(value, new()
            {
                Timeout = timeout
            });
        }

        #endregion

        #region ===== SLOW FILL =====

        public static async Task SlowFillAsync(
            this ILocator locator,
            string value,
            int delayMilliseconds = 100)
        {
            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible
            });

            await locator.ClearAsync();

            foreach (char character in value)
            {
                await locator.TypeAsync(character.ToString());

                await Task.Delay(delayMilliseconds);
            }
        }

        #endregion

        #region ===== TYPE FILL =====

        public static async Task TypeFillAsync(
            this ILocator locator,
            string value,
            int delayMilliseconds = 50)
        {
            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible
            });

            await locator.ClickAsync();

            await locator.PressAsync("Control+A");

            await locator.PressAsync("Backspace");

            await locator.TypeAsync(value, new()
            {
                Delay = delayMilliseconds
            });
        }

        #endregion

        #region ===== JAVASCRIPT FILL =====

        public static async Task JsFillAsync(
     this ILocator locator,
     string value)
        {
            var page = locator.Page;

            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Attached
            });

            var elementHandle =
                await locator.ElementHandleAsync();

            if (elementHandle != null)
            {
                await page.EvaluateAsync(
                    @"([element, value]) =>
            {
                element.value = value;
            }",
                    new object[]
                    {
                elementHandle,
                value
                    });
            }
        }

        #endregion

        #region ===== CLEAR AND FILL =====

        public static async Task ClearAndFillAsync(
            this ILocator locator,
            string value)
        {
            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible
            });

            await locator.ClickAsync();

            await locator.PressAsync("Control+A");

            await locator.PressAsync("Backspace");

            await locator.FillAsync(value);
        }

        #endregion

        #region ===== RETRY FILL =====

        public static async Task RetryFillAsync(
            this ILocator locator,
            string value,
            int retryCount = 3,
            int delayMilliseconds = 1000)
        {
            Exception? lastException = null;

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    await locator.FillExAsync(value);

                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    await Task.Delay(delayMilliseconds);
                }
            }

            throw new Exception(
                $"Retry fill failed after {retryCount} attempts.",
                lastException);
        }

        #endregion

        #region ===== DYNAMICS 365 FILL =====

        public static async Task DynamicsFillAsync(
            this ILocator locator,
            string value,
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
                await locator.FillAsync(value, new()
                {
                    Timeout = timeout
                });
            }
            catch
            {
                try
                {
                    await locator.TypeFillAsync(value);
                }
                catch
                {
                    await locator.JsFillAsync(value);
                }
            }
        }

        #endregion

        #region ===== FILL AND ENTER =====

        public static async Task FillAndEnterAsync(
            this ILocator locator,
            string value)
        {
            await locator.FillExAsync(value);

            await locator.PressAsync("Enter");
        }

        #endregion

        #region ===== FILL DATE =====

        public static async Task FillDateAsync(
            this ILocator locator,
            DateTime date)
        {
            string formattedDate =
                date.ToString("MM/dd/yyyy");

            await locator.FillExAsync(formattedDate);
        }

        #endregion

        #region ===== FILL NUMBER =====

        public static async Task FillNumberAsync(
            this ILocator locator,
            decimal number)
        {
            await locator.FillExAsync(number.ToString());
        }

        #endregion

        #region ===== SEARCH AND FILL =====

        public static async Task SearchAndFillAsync(
            this IPage page,
            string labelText,
            string value)
        {
            var textbox = page
                .GetByLabel(labelText);

            await textbox.DynamicsFillAsync(value);
        }

        #endregion

        #region ===== GRID CELL FILL =====

        public static async Task FillGridCellAsync(
            this ILocator table,
            string rowText,
            int columnIndex,
            string value)
        {
            var cell = table
                .Locator("tr")
                .Filter(new()
                {
                    HasText = rowText
                })
                .Locator("td")
                .Nth(columnIndex);

            await cell.ClickAsync();

            var input = cell.Locator("input");

            await input.FillExAsync(value);
        }

        #endregion

        #region ===== LOOKUP FIELD FILL =====

        public static async Task FillLookupFieldAsync(
            this IPage page,
            string fieldName,
            string value)
        {
            var lookupField = page
                .GetByRole(AriaRole.Textbox,
                    new()
                    {
                        Name = fieldName
                    });

            await lookupField.FillExAsync(value);

            await lookupField.PressAsync("Enter");
        }

        #endregion
    }
}