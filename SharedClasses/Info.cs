using System.Collections.Generic;

namespace SharedClasses
{
    public class Info
    {
        public string TerminalID { get; set; }
        public string StoreId { get; set; }
        public string Merchant { get; set; }
        public PluginInfo PluginInfo { get; set; }
        public string Version { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}