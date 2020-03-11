using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.MediaMarks
{
    [Serializable]
    public class DomainDevice
    {
        [JsonProperty("udid")]
        public string UDID { get; set; }

        [JsonProperty("deviceFamilyId")]
        public int DeviceFamilyId { get; set; }
    }
}
