using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Managers.Schema;
using WebAPI.Exceptions;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaLicensedUrlMediaRequest : KalturaLicensedUrlBaseRequest
    {
        /// <summary>
        /// Identifier of the content to get the link for (file identifier)
        /// </summary>
        [DataMember(Name = "contentId")]
        [JsonProperty("contentId")]
        [XmlElement(ElementName = "contentId")]
        public int ContentId { get; set; }
        
        /// <summary>
        /// Base URL for the licensed URLs
        /// </summary>
        [DataMember(Name = "baseUrl")]
        [JsonProperty("baseUrl")]
        [XmlElement(ElementName = "baseUrl")]
        public string BaseUrl { get; set; }

        internal override void Validate()
        {
            base.Validate();
            if (ContentId == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "contentId cannot be empty");
            }
        }
    }
}