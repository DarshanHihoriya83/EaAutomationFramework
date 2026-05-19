using EAFramework.AIHealing;
using EAFramework.Extension;
using Microsoft.Playwright;

namespace EAFramework.Base
{
    public class ComponentBase
    {
        #region ===== VARIABLES =====

        protected readonly IPage _page;

        protected readonly ILocator _rootElement;

        protected readonly SelfHealingEngine _healingEngine;

        protected readonly LocatorRepository _locatorRepository;

        #endregion

        #region ===== CONSTRUCTOR =====

        public ComponentBase(
            IPage page,
            ILocator rootElement)
        {
            _page = page;

            _rootElement = rootElement;

            _healingEngine = new SelfHealingEngine(_page);

            _locatorRepository = new LocatorRepository(_page);
        }

        #endregion

        #region ===== PAGE =====

        public IPage Page => _page;

        #endregion

        #region ===== ROOT ELEMENT =====

        public ILocator RootElement =>
            _rootElement;

        #endregion

        #region ===== FIND ELEMENT =====

        protected ILocator Find(
            string selector)
        {
            return _rootElement
                .Locator(selector);
        }

        #endregion

        #region ===== FIND ALL ELEMENTS =====

        protected ILocator FindAll(
            string selector)
        {
            return _rootElement
                .Locator(selector);
        }

        #endregion

        #region ===== HIGHLIGHT =====

