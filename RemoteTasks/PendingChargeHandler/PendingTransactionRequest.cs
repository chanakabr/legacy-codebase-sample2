using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PendingTransactionHandler
{
    [Serializable]
    public class PendingTransactionRequest
    {
        [JsonProperty("group_id")]
        public int GroupID
        {
            get;
            set;
        }

        [JsonProperty("id")]
        public long ID
        {
            get;
            set;
        }
    }
}
