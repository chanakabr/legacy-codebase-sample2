using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using System.Collections.Generic;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Base list wrapper
    /// </summary>
    [Serializable]
    public partial class KalturaListResponse : KalturaOTTObject
    {
        /// <summary>
        /// Total items
        /// </summary>
        [DataMember(Name = "totalCount")]
        [JsonProperty(PropertyName = "totalCount")]
        [XmlElement(ElementName = "totalCount")]
        [ValidationException(SchemeValidationType.NULLABLE)]
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Base list wrapper
    /// </summary>
    [Serializable]
    public abstract partial class KalturaListResponse<KalturaT> : KalturaOTTObject where KalturaT : KalturaOTTObject
    {
        /// <summary>
        /// Total items
        /// </summary>
        [DataMember(Name = "totalCount")]
        [JsonProperty(PropertyName = "totalCount")]
        [XmlElement(ElementName = "totalCount")]
        [ValidationException(SchemeValidationType.NULLABLE)]
        public int TotalCount { get; set; }

        internal abstract void SetData(KalturaGenericListResponse<KalturaT> kalturaGenericList);
        
        public KalturaListResponse(Dictionary<string, object> parameters = null) : base(parameters)
        {
            if (parameters != null)
            {
                if (parameters.ContainsKey("totalCount") && parameters["totalCount"] != null)
                {
                    TotalCount = (Int32)Convert.ChangeType(parameters["totalCount"], typeof(Int32));
                }
            }
        }
    }
}