using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Models
{
    [Serializable]
    public class EventNotification
    {
        [JsonProperty("webservice_type")]
        public Type WebServiceType
        {
            get;
            set;
        }

        [JsonProperty("phoenix_type")]
        public Type PhoenixType
        {
            get;
            set;
        }

        [JsonProperty("group_status")]
        public Dictionary<int, bool> GroupStatus
        {
            get;
            set;
        }
    }
}