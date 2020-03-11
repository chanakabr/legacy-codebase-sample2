using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace EPG_XDTVTransform
{
    public class EPG_XDTVTransformRequest
    {
        [JsonProperty("xml", Required= Required.Always)]
        public string sXml;
        [JsonProperty("group_id", Required = Required.Always)]
        public int nGroupID;
    }
}
