using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlancesClientSharp.Glances.Plugins
{
    [GlancesPluginAttribute("quicklook")]
    public class QuickLook
    {
        [JsonProperty(PropertyName = "mem")]
        public float Mem { get; set; }
        [JsonProperty(PropertyName = "cpu_hz")]
        public long CpuHz { get; set; }
        [JsonProperty(PropertyName = "cpu_hz_current")]
        public long CpuHzCurrent { get; set; }
        [JsonProperty(PropertyName = "cpu_name")]
        public string CpuName { get; set; }
        [JsonProperty(PropertyName = "swap")]
        public float Swap { get; set; }
        [JsonProperty(PropertyName = "cpu")]
        public float Cpu { get; set; }
        [JsonProperty(PropertyName = "percpu")]
        public PerCpu[] PerCpu { get; set; }

        //public QuickLook() { }
    }
}
