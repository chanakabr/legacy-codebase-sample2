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

        [JsonProperty("partner_id")]
        public int PartnerId
        {
            get;
            set;
        }

        [JsonProperty("phoenix_type")]
        public string PhoenixType
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