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
