using Microsoft.Playwright;
using System.Reflection;

namespace EAFramework.AIHealing
{
    /// <summary>
    /// Reads the combined Playwright selector from chained <see cref="ILocator"/> instances.
    /// </summary>
    public static class LocatorSelectorHelper
    {
        private static readonly FieldInfo? SelectorField =
            typeof(ILocator).Assembly
                .GetType("Microsoft.Playwright.Core.Locator")?
                .GetField("_selector", BindingFlags.Instance | BindingFlags.NonPublic);

        public static string? TryGetSelector(ILocator? locator)
        {
            if (locator == null || SelectorField == null)
            {
                return null;
            }

            if (!SelectorField.DeclaringType!.IsInstanceOfType(locator))
            {
                return null;
            }

            return SelectorField.GetValue(locator) as string;
        }
    }
}
