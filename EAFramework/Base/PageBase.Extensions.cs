using EAFramework.Extension;
using Microsoft.Playwright;

namespace EAFramework.Base
{
    /// <summary>
    /// Exposes all <see cref="EAFramework.Extension"/> methods through <see cref="PageBase"/>
    /// so page classes can use framework actions without calling extensions directly.
    /// </summary>
    public partial class PageBase
    {
        #region ===== HELPERS =====

        protected async Task<ILocator> ResolveAsync(string selector) =>
            await _healingEngine.FindElementAsync(selector);

        protected ILocator Table(string tableSelector) =>
            _page.Locator(tableSelector);

        protected async Task WithHighlightAsync(
            ILocator locator,
            Func<ILocator, Task> action)
        {
            await HighlightElementAsync(locator);
            await action(locator);
        }

        protected async Task WithHighlightAsync(
            string selector,
            Func<ILocator, Task> action)
        {
            ILocator locator = await ResolveAsync(selector);
            await WithHighlightAsync(locator, action);
        }

        #endregion

        #region ===== CLICK EXTENSIONS =====

        public async Task ClickExAsync(string selector, int timeout = 30000) =>
            await WithHighlightAsync(selector, loc => loc.ClickExAsync(timeout));

        public async Task ClickExAsync(ILocator locator, int timeout = 30000) =>
            await WithHighlightAsync(locator, loc => loc.ClickExAsync(timeout));

        public async Task ForceClickAsync(string selector, int timeout = 30000) =>
            await WithHighlightAsync(selector, loc => loc.ForceClickAsync(timeout));

        public async Task ForceClickAsync(ILocator locator, int timeout = 30000) =>
            await WithHighlightAsync(locator, loc => loc.ForceClickAsync(timeout));

        public async Task RetryClickAsync(string selector, int retryCount = 3, int delayMilliseconds = 1000) =>
            await WithHighlightAsync(selector, loc => loc.RetryClickAsync(retryCount, delayMilliseconds));

        public async Task RetryClickAsync(ILocator locator, int retryCount = 3, int delayMilliseconds = 1000) =>
            await WithHighlightAsync(locator, loc => loc.RetryClickAsync(retryCount, delayMilliseconds));

        public async Task ClickAndWaitAsync(string selector, int waitMilliseconds = 2000) =>
            await WithHighlightAsync(selector, loc => loc.ClickAndWaitAsync(waitMilliseconds));

        public async Task ClickAndWaitAsync(ILocator locator, int waitMilliseconds = 2000) =>
            await WithHighlightAsync(locator, loc => loc.ClickAndWaitAsync(waitMilliseconds));

        public async Task<bool> ClickIfExistsAsync(string selector) =>
            await (await ResolveAsync(selector)).ClickIfExistsAsync();

        public async Task<bool> ClickIfExistsAsync(ILocator locator) =>
            await locator.ClickIfExistsAsync();

        public async Task HealingClickAsync(string selector, int timeout = 30000) =>
            await _page.HealingClickAsync(selector, timeout);

        public async Task ClickGridRowByTextAsync(string tableSelector, string rowText) =>
            await Table(tableSelector).ClickGridRowByTextAsync(rowText);

        public async Task ClickButtonInsideRowAsync(
            string tableSelector,
            string rowText,
            string buttonText) =>
            await Table(tableSelector).ClickButtonInsideRowAsync(rowText, buttonText);

        public Task ClickTabByNameAsync(string tabName) =>
            _page.ClickTabAsync(tabName);

        #endregion

        #region ===== FILL EXTENSIONS =====

        public async Task FillExAsync(string selector, string value, int timeout = 30000) =>
            await WithHighlightAsync(selector, loc => loc.FillExAsync(value, timeout));

        public async Task FillExAsync(ILocator locator, string value, int timeout = 30000) =>
            await WithHighlightAsync(locator, loc => loc.FillExAsync(value, timeout));

        public async Task SlowFillAsync(string selector, string value, int delayMilliseconds = 100) =>
            await WithHighlightAsync(selector, loc => loc.SlowFillAsync(value, delayMilliseconds));

        public async Task SlowFillAsync(ILocator locator, string value, int delayMilliseconds = 100) =>
            await WithHighlightAsync(locator, loc => loc.SlowFillAsync(value, delayMilliseconds));

        public async Task ClearAndFillAsync(string selector, string value) =>
            await WithHighlightAsync(selector, loc => loc.ClearAndFillAsync(value));

