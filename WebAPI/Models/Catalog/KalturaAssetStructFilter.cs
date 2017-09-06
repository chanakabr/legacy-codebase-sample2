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
        [XmlArray(ElementName = "idIn", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public string IdIn { get; set; }

        /// <summary>
        /// Comma separated meta identifiers
        /// </summary>
        [DataMember(Name = "metaIdContains")]
        [JsonProperty("metaIdContains")]
        [XmlArray(ElementName = "metaIdContains", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string MetaIdContains { get; set; }

        public override KalturaAssetStructOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetStructOrderBy.NAME_ASC;
        }

        internal void Validate()
        {
            if (!string.IsNullOrEmpty(IdIn) && !string.IsNullOrEmpty(MetaIdContains))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaAssetStructFilter.idIn, KalturaAssetStructFilter.metaIdContains");
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

        public List<long> GetMetaIdContains()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(MetaIdContains))
            {
                string[] stringValues = MetaIdContains.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetStructFilter.metaIdContainsmo");
                    }
                }
            }

            return list;
        }

    }
}