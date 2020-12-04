using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedClasses
{
    public class ConfigInfo
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string EFT_IPAddress { get; set; }
        public List<PluginInfo> PluginsInfo { get; set; }
    }
}
