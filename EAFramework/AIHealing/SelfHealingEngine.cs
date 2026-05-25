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

            _healingFilePath = HealingPaths.ResolveFailedLocatorStorePath();

            EnsureHealingFileExists();
        }

        public IReadOnlyDictionary<string, string> GetHealedMappings() =>
            LoadHealedLocators();

        public void RemoveHealedMapping(string originalSelector)
        {
            Dictionary<string, string> locators = LoadHealedLocators();

            if (locators.Remove(originalSelector))
            {
                PersistHealedLocators(locators);
            }
        }

        public void ClearHealedMappings()
        {
            PersistHealedLocators(new Dictionary<string, string>());
        }

        public void ExportHealingReport(string reportPath)
        {
            Dictionary<string, string> healingData = LoadHealedLocators();

            List<string> reportLines = new()
            {
                "========== AI SELF-HEALING REPORT ==========",
                $"Generated On : {DateTime.Now:O}",
                $"Store File    : {_healingFilePath}",
                "",
            };

            foreach (KeyValuePair<string, string> item in healingData)
            {
                reportLines.Add($"Original Locator : {item.Key}");
                reportLines.Add($"Healed Selector  : {item.Value}");
                reportLines.Add("------------------------------------");
            }

            File.WriteAllLines(reportPath, reportLines);
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

            if (healedLocators.TryGetValue(failedSelector, out string? cachedSelector))
            {
                if (IsSuspiciousHealedPair(failedSelector, cachedSelector))
                {
                    RemoveHealedMapping(failedSelector);
                }
                else if (await IsSelectorUsableAsync(cachedSelector))
                {
                    return _page.Locator(cachedSelector);
                }
            }

            string? healedSelector =
                await TryHealPlaceholderInputAsync(failedSelector);

            healedSelector ??=
                await TryHealIdTypoSuffixAsync(failedSelector);

            healedSelector ??=
                await TryBuiltInAlternativesAsync(failedSelector);

            healedSelector ??=
                await TryHealEmployeeTableEditAsync(failedSelector);

            healedSelector ??=
                await TryPlaywrightSemanticLocatorAsync(failedSelector);

            healedSelector ??=
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

            healedSelector =
                DiscardSuspiciousHeal(failedSelector, healedSelector);

            if (healedSelector != null
                && await CanUseHealedSelectorAsync(failedSelector, healedSelector))
            {
                SaveHealedLocator(failedSelector, healedSelector);
                return _page.Locator(healedSelector);
            }

            throw new Exception(
                $"Unable to locate element after self-healing: {failedSelector}");
        }

        /// <summary>
        /// Strict visibility when possible; fall back to DOM presence for healed controls so healing
        /// does not lose valid <c>input[placeholder='…']</c> targets during animations or layout.
        /// </summary>
        private async Task<bool> CanUseHealedSelectorAsync(
            string failedSelector,
            string healedSelector)
        {
            if (DiscardSuspiciousHeal(failedSelector, healedSelector) == null)
            {
                return false;
            }

            if (await IsSelectorUsableAsync(healedSelector))
            {
                return true;
            }

            ILocator loc = _page.Locator(healedSelector);

            return await loc.CountAsync() > 0;
        }

        private static string? DiscardSuspiciousHeal(
            string failedSelector,
            string? healedSelector)
        {
            if (healedSelector == null
                || IsSuspiciousHealedPair(failedSelector, healedSelector))
            {
                return null;
            }

            return healedSelector;
        }

        /// <summary>
        /// Detects bad historical heals (e.g. mapping a search <c>input</c> failure to a visible <c>.search-card</c> container).
        /// </summary>
        private static bool IsSuspiciousHealedPair(
            string failedSelector,
            string healedSelector)
        {
            string h = healedSelector.Trim();

            if (h.Length == 0)
            {
                return false;
            }

            bool inputish =
                failedSelector.Contains("input[", StringComparison.OrdinalIgnoreCase)
                || failedSelector.Contains("placeholder=", StringComparison.OrdinalIgnoreCase);

            if (inputish)
            {
                if (h.Contains("input[", StringComparison.OrdinalIgnoreCase)
                    || h.Contains("placeholder=", StringComparison.OrdinalIgnoreCase)
                    || h.Contains("@name=", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // lone class selector — usually a layout wrapper, not the control.
                return h.StartsWith(".", StringComparison.Ordinal)
                       && h.IndexOf(' ') < 0
                       && h.IndexOf(">", StringComparison.Ordinal) < 0;
            }

            bool idish = failedSelector.Contains('#', StringComparison.Ordinal);

            if (idish
                && h.StartsWith(".", StringComparison.Ordinal)
                && !h.Contains('#', StringComparison.Ordinal)
                && !h.Contains("input", StringComparison.OrdinalIgnoreCase)
                && !h.Contains("select", StringComparison.OrdinalIgnoreCase)
                && !h.Contains("textarea", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Heals mistaken ids such as <c>#Salary32</c> → <c>#Salary</c> (trailing digits on a known field id).
        /// </summary>
        private async Task<string?> TryHealIdTypoSuffixAsync(string failedSelector)
        {
            var idMatch = Regex.Match(
                failedSelector,
                @"#([A-Za-z][A-Za-z0-9_]*?)(\d+)",
                RegexOptions.IgnoreCase);

            if (!idMatch.Success)
            {
                return null;
            }

            string baseId = idMatch.Groups[1].Value;
            string prefix = failedSelector[..idMatch.Index];
            string suffix = failedSelector[(idMatch.Index + idMatch.Length)..];

            List<string> candidates = new()
            {
                $"{prefix}#{baseId}{suffix}".Trim(),
                $"#{baseId}",
                $"[id='{baseId}']",
                $".form-card-body .form-row-2 #{baseId}",
                $"input#{baseId}",
            };

            foreach (string candidate in candidates.Distinct())
            {
                if (DiscardSuspiciousHeal(failedSelector, candidate) == null)
                {
                    continue;
                }

                if (await IsSelectorUsableAsync(candidate))
                {
                    return candidate;
                }

                ILocator loc = _page.Locator(candidate);

                if (await loc.CountAsync() > 0)
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// Heals employee-list edit links (row by <c>.emp-name</c> + <c>.btn-edit</c>), matching live DOM structure.
        /// </summary>
        private async Task<string?> TryHealEmployeeTableEditAsync(string failedSelector)
        {
            if (!failedSelector.Contains("emp-name", StringComparison.OrdinalIgnoreCase)
                || !failedSelector.Contains("btn-edit", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var nameMatch = Regex.Match(
                failedSelector,
                @"normalize-space\(\)\s*=\s*'([^']+)'",
                RegexOptions.IgnoreCase);

            if (!nameMatch.Success)
            {
                nameMatch = Regex.Match(
                    failedSelector,
                    @"normalize-space\(\)\s*=\s*""([^""]+)""",
                    RegexOptions.IgnoreCase);
            }

            if (!nameMatch.Success)
            {
                return null;
            }

            string employeeName = nameMatch.Groups[1].Value;

            List<string> candidates = new()
            {
                $".employee-table-card table tbody tr:has(.emp-name:text-is('{employeeName}')) .action-group a.btn-edit",
                $".employee-table-card table tbody tr >> .emp-name:text-is('{employeeName}') >> .. >> .action-group a.btn-edit",
                $"xpath=//div[contains(@class,'employee-table-card')]//table//tbody//tr[.//*[contains(@class,'emp-name') and normalize-space()='{employeeName}']]//a[contains(@class,'btn-edit')]",
            };

            foreach (string candidate in candidates)
            {
                if (DiscardSuspiciousHeal(failedSelector, candidate) == null)
                {
                    continue;
                }

                ILocator loc = _page.Locator(candidate);

                if (await loc.CountAsync() == 0)
                {
                    continue;
                }

                try
                {
                    await loc.First.WaitForAsync(new()
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 10000
                    });

                    return candidate;
                }
                catch
                {
                    if (await loc.First.CountAsync() > 0)
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Prefer a concrete input placeholder match (avoids healing to a broad visible container).
        /// </summary>
        private async Task<string?> TryHealPlaceholderInputAsync(string failedSelector)
        {
            var placeholderMatch = Regex.Match(
                failedSelector,
                @"placeholder=['""]([^'""]+)['""]",
                RegexOptions.IgnoreCase);

            if (!placeholderMatch.Success)
            {
                return null;
            }

            string placeholder = placeholderMatch.Groups[1].Value;
            List<string> candidates = new()
            {
                $"input[placeholder='{placeholder}']",
            };

                if (failedSelector.Contains(
                        "form.search-card",
                        StringComparison.OrdinalIgnoreCase))
                {
                    candidates.Add("form.search-card input[name='searchTerm']");
                }

            foreach (string candidate in candidates)
            {
                ILocator loc = _page.Locator(candidate);

                if (await loc.CountAsync() == 0)
                {
                    continue;
                }

                try
                {
                    await loc.First.WaitForAsync(new()
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 15000
                    });

                    return candidate;
                }
                catch
                {
                    if (await loc.First.CountAsync() > 0)
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private async Task<string?> TryBuiltInAlternativesAsync(
            string failedSelector)
        {
            foreach (string candidate in BuildAlternativeSelectors(failedSelector))
            {
                if (DiscardSuspiciousHeal(failedSelector, candidate) == null)
                {
                    continue;
                }

                if (await IsSelectorUsableAsync(candidate))
                {
                    return candidate;
                }

                ILocator loc = _page.Locator(candidate);

                if (await loc.CountAsync() > 0)
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

            var placeholderMatch = Regex.Match(
                failedSelector,
                @"placeholder=['""]([^'""]+)['""]",
                RegexOptions.IgnoreCase);

            if (placeholderMatch.Success)
            {
                alternatives.Add(
                    $"input[placeholder='{placeholderMatch.Groups[1].Value}']");

                if (failedSelector.Contains(
                        "form.search-card",
                        StringComparison.OrdinalIgnoreCase))
                {
                    alternatives.Add("form.search-card input[name='searchTerm']");
                }
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
            try
            {
                string json = File.ReadAllText(_healingFilePath);

                return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                       ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        private void SaveHealedLocator(
            string originalSelector,
            string healedSelector)
        {
            Dictionary<string, string> locators = LoadHealedLocators();
            locators[originalSelector] = healedSelector;

            PersistHealedLocators(locators);
            AppendAutoHealReport(originalSelector, healedSelector);
        }

        private void PersistHealedLocators(Dictionary<string, string> locators)
        {
            string json = JsonSerializer.Serialize(
                locators,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(_healingFilePath, json);
        }

        private static void AppendAutoHealReport(
            string originalSelector,
            string healedSelector)
        {
            string reportPath = HealingPaths.ResolveAutoHealReportPath();
            string? folder = Path.GetDirectoryName(reportPath);

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string block =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]" + Environment.NewLine
                + $"  Original: {originalSelector}" + Environment.NewLine
                + $"  Healed  : {healedSelector}" + Environment.NewLine
                + Environment.NewLine;

            File.AppendAllText(reportPath, block);
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
