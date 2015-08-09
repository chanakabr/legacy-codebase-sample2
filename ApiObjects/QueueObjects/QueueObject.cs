using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace ApiObjects
{
    public abstract class QueueObject
    {
        public QueueObject()
        {
        }
        
        [DataMember]
        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
