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

namespace WebAPI.Models.Notification
{
    [Serializable]
    public class KalturaPersonalList : KalturaOTTObject
    {
        /// <summary>
        /// Id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(MinLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Create Date
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Ksql
        /// </summary>
        [DataMember(Name = "ksql")]
        [JsonProperty(PropertyName = "ksql")]
        [XmlElement(ElementName = "ksql")]
        [SchemeProperty(MinLength = 1)]
        public string Ksql { get; set; }

        /// <summary>
        /// Partner List Type (optional)
        /// </summary>
        [DataMember(Name = "partnerListType")]
        [JsonProperty(PropertyName = "partnerListType")]
        [XmlElement(ElementName = "partnerListType")]
        public int PartnerListType { get; set; }

        /// <summary>
        /// Asset types
        /// </summary>
        [DataMember(Name = "assetTypes")]
        [JsonProperty(PropertyName = "assetTypes")]
        [XmlArray(ElementName = "assetTypes", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIntegerValue> AssetTypes { get; set; }

        public int[] GetAssetTypes()
        {
            if (AssetTypes == null)
                return null;

            int[] assetTypes = new int[AssetTypes.Count];
            for (int i = 0; i < AssetTypes.Count; i++)
            {
                assetTypes[i] = AssetTypes[i].value;
            }

            return assetTypes;
        }
    }

    /// <summary>
    /// List of KalturaPersonalList.
    /// </summary>
    [DataContract(Name = "KalturaPersonalListListResponse", Namespace = "")]
    [XmlRoot("KalturaPersonalListListResponse")]
    public class KalturaPersonalListListResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaPersonalList> PersonalListList { get; set; }
    }

    public enum KalturaPersonalListOrderBy
    {
        START_DATE_DESC,
        START_DATE_ASC
    }

    public class KalturaPersonalListFilter : KalturaFilter<KalturaPersonalListOrderBy>
    {
        /// <summary>
        /// Comma separated list of partner list types to search within. 
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "partnerListTypeIn")]
        [JsonProperty("partnerListTypeIn")]
        [XmlElement(ElementName = "partnerListTypeIn", IsNullable = true)]
        public string PartnerListTypeIn { get; set; }

        internal HashSet<int> GetPartnerListTypeIn()
        {
            if (string.IsNullOrEmpty(PartnerListTypeIn))
                return null;

            HashSet<int> values = new HashSet<int>();
            string[] stringValues = PartnerListTypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value) && value != 0)
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaPersonalListFilter.PartnerListTypeIn");
                }
            }

            return values;
        }

        public override KalturaPersonalListOrderBy GetDefaultOrderByValue()
        {
            return KalturaPersonalListOrderBy.START_DATE_DESC;
        }
    }
}