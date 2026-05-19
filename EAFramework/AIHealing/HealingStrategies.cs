using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace EAFramework.AIHealing
{
    public class HealingStrategies
    {
        private readonly IPage _page;

        private readonly LocatorAnalyzer _locatorAnalyzer;

        public HealingStrategies(IPage page)
        {
            _page = page;

            _locatorAnalyzer =
                new LocatorAnalyzer(_page);
        }

        #region ===== HEAL BY ID =====

        public async Task<ILocator?> HealByIdAsync(
            string failedLocator)
        {
            string cleaned =
                failedLocator.Replace("#", "");

            List<string> strategies = new()
            {
                $"#{cleaned}",
                $"[id='{cleaned}']",
                $"[id*='{cleaned}']",
                $"input[id*='{cleaned}']",
                $"button[id*='{cleaned}']"
            };

            return await ValidateStrategiesAsync(
                strategies);
        }

        #endregion

        #region ===== HEAL BY NAME =====

        public async Task<ILocator?> HealByNameAsync(
            string failedLocator)
        {
            string cleaned =
                ExtractLocatorValue(
                    failedLocator);

            List<string> strategies = new()
            {
                $"[name='{cleaned}']",
                $"input[name='{cleaned}']",
                $"textarea[name='{cleaned}']",
                $"select[name='{cleaned}']"
            };

            return await ValidateStrategiesAsync(
                strategies);
        }

        #endregion

        #region ===== HEAL BY CLASS =====

        public async Task<ILocator?> HealByClassAsync(
            string failedLocator)
        {
            string cleaned =
                failedLocator.Replace(".", "");

            List<string> strategies = new()
            {
                $".{cleaned}",
                $"[class*='{cleaned}']",
                $"div[class*='{cleaned}']",
                $"span[class*='{cleaned}']",
                $"button[class*='{cleaned}']"
            };

            return await ValidateStrategiesAsync(
                strategies);
        }

        #endregion

        #region ===== HEAL BY TEXT =====

        public async Task<ILocator?> HealByTextAsync(
            string text)
        {
            List<string> strategies = new()
            {
                $"text='{text}'",
                $"button:has-text('{text}')",
                $"a:has-text('{text}')",
                $"label:has-text('{text}')",
                $"div:has-text('{text}')"
            };

            return await ValidateStrategiesAsync(
                strategies);
        }

        #endregion

        #region ===== HEAL BY DATA-ID =====

        public async Task<ILocator?> HealByDataIdAsync(
            string failedLocator)
        {
            string cleaned =
                ExtractLocatorValue(
                    failedLocator);

            List<string> strategies = new()
            {
                $"[data-id='{cleaned}']",
                $"[data-id*='{cleaned}']",
                $"[data-testid*='{cleaned}']",
                $"button[data-id*='{cleaned}']",
                $"input[data-id*='{cleaned}']"
            };

            return await ValidateStrategiesAsync(
                strategies);
        }

        #endregion

        #region ===== HEAL BY ARIA =====

        public async Task<ILocator?> HealByAriaAsync(
            string failedLocator)
        {
            string cleaned =
                ExtractLocatorValue(
                    failedLocator);

            List<string> strategies = new()
            {
                $"[aria-label='{cleaned}']",
                $"[aria-label*='{cleaned}']",
                $"[role='button'][aria-label*='{cleaned}']",
                $"input[aria-label*='{cleaned}']"
            };

            return await ValidateStrategiesAsync(
                strategies);
        }

        #endregion

        #region ===== HEAL BY XPATH =====

        public async Task<ILocator?> HealByXpathAsync(
            string failedLocator)
        {
            string cleaned =
                ExtractLocatorValue(
                    failedLocator);

            List<string> strategies = new()
            {
                $"//*[@id='{cleaned}']",
                $"//*[contains(@id,'{cleaned}')]",
                $"//*[contains(@class,'{cleaned}')]",
                $"//*[contains(text(),'{cleaned}')]",
                $"//button[contains(text(),'{cleaned}')]"
            };

            return await ValidateStrategiesAsync(
                strategies);
        }

        #endregion

        #region ===== HEAL DYNAMIC ELEMENT =====

        public async Task<ILocator?> HealDynamicLocatorAsync(
            string failedLocator)
        {
            string cleaned =
                Regex.Replace(
                    failedLocator,
                    @"[\d]",
                    "");

            cleaned = cleaned
                .Replace("__", "_")
                .Replace("--", "-");

            List<string> strategies = new()
            {
                $"[id*='{cleaned}']",
                $"[class*='{cleaned}']",
                $"[data-id*='{cleaned}']",
                $"//*[contains(@id,'{cleaned}')]"
            };

            return await ValidateStrategiesAsync(
                strategies);
        }

        #endregion

        #region ===== DYNAMICS HEALING =====

        public async Task<ILocator?> HealDynamicsLocatorAsync(
            string failedLocator)
        {
            string cleaned =
                ExtractLocatorValue(
                    failedLocator);

            List<string> strategies = new()
            {
                $"[data-id*='{cleaned}']",
                $"[aria-label*='{cleaned}']",
                $"[title*='{cleaned}']",
                $"button[data-id*='{cleaned}']",
                $"input[data-id*='{cleaned}']",
                $"div[data-id*='{cleaned}']",
                $"span[data-id*='{cleaned}']"
            };

            return await ValidateStrategiesAsync(
                strategies);
        }

        #endregion

        #region ===== HEAL BY LABEL =====

        public async Task<ILocator?> HealByLabelAsync(
            string labelText)
        {
            try
            {
                var locator =
                    _page.GetByLabel(labelText);

                if (await locator.CountAsync() > 0)
                {
                    return locator;
                }
            }
            catch
            {
            }

            return null;
        }

        #endregion

        #region ===== HEAL BY PLACEHOLDER =====

        public async Task<ILocator?> HealByPlaceholderAsync(
            string placeholder)
        {
            try
            {
                var locator =
                    _page.GetByPlaceholder(
                        placeholder);

                if (await locator.CountAsync() > 0)
                {
                    return locator;
                }
            }
            catch
            {
            }

            return null;
        }

        #endregion

        #region ===== HEAL BY ROLE =====

        public async Task<ILocator?> HealByRoleAsync(
            AriaRole role,
            string name)
        {
            try
            {
                var locator =
                    _page.GetByRole(
                        role,
                        new()
                        {
                            Name = name
                        });

                if (await locator.CountAsync() > 0)
                {
                    return locator;
                }
            }
            catch
            {
            }

            return null;
        }

        #endregion

        #region ===== VALIDATE STRATEGIES =====

        public async Task<string?> TryHealToSelectorAsync(
            string failedLocator)
        {
            string? selector =
                await HealXPathNameToSelectorAsync(failedLocator);
            if (selector != null) return selector;

            selector = await HealHasTextToSelectorAsync(failedLocator);
            if (selector != null) return selector;

            selector = await HealPlaceholderToSelectorAsync(failedLocator);
            if (selector != null) return selector;

            selector = await HealIdToSelectorAsync(failedLocator);
            if (selector != null) return selector;

            selector = await HealNameToSelectorAsync(failedLocator);
            if (selector != null) return selector;

            selector = await HealClassToSelectorAsync(failedLocator);
            if (selector != null) return selector;

            selector = await HealDataIdToSelectorAsync(failedLocator);
            if (selector != null) return selector;

            selector = await HealAriaToSelectorAsync(failedLocator);
            if (selector != null) return selector;

            selector = await HealXpathToSelectorAsync(failedLocator);
            if (selector != null) return selector;

            selector = await HealDynamicsToSelectorAsync(failedLocator);
            if (selector != null) return selector;

            return await HealDynamicToSelectorAsync(failedLocator);
        }

        private async Task<string?> HealXPathNameToSelectorAsync(
            string failedLocator)
        {
            var strategies = BuildXPathNameStrategies(failedLocator);

            if (strategies.Count == 0)
            {
                return null;
            }

            return await ValidateStrategiesToSelectorAsync(strategies);
        }

        private async Task<string?> HealHasTextToSelectorAsync(string failedLocator)
        {
            var textMatch = Regex.Match(
                failedLocator,
                @"has-text\('([^']+)'\)");

            if (!textMatch.Success)
            {
                return null;
            }

            List<string> strategies = new()
            {
                $"text={textMatch.Groups[1].Value}",
                $"button:has-text('{textMatch.Groups[1].Value}')",
                $"a:has-text('{textMatch.Groups[1].Value}')",
                $"label:has-text('{textMatch.Groups[1].Value}')",
                $"div:has-text('{textMatch.Groups[1].Value}')"
            };

            return await ValidateStrategiesToSelectorAsync(strategies);
        }

        private async Task<string?> HealPlaceholderToSelectorAsync(
            string failedLocator)
        {
            var placeholderMatch = Regex.Match(
                failedLocator,
                @"placeholder=['""]([^'""]+)['""]",
                RegexOptions.IgnoreCase);

            if (!placeholderMatch.Success)
            {
                return null;
            }

            string placeholder = placeholderMatch.Groups[1].Value;

            List<string> strategies = new()
            {
                $"input[placeholder='{placeholder}']",
                $"[placeholder='{placeholder}']"
            };

            return await ValidateStrategiesToSelectorAsync(strategies);
        }

        private async Task<string?> HealIdToSelectorAsync(string failedLocator)
        {
            if (!failedLocator.StartsWith("#", StringComparison.Ordinal))
            {
                return null;
            }

            string cleaned = failedLocator.Replace("#", "");

            return await ValidateStrategiesToSelectorAsync(new List<string>
            {
                $"#{cleaned}",
                $"[id='{cleaned}']",
                $"[id*='{cleaned}']",
                $"input[id*='{cleaned}']",
                $"button[id*='{cleaned}']"
            });
        }

        private async Task<string?> HealNameToSelectorAsync(string failedLocator)
        {
            string cleaned = ExtractLocatorValue(failedLocator);

            return await ValidateStrategiesToSelectorAsync(new List<string>
            {
                $"[name='{cleaned}']",
                $"input[name='{cleaned}']",
                $"textarea[name='{cleaned}']",
                $"select[name='{cleaned}']"
            });
        }

        private async Task<string?> HealClassToSelectorAsync(string failedLocator)
        {
            if (!failedLocator.StartsWith(".", StringComparison.Ordinal))
            {
                return null;
            }

            string cleaned = failedLocator.Replace(".", "");

            return await ValidateStrategiesToSelectorAsync(new List<string>
            {
                $".{cleaned}",
                $"[class*='{cleaned}']",
                $"div[class*='{cleaned}']",
                $"span[class*='{cleaned}']",
                $"button[class*='{cleaned}']"
            });
        }

        private async Task<string?> HealDataIdToSelectorAsync(string failedLocator)
        {
            string cleaned = ExtractLocatorValue(failedLocator);

            return await ValidateStrategiesToSelectorAsync(new List<string>
            {
                $"[data-id='{cleaned}']",
                $"[data-id*='{cleaned}']",
                $"[data-testid*='{cleaned}']",
                $"button[data-id*='{cleaned}']",
                $"input[data-id*='{cleaned}']"
            });
        }

        private async Task<string?> HealAriaToSelectorAsync(string failedLocator)
        {
            string cleaned = ExtractLocatorValue(failedLocator);

            return await ValidateStrategiesToSelectorAsync(new List<string>
            {
                $"[aria-label='{cleaned}']",
                $"[aria-label*='{cleaned}']",
                $"[role='button'][aria-label*='{cleaned}']",
                $"input[aria-label*='{cleaned}']"
            });
        }

        private async Task<string?> HealXpathToSelectorAsync(string failedLocator)
        {
            string cleaned = ExtractLocatorValue(failedLocator);

            return await ValidateStrategiesToSelectorAsync(new List<string>
            {
                $"//*[@id='{cleaned}']",
                $"//*[contains(@id,'{cleaned}')]",
                $"//*[contains(@class,'{cleaned}')]",
                $"//*[contains(text(),'{cleaned}')]",
                $"//button[contains(text(),'{cleaned}')]"
            });
        }

        private async Task<string?> HealDynamicsToSelectorAsync(string failedLocator)
        {
            string cleaned = ExtractLocatorValue(failedLocator);

            return await ValidateStrategiesToSelectorAsync(new List<string>
            {
                $"[data-id*='{cleaned}']",
                $"[aria-label*='{cleaned}']",
                $"[title*='{cleaned}']",
                $"button[data-id*='{cleaned}']",
                $"input[data-id*='{cleaned}']",
                $"div[data-id*='{cleaned}']",
                $"span[data-id*='{cleaned}']"
            });
        }

        private async Task<string?> HealDynamicToSelectorAsync(string failedLocator)
        {
            string cleaned = Regex.Replace(failedLocator, @"[\d]", "");
            cleaned = cleaned.Replace("__", "_").Replace("--", "-");

            return await ValidateStrategiesToSelectorAsync(new List<string>
            {
                $"[id*='{cleaned}']",
                $"[class*='{cleaned}']",
                $"[data-id*='{cleaned}']",
                $"//*[contains(@id,'{cleaned}')]"
            });
        }

        private static List<string> BuildXPathNameStrategies(string failedLocator)
        {
            var nameMatch = Regex.Match(
                failedLocator,
                @"@name=['""]([^'""]+)['""]",
                RegexOptions.IgnoreCase);

            if (!nameMatch.Success)
            {
                return new List<string>();
            }

            string nameValue = nameMatch.Groups[1].Value;

            return new List<string>
            {
                $"input[name='{nameValue}']",
                $"[name='{nameValue}']",
                $"xpath=//input[@name='{nameValue}']",
                $"textarea[name='{nameValue}']",
                $"select[name='{nameValue}']"
            };
        }

        private async Task<ILocator?> ValidateStrategiesAsync(
            List<string> strategies)
        {
            string? selector = await ValidateStrategiesToSelectorAsync(strategies);

            return selector == null
                ? null
                : _page.Locator(selector);
        }

        private async Task<string?> ValidateStrategiesToSelectorAsync(
            List<string> strategies)
        {
            foreach (var strategy in strategies.Distinct())
            {
                try
                {
                    var locator = _page.Locator(strategy);
                    int count = await locator.CountAsync();

                    if (count > 0 && await locator.First.IsVisibleAsync())
                    {
                        return strategy;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        #endregion

        #region ===== EXTRACT LOCATOR VALUE =====

        private string ExtractLocatorValue(
            string locator)
        {
            return locator.Replace("#", "")
                          .Replace(".", "")
                          .Replace("[", "")
                          .Replace("]", "")
                          .Replace("'", "")
                          .Replace("\"", "")
                          .Trim();
        }

        #endregion

        #region ===== MASTER HEALING =====

        public async Task<ILocator?> HealLocatorAsync(
            string failedLocator)
        {
            ILocator? healedLocator;

            healedLocator = await HealByXPathNameAttributeAsync(failedLocator);
            if (healedLocator != null)
                return healedLocator;

            healedLocator = await HealByHasTextAsync(failedLocator);
            if (healedLocator != null)
                return healedLocator;

            healedLocator = await HealByPlaceholderFromSelectorAsync(failedLocator);
            if (healedLocator != null)
                return healedLocator;

            healedLocator = await HealByIdAsync(failedLocator);
            if (healedLocator != null)
                return healedLocator;

            healedLocator = await HealByNameAsync(failedLocator);
            if (healedLocator != null)
                return healedLocator;

            healedLocator = await HealByClassAsync(failedLocator);
            if (healedLocator != null)
                return healedLocator;

            healedLocator = await HealByDataIdAsync(failedLocator);
            if (healedLocator != null)
                return healedLocator;

            healedLocator = await HealByAriaAsync(failedLocator);
            if (healedLocator != null)
                return healedLocator;

            healedLocator = await HealByXpathAsync(failedLocator);
            if (healedLocator != null)
                return healedLocator;

            healedLocator = await HealDynamicsLocatorAsync(failedLocator);
            if (healedLocator != null)
                return healedLocator;

            healedLocator = await HealDynamicLocatorAsync(failedLocator);
            return healedLocator;
        }

        public async Task<ILocator?> HealByXPathNameAttributeAsync(
            string failedLocator)
        {
            var nameMatch = Regex.Match(
                failedLocator,
                @"@name=['""]([^'""]+)['""]",
                RegexOptions.IgnoreCase);

            if (!nameMatch.Success)
            {
                return null;
            }

            string nameValue = nameMatch.Groups[1].Value;

            List<string> strategies = new()
            {
                $"input[name='{nameValue}']",
                $"[name='{nameValue}']",
                $"xpath=//input[@name='{nameValue}']",
                $"textarea[name='{nameValue}']",
                $"select[name='{nameValue}']"
            };

            return await ValidateStrategiesAsync(strategies);
        }

        public async Task<ILocator?> HealByHasTextAsync(
            string failedLocator)
        {
            var textMatch = Regex.Match(
                failedLocator,
                @"has-text\('([^']+)'\)");

            if (!textMatch.Success)
            {
                return null;
            }

            string text = textMatch.Groups[1].Value;

            return await HealByTextAsync(text);
        }

        public async Task<ILocator?> HealByPlaceholderFromSelectorAsync(
            string failedLocator)
        {
            var placeholderMatch = Regex.Match(
                failedLocator,
                @"placeholder=['""]([^'""]+)['""]",
                RegexOptions.IgnoreCase);

            if (!placeholderMatch.Success)
            {
                return null;
            }

            return await HealByPlaceholderAsync(
                placeholderMatch.Groups[1].Value);
        }

        #endregion
    }
}