        public async Task ClearAndFillAsync(ILocator locator, string value) =>
            await WithHighlightAsync(locator, loc => loc.ClearAndFillAsync(value));

        public async Task RetryFillAsync(
            string selector,
            string value,
            int retryCount = 3,
            int delayMilliseconds = 1000) =>
            await WithHighlightAsync(selector, loc => loc.RetryFillAsync(value, retryCount, delayMilliseconds));

        public async Task RetryFillAsync(
            ILocator locator,
            string value,
            int retryCount = 3,
            int delayMilliseconds = 1000) =>
            await WithHighlightAsync(locator, loc => loc.RetryFillAsync(value, retryCount, delayMilliseconds));

        public async Task HealingFillAsync(string selector, string value, int timeout = 30000) =>
            await _page.HealingFillAsync(selector, value, timeout);

        public async Task DynamicsFillAsync(string selector, string value, int timeout = 60000) =>
            await WithHighlightAsync(selector, loc => loc.DynamicsFillAsync(value, timeout));

        public async Task DynamicsFillAsync(ILocator locator, string value, int timeout = 60000) =>
            await WithHighlightAsync(locator, loc => loc.DynamicsFillAsync(value, timeout));

        public async Task FillAndEnterAsync(string selector, string value) =>
            await WithHighlightAsync(selector, loc => loc.FillAndEnterAsync(value));

        public async Task FillAndEnterAsync(ILocator locator, string value) =>
            await WithHighlightAsync(locator, loc => loc.FillAndEnterAsync(value));

        public async Task FillDateAsync(string selector, DateTime date) =>
            await WithHighlightAsync(selector, loc => loc.FillDateAsync(date));

        public async Task FillDateAsync(ILocator locator, DateTime date) =>
            await WithHighlightAsync(locator, loc => loc.FillDateAsync(date));

        public async Task FillNumberAsync(string selector, decimal number) =>
            await WithHighlightAsync(selector, loc => loc.FillNumberAsync(number));

        public async Task FillNumberAsync(ILocator locator, decimal number) =>
            await WithHighlightAsync(locator, loc => loc.FillNumberAsync(number));

        public Task SearchAndFillAsync(string labelText, string value) =>
            _page.SearchAndFillAsync(labelText, value);

        public async Task FillGridCellAsync(
            string tableSelector,
            string rowText,
            int columnIndex,
            string value) =>
            await Table(tableSelector).FillGridCellAsync(rowText, columnIndex, value);

        public Task FillLookupFieldAsync(string fieldName, string value) =>
            _page.FillLookupFieldAsync(fieldName, value);

        #endregion

        #region ===== DROPDOWN EXTENSIONS =====

        public async Task SelectByValueAsync(string selector, string value) =>
            await WithHighlightAsync(selector, loc => loc.SelectByValueAsync(value));

        public async Task SelectByValueAsync(ILocator locator, string value) =>
            await WithHighlightAsync(locator, loc => loc.SelectByValueAsync(value));

        public async Task SelectByIndexAsync(string selector, int index) =>
            await WithHighlightAsync(selector, loc => loc.SelectByIndexAsync(index));

        public async Task SelectByIndexAsync(ILocator locator, int index) =>
            await WithHighlightAsync(locator, loc => loc.SelectByIndexAsync(index));

        public async Task<string> GetSelectedTextAsync(string selector) =>
            await (await ResolveAsync(selector)).GetSelectedTextAsync();

        public async Task<string> GetSelectedTextAsync(ILocator locator) =>
            await locator.GetSelectedTextAsync();

        public async Task<string?> GetSelectedValueAsync(string selector) =>
            await (await ResolveAsync(selector)).GetSelectedValueAsync();

        public async Task<string?> GetSelectedValueAsync(ILocator locator) =>
            await locator.GetSelectedValueAsync();

        public async Task SelectMultipleAsync(string selector, params string[] values) =>
            await WithHighlightAsync(selector, loc => loc.SelectMultipleAsync(values));

        public async Task SelectMultipleAsync(ILocator locator, params string[] values) =>
            await WithHighlightAsync(locator, loc => loc.SelectMultipleAsync(values));

        public async Task SelectLookupValueAsync(string selector, string value) =>
            await WithHighlightAsync(selector, loc => loc.SelectLookupValueAsync(value));

        public async Task SelectLookupValueAsync(ILocator locator, string value) =>
            await WithHighlightAsync(locator, loc => loc.SelectLookupValueAsync(value));

        public Task SelectSearchableDropdownAsync(string dropdownSelector, string optionText) =>
            _page.SelectSearchableDropdownAsync(dropdownSelector, optionText);

