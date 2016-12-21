using EventManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Models
{
    [Serializable]
    public class NotificationFieldEqualsCondition : NotificationCondition
    {
        [JsonProperty("value")]
        public object Value
        {
            get;
            set;
        }

        [JsonProperty("field")]
        public string Field
        {
            get;
            set;
        }

        public override bool Evaluate(KalturaEvent kalturaEvent)
        {
            return base.Evaluate(kalturaEvent);
        }
    }
}