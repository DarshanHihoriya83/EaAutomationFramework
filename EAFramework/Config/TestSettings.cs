using EAFramework.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAFramework.Config
{
    public class TestSettings
    {
        public float? Timeout = PlaywrightDriverInitializer.DEFAULT_TIMEOUT;
        public string[]? Args { get; set; }
        public  bool Headless { get; set; }
        public  bool DevTools { get; set; }
        public int SlowMo {  get; set; }
        //public string Channel { get; set; }
        public DriverType DriverType { get; set; }
        public string Applicationurl { get; set; }

        /// <summary>When false, xUnit runs tests sequentially (one collection at a time).</summary>
        public bool EnableParallelExecution { get; set; }

        /// <summary>Max concurrent browser sessions when parallel execution is enabled.</summary>
        public int MaxParallelBrowsers { get; set; } = 1;

        /// <summary>Reserved cap for UI tab navigation per browser (documented for operators).</summary>
        public int MaxBrowserTabs { get; set; } = 1;

        /// <summary>When true, Playwright records video under each test artifact <c>video/</c> folder.</summary>
        public bool EnableVideo { get; set; } = true;
    }

    public enum DriverType
    {
        Chromium,
        Firefox,
        Edge,
        Chrome,
        Webkit
    }

}
