using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Models
{
    [Serializable]
    public class PartnerEventNotification
    {
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