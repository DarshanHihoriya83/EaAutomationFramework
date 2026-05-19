using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace EAFramework.AIHealing
{
    public class LocatorAnalyzer
    {
        private static readonly Regex IdTokenRegex =
            new(@"#([a-zA-Z0-9_-]+)", RegexOptions.Compiled);

        private static readonly Regex ClassTokenRegex =
            new(@"\.([a-zA-Z0-9_-]+)", RegexOptions.Compiled);

        private static readonly Regex NameAttributeRegex =
            new(@"name=['""]([^'""]+)['""]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PlaceholderAttributeRegex =
            new(@"placeholder=['""]([^'""]+)['""]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IPage _page;

        public LocatorAnalyzer(IPage page)
        {
            _page = page;
        }

        public async Task<List<string>> AnalyzeLocatorAsync(
            string failedLocator)
        {
            List<string> healedLocators = new();

            try
            {
                healedLocators.AddRange(GenerateIdBasedLocators(failedLocator));
                healedLocators.AddRange(GenerateNameBasedLocators(failedLocator));
                healedLocators.AddRange(GenerateClassBasedLocators(failedLocator));
                healedLocators.AddRange(GeneratePlaceholderBasedLocators(failedLocator));
                healedLocators.AddRange(GenerateDataIdLocators(failedLocator));
                healedLocators.AddRange(GenerateXpathAlternatives(failedLocator));
                healedLocators.AddRange(GenerateDynamicLocatorPatterns(failedLocator));
            }
            catch (RegexParseException)
            {
                // Skip invalid regex-derived candidates; other strategies may still work.
            }

            return await ValidateLocatorsAsync(healedLocators);
        }

        private List<string> GenerateIdBasedLocators(string failedLocator)
        {
            List<string> locators = new();
            var idMatch = IdTokenRegex.Match(failedLocator);

            if (!idMatch.Success)
            {
                return locators;
            }

            string idValue = idMatch.Groups[1].Value;

            locators.Add($"#{idValue}");
            locators.Add($"[id='{idValue}']");
            locators.Add($"[id*='{idValue}']");
            locators.Add($"input[id*='{idValue}']");
            locators.Add($"button[id*='{idValue}']");

            return locators;
        }

        private List<string> GenerateNameBasedLocators(string failedLocator)
        {
            List<string> locators = new();
            var nameMatch = NameAttributeRegex.Match(failedLocator);

            if (!nameMatch.Success)
            {
                return locators;
            }

            string nameValue = nameMatch.Groups[1].Value;

            locators.Add($"[name='{nameValue}']");
            locators.Add($"input[name='{nameValue}']");
            locators.Add($"textarea[name='{nameValue}']");
            locators.Add($"select[name='{nameValue}']");
            locators.Add($"xpath=//input[@name='{nameValue}']");

            return locators;
        }

        private List<string> GenerateClassBasedLocators(string failedLocator)
        {
            List<string> locators = new();
            var classMatch = ClassTokenRegex.Match(failedLocator);

            if (!classMatch.Success)
            {
                return locators;
            }

            string classValue = classMatch.Groups[1].Value;

            locators.Add($".{classValue}");
            locators.Add($"[class*='{classValue}']");
            locators.Add($"div.{classValue}");
            locators.Add($"button.{classValue}");

            return locators;
        }

        private List<string> GeneratePlaceholderBasedLocators(string failedLocator)
        {
            List<string> locators = new();
            var placeholderMatch = PlaceholderAttributeRegex.Match(failedLocator);

            if (!placeholderMatch.Success)
            {
                return locators;
            }

            string placeholder = placeholderMatch.Groups[1].Value;

            locators.Add($"input[placeholder='{placeholder}']");
            locators.Add($"[placeholder='{placeholder}']");
            locators.Add($"form.search-card input[placeholder='{placeholder}']");

            return locators;
        }

        private List<string> GenerateDataIdLocators(string failedLocator)
        {
            List<string> locators = new();

            var idMatch = IdTokenRegex.Match(failedLocator);
            if (!idMatch.Success)
            {
                return locators;
            }

            string idValue = idMatch.Groups[1].Value;

            locators.Add($"[data-id='{idValue}']");
            locators.Add($"[data-id*='{idValue}']");
            locators.Add($"[data-testid='{idValue}']");
            locators.Add($"[aria-label*='{idValue}']");

            return locators;
        }

        private List<string> GenerateXpathAlternatives(string failedLocator)
        {
            List<string> locators = new();

            var nameMatch = NameAttributeRegex.Match(failedLocator);
            if (nameMatch.Success)
            {
                string nameValue = nameMatch.Groups[1].Value;
                locators.Add($"xpath=//input[@name='{nameValue}']");
                locators.Add($"xpath=//*[@name='{nameValue}']");
            }

            var idMatch = IdTokenRegex.Match(failedLocator);
            if (idMatch.Success)
            {
                string idValue = idMatch.Groups[1].Value;
                locators.Add($"xpath=//*[@id='{idValue}']");
                locators.Add($"xpath=//*[contains(@id,'{idValue}')]");
            }

            return locators;
        }

        private static List<string> GenerateDynamicLocatorPatterns(string failedLocator)
        {
            List<string> locators = new();

            string cleaned = Regex.Replace(failedLocator, @"\d", "");
            cleaned = cleaned.Replace("__", "_").Replace("--", "-");

            if (cleaned.Length < 3)
            {
                return locators;
            }

            locators.Add($"[id*='{cleaned}']");
            locators.Add($"[class*='{cleaned}']");
            locators.Add($"[data-id*='{cleaned}']");

            return locators;
        }

        public async Task<List<string>> ValidateLocatorsAsync(List<string> locators)
        {
            List<string> validLocators = new();

            foreach (var locatorText in locators.Distinct(StringComparer.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(locatorText))
                {
                    continue;
                }

                try
                {
                    var locator = _page.Locator(locatorText);
                    int count = await locator.CountAsync();

                    if (count > 0)
                    {
                        validLocators.Add(locatorText);
                    }
                }
                catch
                {
                }
            }

            return validLocators;
        }

        public async Task<string?> GetBestMatchingLocatorAsync(string failedLocator)
        {
            try
            {
                var validLocators = await AnalyzeLocatorAsync(failedLocator);
                return validLocators.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public double CalculateSimilarityScore(string original, string candidate)
        {
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(candidate))
            {
                return 0;
            }

            int matches = 0;

            foreach (char c in original)
            {
                if (candidate.Contains(c))
                {
                    matches++;
                }
            }

            return (double)matches / Math.Max(original.Length, candidate.Length);
        }

        public async Task<string?> FindClosestMatchAsync(
            string failedLocator,
            List<string> candidateLocators)
        {
            if (candidateLocators.Count == 0)
            {
                return null;
            }

            Dictionary<string, double> scores = new();

            foreach (var locator in candidateLocators)
            {
                double score = CalculateSimilarityScore(failedLocator, locator);
                scores[locator] = score;
            }

            var bestMatch = scores.OrderByDescending(x => x.Value).FirstOrDefault();

            if (bestMatch.Value <= 0.4)
            {
                return null;
            }

            try
            {
                var locator = _page.Locator(bestMatch.Key);
                if (await locator.CountAsync() > 0)
                {
                    return bestMatch.Key;
                }
            }
            catch
            {
            }

            return null;
        }

        public async Task<List<string>> AnalyzeDynamicsLocatorAsync(string failedLocator)
        {
            List<string> locators = new();

            var idMatch = IdTokenRegex.Match(failedLocator);
            if (!idMatch.Success)
            {
                return locators;
            }

            string cleaned = idMatch.Groups[1].Value;

            locators.Add($"[data-id*='{cleaned}']");
            locators.Add($"[aria-label*='{cleaned}']");
            locators.Add($"[title*='{cleaned}']");
            locators.Add($"button[data-id*='{cleaned}']");
            locators.Add($"input[data-id*='{cleaned}']");

            return await ValidateLocatorsAsync(locators);
        }
    }
}
