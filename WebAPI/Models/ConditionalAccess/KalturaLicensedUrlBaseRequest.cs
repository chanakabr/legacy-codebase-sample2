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
    public class KalturaLicensedUrlBaseRequest : KalturaOTTObject
    {
        /// <summary>
        /// Asset identifier
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty("assetId")]
        [XmlElement(ElementName = "assetId")]
        public string AssetId { get; set; }

        internal virtual void Validate()
        {
            if (string.IsNullOrEmpty(AssetId))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "assetId cannot be empty");
            }
        }
    }
}