        protected async Task HighlightAsync(
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
                            '#ffe6e6';

                        element.style.transition =
                            'all 0.3s ease';
                    }",
                    new object[]
                    {
                        elementHandle
                    });
            }
        }

        #endregion

        #region ===== CLICK =====

        protected async Task ClickAsync(
            string selector)
        {
            var locator =
                await _healingEngine.FindElementAsync(selector);

            await HighlightAsync(locator);

            await locator.DynamicsClickAsync();
        }

        protected async Task ClickAsync(
            ILocator locator)
        {
            await HighlightAsync(locator);

            await locator.DynamicsClickAsync();
        }

        #endregion

        #region ===== FILL =====

        protected async Task FillAsync(
            string selector,
            string value)
        {
            var locator =
                await _healingEngine.FindElementAsync(selector);

            await HighlightAsync(locator);

            await locator.DynamicsFillAsync(value);
        }

        protected async Task FillAsync(
            ILocator locator,
            string value)
        {
            await HighlightAsync(locator);

            await locator.DynamicsFillAsync(value);
        }

        #endregion

        #region ===== TYPE =====

        protected async Task TypeAsync(
            string selector,
            string value)
        {
            var locator =
                Find(selector);

            await HighlightAsync(locator);

            await locator.TypeFillAsync(value);
        }

        #endregion

        #region ===== DROPDOWN =====

        protected async Task SelectDropdownAsync(
            string selector,
            string visibleText)
        {
            var locator =
                Find(selector);

            await HighlightAsync(locator);

            await locator.SelectByTextAsync(
                visibleText);
        }

        #endregion

        #region ===== CHECKBOX =====

        protected async Task CheckAsync(
            string selector)
        {
            var locator =
                Find(selector);

            await HighlightAsync(locator);

            if (!await locator.IsCheckedAsync())
            {
                await locator.CheckAsync();
            }
        }

        protected async Task UnCheckAsync(
            string selector)
        {
            var locator =
                Find(selector);

            await HighlightAsync(locator);

            if (await locator.IsCheckedAsync())
            {
                await locator.UncheckAsync();
            }
        }

        #endregion

        #region ===== HOVER =====

        protected async Task HoverAsync(
            string selector)
        {
            var locator =
                Find(selector);

            await HighlightAsync(locator);

            await locator.HoverAsync();
        }

        #endregion

        #region ===== DOUBLE CLICK =====

        protected async Task DoubleClickAsync(
            string selector)
        {
            var locator =
                Find(selector);

            await HighlightAsync(locator);

            await locator.DoubleClickExAsync();
        }

        #endregion

        #region ===== RIGHT CLICK =====

        protected async Task RightClickAsync(
            string selector)
        {
            var locator =
                Find(selector);

            await HighlightAsync(locator);

            await locator.RightClickAsync();
        }

        #endregion

        #region ===== SCROLL =====

        protected async Task ScrollIntoViewAsync(
            string selector)
        {
            var locator =
                Find(selector);

            await locator.ScrollIntoViewExAsync();
        }

        #endregion

        #region ===== KEYBOARD =====

        protected async Task PressEnterAsync(
            string selector)
        {
            var locator =
                Find(selector);

            await locator.PressEnterAsync();
        }

        protected async Task PressTabAsync(
            string selector)
        {
            var locator =
                Find(selector);

            await locator.PressTabAsync();
        }

        protected async Task ClearTextAsync(
            string selector)
        {
            var locator =
                Find(selector);

            await locator.ClearTextAsync();
        }

        #endregion

        #region ===== JAVASCRIPT =====

        protected async Task JsClickAsync(
            string selector)
        {
            var locator =
                Find(selector);

            await HighlightAsync(locator);

            await locator.JsClickAsync();
        }

        protected async Task JsFillAsync(
            string selector,
            string value)
        {
            var locator =
                Find(selector);

            await HighlightAsync(locator);

            await locator.JsFillAsync(value);
        }

        #endregion

        #region ===== WAITS =====

        protected async Task WaitForVisibleAsync(
            string selector)
        {
            var locator =
                Find(selector);

            await locator.WaitForAsync(new()
            {
                State =
                    WaitForSelectorState.Visible
            });
        }

        protected async Task WaitForHiddenAsync(
            string selector)
        {
            var locator =
                Find(selector);

            await locator.WaitForAsync(new()
            {
                State =
                    WaitForSelectorState.Hidden
            });
        }

        #endregion

        #region ===== VALIDATIONS =====

        protected async Task<bool> IsVisibleAsync(
            string selector)
        {
            return await Find(selector)
                .IsVisibleAsync();
        }

        protected async Task<bool> IsEnabledAsync(
            string selector)
        {
            return await Find(selector)
                .IsEnabledAsync();
        }

        protected async Task<string> GetTextAsync(
            string selector)
        {
            return await Find(selector)
                .InnerTextAsync();
        }

        protected async Task<string> GetValueAsync(
            string selector)
        {
            return await Find(selector)
                .InputValueAsync();
        }

        #endregion

        #region ===== SCREENSHOT =====

        protected async Task<string> TakeScreenshotAsync(
            string screenshotName)
        {
            return await _page
                .TakePageScreenshotAsync(
                    screenshotName);
        }

        #endregion

        #region ===== LOCATOR CHAINING =====

        protected ILocator GetRowByText(
            string rowText)
        {
            return _rootElement
                .Locator("tr")
                .Filter(new()
                {
                    HasText = rowText
                });
        }

        protected ILocator GetButtonInsideRow(
            string rowText,
            string buttonText)
        {
            return GetRowByText(rowText)
                .Locator("button")
                .Filter(new()
                {
                    HasText = buttonText
                });
        }

        protected ILocator GetCellInsideRow(
            string rowText,
            int columnIndex)
        {
            return GetRowByText(rowText)
                .Locator("td")
                .Nth(columnIndex);
        }

        #endregion

        #region ===== DYNAMICS COMPONENT =====

        protected async Task ClickCommandButtonAsync(
            string buttonName)
        {
            var button =
                _locatorRepository
                    .GetButton(buttonName);

            await HighlightAsync(button);

            await button.DynamicsClickAsync();
        }

        protected async Task FillLookupAsync(
            string fieldName,
            string value)
        {
            var lookup =
                _page.GetByLabel(fieldName);

            await HighlightAsync(lookup);

            await lookup.FillAsync(value);

            await lookup.PressAsync("ArrowDown");

            await lookup.PressAsync("Enter");
        }

        #endregion
    }
}