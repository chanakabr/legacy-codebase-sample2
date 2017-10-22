using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public enum KalturaAssetStructOrderBy
    {
        NAME_ASC,
        NAME_DESC,
        SYSTEM_NAME_ASC,
        SYSTEM_NAME_DESC,
        CREATE_DATE_ASC,
        CREATE_DATE_DESC,
        UPDATE_DATE_ASC,
        UPDATE_DATE_DESC
    }

    /// <summary>
    /// Filtering Asset Structs
    /// </summary>
    [Serializable]
    public class KalturaAssetStructFilter: KalturaFilter<KalturaAssetStructOrderBy>
    {
        /// <summary>
        /// Comma separated identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        [SchemeProperty(DynamicMinInt = 1)]
        public string IdIn { get; set; }

        /// <summary>
        /// Comma separated meta identifiers
        /// </summary>
        [DataMember(Name = "metaIdEqual")]
        [JsonProperty("metaIdEqual")]
        [XmlElement(ElementName = "metaIdEqual", IsNullable = true)]
        [SchemeProperty(DynamicMinInt = 1)]
        public long? MetaIdEqual { get; set; }

        /// <summary>
        /// Comma separated meta identifiers
        /// </summary>
        [DataMember(Name = "isProtected")]
        [JsonProperty("isProtected")]
        [XmlElement(ElementName = "isProtected", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool? IsProtected { get; set; }

        public override KalturaAssetStructOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetStructOrderBy.NAME_ASC;
        }

        internal void Validate()
        {
            if (!string.IsNullOrEmpty(IdIn) && MetaIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaAssetStructFilter.idIn, KalturaAssetStructFilter.metaIdEqual");
            }
        }

        public List<long> GetIdIn()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetStructFilter.idIn");
                    }
                }
            }

            return list;
        }        

    }
}