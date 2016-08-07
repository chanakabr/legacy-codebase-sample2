using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.Catalog
{
    public class KalturaRelatedExternalFilter : KalturaAssetFilter
    {
         /// <summary>
        /// the External ID of the asset for which to return related assets
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual", IsNullable = true)]
        public int IdEqual { get; set; }

         /// <summary>
        /// Comma separated list of asset types to search within. 
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty("typeIn")]
        [XmlElement(ElementName = "typeIn", IsNullable = true)]
        public string TypeIn { get; set; }        

        /// <summary>
        /// UtcOffsetEqual 
        /// </summary>
        [DataMember(Name = "utcOffsetEqual")]
        [JsonProperty("utcOffsetEqual")]
        [XmlElement(ElementName = "utcOffsetEqual", IsNullable = true)]
        public int UtcOffsetEqual { get; set; }

         /// <summary>
        ///FreeText
        /// </summary>
        [DataMember(Name = "freeTextEqual")]
        [JsonProperty("freeTextEqual")]
        [XmlElement(ElementName = "freeTextEqual", IsNullable = true)]
        public string FreeTextEqual { get; set; }

       
        internal List<int> getTypeIn()
        {
            if (string.IsNullOrEmpty(TypeIn))
                return null;

            List<int> values = new List<int>();
            string[] stringValues = TypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, string.Format("Filter.TypeIn contains invalid id {0}", value));
                }
            }

            return values;
        }
                       
        public KalturaRelatedExternalFilter(KalturaRelatedExternalFilter k)
        {
            this.FreeTextEqual = k.FreeTextEqual;
            this.IdEqual = k.IdEqual;
            this.TypeIn = k.TypeIn;
            this.UtcOffsetEqual = k.UtcOffsetEqual;
        }
        internal override void Validate()
        {
            if (IdEqual == 0)
            {
                throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id cannot be 0");
            }           
        }
    }
}