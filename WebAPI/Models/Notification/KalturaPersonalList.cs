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
        /// <![CDATA[
        /// Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// Possible keys: any Tag or Meta defined in the system and the following reserved keys: start_date, end_date. 
        /// epg_id, media_id - for specific asset IDs.
        /// geo_block - only valid value is "true": When enabled, only assets that are not restricted to the user by geo-block rules will return.
        /// parental_rules - only valid value is "true": When enabled, only assets that the user doesn't need to provide PIN code will return.
        /// user_interests - only valid value is "true". When enabled, only assets that the user defined as his interests (by tags and metas) will return.
        /// epg_channel_id – the channel identifier of the EPG program.
        /// entitled_assets - valid values: "free", "entitled", "not_entitled", "both". free - gets only free to watch assets. entitled - only those that the user is implicitly entitled to watch.
        /// asset_type - valid values: "media", "epg", "recording" or any number that represents media type in group.
        /// Comparison operators: for numerical fields =, >, >=, <, <=, : (in). 
        /// For alpha-numerical fields =, != (not), ~ (like), !~, ^ (any word starts with), ^= (phrase starts with), + (exists), !+ (not exists).
        /// Logical conjunction: and, or. 
        /// Search values are limited to 20 characters each for the next operators: ~, !~, ^, ^=
        /// (maximum length of entire filter is 2048 characters)]]>
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
        CREATE_DATE_DESC,
        CREATE_DATE_ASC
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
            return KalturaPersonalListOrderBy.CREATE_DATE_DESC;
        }
    }
}