        public Task SelectOptionSetAsync(string fieldLabel, string optionText) =>
            _page.SelectOptionSetAsync(fieldLabel, optionText);

        public async Task<List<string>> GetAllOptionsAsync(string selector) =>
            await (await ResolveAsync(selector)).GetAllOptionsAsync();

        public async Task<List<string>> GetAllOptionsAsync(ILocator locator) =>
            await locator.GetAllOptionsAsync();

        public async Task<bool> IsOptionExistsAsync(string selector, string optionText) =>
            await (await ResolveAsync(selector)).IsOptionExistsAsync(optionText);

        public async Task<bool> IsOptionExistsAsync(ILocator locator, string optionText) =>
            await locator.IsOptionExistsAsync(optionText);

        public async Task ClearDropdownAsync(string selector) =>
            await WithHighlightAsync(selector, loc => loc.ClearDropdownAsync());

        public async Task ClearDropdownAsync(ILocator locator) =>
            await WithHighlightAsync(locator, loc => loc.ClearDropdownAsync());

        public Task SelectCascadingDropdownAsync(
            string parentSelector,
            string parentValue,
            string childSelector,
            string childValue) =>
            _page.SelectCascadingDropdownAsync(parentSelector, parentValue, childSelector, childValue);

        public async Task SelectGridDropdownAsync(
            string tableSelector,
            string rowText,
            int columnIndex,
            string optionText) =>
            await Table(tableSelector).SelectGridDropdownAsync(rowText, columnIndex, optionText);

        public Task SelectBusinessProcessFlowStageAsync(string stageName) =>
            _page.SelectBusinessProcessFlowStageAsync(stageName);

        public async Task WaitForDropdownOptionsAsync(
            string selector,
            int minimumOptionCount = 1,
            int timeout = 30000) =>
            await (await ResolveAsync(selector)).WaitForDropdownOptionsAsync(minimumOptionCount, timeout);

        public Task SelectModernDropdownAsync(string dropdownSelector, string optionText) =>
            _page.SelectModernDropdownAsync(dropdownSelector, optionText);

        public async Task SelectAutoCompleteAsync(string selector, string value) =>
            await WithHighlightAsync(selector, loc => loc.SelectAutoCompleteAsync(value));

        public async Task SelectAutoCompleteAsync(ILocator locator, string value) =>
            await WithHighlightAsync(locator, loc => loc.SelectAutoCompleteAsync(value));

        public Task SelectPageOptionAsync(string selector, string label) =>
            _page.SelectOptionAsync(selector, new SelectOptionValue { Label = label });

        #endregion

        #region ===== WAIT EXTENSIONS =====

        public Task WaitAsync(int milliseconds) =>
            _page.WaitAsync(milliseconds);

        public async Task WaitForVisibleAsync(string selector, int timeout = 30000) =>
            await (await ResolveAsync(selector)).WaitForVisibleAsync(timeout);

        public Task WaitForVisibleAsync(ILocator locator, int timeout = 30000) =>
            locator.WaitForVisibleAsync(timeout);

        public async Task WaitForHiddenAsync(string selector, int timeout = 30000) =>
            await (await ResolveAsync(selector)).WaitForHiddenAsync(timeout);

        public Task WaitForHiddenAsync(ILocator locator, int timeout = 30000) =>
            locator.WaitForHiddenAsync(timeout);

        public async Task WaitForAttachedAsync(string selector, int timeout = 30000) =>
            await (await ResolveAsync(selector)).WaitForAttachedAsync(timeout);

        public Task WaitForAttachedAsync(ILocator locator, int timeout = 30000) =>
            locator.WaitForAttachedAsync(timeout);

        public async Task WaitForDetachedAsync(string selector, int timeout = 30000) =>
            await (await ResolveAsync(selector)).WaitForDetachedAsync(timeout);

        public Task WaitForDetachedAsync(ILocator locator, int timeout = 30000) =>
            locator.WaitForDetachedAsync(timeout);

        public async Task WaitForEnabledAsync(string selector, int timeout = 30000) =>
            await (await ResolveAsync(selector)).WaitForEnabledAsync(timeout);

        public Task WaitForEnabledAsync(ILocator locator, int timeout = 30000) =>
            locator.WaitForEnabledAsync(timeout);

        public async Task WaitForDisabledAsync(string selector, int timeout = 30000) =>
            await (await ResolveAsync(selector)).WaitForDisabledAsync(timeout);

