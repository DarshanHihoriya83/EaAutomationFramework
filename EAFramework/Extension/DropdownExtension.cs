using EAFramework.AIHealing;
using Microsoft.Playwright;

namespace EAFramework.Extension
{
    public static class DropdownExtension
    {
        #region ===== SELECT BY TEXT =====

        public static async Task SelectByTextAsync(
            this ILocator locator,
            string visibleText)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);

            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible
            });

            await locator.SelectOptionAsync(new SelectOptionValue
            {
                Label = visibleText
            });
        }

        #endregion

        #region ===== SELECT BY VALUE =====

        public static async Task SelectByValueAsync(
            this ILocator locator,
            string value)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);

            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible
            });

            await locator.SelectOptionAsync(new SelectOptionValue
            {
                Value = value
            });
        }

        #endregion

        #region ===== SELECT BY INDEX =====

        public static async Task SelectByIndexAsync(
            this ILocator locator,
            int index)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);

            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible
            });

            await locator.SelectOptionAsync(new SelectOptionValue()
            {
                Index = index
            });
        }

        #endregion

        #region ===== GET SELECTED TEXT =====

        public static async Task<string> GetSelectedTextAsync(
            this ILocator locator)
        {
            var selectedOption =
                await locator.Locator("option:checked")
                             .InnerTextAsync();

            return selectedOption;
        }

        #endregion

        #region ===== GET SELECTED VALUE =====

        public static async Task<string?> GetSelectedValueAsync(
            this ILocator locator)
        {
            return await locator.InputValueAsync();
        }

        #endregion

        #region ===== SELECT MULTIPLE =====

        public static async Task SelectMultipleAsync(
            this ILocator locator,
            params string[] values)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);

            await locator.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible
            });

            await locator.SelectOptionAsync(values);
        }

        #endregion

        #region ===== DYNAMICS LOOKUP DROPDOWN =====

        public static async Task SelectLookupValueAsync(
            this ILocator locator,
            string value)
        {
            await locator.ClickAsync();

            await locator.FillAsync(value);

            await locator.PressAsync("ArrowDown");

            await locator.PressAsync("Enter");
        }

        #endregion

        #region ===== SEARCHABLE DROPDOWN =====

        public static async Task SelectSearchableDropdownAsync(
            this IPage page,
            string dropdownSelector,
            string optionText)
        {
            var dropdown = page.Locator(dropdownSelector);

            await dropdown.ClickAsync();

            var option = page
                .Locator("[role='option']")
                .Filter(new()
                {
                    HasText = optionText
                });

            await option.ClickAsync();
        }

        #endregion

        #region ===== DYNAMICS OPTIONSET =====

        public static async Task SelectOptionSetAsync(
            this IPage page,
            string fieldLabel,
            string optionText)
        {
            var optionSet = page
                .GetByLabel(fieldLabel);

            await optionSet.ClickAsync();

            var option = page
                .Locator("[role='option']")
                .Filter(new()
                {
                    HasText = optionText
                });

            await option.ClickAsync();
        }

        #endregion

        #region ===== GET ALL OPTIONS =====

        public static async Task<List<string>> GetAllOptionsAsync(
            this ILocator locator)
        {
            var options = locator.Locator("option");

            int count = await options.CountAsync();

            List<string> optionTexts = new();

            for (int i = 0; i < count; i++)
            {
                optionTexts.Add(
                    await options.Nth(i).InnerTextAsync());
            }

            return optionTexts;
        }

        #endregion

        #region ===== VERIFY OPTION EXISTS =====

        public static async Task<bool> IsOptionExistsAsync(
            this ILocator locator,
            string optionText)
        {
            var options = await GetAllOptionsAsync(locator);

            return options.Any(option =>
                option.Equals(optionText,
                    StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region ===== CLEAR DROPDOWN =====

        public static async Task ClearDropdownAsync(
            this ILocator locator)
        {
            await locator.SelectOptionAsync(new string[] { });
        }

        #endregion

        #region ===== CASCADING DROPDOWN =====

        public static async Task SelectCascadingDropdownAsync(
            this IPage page,
            string parentSelector,
            string parentValue,
            string childSelector,
            string childValue)
        {
            var parentDropdown =
                page.Locator(parentSelector);

            await parentDropdown.SelectByTextAsync(parentValue);

            await page.WaitForTimeoutAsync(2000);

            var childDropdown =
                page.Locator(childSelector);

            await childDropdown.SelectByTextAsync(childValue);
        }

        #endregion

        #region ===== GRID DROPDOWN =====

        public static async Task SelectGridDropdownAsync(
            this ILocator table,
            string rowText,
            int columnIndex,
            string optionText)
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

            var dropdown =
                cell.Locator("select");

            await dropdown.SelectByTextAsync(optionText);
        }

        #endregion

        #region ===== DYNAMICS BUSINESS PROCESS FLOW =====

        public static async Task SelectBusinessProcessFlowStageAsync(
            this IPage page,
            string stageName)
        {
            var stage = page
                .Locator("[data-id='processStepsContainer']")
                .Locator("button")
                .Filter(new()
                {
                    HasText = stageName
                });

            await stage.ClickAsync();
        }

        #endregion

        #region ===== WAIT FOR DROPDOWN OPTIONS =====

        public static async Task WaitForDropdownOptionsAsync(
            this ILocator locator,
            int minimumOptionCount = 1,
            int timeout = 30000)
        {
            var options = locator.Locator("option");

            for (int second = 0;
                 second < timeout / 1000;
                 second++)
            {
                int count = await options.CountAsync();

                if (count >= minimumOptionCount)
                {
                    return;
                }

                await Task.Delay(1000);
            }

            throw new TimeoutException(
                "Dropdown options not loaded.");
        }

        #endregion

        #region ===== MODERN UI DROPDOWN =====

        public static async Task SelectModernDropdownAsync(
            this IPage page,
            string dropdownSelector,
            string optionText)
        {
            var dropdown =
                page.Locator(dropdownSelector);

            await dropdown.ClickAsync();

            var option = page
                .Locator("div[role='option']")
                .Filter(new()
                {
                    HasText = optionText
                });

            await option.ClickAsync();
        }

        #endregion

        #region ===== AUTO COMPLETE DROPDOWN =====

        public static async Task SelectAutoCompleteAsync(
            this ILocator locator,
            string value)
        {
            await locator.FillAsync(value);

            await locator.PressAsync("ArrowDown");

            await locator.PressAsync("Enter");
        }

        #endregion
    }
}