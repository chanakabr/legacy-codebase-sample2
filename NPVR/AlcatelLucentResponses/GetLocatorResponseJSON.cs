using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPVR.AlcatelLucentResponses
{
    [Serializable]
    public class GetLocatorResponseJSON
    {
        [JsonProperty("locator")]
        public string Locator { get; set; }
    }
}
