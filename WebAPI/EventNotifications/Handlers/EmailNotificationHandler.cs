using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.EventNotifications
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class EmailNotificationHandler : NotificationAction
    {
        public EmailNotificationHandler() : base()
        {
        }

        internal override void Handle(EventManager.KalturaEvent kalturaEvent, KalturaEventWrapper t)
        {
            // to do
        }

        [JsonProperty("subject")]
        public string Subject
        {
            get;
            set;
        }

        [JsonProperty("body")]
        public string Body
        {
            get;
            set;
        }

        [JsonProperty("from_email")]
        public string FromEmail
        {
            get;
            set;
        }

        [JsonProperty("from_name")]
        public string FromName
        {
            get;
            set;
        }

        [JsonProperty("to")]
        public string To
        {
            get;
            set;
        }

        [JsonProperty("cc")]
        public string Cc
        {
            get;
            set;
        }

        [JsonProperty("bcc")]
        public string Bcc
        {
            get;
            set;
        }

        [JsonProperty("reply_to")]
        public string ReplyTo
        {
            get;
            set;
        }

        [JsonProperty("hostname")]
        public string Hostname
        {
            get;
            set;
        }
    }
}