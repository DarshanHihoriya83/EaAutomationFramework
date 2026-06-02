using EAFramework.Extension;
using Microsoft.Playwright;
using System.Collections.Generic;

namespace EAFramework.Base
{
    /// <summary>
    /// Exposes framework extension methods through <see cref="PageBase"/>.
    /// All string and <see cref="ILocator"/> overloads route through
    /// <see cref="PageBase.AiSelfHealingImplementation"/> (AI self-healing).
    /// </summary>
    public partial class PageBase
    {
        #region ===== HELPERS (AI SELF-HEALING) =====

        protected Task<ILocator> ResolveAsync(string selector) =>
            AiResolveAsync(selector);

        protected Task<ILocator> ResolveLocatorAsync(ILocator locator) =>
            AiResolveAsync(locator);

        protected Task WithHighlightAsync(
            string selector,
            Func<ILocator, Task> action) =>
            AiExecuteAsync(selector, action);

        protected Task WithHighlightAsync(
            ILocator locator,
            Func<ILocator, Task> action) =>
            AiExecuteAsync(locator, action);

        protected ILocator Table(string tableSelector) =>
            _page.Locator(tableSelector);

        #endregion

        #region ===== CLICK EXTENSIONS =====

        public Task ClickExAsync(string selector, int timeout = 30000) =>
            AiExecuteAsync(selector, loc => loc.ClickExAsync(timeout));

        public Task ClickExAsync(ILocator locator, int timeout = 30000) =>
            AiExecuteAsync(locator, loc => loc.ClickExAsync(timeout));

        public Task ForceClickAsync(string selector, int timeout = 30000) =>
            AiExecuteAsync(selector, loc => loc.ForceClickAsync(timeout));

        public Task ForceClickAsync(ILocator locator, int timeout = 30000) =>
            AiExecuteAsync(locator, loc => loc.ForceClickAsync(timeout));

        public Task RetryClickAsync(string selector, int retryCount = 3, int delayMilliseconds = 1000) =>
            AiExecuteAsync(selector, loc => loc.RetryClickAsync(retryCount, delayMilliseconds));

        public Task RetryClickAsync(ILocator locator, int retryCount = 3, int delayMilliseconds = 1000) =>
            AiExecuteAsync(locator, loc => loc.RetryClickAsync(retryCount, delayMilliseconds));

        public Task ClickAndWaitAsync(string selector, int waitMilliseconds = 2000) =>
            AiExecuteAsync(selector, loc => loc.ClickAndWaitAsync(waitMilliseconds));

        public Task ClickAndWaitAsync(ILocator locator, int waitMilliseconds = 2000) =>
            AiExecuteAsync(locator, loc => loc.ClickAndWaitAsync(waitMilliseconds));

        public async Task<bool> ClickIfExistsAsync(string selector) =>
            await AiExecuteAsync(selector, loc => loc.ClickIfExistsAsync());

        public async Task<bool> ClickIfExistsAsync(ILocator locator) =>
            await AiExecuteAsync(locator, loc => loc.ClickIfExistsAsync());

        public Task HealingClickAsync(string selector, int timeout = 30000) =>
            _page.HealingClickAsync(selector, timeout);

        public Task ClickGridRowByTextAsync(string tableSelector, string rowText) =>
            Table(tableSelector).ClickGridRowByTextAsync(rowText);

        public Task ClickButtonInsideRowAsync(
            string tableSelector,
            string rowText,
            string buttonText) =>
            Table(tableSelector).ClickButtonInsideRowAsync(rowText, buttonText);

        public Task ClickTabByNameAsync(string tabName) =>
            _page.ClickTabAsync(tabName);

        #endregion

        #region ===== FILL EXTENSIONS =====

        public Task FillExAsync(string selector, string value, int timeout = 30000) =>
            AiExecuteAsync(selector, loc => loc.FillExAsync(value, timeout));

