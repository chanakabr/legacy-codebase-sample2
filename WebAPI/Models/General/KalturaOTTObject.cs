using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using WebAPI.Exceptions;

namespace WebAPI.Models.General
{
    public interface IKalturaOTTObject
    {
    }

    /// <summary>
    /// Base class
    /// </summary>
    public class KalturaOTTObject : IKalturaOTTObject
    {
        [DataMember(Name = "objectType")]
        [JsonProperty(PropertyName = "objectType")]
        [XmlElement(ElementName = "objectType")]
        public virtual string objectType { get { return this.GetType().Name; } set { } }

        [DataMember(Name = "relatedObjects")]
        [JsonProperty(PropertyName = "relatedObjects")]
        [XmlElement(ElementName = "relatedObjects")]
        public SerializableDictionary<string, KalturaListResponse> relatedObjects
        {
            get;
            set;
        }

        public virtual void AfterRequestParsed(string service, string action, string language, int groupId, string userId, string deviceId, JObject json = null)
        {

        }
    }

    /// <summary>
    /// Base class
    /// </summary>
    public class KalturaOTTFile
    {
        public KalturaOTTFile(string filepath)
        {
            path = filepath;
        }

        public string path 
        {
            get;
            set;
        }
    }
}