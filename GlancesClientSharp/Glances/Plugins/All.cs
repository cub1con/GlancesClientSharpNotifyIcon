using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlancesClientSharp.Glances.Plugins
{
    [GlancesPluginAttribute("all")]
    public class All
    {
        //[JsonProperty(PropertyName = "core")]
        //public Core Core { get; set; }
        [JsonProperty(PropertyName = "cpu")]
        public Cpu Cpu { get; set; }

        [JsonProperty(PropertyName = "network")]
        public List<Network> Network { get; set; }

        [JsonProperty(PropertyName ="fs")]
        public List<FileSystem> FileSystem { get; set; }

        [JsonProperty(PropertyName = "mem")]
        public Memory Memory { get; set; }

        [JsonProperty(PropertyName = "uptime")]
        public string Uptime { get; set; }

        [JsonProperty(PropertyName = "system")]
        public System System { get; set; }

        /*
        [JsonProperty(PropertyName = "quicklook")]
        public QuickLook QuickLook { get; set; }

        [JsonProperty(PropertyName = "core")]
        public Core Core { get; set; }
        */    

        [JsonProperty(PropertyName = "ip")]
        public Ip Ip { get; set; }
    }
}
