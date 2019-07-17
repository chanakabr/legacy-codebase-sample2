using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.App_Start;
using WebAPI.Managers.Scheme;

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