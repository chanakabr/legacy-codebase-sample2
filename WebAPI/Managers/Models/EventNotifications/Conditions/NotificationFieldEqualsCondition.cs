using EventManager;
using EventManager.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Models
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
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

        public override bool Evaluate(KalturaEvent kalturaEvent, object eventObject)
        {
            bool result = false;

            KalturaObjectEvent kalturaObjectEvent = kalturaEvent as KalturaObjectEvent;

            if (kalturaObjectEvent != null && eventObject != null)
            {
                var property = eventObject.GetType().GetProperty(this.Field);

                if (property != null)
                {
                    var objectValue = property.GetValue(eventObject, null);

                    if (objectValue != null)
                    {
                        result = objectValue.Equals(this.Value);
                    }
                    else if (this.Value == null)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }
    }
}