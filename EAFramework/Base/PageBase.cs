using EAFramework.AIHealing;
using EAFramework.Extension;
using Microsoft.Playwright;

namespace EAFramework.Base
{
    public partial class PageBase
    {
        #region ===== VARIABLES =====

        protected readonly IPage _page;

        protected readonly SelfHealingEngine _healingEngine;

        #endregion

        #region ===== CONSTRUCTOR =====

        public PageBase(IPage page)
        {
            _page = page;

            _healingEngine = HealingEngineCache.Get(_page);
        }

        #endregion

        #region ===== PAGE =====

        public IPage Page => _page;

        #endregion

        #region ===== LOCATOR =====

        protected async Task<ILocator> FindAsync(string selector) =>
            await _healingEngine.FindElementAsync(selector);

        protected ILocator Locator(string selector) =>
            _page.Locator(selector);

        #endregion

        #region ===== HIGHLIGHT ELEMENT =====

        public async Task HighlightElementAsync(
            ILocator locator)
        {
            var elementHandle =
                await locator.ElementHandleAsync();

            if (elementHandle != null)
            {
                await _page.EvaluateAsync(
                    @"([element]) =>
                    {
                        element.style.border =
                            '3px solid red';

                        element.style.backgroundColor =
                            '#fff3cd';
                    }",
                    new object[]
                    {
                        elementHandle
                    });
            }
        }

        #endregion

        #region ===== CLICK =====

        public async Task ClickAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await HighlightElementAsync(locator);

            await locator.DynamicsClickAsync();
        }

        public async Task ClickAsync(
            ILocator locator)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);

            await HighlightElementAsync(locator);

            await locator.DynamicsClickAsync();
        }

        #endregion

        #region ===== FILL =====

        public async Task FillAsync(
            string selector,
            string value)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await HighlightElementAsync(locator);

            await locator.DynamicsFillAsync(value);
        }

        public async Task FillAsync(
            ILocator locator,
            string value)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);

            await HighlightElementAsync(locator);

            await locator.DynamicsFillAsync(value);
        }

        #endregion

        #region ===== TYPE =====

        public async Task TypeAsync(
            string selector,
            string value)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await HighlightElementAsync(locator);

            await locator.TypeFillAsync(value);
        }

        #endregion

        #region ===== DROPDOWN =====

        public async Task SelectDropdownAsync(
            string selector,
            string visibleText)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await HighlightElementAsync(locator);

            await locator.SelectByTextAsync(
                visibleText);
        }

        #endregion

        #region ===== CHECKBOX =====

        public async Task CheckAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await HighlightElementAsync(locator);

            if (!await locator.IsCheckedAsync())
            {
                await locator.CheckAsync();
            }
        }

        public async Task UnCheckAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await HighlightElementAsync(locator);

            if (await locator.IsCheckedAsync())
            {
                await locator.UncheckAsync();
            }
        }

        #endregion

        #region ===== HOVER =====

        public async Task HoverAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await HighlightElementAsync(locator);

            await locator.HoverAsync();
        }

        #endregion

        #region ===== DOUBLE CLICK =====

        public async Task DoubleClickAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await HighlightElementAsync(locator);

            await locator.DoubleClickExAsync();
        }

        #endregion

        #region ===== RIGHT CLICK =====

        public async Task RightClickAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await HighlightElementAsync(locator);

            await locator.RightClickAsync();
        }

        #endregion

        #region ===== WAITS =====

        public async Task WaitForPageLoadAsync()
        {
            await _page.WaitForPageLoadAsync();
        }

        public async Task WaitForNetworkIdleAsync()
        {
            await _page.WaitForNetworkIdleAsync();
        }

        public async Task SmartWaitAsync()
        {
            await _page.SmartWaitAsync();
        }

        #endregion

        #region ===== SCREENSHOT =====

        public async Task<string> TakeScreenshotAsync(
            string screenshotName)
        {
            return await _page
                .TakePageScreenshotAsync(
                    screenshotName);
        }

        public async Task<string> TakeFailureScreenshotAsync(
            string testName)
        {
            return await _page
                .TakeFailureScreenshotAsync(
                    testName);
        }

        #endregion

        #region ===== SCROLL =====

        public async Task ScrollToBottomAsync()
        {
            await _page.ScrollToBottomAsync();
        }

        public async Task ScrollToTopAsync()
        {
            await _page.ScrollToTopAsync();
        }

        public async Task ScrollIntoViewAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await locator.ScrollIntoViewExAsync();
        }

        #endregion

        #region ===== KEYBOARD =====

        public async Task PressEnterAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await locator.PressEnterAsync();
        }

        public async Task PressTabAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await locator.PressTabAsync();
        }

        public async Task ClearTextAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await locator.ClearTextAsync();
        }

        #endregion

        #region ===== JAVASCRIPT ACTIONS =====

        public async Task JsClickAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await HighlightElementAsync(locator);

            await locator.JsClickAsync();
        }

        public async Task JsFillAsync(
            string selector,
            string value)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            await HighlightElementAsync(locator);

            await locator.JsFillAsync(value);
        }

        #endregion

        #region ===== LOCATOR CHAINING =====

        public ILocator GetTableRow(
            string tableSelector,
            string rowText)
        {
            return _page
                .Locator(tableSelector)
                .GetTableRow(rowText);
        }

        public ILocator GetButtonInsideRow(
            string tableSelector,
            string rowText,
            string buttonText)
        {
            return _page
                .Locator(tableSelector)
                .GetButtonInsideRow(
                    rowText,
                    buttonText);
        }

        public ILocator GetGridCell(
            string tableSelector,
            string rowText,
            int columnIndex)
        {
            return _page
                .Locator(tableSelector)
                .GetTableCell(
                    rowText,
                    columnIndex);
        }

        #endregion

        #region ===== DYNAMICS HELPERS =====

        public async Task ClickCommandBarButtonAsync(
            string buttonName)
        {
            var button =
                _page.GetCommandBarButton(buttonName);

            await HighlightElementAsync(button);

            await button.DynamicsClickAsync();
        }

        public async Task OpenTabAsync(
            string tabName)
        {
            var tab = _page
                .Locator("[role='tab']")
                .Filter(new()
                {
                    HasText = tabName
                });

            await HighlightElementAsync(tab);

            await tab.DynamicsClickAsync();
        }

        public async Task FillLookupAsync(
            string fieldLabel,
            string value)
        {
            var lookup =
                _page.GetByLabel(fieldLabel);

            await HighlightElementAsync(lookup);

            await lookup.FillAsync(value);

            await lookup.PressAsync("ArrowDown");

            await lookup.PressAsync("Enter");
        }

        #endregion

        #region ===== FRAME =====

        public IFrameLocator GetFrame(
            string frameSelector)
        {
            return _page.GetFrame(frameSelector);
        }

        #endregion

        #region ===== VALIDATIONS =====

        public async Task<bool> IsVisibleAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            return await locator.IsVisibleAsync();
        }

        public async Task<bool> IsEnabledAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            return await locator.IsEnabledAsync();
        }

        public async Task<string> GetTextAsync(
            string selector)
        {
            var locator =
                await _healingEngine
                    .FindElementAsync(selector);

            return await locator.InnerTextAsync();
        }

        #endregion

        #region ===== PAGE NAVIGATION =====

        public async Task NavigateAsync(
            string url)
        {
            await _page.GotoAsync(url);

            await SmartWaitAsync();
        }

        public async Task RefreshAsync()
        {
            await _page.ReloadAsync();

            await SmartWaitAsync();
        }

        #endregion
    }
}