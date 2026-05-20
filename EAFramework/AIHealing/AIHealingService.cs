using Microsoft.Playwright;

namespace EAFramework.AIHealing
{
    /// <summary>
    /// Facade over <see cref="SelfHealingEngine"/> so healing, JSON store, and reports stay consistent.
    /// </summary>
    public class AIHealingService
    {
        private readonly SelfHealingEngine _engine;

        public AIHealingService(IPage page)
        {
            _engine = new SelfHealingEngine(page);
        }

        public Task<ILocator> FindElementAsync(string locator) =>
            _engine.FindElementAsync(locator);

        public Task<ILocator> HealFailedLocatorAsync(string failedLocator) =>
            _engine.FindElementAsync(failedLocator);

        public Task<ILocator> GenerateAILocatorAsync(string failedLocator) =>
            _engine.FindElementAsync(failedLocator);

        public void RemoveInvalidHealing(string locator) =>
            _engine.RemoveHealedMapping(locator);

        public void ClearHealingStore() =>
            _engine.ClearHealedMappings();

        public Dictionary<string, string> GetAllHealedLocators() =>
            new(_engine.GetHealedMappings());

        public async Task<bool> VerifyHealingAsync(string locator)
        {
            ILocator healed = await _engine.FindElementAsync(locator);

            return await healed.CountAsync() > 0;
        }

        public int GetHealingCount() =>
            _engine.GetHealedMappings().Count;

        public void ExportHealingReport(string reportPath) =>
            _engine.ExportHealingReport(reportPath);
    }
}
