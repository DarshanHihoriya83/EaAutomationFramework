using Microsoft.Playwright;
using System.Text.Json;

namespace EAFramework.AIHealing
{
    public class AIHealingService
    {
        private readonly IPage _page;

        private readonly LocatorAnalyzer _locatorAnalyzer;

        private readonly HealingStrategies _healingStrategies;

        private readonly string _healingStoragePath;

        public AIHealingService(IPage page)
        {
            _page = page;

            _locatorAnalyzer =
                new LocatorAnalyzer(_page);

            _healingStrategies =
                new HealingStrategies(_page);

            _healingStoragePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "AIHealing",
                "FailedLocatorStore.json");

            EnsureStorageExists();
        }

        #region ===== FIND ELEMENT WITH AI HEALING =====

        public async Task<ILocator> FindElementAsync(
            string locator)
        {
            try
            {
                var originalLocator =
                    _page.Locator(locator);

                if (await originalLocator.CountAsync() > 0)
                {
                    return originalLocator;
                }
            }
            catch
            {
            }

            return await HealFailedLocatorAsync(locator);
        }

        #endregion

        #region ===== HEAL FAILED LOCATOR =====

        public async Task<ILocator> HealFailedLocatorAsync(
            string failedLocator)
        {
            var healedLocatorFromStore =
                await GetStoredHealedLocatorAsync(
                    failedLocator);

            if (healedLocatorFromStore != null)
            {
                return healedLocatorFromStore;
            }

            var healedLocator =
                await _healingStrategies
                    .HealLocatorAsync(
                        failedLocator);

            if (healedLocator != null)
            {
                string healedSelector =
                    await healedLocator
                        .EvaluateAsync<string>(
                            "element => element.outerHTML");

                SaveHealingResult(
                    failedLocator,
                    healedSelector);

                return healedLocator;
            }

            var aiLocator =
                await GenerateAILocatorAsync(
                    failedLocator);

            if (aiLocator != null)
            {
                return aiLocator;
            }

            throw new Exception(
                $"AI Healing failed for locator: {failedLocator}");
        }

        #endregion

        #region ===== GENERATE AI LOCATOR =====

        public async Task<ILocator?> GenerateAILocatorAsync(
            string failedLocator)
        {
            var analyzedLocators =
                await _locatorAnalyzer
                    .AnalyzeLocatorAsync(
                        failedLocator);

            foreach (var locator in analyzedLocators)
            {
                try
                {
                    var healedLocator =
                        _page.Locator(locator);

                    if (await healedLocator.CountAsync() > 0)
                    {
                        SaveHealingResult(
                            failedLocator,
                            locator);

                        return healedLocator;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        #endregion

        #region ===== STORE HEALED LOCATOR =====

        private void SaveHealingResult(
            string failedLocator,
            string healedLocator)
        {
            var healingData =
                LoadHealingStore();

            if (!healingData.ContainsKey(failedLocator))
            {
                healingData.Add(
                    failedLocator,
                    healedLocator);
            }
            else
            {
                healingData[failedLocator] =
                    healedLocator;
            }

            string json =
                JsonSerializer.Serialize(
                    healingData,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                _healingStoragePath,
                json);
        }

        #endregion

        #region ===== LOAD HEALING STORE =====

        private Dictionary<string, string> LoadHealingStore()
        {
            if (!File.Exists(_healingStoragePath))
            {
                return new Dictionary<string, string>();
            }

            string json =
                File.ReadAllText(_healingStoragePath);

            return JsonSerializer.Deserialize
                <Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>();
        }

        #endregion

        #region ===== GET STORED HEALED LOCATOR =====

        private async Task<ILocator?> GetStoredHealedLocatorAsync(
            string failedLocator)
        {
            var healingStore =
                LoadHealingStore();

            if (healingStore.ContainsKey(failedLocator))
            {
                string healedLocator =
                    healingStore[failedLocator];

                try
                {
                    var locator =
                        _page.Locator(healedLocator);

                    if (await locator.CountAsync() > 0)
                    {
                        return locator;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        #endregion

        #region ===== REMOVE INVALID HEALING =====

        public void RemoveInvalidHealing(
            string locator)
        {
            var healingStore =
                LoadHealingStore();

            if (healingStore.ContainsKey(locator))
            {
                healingStore.Remove(locator);

                string json =
                    JsonSerializer.Serialize(
                        healingStore,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                File.WriteAllText(
                    _healingStoragePath,
                    json);
            }
        }

        #endregion

        #region ===== CLEAR HEALING STORE =====

        public void ClearHealingStore()
        {
            File.WriteAllText(
                _healingStoragePath,
                "{}");
        }

        #endregion

        #region ===== GET ALL HEALED LOCATORS =====

        public Dictionary<string, string>
            GetAllHealedLocators()
        {
            return LoadHealingStore();
        }

        #endregion

        #region ===== VERIFY HEALING =====

        public async Task<bool> VerifyHealingAsync(
            string locator)
        {
            try
            {
                var healed =
                    await FindElementAsync(locator);

                return await healed.CountAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region ===== GET HEALING COUNT =====

        public int GetHealingCount()
        {
            return LoadHealingStore().Count;
        }

        #endregion

        #region ===== EXPORT HEALING REPORT =====

        public void ExportHealingReport(
            string reportPath)
        {
            var healingData =
                LoadHealingStore();

            List<string> reportLines = new();

            reportLines.Add(
                "========== AI HEALING REPORT ==========");

            reportLines.Add(
                $"Generated On : {DateTime.Now}");

            reportLines.Add("");

            foreach (var item in healingData)
            {
                reportLines.Add(
                    $"Original Locator : {item.Key}");

                reportLines.Add(
                    $"Healed Locator   : {item.Value}");

                reportLines.Add(
                    "------------------------------------");
            }

            File.WriteAllLines(
                reportPath,
                reportLines);
        }

        #endregion

        #region ===== ENSURE STORAGE EXISTS =====

        private void EnsureStorageExists()
        {
            string? directory =
                Path.GetDirectoryName(
                    _healingStoragePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            if (!File.Exists(_healingStoragePath))
            {
                File.WriteAllText(
                    _healingStoragePath,
                    "{}");
            }
        }

        #endregion
    }
}