        public Task FillExAsync(ILocator locator, string value, int timeout = 30000) =>
            AiExecuteAsync(locator, loc => loc.FillExAsync(value, timeout));

        public Task SlowFillAsync(string selector, string value, int delayMilliseconds = 100) =>
            AiExecuteAsync(selector, loc => loc.SlowFillAsync(value, delayMilliseconds));

        public Task SlowFillAsync(ILocator locator, string value, int delayMilliseconds = 100) =>
            AiExecuteAsync(locator, loc => loc.SlowFillAsync(value, delayMilliseconds));

        public Task ClearAndFillAsync(string selector, string value) =>
            AiExecuteAsync(selector, loc => loc.ClearAndFillAsync(value));

        public Task ClearAndFillAsync(ILocator locator, string value) =>
            AiExecuteAsync(locator, loc => loc.ClearAndFillAsync(value));

        public Task RetryFillAsync(
            string selector,
            string value,
            int retryCount = 3,
            int delayMilliseconds = 1000) =>
            AiExecuteAsync(selector, loc => loc.RetryFillAsync(value, retryCount, delayMilliseconds));

        public Task RetryFillAsync(
            ILocator locator,
            string value,
            int retryCount = 3,
            int delayMilliseconds = 1000) =>
            AiExecuteAsync(locator, loc => loc.RetryFillAsync(value, retryCount, delayMilliseconds));

        public Task HealingFillAsync(string selector, string value, int timeout = 30000) =>
            _page.HealingFillAsync(selector, value, timeout);

        public Task DynamicsFillAsync(string selector, string value, int timeout = 60000) =>
            AiExecuteAsync(selector, loc => loc.DynamicsFillAsync(value, timeout));

        public Task DynamicsFillAsync(ILocator locator, string value, int timeout = 60000) =>
            AiExecuteAsync(locator, loc => loc.DynamicsFillAsync(value, timeout));

        public Task FillAndEnterAsync(string selector, string value) =>
            AiExecuteAsync(selector, loc => loc.FillAndEnterAsync(value));

        public Task FillAndEnterAsync(ILocator locator, string value) =>
            AiExecuteAsync(locator, loc => loc.FillAndEnterAsync(value));

        public Task FillDateAsync(string selector, DateTime date) =>
            AiExecuteAsync(selector, loc => loc.FillDateAsync(date));

        public Task FillDateAsync(ILocator locator, DateTime date) =>
            AiExecuteAsync(locator, loc => loc.FillDateAsync(date));

        public Task FillNumberAsync(string selector, decimal number) =>
            AiExecuteAsync(selector, loc => loc.FillNumberAsync(number));

        public Task FillNumberAsync(ILocator locator, decimal number) =>
            AiExecuteAsync(locator, loc => loc.FillNumberAsync(number));

        public Task SearchAndFillAsync(string labelText, string value) =>
            _page.SearchAndFillAsync(labelText, value);

        public Task FillGridCellAsync(
            string tableSelector,
            string rowText,
            int columnIndex,
            string value) =>
            Table(tableSelector).FillGridCellAsync(rowText, columnIndex, value);

        public Task FillLookupFieldAsync(string fieldName, string value) =>
            _page.FillLookupFieldAsync(fieldName, value);

        #endregion

        #region ===== DROPDOWN EXTENSIONS =====

        public Task SelectByValueAsync(string selector, string value) =>
            AiExecuteAsync(selector, loc => loc.SelectByValueAsync(value));

        public Task SelectByValueAsync(ILocator locator, string value) =>
            AiExecuteAsync(locator, loc => loc.SelectByValueAsync(value));

        public Task SelectByIndexAsync(string selector, int index) =>
            AiExecuteAsync(selector, loc => loc.SelectByIndexAsync(index));

        public Task SelectByIndexAsync(ILocator locator, int index) =>
            AiExecuteAsync(locator, loc => loc.SelectByIndexAsync(index));

        public Task<string> GetSelectedTextAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.GetSelectedTextAsync());

