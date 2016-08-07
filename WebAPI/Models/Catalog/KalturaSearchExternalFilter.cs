using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.Catalog
{
    public class KalturaSearchExternalFilter : KalturaAssetFilter
    {
        /// <summary>
        ///Query
        /// </summary>
        [DataMember(Name = "queryEqual")]
        [JsonProperty("queryEqual")]
        [XmlElement(ElementName = "queryEqual", IsNullable = true)]
        public string QueryEqual { get; set; }

        /// <summary>
        /// UtcOffsetEqual 
        /// </summary>
        [DataMember(Name = "utcOffsetEqual")]
        [JsonProperty("utcOffsetEqual")]
        [XmlElement(ElementName = "utcOffsetEqual", IsNullable = true)]
        public int UtcOffsetEqual { get; set; }

        /// <summary>
        /// Comma separated list of asset types to search within. 
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty("typeIn")]
        [XmlElement(ElementName = "typeIn", IsNullable = true)]
        public string TypeIn { get; set; }

        internal override void Validate()
        {          

        }

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

        public KalturaSearchExternalFilter(KalturaSearchExternalFilter k)
        {
            this.QueryEqual = k.QueryEqual;
            this.UtcOffsetEqual = k.UtcOffsetEqual;
            this.TypeIn = k.TypeIn;
        }

    }
}