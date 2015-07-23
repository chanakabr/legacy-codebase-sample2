using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Base class
    /// </summary>
    public class KalturaOTTObject
    {
        [DataMember(Name = "objectType")]
        [JsonProperty(PropertyName = "objectType")]
        public string objectType { get { return this.GetType().Name; } set { } }
    }
}