        public Task<string> GetSelectedTextAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.GetSelectedTextAsync());

        public Task<string?> GetSelectedValueAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.GetSelectedValueAsync());

        public Task<string?> GetSelectedValueAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.GetSelectedValueAsync());

        public Task SelectMultipleAsync(string selector, params string[] values) =>
            AiExecuteAsync(selector, loc => loc.SelectMultipleAsync(values));

        public Task SelectMultipleAsync(ILocator locator, params string[] values) =>
            AiExecuteAsync(locator, loc => loc.SelectMultipleAsync(values));

        public Task SelectLookupValueAsync(string selector, string value) =>
            AiExecuteAsync(selector, loc => loc.SelectLookupValueAsync(value));

        public Task SelectLookupValueAsync(ILocator locator, string value) =>
            AiExecuteAsync(locator, loc => loc.SelectLookupValueAsync(value));

        public Task SelectSearchableDropdownAsync(string dropdownSelector, string optionText) =>
            _page.SelectSearchableDropdownAsync(dropdownSelector, optionText);

        public Task SelectOptionSetAsync(string fieldLabel, string optionText) =>
            _page.SelectOptionSetAsync(fieldLabel, optionText);

        public Task<List<string>> GetAllOptionsAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.GetAllOptionsAsync());

        public Task<List<string>> GetAllOptionsAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.GetAllOptionsAsync());

        public Task<bool> IsOptionExistsAsync(string selector, string optionText) =>
            AiExecuteAsync(selector, loc => loc.IsOptionExistsAsync(optionText));

        public Task<bool> IsOptionExistsAsync(ILocator locator, string optionText) =>
            AiExecuteAsync(locator, loc => loc.IsOptionExistsAsync(optionText));

        public Task ClearDropdownAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.ClearDropdownAsync());

        public Task ClearDropdownAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.ClearDropdownAsync());

        public Task SelectCascadingDropdownAsync(
            string parentSelector,
            string parentValue,
            string childSelector,
            string childValue) =>
            _page.SelectCascadingDropdownAsync(parentSelector, parentValue, childSelector, childValue);

        public Task SelectGridDropdownAsync(
            string tableSelector,
            string rowText,
            int columnIndex,
            string optionText) =>
            Table(tableSelector).SelectGridDropdownAsync(rowText, columnIndex, optionText);

        public Task SelectBusinessProcessFlowStageAsync(string stageName) =>
            _page.SelectBusinessProcessFlowStageAsync(stageName);

        public Task WaitForDropdownOptionsAsync(
            string selector,
            int minimumOptionCount = 1,
            int timeout = 30000) =>
            AiExecuteAsync(selector, loc => loc.WaitForDropdownOptionsAsync(minimumOptionCount, timeout));

        public Task SelectModernDropdownAsync(string dropdownSelector, string optionText) =>
            _page.SelectModernDropdownAsync(dropdownSelector, optionText);

        public Task SelectAutoCompleteAsync(string selector, string value) =>
            AiExecuteAsync(selector, loc => loc.SelectAutoCompleteAsync(value));

        public Task SelectAutoCompleteAsync(ILocator locator, string value) =>
            AiExecuteAsync(locator, loc => loc.SelectAutoCompleteAsync(value));

        public Task SelectPageOptionAsync(string selector, string label) =>
            _page.SelectOptionAsync(selector, new SelectOptionValue { Label = label });

        #endregion

        #region ===== WAIT EXTENSIONS =====

        public Task WaitAsync(int milliseconds) =>
            _page.WaitAsync(milliseconds);

        public Task WaitForVisibleAsync(string selector, int timeout = 30000) =>
            AiWaitForAsync(selector, WaitForSelectorState.Visible, timeout);

        public Task WaitForVisibleAsync(ILocator locator, int timeout = 30000) =>
            AiWaitForAsync(locator, WaitForSelectorState.Visible, timeout);

        public Task WaitForHiddenAsync(string selector, int timeout = 30000) =>
            AiWaitForAsync(selector, WaitForSelectorState.Hidden, timeout);

        public Task WaitForHiddenAsync(ILocator locator, int timeout = 30000) =>
            AiWaitForAsync(locator, WaitForSelectorState.Hidden, timeout);

        public Task WaitForAttachedAsync(string selector, int timeout = 30000) =>
            AiWaitForAsync(selector, WaitForSelectorState.Attached, timeout);

        public Task WaitForAttachedAsync(ILocator locator, int timeout = 30000) =>
            AiWaitForAsync(locator, WaitForSelectorState.Attached, timeout);

        public Task WaitForDetachedAsync(string selector, int timeout = 30000) =>
            AiWaitForAsync(selector, WaitForSelectorState.Detached, timeout);

        public Task WaitForDetachedAsync(ILocator locator, int timeout = 30000) =>
            AiWaitForAsync(locator, WaitForSelectorState.Detached, timeout);

        public Task WaitForEnabledAsync(string selector, int timeout = 30000) =>
            AiWaitForAsync(selector, WaitForSelectorState.Visible, timeout);

        public Task WaitForEnabledAsync(ILocator locator, int timeout = 30000) =>
            AiWaitForAsync(locator, WaitForSelectorState.Visible, timeout);

        public Task WaitForDisabledAsync(string selector, int timeout = 30000) =>
            AiWaitForAsync(selector, WaitForSelectorState.Hidden, timeout);

        public Task WaitForDisabledAsync(ILocator locator, int timeout = 30000) =>
            AiWaitForAsync(locator, WaitForSelectorState.Hidden, timeout);

        public Task WaitForTextAsync(string selector, string expectedText, int timeout = 30000) =>
            AiExecuteAsync(selector, loc => loc.WaitForTextAsync(expectedText, timeout), highlight: false);

        public Task WaitForTextAsync(ILocator locator, string expectedText, int timeout = 30000) =>
            AiExecuteAsync(locator, loc => loc.WaitForTextAsync(expectedText, timeout), highlight: false);

        public Task WaitForUrlContainsAsync(string partialUrl, int timeout = 30000) =>
            _page.WaitForUrlContainsAsync(partialUrl, timeout);

        public Task WaitForTitleAsync(string expectedTitle, int timeout = 30000) =>
            _page.WaitForTitleAsync(expectedTitle, timeout);

        public Task WaitForDynamicsSpinnerAsync(int timeout = 60000) =>
            _page.WaitForDynamicsSpinnerAsync(timeout);

        public Task<IFrame?> WaitForFrameAsync(string frameName, int timeout = 30000) =>
            _page.WaitForFrameAsync(frameName, timeout);

        public Task WaitForGridLoadAsync(string gridSelector, int timeout = 30000) =>
            _page.Locator(gridSelector).WaitForGridLoadAsync(timeout);

        public Task WaitForElementCountAsync(
            string selector,
            int expectedCount,
            int timeout = 30000) =>
            AiExecuteAsync(selector, loc => loc.WaitForElementCountAsync(expectedCount, timeout), highlight: false);

        public Task WaitForElementCountAsync(
            ILocator locator,
            int expectedCount,
            int timeout = 30000) =>
            AiExecuteAsync(locator, loc => loc.WaitForElementCountAsync(expectedCount, timeout), highlight: false);

        public async Task WaitUntilClickableAsync(string selector, int timeout = 30000)
        {
            await AiWaitForAsync(selector, WaitForSelectorState.Visible, timeout);
            await AiExecuteAsync(selector, loc => loc.WaitForEnabledAsync(timeout), highlight: false);
        }

        public async Task WaitUntilClickableAsync(ILocator locator, int timeout = 30000)
        {
            await AiWaitForAsync(locator, WaitForSelectorState.Visible, timeout);
            await AiExecuteAsync(locator, loc => loc.WaitForEnabledAsync(timeout), highlight: false);
        }

        public Task WaitForLookupResultsAsync(int timeout = 30000) =>
            _page.WaitForLookupResultsAsync(timeout);

        #endregion

        #region ===== KEYBOARD EXTENSIONS =====

        public Task PressKeyAsync(string key) =>
            _page.PressKeyAsync(key);

        public Task PressKeyOnAsync(string selector, string key) =>
            AiExecuteAsync(selector, loc => loc.PressKeyAsync(key), highlight: false);

        public Task PressKeyOnAsync(ILocator locator, string key) =>
            AiExecuteAsync(locator, loc => loc.PressKeyAsync(key), highlight: false);

        public Task TypeTextAsync(string text, int delayMilliseconds = 50) =>
            _page.TypeTextAsync(text, delayMilliseconds);

        public Task TypeTextOnAsync(string selector, string text, int delayMilliseconds = 50) =>
            AiExecuteAsync(selector, loc => loc.TypeTextAsync(text, delayMilliseconds), highlight: false);

        public Task TypeTextOnAsync(ILocator locator, string text, int delayMilliseconds = 50) =>
            AiExecuteAsync(locator, loc => loc.TypeTextAsync(text, delayMilliseconds), highlight: false);

        public Task KeyDownAsync(string key) => _page.KeyDownAsync(key);
        public Task KeyUpAsync(string key) => _page.KeyUpAsync(key);

        public Task CopyAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.CopyAsync(), highlight: false);

        public Task CopyAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.CopyAsync(), highlight: false);

        public Task PasteAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.PasteAsync(), highlight: false);

        public Task PasteAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.PasteAsync(), highlight: false);

        public Task CutAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.CutAsync(), highlight: false);

        public Task CutAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.CutAsync(), highlight: false);

        public Task SelectAllAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.SelectAllAsync(), highlight: false);

        public Task SelectAllAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.SelectAllAsync(), highlight: false);

        public Task PressEscapeAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.PressEscapeAsync(), highlight: false);

        public Task PressEscapeAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.PressEscapeAsync(), highlight: false);

        public Task ArrowDownAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.ArrowDownAsync(), highlight: false);

        public Task ArrowDownAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.ArrowDownAsync(), highlight: false);

        public Task ArrowUpAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.ArrowUpAsync(), highlight: false);

        public Task ArrowUpAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.ArrowUpAsync(), highlight: false);

        public Task ArrowLeftAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.ArrowLeftAsync(), highlight: false);

        public Task ArrowLeftAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.ArrowLeftAsync(), highlight: false);

        public Task ArrowRightAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.ArrowRightAsync(), highlight: false);

        public Task ArrowRightAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.ArrowRightAsync(), highlight: false);

        public Task PressFunctionKeyAsync(int functionKeyNumber) =>
            _page.PressFunctionKeyAsync(functionKeyNumber);

        public Task PressShortcutAsync(string shortcutKey) =>
            _page.PressShortcutAsync(shortcutKey);

        public Task DynamicsSaveAsync() => _page.DynamicsSaveAsync();
        public Task DynamicsRefreshAsync() => _page.DynamicsRefreshAsync();
        public Task DynamicsSearchAsync(string searchText) => _page.DynamicsSearchAsync(searchText);

        public Task LookupSearchAsync(string selector, string searchText) =>
            AiExecuteAsync(selector, loc => loc.LookupSearchAsync(searchText), highlight: false);

        public Task LookupSearchAsync(ILocator locator, string searchText) =>
            AiExecuteAsync(locator, loc => loc.LookupSearchAsync(searchText), highlight: false);

        public Task NavigateGridAsync(string selector, int moveDownCount = 1) =>
            AiExecuteAsync(selector, loc => loc.NavigateGridAsync(moveDownCount), highlight: false);

        public Task NavigateGridAsync(ILocator locator, int moveDownCount = 1) =>
            AiExecuteAsync(locator, loc => loc.NavigateGridAsync(moveDownCount), highlight: false);

        public Task PageDownAsync() => _page.PageDownAsync();
        public Task PageUpAsync() => _page.PageUpAsync();

        public Task PressDeleteAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.PressDeleteAsync(), highlight: false);

        public Task PressDeleteAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.PressDeleteAsync(), highlight: false);

        public Task PressBackspaceAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.PressBackspaceAsync(), highlight: false);

        public Task PressBackspaceAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.PressBackspaceAsync(), highlight: false);

        public Task ClearTextAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.ClearTextAsync(), highlight: false);

        #endregion

        #region ===== SCREENSHOT EXTENSIONS =====

        public async Task<string> TakeElementScreenshotAsync(string selector, string screenshotName)
        {
            ILocator locator = await AiResolveAsync(selector);
            return await locator.TakeElementScreenshotAsync(screenshotName);
        }

        public async Task<string> TakeElementScreenshotAsync(ILocator locator, string screenshotName) =>
            await AiExecuteAsync(
                locator,
                loc => loc.TakeElementScreenshotAsync(screenshotName),
                highlight: false);

        public Task<string> TakeStepScreenshotAsync(string stepName) =>
            _page.TakeStepScreenshotAsync(stepName);

        public Task<string> GetBase64ScreenshotAsync() =>
            _page.GetBase64ScreenshotAsync();

        public Task<string> TakeDynamicsFormScreenshotAsync(string formName) =>
            _page.TakeDynamicsFormScreenshotAsync(formName);

        public Task<string> TakeGridScreenshotAsync(string gridSelector, string gridName) =>
            _page.Locator(gridSelector).TakeGridScreenshotAsync(gridName);

        public Task<string> TakeCommandBarScreenshotAsync() =>
            _page.TakeCommandBarScreenshotAsync();

        public Task<string> TakeFullPageScreenshotAsync() =>
            _page.TakeFullPageScreenshotAsync();

        public async Task<string?> TakeScreenshotIfExistsAsync(string selector, string screenshotName) =>
            await AiExecuteAsync(
                selector,
                loc => loc.TakeScreenshotIfExistsAsync(screenshotName));

        public async Task<string?> TakeScreenshotIfExistsAsync(ILocator locator, string screenshotName) =>
            await AiExecuteAsync(
                locator,
                loc => loc.TakeScreenshotIfExistsAsync(screenshotName),
                highlight: false);

        public async Task<string> HighlightAndScreenshotAsync(string selector, string screenshotName) =>
            await AiExecuteAsync(
                selector,
                loc => loc.HighlightAndScreenshotAsync(screenshotName),
                highlight: true);

        public async Task<string> HighlightAndScreenshotAsync(ILocator locator, string screenshotName) =>
            await AiExecuteAsync(
                locator,
                loc => loc.HighlightAndScreenshotAsync(screenshotName),
                highlight: true);

        public Task<(string before, string after)> TakeBeforeAfterScreenshotAsync(
            string actionName,
            Func<Task> action) =>
            _page.TakeBeforeAfterScreenshotAsync(actionName, action);

        public void CleanOldScreenshots(int olderThanDays = 7) =>
            ScreenshotExtension.CleanOldScreenshots(olderThanDays);

        #endregion

        #region ===== SCROLL EXTENSIONS =====

        public Task ScrollByAsync(int xPixels, int yPixels) =>
            _page.ScrollByAsync(xPixels, yPixels);

        public Task ScrollHorizontalAsync(string selector, int scrollAmount) =>
            AiExecuteAsync(selector, loc => loc.ScrollHorizontalAsync(scrollAmount), highlight: false);

        public Task ScrollHorizontalAsync(ILocator locator, int scrollAmount) =>
            AiExecuteAsync(locator, loc => loc.ScrollHorizontalAsync(scrollAmount), highlight: false);

        public Task ScrollVerticalAsync(string selector, int scrollAmount) =>
            AiExecuteAsync(selector, loc => loc.ScrollVerticalAsync(scrollAmount), highlight: false);

        public Task ScrollVerticalAsync(ILocator locator, int scrollAmount) =>
            AiExecuteAsync(locator, loc => loc.ScrollVerticalAsync(scrollAmount), highlight: false);

        public Task ScrollUntilVisibleAsync(string selector, int maxScrollAttempts = 20) =>
            _page.ScrollUntilVisibleAsync(selector, maxScrollAttempts);

        public Task MouseWheelScrollAsync(int deltaX, int deltaY) =>
            _page.MouseWheelScrollAsync(deltaX, deltaY);

        public Task ScrollGridAsync(string gridSelector, int scrollAmount = 1000) =>
            _page.Locator(gridSelector).ScrollGridAsync(scrollAmount);

        public Task ScrollDynamicsFormAsync() =>
            _page.ScrollDynamicsFormAsync();

        public Task ScrollAndClickAsync(string selector) =>
            AiExecuteAsync(selector, loc => loc.ScrollAndClickAsync());

        public Task ScrollAndClickAsync(ILocator locator) =>
            AiExecuteAsync(locator, loc => loc.ScrollAndClickAsync());

        public Task ScrollAndFillAsync(string selector, string value) =>
            AiExecuteAsync(selector, loc => loc.ScrollAndFillAsync(value));

        public Task ScrollAndFillAsync(ILocator locator, string value) =>
            AiExecuteAsync(locator, loc => loc.ScrollAndFillAsync(value));

        public Task InfiniteScrollAsync(int scrollCount = 10) =>
            _page.InfiniteScrollAsync(scrollCount);

        public Task ScrollUntilTextVisibleAsync(string text, int maxScrollAttempts = 20) =>
            _page.ScrollUntilTextVisibleAsync(text, maxScrollAttempts);

        public Task ScrollToLastGridRowAsync(string tableSelector) =>
            Table(tableSelector).ScrollToLastGridRowAsync();

        #endregion

        #region ===== LOCATOR CHAIN EXTENSIONS =====

        public ILocator GetLinkInsideRow(string tableSelector, string rowText, string linkText) =>
            Table(tableSelector).GetLinkInsideRow(rowText, linkText);

        public ILocator GetInputInsideRow(string tableSelector, string rowText) =>
            Table(tableSelector).GetInputInsideRow(rowText);

        public ILocator GetCheckboxInsideRow(string tableSelector, string rowText) =>
            Table(tableSelector).GetCheckboxInsideRow(rowText);

        public ILocator GetDropdownInsideRow(string tableSelector, string rowText) =>
            Table(tableSelector).GetDropdownInsideRow(rowText);

        public ILocator GetGridRowByColumnValue(string tableSelector, int columnIndex, string cellValue) =>
            Table(tableSelector).GetGridRowByColumnValue(columnIndex, cellValue);

        public ILocator GetDynamicsGridRow(string rowText) =>
            _page.GetDynamicsGridRow(rowText);

        public ILocator GetDynamicsGridCell(string rowText, int columnIndex) =>
            _page.GetDynamicsGridCell(rowText, columnIndex);

        public ILocator GetTabInsideSection(string sectionName, string tabName) =>
            _page.GetTabInsideSection(sectionName, tabName);

        public ILocator GetFieldInsideSection(string sectionName, string fieldLabel) =>
            _page.GetFieldInsideSection(sectionName, fieldLabel);

        public ILocator GetButtonInsideCard(string cardTitle, string buttonText) =>
            _page.GetButtonInsideCard(cardTitle, buttonText);

        public ILocator GetButtonInsideModal(string modalTitle, string buttonText) =>
            _page.GetButtonInsideModal(modalTitle, buttonText);

        public ILocator GetNestedMenu(string parentMenu, string childMenu) =>
            _page.GetNestedMenu(parentMenu, childMenu);

        public ILocator GetElementInsideFrame(string frameSelector, string elementSelector) =>
            _page.GetElementInsideFrame(frameSelector, elementSelector);

        public ILocator GetLookupResult(string resultText) =>
            _page.GetLookupResult(resultText);

        public ILocator GetBusinessProcessStage(string stageName) =>
            _page.GetBusinessProcessStage(stageName);

        public ILocator GetSubGridRow(string subGridName, string rowText) =>
            _page.GetSubGridRow(subGridName, rowText);

        public ILocator GetSubGridButton(string subGridName, string rowText, string buttonText) =>
            _page.GetSubGridButton(subGridName, rowText, buttonText);

        public ILocator GetTooltipElement(string tooltipText) =>
            _page.GetTooltipElement(tooltipText);

        public ILocator GetRowActionMenu(string tableSelector, string rowText) =>
            Table(tableSelector).GetRowActionMenu(rowText);

        public ILocator GetQuickCreateField(string fieldLabel) =>
            _page.GetQuickCreateField(fieldLabel);

        public ILocator GetHeaderField(string fieldLabel) =>
            _page.GetHeaderField(fieldLabel);

        #endregion

        #region ===== AUTO RETRY LOCATORS (ERP / DYNAMICS) =====

        public Task WaitForErpReadyAsync(int overlayTimeoutMs = 60000, int postSettleMs = 300) =>
            _page.WaitForErpReadyAsync(overlayTimeoutMs, postSettleMs);

        public Task AutoRetryClickAsync(
            ILocator locator,
            AutoRetryLocatorsExtension.AutoRetryLocatorOptions? options = null) =>
            locator.AutoRetryClickAsync(options);

        public Task AutoRetryClickAsync(
            string selector,
            AutoRetryLocatorsExtension.AutoRetryLocatorOptions? options = null) =>
            _page.AutoRetryClickAsync(selector, options);

        public Task AutoRetryFillAsync(
            ILocator locator,
            string value,
            AutoRetryLocatorsExtension.AutoRetryLocatorOptions? options = null) =>
            locator.AutoRetryFillAsync(value, options);

        public Task AutoRetryFillAsync(
            string selector,
            string value,
            AutoRetryLocatorsExtension.AutoRetryLocatorOptions? options = null) =>
            _page.AutoRetryFillAsync(selector, value, options);

        public Task AutoRetryDynamicsClickAsync(ILocator locator) =>
            locator.AutoRetryDynamicsClickAsync(
                AutoRetryLocatorsExtension.DynamicsDefaults());

        public Task AutoRetryDynamicsClickAsync(string selector) =>
            _page.AutoRetryDynamicsClickAsync(
                selector,
                AutoRetryLocatorsExtension.DynamicsDefaults());

        public Task AutoRetryDynamicsFillAsync(ILocator locator, string value) =>
            locator.AutoRetryDynamicsFillAsync(
                value,
                AutoRetryLocatorsExtension.DynamicsDefaults());

        public Task AutoRetryDynamicsFillAsync(string selector, string value) =>
            _page.AutoRetryDynamicsFillAsync(
                selector,
                value,
                AutoRetryLocatorsExtension.DynamicsDefaults());

        public Task AutoRetrySelectByTextAsync(
            ILocator locator,
            string visibleText) =>
            locator.AutoRetrySelectByTextAsync(visibleText);

        #endregion

        #region ===== AI SELF-HEALING STORE (PAGE) =====

        public void ExportAiHealingReport(string? reportPath = null) =>
            _page.ExportHealingReport(reportPath);

        public IReadOnlyDictionary<string, string> GetAiHealedMappings() =>
            _page.GetHealedMappings();

        public void ClearAiHealingStore() =>
            _page.ClearHealingStore();

        public Task<bool> VerifyAiHealingAsync(string selector) =>
            _page.VerifyHealingAsync(selector);

        #endregion
    }
}
