using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVCClient.DataObjects
{
    class UnitLog
    {
        public String Id { get; set; }

        [JsonProperty(PropertyName = "Message")]
        public String Message { get; set; }

        [JsonProperty(PropertyName = "BatteryPercentage")]
        public Double BatteryPercentage { get; set; }

        [JsonProperty(PropertyName = "ReportedTime")]
        public DateTime ReportedTime { get; set; }

        [JsonProperty(PropertyName = "unitId")]
        public String UnitId { get; set; }
    }
}
