using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public class KalturaTagFilter : KalturaFilter<KalturaTagOrderBy>
    {
        /// <summary>
        /// Tag to filter by
        /// </summary>
        [DataMember(Name = "tagEqual")]
        [JsonProperty("tagEqual")]
        [XmlElement(ElementName = "tagEqual")]
        public string TagEqual { get; set; }

        /// <summary>
        /// Tag to filter by
        /// </summary>
        [DataMember(Name = "tagStartsWith")]
        [JsonProperty("tagStartsWith")]
        [XmlElement(ElementName = "tagStartsWith")]
        public string TagStartsWith { get; set; }


        /// <summary>
        /// Topic identifier
        /// </summary>
        [DataMember(Name = "topicIdEqual")]
        [JsonProperty("topicIdEqual")]
        [XmlElement(ElementName = "topicIdEqual")]
        public int TopicIdEqual { get; set; }

        public override KalturaTagOrderBy GetDefaultOrderByValue()
        {
            return KalturaTagOrderBy.NONE;
        }
    }

    public enum KalturaTagOrderBy
    {
        NONE
    }
}