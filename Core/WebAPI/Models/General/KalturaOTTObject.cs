using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.Social;
using WebAPI.ObjectsConvertor.Extensions;

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

        public KalturaOTTObject(Dictionary<string, object> parameters = null, bool fromRequest = false)
        {
            Init();
        }

        protected virtual void Init()
        {
        }

        public override string ToString()
        {
            switch (this)
            {
                case KalturaMultilingualString k: return MultilingualStringMapper.ToString(k);
                case KalturaSocialActionRate k: return SocialActionMapper.ToString(k);
                case KalturaSocialAction k: return SocialActionMapper.ToString(k);
                default: return base.ToString();
            }
        }
    }
}