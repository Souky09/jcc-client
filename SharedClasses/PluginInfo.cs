using System.Collections.Generic;

namespace SharedClasses
{
    public class PluginInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public List<string> JCCSupportedVersions { get; set; }
        public List<string> Capabilities { get; set; }

    }
}