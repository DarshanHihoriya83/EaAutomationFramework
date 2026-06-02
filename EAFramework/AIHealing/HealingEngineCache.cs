using Microsoft.Playwright;
using System.Runtime.CompilerServices;

namespace EAFramework.AIHealing
{
    /// <summary>
    /// One <see cref="SelfHealingEngine"/> per page so healing state and JSON cache stay consistent.
    /// </summary>
    internal static class HealingEngineCache
    {
        private static readonly ConditionalWeakTable<IPage, SelfHealingEngine> Engines = new();

        public static SelfHealingEngine Get(IPage page) =>
            Engines.GetValue(page, static p => new SelfHealingEngine(p));
    }
}
