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

        [JsonProperty("partner_id")]
        public int PartnerId
        {
            get;
            set;
        }

        [JsonProperty("status")]
        public int Status
        {
            get;
            set;
        }

        [JsonProperty("actions")]
        public List<NotificationAction> Actions
        {
            get;
            set;
        }
    }
}