        public Task WaitForDisabledAsync(ILocator locator, int timeout = 30000) =>
            locator.WaitForDisabledAsync(timeout);

        public async Task WaitForTextAsync(string selector, string expectedText, int timeout = 30000) =>
            await (await ResolveAsync(selector)).WaitForTextAsync(expectedText, timeout);

        public Task WaitForTextAsync(ILocator locator, string expectedText, int timeout = 30000) =>
            locator.WaitForTextAsync(expectedText, timeout);

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

        public async Task WaitForElementCountAsync(
            string selector,
            int expectedCount,
            int timeout = 30000) =>
            await (await ResolveAsync(selector)).WaitForElementCountAsync(expectedCount, timeout);

        public Task WaitForElementCountAsync(
            ILocator locator,
            int expectedCount,
            int timeout = 30000) =>
            locator.WaitForElementCountAsync(expectedCount, timeout);

        public async Task WaitUntilClickableAsync(string selector, int timeout = 30000) =>
            await (await ResolveAsync(selector)).WaitUntilClickableAsync(timeout);

        public Task WaitUntilClickableAsync(ILocator locator, int timeout = 30000) =>
            locator.WaitUntilClickableAsync(timeout);

        public Task WaitForLookupResultsAsync(int timeout = 30000) =>
            _page.WaitForLookupResultsAsync(timeout);

        #endregion

        #region ===== KEYBOARD EXTENSIONS =====

        public Task PressKeyAsync(string key) =>
            _page.PressKeyAsync(key);

        public async Task PressKeyOnAsync(string selector, string key) =>
            await (await ResolveAsync(selector)).PressKeyAsync(key);

        public Task PressKeyOnAsync(ILocator locator, string key) =>
            locator.PressKeyAsync(key);

        public Task TypeTextAsync(string text, int delayMilliseconds = 50) =>
            _page.TypeTextAsync(text, delayMilliseconds);

        public async Task TypeTextOnAsync(string selector, string text, int delayMilliseconds = 50) =>
            await (await ResolveAsync(selector)).TypeTextAsync(text, delayMilliseconds);

        public Task TypeTextOnAsync(ILocator locator, string text, int delayMilliseconds = 50) =>
            locator.TypeTextAsync(text, delayMilliseconds);

        public Task KeyDownAsync(string key) => _page.KeyDownAsync(key);
        public Task KeyUpAsync(string key) => _page.KeyUpAsync(key);

        public async Task CopyAsync(string selector) =>
            await (await ResolveAsync(selector)).CopyAsync();

        public Task CopyAsync(ILocator locator) => locator.CopyAsync();

        public async Task PasteAsync(string selector) =>
            await (await ResolveAsync(selector)).PasteAsync();

        public Task PasteAsync(ILocator locator) => locator.PasteAsync();

        public async Task CutAsync(string selector) =>
            await (await ResolveAsync(selector)).CutAsync();

        public Task CutAsync(ILocator locator) => locator.CutAsync();

        public async Task SelectAllAsync(string selector) =>
            await (await ResolveAsync(selector)).SelectAllAsync();

        public Task SelectAllAsync(ILocator locator) => locator.SelectAllAsync();

        public async Task PressEscapeAsync(string selector) =>
            await (await ResolveAsync(selector)).PressEscapeAsync();

        public Task PressEscapeAsync(ILocator locator) => locator.PressEscapeAsync();

        public async Task ArrowDownAsync(string selector) =>
            await (await ResolveAsync(selector)).ArrowDownAsync();

        public Task ArrowDownAsync(ILocator locator) => locator.ArrowDownAsync();

        public async Task ArrowUpAsync(string selector) =>
            await (await ResolveAsync(selector)).ArrowUpAsync();

        public Task ArrowUpAsync(ILocator locator) => locator.ArrowUpAsync();

        public async Task ArrowLeftAsync(string selector) =>
            await (await ResolveAsync(selector)).ArrowLeftAsync();

        public Task ArrowLeftAsync(ILocator locator) => locator.ArrowLeftAsync();

        public async Task ArrowRightAsync(string selector) =>
            await (await ResolveAsync(selector)).ArrowRightAsync();

        public Task ArrowRightAsync(ILocator locator) => locator.ArrowRightAsync();

        public Task PressFunctionKeyAsync(int functionKeyNumber) =>
            _page.PressFunctionKeyAsync(functionKeyNumber);

        public Task PressShortcutAsync(string shortcutKey) =>
            _page.PressShortcutAsync(shortcutKey);

