using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaUploadedFileTokenResource : KalturaContentResource
    {
        /// <summary>
        /// Token that returned from uploadToken.add action
        /// </summary>
        [DataMember(Name = "token")]
        [JsonProperty(PropertyName = "token")]
        [XmlElement(ElementName = "token")]
        public string Token { get; set; }
    }
}