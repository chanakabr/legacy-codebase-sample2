using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace WebAPI.Models.General
{
    public interface IKalturaListResponse
    {
        string ToJson(Version currentVersion, bool omitObsolete);
        string ToXml(Version currentVersion, bool omitObsolete);
    }

    // TODO SHIR - use it in all places and delete to old object
    /// <summary>
    /// Base wrapper for list of KalturaOTTObject
    /// </summary>
    [Serializable]
    public abstract partial class KalturaListResponse<KalturaT> : KalturaOTTObject, IKalturaListResponse where KalturaT : KalturaOTTObject
    {
        /// <summary>
        /// Total items
        /// </summary>
        [DataMember(Name = "totalCount")]
        [JsonProperty(PropertyName = "totalCount")]
        [XmlElement(ElementName = "totalCount")]
        [ValidationException(SchemeValidationType.NULLABLE)]
        public int TotalCount { get; set; }
        
        /// <summary>
        /// A list of objects
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaT> Objects { get; set; }
        
        //public KalturaListResponse(Dictionary<string, object> parameters = null) : base(parameters)
        //{
        //    if (parameters != null)
        //    {
        //        if (parameters.ContainsKey("objects") && parameters["objects"] != null)
        //        {
        //            if (parameters["objects"] is JArray)
        //            {
        //                Objects = buildList<KalturaT>(typeof(KalturaT), (JArray)parameters["objects"]);
        //            }
        //            else if (parameters["objects"] is IList)
        //            {
        //                Objects = buildList(typeof(KalturaT), parameters["objects"] as object[]);
        //            }
        //        }

        //        if (parameters.ContainsKey("totalCount") && parameters["totalCount"] != null)
        //        {
        //            TotalCount = (Int32)Convert.ChangeType(parameters["totalCount"], typeof(Int32));
        //        }
        //    }
        //}

        protected override void Init()
        {
            base.Init();
            TotalCount = 0;
        }

        public string ToJson(Version currentVersion, bool omitObsolete)
        {
            return base.ToJson(currentVersion, omitObsolete);
        }

        public string ToXml(Version currentVersion, bool omitObsolete)
        {
            return base.ToXml(currentVersion, omitObsolete);
        }
    }
    
    public partial class KalturaListResponse : KalturaOTTObject, IKalturaListResponse
    {
        /// <summary>
        /// Total items
        /// </summary>
        [DataMember(Name = "totalCount")]
        [JsonProperty(PropertyName = "totalCount")]
        [XmlElement(ElementName = "totalCount")]
        [ValidationException(SchemeValidationType.NULLABLE)]
        public int TotalCount { get; set; }
        
        public string ToJson(Version currentVersion, bool omitObsolete)
        {
            return base.ToJson(currentVersion, omitObsolete);
        }

        public string ToXml(Version currentVersion, bool omitObsolete)
        {
            return base.ToXml(currentVersion, omitObsolete);
        }
    }
}