using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phx.Lib.Log;
using System.Reflection;

namespace ElasticSearch.Common.DeleteResults
{
    [Serializable]
    public class ESDeleteResult
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [JsonProperty("ok")]
        public bool Ok { get; set; }
        [JsonProperty("found")]
        public bool Found { get; set; }
        [JsonProperty("_index")]
        public string Index { get; set; }
        [JsonProperty("_type")]
        public string Type { get; set; }
        [JsonProperty("_id")]
        public string Id { get; set; }


        public static ESDeleteResult GetDeleteResult(string response)
        {
            ESDeleteResult result = null;

            try
            {
                result = JsonConvert.DeserializeObject<ESDeleteResult>(response);
            }
            catch (Exception ex)
            {
                result = new ESDeleteResult();
                log.Error("Error - " + string.Format("Could not convert ES delete response json. input={0}; ex={1}; stack={2}", response, ex.Message, ex.StackTrace));
            }

            return result;
        }

    }
}
