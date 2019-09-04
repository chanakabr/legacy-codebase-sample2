using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WebAPI.Managers.Models
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
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

        [JsonProperty("save_to_db")]
        public bool? SaveToDB
        {
            get;
            set;
        }

        [JsonProperty("actions")]
        public List<NotificationAction> Actions { get; set; }
    }
}