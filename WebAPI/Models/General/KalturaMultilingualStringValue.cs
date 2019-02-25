using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.App_Start;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Array of translated strings
    /// </summary>
    public partial class KalturaMultilingualStringValue : KalturaValue
    {
        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value")]
        [XmlElement("value", IsNullable = true)]
        [JsonProperty("value")]
        public KalturaMultilingualString value { get; set; }
    }
}