using Microsoft.Playwright;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EAFramework.AIHealing
{
    public class SelfHealingEngine
    {
        private readonly IPage _page;
        private readonly HealingStrategies _healingStrategies;
        private readonly LocatorAnalyzer _locatorAnalyzer;
        private readonly string _healingFilePath;

        public SelfHealingEngine(IPage page)
        {
            _page = page;
            _healingStrategies = new HealingStrategies(page);
            _locatorAnalyzer = new LocatorAnalyzer(page);

            _healingFilePath = Path.Combine(
                AppContext.BaseDirectory,
                "AIHealing",
                "FailedLocatorStore.json");

            EnsureHealingFileExists();
        }

        public async Task<ILocator> FindElementAsync(string selector)
        {
            if (await IsSelectorUsableAsync(selector))
            {
                return _page.Locator(selector);
            }

            return await HealLocatorAsync(selector);
        }

        private async Task<bool> IsSelectorUsableAsync(string selector)
        {
            try
            {
                var locator = _page.Locator(selector);
                if (await locator.CountAsync() == 0)
                {
                    return false;
                }

                try
                {
                    await locator.First.WaitForAsync(new()
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 5000
                    });
                    return true;
                }
                catch
                {
                    // Element exists in DOM; PageBase extensions will wait before action.
                    return await locator.First.CountAsync() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<ILocator> HealLocatorAsync(string failedSelector)
        {
            var healedLocators = LoadHealedLocators();

            if (healedLocators.TryGetValue(failedSelector, out string? cachedSelector)
                && await IsSelectorUsableAsync(cachedSelector))
            {
                return _page.Locator(cachedSelector);
            }

            string? healedSelector =
                await _healingStrategies.TryHealToSelectorAsync(failedSelector);

            try
            {
                healedSelector ??=
                    await _locatorAnalyzer.GetBestMatchingLocatorAsync(failedSelector);
            }
            catch (RegexParseException)
            {
                // Analyzer regex safety net; continue with built-in fallbacks.
            }

            healedSelector ??=
                await TryBuiltInAlternativesAsync(failedSelector);

            healedSelector ??=
                await TryPlaywrightSemanticLocatorAsync(failedSelector);

            if (healedSelector != null
                && await IsSelectorUsableAsync(healedSelector))
            {
                SaveHealedLocator(failedSelector, healedSelector);
                return _page.Locator(healedSelector);
            }

            throw new Exception(
                $"Unable to locate element after self-healing: {failedSelector}");
        }

        private async Task<string?> TryBuiltInAlternativesAsync(
            string failedSelector)
        {
            foreach (var candidate in BuildAlternativeSelectors(failedSelector))
            {
                if (await IsSelectorUsableAsync(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static IEnumerable<string> BuildAlternativeSelectors(
            string failedSelector)
        {
            var alternatives = new List<string>();

            if (failedSelector.StartsWith("#", StringComparison.Ordinal))
            {
                string idValue = failedSelector[1..];
                alternatives.Add($"[id*='{idValue}']");
                alternatives.Add($"[name*='{idValue}']");
                alternatives.Add($"input#{idValue}");
            }

            if (failedSelector.StartsWith(".", StringComparison.Ordinal))
            {
                string classValue = failedSelector[1..];
                alternatives.Add($"[class*='{classValue}']");
            }

            var nameMatch = Regex.Match(
                failedSelector,
                @"@name=['""]([^'""]+)['""]",
                RegexOptions.IgnoreCase);

            if (nameMatch.Success)
            {
                string nameValue = nameMatch.Groups[1].Value;
                alternatives.Add($"input[name='{nameValue}']");
                alternatives.Add($"[name='{nameValue}']");
                alternatives.Add($"xpath=//input[@name='{nameValue}']");
            }

            var textMatch = Regex.Match(
                failedSelector,
                @"has-text\('([^']+)'\)");

            if (textMatch.Success)
            {
                string text = textMatch.Groups[1].Value;
                alternatives.Add($"text={text}");
                alternatives.Add($"a:has-text('{text}')");
                alternatives.Add($"button:has-text('{text}')");
            }

            var placeholderMatch = Regex.Match(
                failedSelector,
                @"placeholder=['""]([^'""]+)['""]",
                RegexOptions.IgnoreCase);

            if (placeholderMatch.Success)
            {
                alternatives.Add(
                    $"input[placeholder='{placeholderMatch.Groups[1].Value}']");
            }

            return alternatives.Distinct();
        }

        private async Task<string?> TryPlaywrightSemanticLocatorAsync(
            string failedSelector)
        {
            var placeholderMatch = Regex.Match(
                failedSelector,
                @"placeholder=['""]([^'""]+)['""]",
                RegexOptions.IgnoreCase);

            if (placeholderMatch.Success)
            {
                string placeholder = placeholderMatch.Groups[1].Value;
                var byPlaceholder = _page.GetByPlaceholder(placeholder);

                if (await byPlaceholder.CountAsync() > 0)
                {
                    return $"input[placeholder='{placeholder}']";
                }
            }

            var textMatch = Regex.Match(
                failedSelector,
                @"has-text\('([^']+)'\)");

            if (textMatch.Success)
            {
                string text = textMatch.Groups[1].Value;
                var byText = _page.GetByText(text, new() { Exact = true });

                if (await byText.CountAsync() > 0)
                {
                    return $"text={text}";
                }
            }

            return null;
        }

        private Dictionary<string, string> LoadHealedLocators()
        {
            string json = File.ReadAllText(_healingFilePath);

            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>();
        }

        private void SaveHealedLocator(
            string originalSelector,
            string healedSelector)
        {
            Dictionary<string, string> locators = LoadHealedLocators();
            locators[originalSelector] = healedSelector;

            string json = JsonSerializer.Serialize(
                locators,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(_healingFilePath, json);
        }

        private void EnsureHealingFileExists()
        {
            string? folder = Path.GetDirectoryName(_healingFilePath);

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (!File.Exists(_healingFilePath))
            {
                File.WriteAllText(_healingFilePath, "{}");
            }
        }
    }
}
