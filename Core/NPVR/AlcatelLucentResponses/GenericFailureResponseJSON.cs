using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPVR.AlcatelLucentResponses
{
    [Serializable]
    public class GenericFailureResponseJSON
    {
        [JsonProperty("resultCode")]
        public int ResultCode { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("GenericFailureResponseJSON. ");
            sb.Append(String.Concat(" Result Code: ", ResultCode));
            sb.Append(String.Concat(" Title: ", Title));
            sb.Append(String.Concat(" Desc: ", Description));

            return sb.ToString();
        }
    }
}
