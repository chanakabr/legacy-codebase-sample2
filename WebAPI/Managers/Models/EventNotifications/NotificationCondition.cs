using EventManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebAPI.Managers.Models
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class NotificationCondition
    {
        public virtual bool Evaluate(KalturaEvent kalturaEvent, object eventObject)
        {
            return true;
        }
    }
}