        public Task DynamicsSaveAsync() => _page.DynamicsSaveAsync();
        public Task DynamicsRefreshAsync() => _page.DynamicsRefreshAsync();
        public Task DynamicsSearchAsync(string searchText) => _page.DynamicsSearchAsync(searchText);

        public async Task LookupSearchAsync(string selector, string searchText) =>
            await (await ResolveAsync(selector)).LookupSearchAsync(searchText);

        public Task LookupSearchAsync(ILocator locator, string searchText) =>
            locator.LookupSearchAsync(searchText);

        public async Task NavigateGridAsync(string selector, int moveDownCount = 1) =>
            await (await ResolveAsync(selector)).NavigateGridAsync(moveDownCount);

        public Task NavigateGridAsync(ILocator locator, int moveDownCount = 1) =>
            locator.NavigateGridAsync(moveDownCount);

        public Task PageDownAsync() => _page.PageDownAsync();
        public Task PageUpAsync() => _page.PageUpAsync();

        public async Task PressDeleteAsync(string selector) =>
            await (await ResolveAsync(selector)).PressDeleteAsync();

        public Task PressDeleteAsync(ILocator locator) => locator.PressDeleteAsync();

        public async Task PressBackspaceAsync(string selector) =>
            await (await ResolveAsync(selector)).PressBackspaceAsync();

        public Task PressBackspaceAsync(ILocator locator) => locator.PressBackspaceAsync();

        public Task ClearTextAsync(ILocator locator) => locator.ClearTextAsync();

        #endregion

        #region ===== SCREENSHOT EXTENSIONS =====

        public async Task<string> TakeElementScreenshotAsync(string selector, string screenshotName)
        {
            ILocator locator = await ResolveAsync(selector);
            return await locator.TakeElementScreenshotAsync(screenshotName);
        }

        public Task<string> TakeElementScreenshotAsync(ILocator locator, string screenshotName) =>
            locator.TakeElementScreenshotAsync(screenshotName);

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

        public async Task<string?> TakeScreenshotIfExistsAsync(string selector, string screenshotName)
        {
            ILocator locator = await ResolveAsync(selector);
            return await locator.TakeScreenshotIfExistsAsync(screenshotName);
        }

        public Task<string?> TakeScreenshotIfExistsAsync(ILocator locator, string screenshotName) =>
            locator.TakeScreenshotIfExistsAsync(screenshotName);

        public async Task<string> HighlightAndScreenshotAsync(string selector, string screenshotName) =>
            await (await ResolveAsync(selector)).HighlightAndScreenshotAsync(screenshotName);

        public Task<string> HighlightAndScreenshotAsync(ILocator locator, string screenshotName) =>
            locator.HighlightAndScreenshotAsync(screenshotName);

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

        public async Task ScrollHorizontalAsync(string selector, int scrollAmount) =>
            await (await ResolveAsync(selector)).ScrollHorizontalAsync(scrollAmount);

        public Task ScrollHorizontalAsync(ILocator locator, int scrollAmount) =>
            locator.ScrollHorizontalAsync(scrollAmount);

        public async Task ScrollVerticalAsync(string selector, int scrollAmount) =>
            await (await ResolveAsync(selector)).ScrollVerticalAsync(scrollAmount);

        public Task ScrollVerticalAsync(ILocator locator, int scrollAmount) =>
            locator.ScrollVerticalAsync(scrollAmount);

        public Task ScrollUntilVisibleAsync(string selector, int maxScrollAttempts = 20) =>
            _page.ScrollUntilVisibleAsync(selector, maxScrollAttempts);

        public Task MouseWheelScrollAsync(int deltaX, int deltaY) =>
            _page.MouseWheelScrollAsync(deltaX, deltaY);

        public Task ScrollGridAsync(string gridSelector, int scrollAmount = 1000) =>
            _page.Locator(gridSelector).ScrollGridAsync(scrollAmount);

        public Task ScrollDynamicsFormAsync() =>
            _page.ScrollDynamicsFormAsync();

        public async Task ScrollAndClickAsync(string selector) =>
            await (await ResolveAsync(selector)).ScrollAndClickAsync();

        public Task ScrollAndClickAsync(ILocator locator) =>
            locator.ScrollAndClickAsync();

        public async Task ScrollAndFillAsync(string selector, string value) =>
            await (await ResolveAsync(selector)).ScrollAndFillAsync(value);

        public Task ScrollAndFillAsync(ILocator locator, string value) =>
            locator.ScrollAndFillAsync(value);

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
    }
}
