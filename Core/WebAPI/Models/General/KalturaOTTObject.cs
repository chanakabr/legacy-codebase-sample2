using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Base class
    /// </summary>
    public partial class KalturaOTTObject : KalturaSerializable, IKalturaOTTObject
    {
        [DataMember(Name = "objectType")]
        [JsonProperty(PropertyName = "objectType")]
        [XmlElement(ElementName = "objectType")]
        public string objectType { get { return this.GetType().Name; } set { } }

        [DataMember(Name = "relatedObjects")]
        [JsonProperty(PropertyName = "relatedObjects")]
        [XmlElement(ElementName = "relatedObjects")]
        public SerializableDictionary<string, IKalturaListResponse> relatedObjects { get; set; }
        
        public KalturaOTTObject(Dictionary<string, object> parameters = null)
        {
            Init();
        }

        protected virtual void Init()
        {
        }
    }
}