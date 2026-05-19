using Microsoft.Playwright;

namespace EAFramework.Extension
{
    public static class KeyboardAction
    {
        #region ===== PRESS SINGLE KEY =====

        public static async Task PressKeyAsync(
            this IPage page,
            string key)
        {
            await page.Keyboard.PressAsync(key);
        }

        #endregion

        #region ===== PRESS KEY ON LOCATOR =====

        public static async Task PressKeyAsync(
            this ILocator locator,
            string key)
        {
            await locator.PressAsync(key);
        }

        #endregion

        #region ===== TYPE TEXT =====

        public static async Task TypeTextAsync(
            this IPage page,
            string text,
            int delayMilliseconds = 50)
        {
            await page.Keyboard.TypeAsync(text, new()
            {
                Delay = delayMilliseconds
            });
        }

        #endregion

        #region ===== TYPE TEXT ON LOCATOR =====

        public static async Task TypeTextAsync(
            this ILocator locator,
            string text,
            int delayMilliseconds = 50)
        {
            await locator.TypeAsync(text, new()
            {
                Delay = delayMilliseconds
            });
        }

        #endregion

        #region ===== KEY DOWN =====

        public static async Task KeyDownAsync(
            this IPage page,
            string key)
        {
            await page.Keyboard.DownAsync(key);
        }

        #endregion

        #region ===== KEY UP =====

        public static async Task KeyUpAsync(
            this IPage page,
            string key)
        {
            await page.Keyboard.UpAsync(key);
        }

        #endregion

        #region ===== COPY =====

        public static async Task CopyAsync(
            this ILocator locator)
        {
            await locator.ClickAsync();

            await locator.PressAsync("Control+A");

            await locator.PressAsync("Control+C");
        }

        #endregion

        #region ===== PASTE =====

        public static async Task PasteAsync(
            this ILocator locator)
        {
            await locator.ClickAsync();

            await locator.PressAsync("Control+V");
        }

        #endregion

        #region ===== CUT =====

        public static async Task CutAsync(
            this ILocator locator)
        {
            await locator.ClickAsync();

            await locator.PressAsync("Control+A");

            await locator.PressAsync("Control+X");
        }

        #endregion

        #region ===== SELECT ALL =====

        public static async Task SelectAllAsync(
            this ILocator locator)
        {
            await locator.ClickAsync();

            await locator.PressAsync("Control+A");
        }

        #endregion

        #region ===== CLEAR TEXT =====

        public static async Task ClearTextAsync(
            this ILocator locator)
        {
            await locator.ClickAsync();

            await locator.PressAsync("Control+A");

            await locator.PressAsync("Backspace");
        }

        #endregion

        #region ===== ENTER =====

        public static async Task PressEnterAsync(
            this ILocator locator)
        {
            await locator.PressAsync("Enter");
        }

        #endregion

        #region ===== TAB =====

        public static async Task PressTabAsync(
            this ILocator locator)
        {
            await locator.PressAsync("Tab");
        }

        #endregion

        #region ===== ESCAPE =====

        public static async Task PressEscapeAsync(
            this ILocator locator)
        {
            await locator.PressAsync("Escape");
        }

        #endregion

        #region ===== ARROW DOWN =====

        public static async Task ArrowDownAsync(
            this ILocator locator)
        {
            await locator.PressAsync("ArrowDown");
        }

        #endregion

        #region ===== ARROW UP =====

        public static async Task ArrowUpAsync(
            this ILocator locator)
        {
            await locator.PressAsync("ArrowUp");
        }

        #endregion

        #region ===== ARROW LEFT =====

        public static async Task ArrowLeftAsync(
            this ILocator locator)
        {
            await locator.PressAsync("ArrowLeft");
        }

        #endregion

        #region ===== ARROW RIGHT =====

        public static async Task ArrowRightAsync(
            this ILocator locator)
        {
            await locator.PressAsync("ArrowRight");
        }

        #endregion

        #region ===== FUNCTION KEY =====

        public static async Task PressFunctionKeyAsync(
            this IPage page,
            int functionKeyNumber)
        {
            await page.Keyboard.PressAsync(
                $"F{functionKeyNumber}");
        }

        #endregion

        #region ===== SHORTCUT KEY =====

        public static async Task PressShortcutAsync(
            this IPage page,
            string shortcutKey)
        {
            await page.Keyboard.PressAsync(shortcutKey);
        }

        #endregion

        #region ===== DYNAMICS SAVE =====

        public static async Task DynamicsSaveAsync(
            this IPage page)
        {
            await page.Keyboard.PressAsync("Control+S");
        }

        #endregion

        #region ===== DYNAMICS REFRESH =====

        public static async Task DynamicsRefreshAsync(
            this IPage page)
        {
            await page.Keyboard.PressAsync("Control+Shift+R");
        }

        #endregion

        #region ===== DYNAMICS SEARCH =====

        public static async Task DynamicsSearchAsync(
            this IPage page,
            string searchText)
        {
            await page.Keyboard.PressAsync("Control+K");

            await page.WaitForTimeoutAsync(1000);

            await page.Keyboard.TypeAsync(searchText);

            await page.Keyboard.PressAsync("Enter");
        }

        #endregion

        #region ===== DYNAMICS LOOKUP SEARCH =====

        public static async Task LookupSearchAsync(
            this ILocator locator,
            string searchText)
        {
            await locator.ClickAsync();

            await locator.FillAsync(searchText);

            await locator.PressAsync("ArrowDown");

            await locator.PressAsync("Enter");
        }

        #endregion

        #region ===== DYNAMICS GRID NAVIGATION =====

        public static async Task NavigateGridAsync(
            this ILocator locator,
            int moveDownCount = 1)
        {
            await locator.ClickAsync();

            for (int i = 0; i < moveDownCount; i++)
            {
                await locator.PressAsync("ArrowDown");
            }
        }

        #endregion

        #region ===== PAGE DOWN =====

        public static async Task PageDownAsync(
            this IPage page)
        {
            await page.Keyboard.PressAsync("PageDown");
        }

        #endregion

        #region ===== PAGE UP =====

        public static async Task PageUpAsync(
            this IPage page)
        {
            await page.Keyboard.PressAsync("PageUp");
        }

        #endregion

        #region ===== DELETE KEY =====

        public static async Task PressDeleteAsync(
            this ILocator locator)
        {
            await locator.PressAsync("Delete");
        }

        #endregion

        #region ===== BACKSPACE KEY =====

        public static async Task PressBackspaceAsync(
            this ILocator locator)
        {
            await locator.PressAsync("Backspace");
        }

        #endregion
    }
}