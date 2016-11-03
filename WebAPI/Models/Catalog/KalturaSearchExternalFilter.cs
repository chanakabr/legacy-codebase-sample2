using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public class KalturaSearchExternalFilter : KalturaAssetFilter
    {
        /// <summary>
        ///Query
        /// </summary>
        [DataMember(Name = "query")]
        [JsonProperty("query")]
        [XmlElement(ElementName = "query", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Query { get; set; }

        /// <summary>
        /// UtcOffsetEqual 
        /// </summary>
        [DataMember(Name = "utcOffsetEqual")]
        [JsonProperty("utcOffsetEqual")]
        [XmlElement(ElementName = "utcOffsetEqual")]
        public int UtcOffsetEqual { get; set; }

        /// <summary>
        /// Comma separated list of asset types to search within. 
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty("typeIn")]
        [XmlElement(ElementName = "typeIn")]
        public string TypeIn { get; set; }

        internal List<int> getTypeIn()
        {
            if (string.IsNullOrEmpty(TypeIn))
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchExternalFilter.typeIn");

            List<int> values = new List<int>();
            string[] stringValues = TypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            bool containsEpg = false;
            bool containsMedia = false;
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    if (value == 0)
                    {
                        containsEpg = true;
                    }
                    else
                    {
                        containsMedia = true;
                    }

                    if (containsEpg && containsMedia)
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchExternalFilter.typeIn can't contain both EPG and Media");
                    }
                    else
                    {
                        values.Add(value);
                    }
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchExternalFilter.typeIn");
                }
            }

            return values;
        }

        internal List<string> convertQueryToList()
        {
            if (string.IsNullOrEmpty(Query))
                return null;

            string[] stringValues = Query.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                if (string.IsNullOrEmpty(stringValue))
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchExternalFilter.query");
                }
            }

            return stringValues.ToList();
        }

    }
}