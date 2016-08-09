using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public class KalturaChannelExternalFilter : KalturaAssetFilter
    {
        /// <summary>
        ///External Channel Id. 
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual", IsNullable = true)]
        public int IdEqual { get; set; }
        
        /// <summary>
        /// UtcOffsetEqual 
        /// </summary>
        [DataMember(Name = "utcOffsetEqual")]
        [JsonProperty("utcOffsetEqual")]
        [XmlElement(ElementName = "utcOffsetEqual", IsNullable = true)]
        public string UtcOffsetEqual { get; set; }

        /// <summary>
        ///FreeTextEqual
        /// </summary>
        [DataMember(Name = "freeText")]
        [JsonProperty("freeText")]
        [XmlElement(ElementName = "freeText", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string FreeText { get; set; }
              
        public KalturaChannelExternalFilter()
        {
        }

        internal override void Validate()
        {
            if (IdEqual <= 0)
            {
                throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id must be positive");
            }
            else
            {
                double utcOffsetDouble;
                if (!string.IsNullOrEmpty(UtcOffsetEqual))
                {
                    if (!double.TryParse(UtcOffsetEqual, out utcOffsetDouble))
                    {
                        throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "UTC Offset must be a valid number between -12 and 12");
                    }
                    else if (utcOffsetDouble > 12 || utcOffsetDouble < -12)
                    {
                        throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "UTC Offset must be a valid number between -12 and 12");
                    }
                }
            }
        }
    }
}