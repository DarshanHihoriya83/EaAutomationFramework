using EAFramework.AIHealing;
using Microsoft.Playwright;

namespace EAFramework.Extension
{
    public static class ScreenshotExtension
    {
        #region ===== PAGE SCREENSHOT =====

        public static async Task<string> TakePageScreenshotAsync(
            this IPage page,
            string screenshotName,
            bool fullPage = true)
        {
            string folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Reports",
                "Screenshots");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName =
                $"{screenshotName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            string fullPath =
                Path.Combine(folderPath, fileName);

            await page.ScreenshotAsync(new()
            {
                Path = fullPath,
                FullPage = fullPage
            });

            return fullPath;
        }

        #endregion

        #region ===== ELEMENT SCREENSHOT =====

        public static async Task<string> TakeElementScreenshotAsync(
            this ILocator locator,
            string screenshotName)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);

            string folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Reports",
                "ElementScreenshots");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName =
                $"{screenshotName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            string fullPath =
                Path.Combine(folderPath, fileName);

            await locator.ScreenshotAsync(new()
            {
                Path = fullPath
            });

            return fullPath;
        }

        #endregion

        #region ===== FAILURE SCREENSHOT =====

        public static async Task<string> TakeFailureScreenshotAsync(
            this IPage page,
            string testName)
        {
            string folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Reports",
                "FailureScreenshots");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName =
                $"FAIL_{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            string fullPath =
                Path.Combine(folderPath, fileName);

            await page.ScreenshotAsync(new()
            {
                Path = fullPath,
                FullPage = true
            });

            return fullPath;
        }

        #endregion

        #region ===== STEP SCREENSHOT =====

        public static async Task<string> TakeStepScreenshotAsync(
            this IPage page,
            string stepName)
        {
            string folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Reports",
                "StepScreenshots");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName =
                $"STEP_{stepName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            string fullPath =
                Path.Combine(folderPath, fileName);

            await page.ScreenshotAsync(new()
            {
                Path = fullPath,
                FullPage = true
            });

            return fullPath;
        }

        #endregion

        #region ===== BASE64 SCREENSHOT =====

        public static async Task<string> GetBase64ScreenshotAsync(
            this IPage page)
        {
            byte[] screenshotBytes =
                await page.ScreenshotAsync(new()
                {
                    FullPage = true
                });

            return Convert.ToBase64String(screenshotBytes);
        }

        #endregion

        #region ===== DYNAMICS FORM SCREENSHOT =====

        public static async Task<string> TakeDynamicsFormScreenshotAsync(
            this IPage page,
            string formName)
        {
            var formContainer = page
                .Locator("[data-id='form-container']");

            string folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Reports",
                "DynamicsForms");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName =
                $"FORM_{formName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            string fullPath =
                Path.Combine(folderPath, fileName);

            if (await formContainer.CountAsync() > 0)
            {
                await formContainer.ScreenshotAsync(new()
                {
                    Path = fullPath
                });
            }
            else
            {
                await page.ScreenshotAsync(new()
                {
                    Path = fullPath,
                    FullPage = true
                });
            }

            return fullPath;
        }

        #endregion

        #region ===== GRID SCREENSHOT =====

        public static async Task<string> TakeGridScreenshotAsync(
            this ILocator grid,
            string gridName)
        {
            string folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Reports",
                "GridScreenshots");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName =
                $"GRID_{gridName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            string fullPath =
                Path.Combine(folderPath, fileName);

            await grid.ScreenshotAsync(new()
            {
                Path = fullPath
            });

            return fullPath;
        }

        #endregion

        #region ===== COMMAND BAR SCREENSHOT =====

        public static async Task<string> TakeCommandBarScreenshotAsync(
            this IPage page)
        {
            var commandBar = page
                .Locator("[data-id='command-bar']");

            string folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Reports",
                "CommandBar");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName =
                $"COMMAND_BAR_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            string fullPath =
                Path.Combine(folderPath, fileName);

            if (await commandBar.CountAsync() > 0)
            {
                await commandBar.ScreenshotAsync(new()
                {
                    Path = fullPath
                });
            }

            return fullPath;
        }

        #endregion

        #region ===== FULL PAGE WITH TIMESTAMP =====

        public static async Task<string> TakeFullPageScreenshotAsync(
            this IPage page)
        {
            string folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Reports",
                "FullPage");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName =
                $"FULLPAGE_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            string fullPath =
                Path.Combine(folderPath, fileName);

            await page.ScreenshotAsync(new()
            {
                Path = fullPath,
                FullPage = true
            });

            return fullPath;
        }

        #endregion

        #region ===== TAKE SCREENSHOT IF ELEMENT EXISTS =====

        public static async Task<string?> TakeScreenshotIfExistsAsync(
            this ILocator locator,
            string screenshotName)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);

            if (await locator.CountAsync() > 0)
            {
                return await locator
                    .TakeElementScreenshotAsync(screenshotName);
            }

            return null;
        }

        #endregion

        #region ===== HIGHLIGHT AND SCREENSHOT =====

        public static async Task<string> HighlightAndScreenshotAsync(
            this ILocator locator,
            string screenshotName)
        {
            locator = await LocatorHealingResolver.ResolveAsync(locator);
            var page = locator.Page;

            var elementHandle =
                await locator.ElementHandleAsync();

            if (elementHandle != null)
            {
                await page.EvaluateAsync(
                    @"(element) =>
                    {
                        element.style.border =
                            '3px solid red';
                    }",
                    elementHandle);
            }

            return await locator
                .TakeElementScreenshotAsync(screenshotName);
        }

        #endregion

        #region ===== BEFORE AND AFTER SCREENSHOT =====

        public static async Task<(string before, string after)>
            TakeBeforeAfterScreenshotAsync(
            this IPage page,
            string actionName,
            Func<Task> action)
        {
            string before =
                await page.TakePageScreenshotAsync(
                    $"{actionName}_Before");

            await action();

            string after =
                await page.TakePageScreenshotAsync(
                    $"{actionName}_After");

            return (before, after);
        }

        #endregion

        #region ===== CLEAN OLD SCREENSHOTS =====

        public static void CleanOldScreenshots(
            int olderThanDays = 7)
        {
            string reportsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Reports");

            if (!Directory.Exists(reportsFolder))
            {
                return;
            }

            var files = Directory.GetFiles(
                reportsFolder,
                "*.png",
                SearchOption.AllDirectories);

            foreach (var file in files)
            {
                FileInfo fileInfo = new(file);

                if (fileInfo.CreationTime <
                    DateTime.Now.AddDays(-olderThanDays))
                {
                    fileInfo.Delete();
                }
            }
        }

        #endregion
    }
}