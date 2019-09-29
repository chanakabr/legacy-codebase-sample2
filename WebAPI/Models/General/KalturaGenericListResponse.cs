using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Generic response list
    /// </summary>
    [Serializable]
    public partial class KalturaGenericListResponse<KalturaT> : KalturaListResponse
        where KalturaT : KalturaOTTObject
    {
        public KalturaGenericListResponse(Dictionary<string, object> parameters = null) : base(parameters)
        {
            if (parameters != null)
            {
                if (parameters.ContainsKey("objects") && parameters["objects"] != null)
                {
                    if (parameters["objects"] is JArray)
                    {
                        Objects = buildList<KalturaT>(typeof(KalturaT), (JArray)parameters["objects"]);
                    }
                    else if (parameters["objects"] is IList)
                    {
                        Objects = buildList(typeof(KalturaT), parameters["objects"] as object[]);
                    }
                }
            }
        }

        /// <summary>
        /// A list of objects
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaT> Objects { get; set; }

        protected override void Init()
        {
            base.Init();
            TotalCount = 0;
        }
    }
}